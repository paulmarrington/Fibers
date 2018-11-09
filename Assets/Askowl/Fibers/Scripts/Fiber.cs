// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;
  using System.Collections;
  using UnityEngine;

  /// <a href=""></a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber : IDisposable {
    /// <a href=""></a> <inheritdoc />
    internal class Queue : LinkedList<Fiber> {
      internal static readonly Queue Update      = new Queue { Name = "Update Fibers" };
      internal static readonly Queue LateUpdate  = new Queue { Name = "Late Update Fibers" };
      internal static readonly Queue FixedUpdate = new Queue { Name = "Fixed Update Fibers" };
    }

//    private static  Log.EventRecorder debug = Log.Events("Debug");
    internal static MonoBehaviour Controller;
    internal        Action        Update;

    #region Fiber Instantiation
    private LinkedList<Fiber>.Node node;

    private static void OnUpdate(Fiber fiber) {
      fiber.running = true;
      if (fiber.action?.Previous == null) { ReturnFromCallee(fiber); }
      else { fiber.SetAction("Call", fiber.action.Previous).Item(fiber); }
    }

    /// <a href=""></a>
    public static Fiber Start {
      get {
        var newFiber = StartWithAction(OnUpdate);
        newFiber.actions = Cache<ActionList>.Instance;
        newFiber.SetAction("Start", null);
        newFiber.running = false;
        return newFiber;
      }
    }

    private LinkedList<Action>.Node SetAction(string reason, LinkedList<Action>.Node nextAction) {
      action = nextAction;
      #if UNITY_EDITOR
      if (Debugging) Log.Debug($"Fiber: {reason,10} for {this}");
      #endif
      return action;
    }

    private bool running;

    private static Fiber StartWithAction(Action onUpdate) {
      if (Controller == null) Controller = Components.Create<FiberController>("FiberController");

      var node  = Queue.Update.GetRecycledOrNew();
      var fiber = node.Item;
      fiber.node   = node;
      fiber.Update = onUpdate;
      fiber.caller = null;
      return fiber;
    }
    #endregion

    #region Fiber Action Support
    /// <a href=""></a> //#TBD#//
    public delegate void Action(Fiber fiber);

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ActionList : LinkedList<Action> { }

    private ActionList              actions;
    private LinkedList<Action>.Node action;

    /// <a href=""></a>
    public void Dispose() {
      actions.Dispose();
      Cache<ActionList>.Dispose(actions);
      waitingOnCallee?.Dispose();
      repeats.Dispose();
    }
    #endregion

    #region Things we can do with Fibers
    /// <a href=""></a>
    public Fiber OnUpdates => MoveTo(Queue.Update);
    /// <a href=""></a>
    public Fiber OnFixedUpdates => MoveTo(Queue.Update);
    /// <a href=""></a>
    public Fiber OnLateUpdates => MoveTo(Queue.Update);

    private Fiber MoveTo(Queue queue) {
      node.MoveTo(queue);
      return this;
    }

    /// <a href=""></a>
    public Fiber Do(Action nextAction, string actionsText = "Actions") {
      actions.Add(nextAction); // Now there is at least one on the list
      #if UNITY_EDITOR
      if (Debugging) Log.Debug($"Fiber: {actionsText,10} for {this}");
      #endif
      return PrepareFiberToRun(); // sets action and always return this
    }

    /// <a href=""></a> //#TBD#//
    public static bool Debugging = false;

    private Fiber PrepareFiberToRun() {
      // add an empty start action so that action.Previous points to last valid action
      if (action == null) SetAction("PrepToRun", actions.Add(start).MoveToEndOf(actions));
      return this;
    }

    // ReSharper disable once InconsistentNaming
    private static void start(Fiber fiber) { }

    /// <a href=""></a>
    public Fiber Begin {
      get {
        Fiber parent = PrepareFiberToRun().StartCall;
        Fiber child  = Start;
        child.caller = parent;
        return child;
      }
    }

    /// <a href=""></a>
    public Fiber Idle => PrepareFiberToRun().StartCall;

    /// <a href=""></a>
    public Fiber Restart {
      get {
        waitingOnCallee.Fire();
        return this;
      }
    }

    /// <a href=""></a>
    public void Break() => SetAction("Break", actions.First);

    /// <a href=""></a>
    public Fiber End => EndCallee(ReturnFromCallee);

    /// <a href=""></a>
    public Fiber Again => EndCallee(ToBegin);

    /// <a href=""></a>
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

    private static void ToBegin(Fiber fiber) => fiber.SetAction("To-Being", fiber.actions.Last);

    private Fiber       caller, idler;
    private CounterFifo repeats = CounterFifo.Instance;

    private Fiber StartCall {
      get {
        running = false;
        return Emitter(waitingOnCallee ?? (waitingOnCallee = Askowl.Emitter.Instance));
      }
    }

    private Emitter waitingOnCallee;

    private static void ReturnFromCallee(Fiber callee) {
      var caller = callee.caller;
      if (caller != null) {
        callee.caller = null;
        caller.waitingOnCallee.Fire();
      }
      callee.node.Recycle();
    }

    /// <a href=""></a>
    public IEnumerator AsCoroutine() {
      bool done;
      void yield(Fiber fiber) => done = true;
      Do(yield);
      for (done = false; done != true;) yield return null;
    }
    #endregion

    #region Debugging
    /// <a href=""></a> //#TBD#// <inheritdoc />
    public override string ToString() {
      string worker = Workers.Empty ? "none" : $"{Workers.Top}";
      return $"{ActionNames} (Worker: {worker} // Owner: {node.Owner.Name} // Home: {node.Home.Name})";
    }

    private string ActionNames {
      get {
        var array = new string[actions.Count];
        // ReSharper disable once LocalVariableHidesMember
        var node = actions.Last;

        for (var idx = 0; idx < array.Length; node = node.Previous, idx++) {
          string name = node.Item.Method.Name;
          array[idx] = (node == action) ? $"[{name}]" : name;
        }
        return Csv.ToString(array);
      }
    }
    #endregion
  }
}