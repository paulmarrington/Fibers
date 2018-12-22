// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using UnityEngine;

namespace Askowl {
  /// <a href="http://bit.ly/2DF6QHw">lightweight cooperative multi-tasking</a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber : IDisposable {
    /// <a href="http://bit.ly/2DF6QHw">Container for different update queues</a> <inheritdoc />
    internal class Queue : LinkedList<Fiber> {
      internal static readonly Queue Update      = new Queue {Name = "Update Fibers"};
      internal static readonly Queue LateUpdate  = new Queue {Name = "Late Update Fibers"};
      internal static readonly Queue FixedUpdate = new Queue {Name = "Fixed Update Fibers"};
    }

    internal static MonoBehaviour Controller;
    internal        Action        Update;

    #region Fiber Instantiation

    private LinkedList<Fiber>.Node node;

    private static void OnUpdate(Fiber fiber) {
      fiber.running = true;
      if (fiber.action?.Previous == null) { ReturnFromCallee(fiber); } else {
        fiber.SetAction("Call", fiber.action.Previous).Item(fiber);
      }
    }

    /// <a href="http://bit.ly/2DDvnwP">Prepare a Fiber and place it on the Update queue</a>
    public static Fiber Start {
      get {
        var newFiber = StartWithAction(OnUpdate);
        newFiber.actions = Cache<ActionList>.Instance;
        newFiber.SetAction("Start", null);
        newFiber.running = false;
        newFiber.exiting = false;
        newFiber.id      = ++nextId;
        return newFiber;
      }
    }
    private static int nextId;
    private        int id;

    private LinkedList<Action>.Node SetAction(string reason, LinkedList<Action>.Node nextAction) {
      action = nextAction;
      #if UNITY_EDITOR
      if (Debugging) Log.Debug($"Run: {reason,10} for {this}");
      #endif
      return action;
    }

    private bool running;

    private static Fiber StartWithAction(Action onUpdate) {
      if (Controller == null) Controller = Components.Create<FiberController>("FiberController");
      var node                           = Queue.Update.GetRecycledOrNew();
      var fiber                          = node.Item;
      fiber.node   = node;
      fiber.Update = onUpdate;
      fiber.caller = null;
      return fiber;
    }

    #endregion

    #region Fiber Action Support

    /// <a href="http://bit.ly/2DDZjbO">Method signature for Do(Action) methods</a>
    public delegate void Action(Fiber fiber);

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ActionList : LinkedList<Action> { }

    private ActionList              actions;
    private LinkedList<Action>.Node action;

    /// <a href="http://bit.ly/2DDvnwP">Cleans up Fiber before it goes into the recycling</a>
    public void Dispose() {
      actions.Dispose();
      Cache<ActionList>.Dispose(actions);
      waitingOnCallee?.Dispose();
      repeats.Dispose();
    }

    #endregion

    #region Things we can do with Fibers

    /// <a href="http://bit.ly/2Pqv2Ub">Return Fiber processing to frame Update queue</a>
    public Fiber OnUpdates => MoveTo(Queue.Update);
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to FixedUpdate queue</a>
    public Fiber OnFixedUpdates => MoveTo(Queue.Update);
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to LateUpdate queue</a>
    public Fiber OnLateUpdates => MoveTo(Queue.Update);

    private Fiber MoveTo(Queue queue) {
      node.MoveTo(queue);
      return this;
    }

    /// <a href="http://bit.ly/2DDZjbO">Business logic activation step</a>
    public Fiber Do(Action nextAction, string actionsText = "Actions") {
      actions.Add(nextAction); // Now there is at least one on the list
      #if UNITY_EDITOR
      if (Debugging) Log.Debug($"Add: {actionsText,10} for {this}");
      #endif
      return PrepareFiberToRun(); // sets action and always return this
    }

    /// <a href="http://bit.ly/2DDvmZN">Displays Do() and action events on Unity console</a>
    public static bool Debugging = false;

    private Fiber PrepareFiberToRun() {
      // add an empty start action so that action.Previous points to last valid action
      if (action == null) SetAction("PrepToRun", actions.Add(start).MoveToEndOf(actions));
      return this;
    }

    // ReSharper disable once InconsistentNaming
    private static void start(Fiber fiber) { }

    /// <a href="http://bit.ly/2DDvnNl">Loops and Blocks - Begin/End, Begin/Again, Begin-Repeat</a>
    public Fiber Begin {
      get {
        Fiber parent = PrepareFiberToRun().StartCall();
        Fiber child  = Start;
        child.caller = parent;
        return child;
      }
    }

    /// <a href="http://bit.ly/2DDvmcf">Separate Fiber into more than one statement</a>
    public Fiber Idle => PrepareFiberToRun().StartCall();

    /// <a href="http://bit.ly/2DDvmcf">Restart a fiber that includes an Idle command</a>
    public Fiber Restart() {
      waitingOnCallee?.Fire();
      return this;
    }

    /// <a href="http://bit.ly/2DDvmcf">Restart another fiber that includes an Idle command</a>
    public Fiber Restart(Fiber fiber) {
      void restart(Fiber _) => fiber.waitingOnCallee?.Fire();
      Do(restart);
      return this;
    }

    /// <a href="http://bit.ly/2DDvlFd">Break a Begin/End/Repeat/Again block</a>
    public void Break() => SetAction("Break", actions.First);

    /// <a href="http://bit.ly/2DBVWCe">Abort fiber processing, cleaning up as we go</a>
    public void Exit() {
      exiting = yielding = true;
      SetAction("Exit", actions.First);
    }

    private bool exiting;

    /// <a href="http://bit.ly/2PtbezT">Begin/End block - use Break() to create an `if`</a>
    public Fiber End => EndCallee(ReturnFromCallee);

    /// <a href="http://bit.ly/2DDvnNl">Begin/Again repeating operations. Use Break() or Exit() to leave</a>
    public Fiber Again => EndCallee(ToBegin);

    /// <a href=""></a> //#TBD#//
    public void Finish() { }

    /// <a href="http://bit.ly/2DDvp7V">Begin/Repeat loop for a specific number of times</a>
    public Fiber Repeat(int count) {
      repeats.Start(-(count + 1));
      return EndCallee(Repeat);
    }

    private Fiber EndCallee(Action endCalleeAction) {
      void endCalleeFiller(Fiber _) { }
      if (actions.Count <= 2) Do(endCalleeFiller); // otherwise termination gets in before activation

      Do(endCalleeAction);
      return caller ?? this;
    }

    private static void Repeat(Fiber fiber) {
      fiber.repeats.Next();
      if (!fiber.repeats.Reached(0)) { ToBegin(fiber); } // loop back
      else { ReturnFromCallee(fiber); }                  // all done
    }

    private static void ToBegin(Fiber fiber) => fiber.SetAction("To-Begin", fiber.actions.Last);

    private Fiber       caller, idler;
    private CounterFifo repeats = CounterFifo.Instance;

    private Fiber StartCall() {
      running = false;
      return WaitFor(waitingOnCallee ?? (waitingOnCallee = Emitter.Instance));
    }

    private Emitter waitingOnCallee;

    private static void ReturnFromCallee(Fiber callee) {
      var caller = callee.caller;
      if (caller != null) {
        callee.caller = null;
        if (callee.exiting) caller.Exit();
        caller.waitingOnCallee.Fire();
      }

      callee.node.Recycle();
    }

    /// <a href="http://bit.ly/2DB3wgx">Return an IEnumerator to use with a yield in a Coroutine</a>
    public IEnumerator AsCoroutine() {
      void yield(Fiber fiber) => yielding = true;

      Do(yield);
      for (yielding = false; yielding != true;) yield return null;
    }

    private bool yielding;

    #endregion

    #region Debugging

    /// <a href="http://bit.ly/2DDvmZN">Return Fiber contents and current state</a><inheritdoc />
    public override string ToString() => $"{ActionNames} // {id} // {node.Owner.Name})";

    private string ActionNames {
      get {
        var array = new string[actions.Count];
        // ReSharper disable once LocalVariableHidesMember
        var node = actions.Last;

        for (var idx = 0; idx < array.Length; node = node.Previous, idx++) {
          string instance = node.Item.Target?.GetType().Name;
          string name     = node.Item.Method.Name;
          name       = name == "ActivateWorker" ? instance : instance != null ? $"{instance}.{name}" : name;
          array[idx] = node == action ? $"[{name}]" : name;
        }
        return Csv.ToString(array);
      }
    }

    #endregion
  }
}