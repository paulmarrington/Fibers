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

    /// <a href=""></a>
    public static Fiber Start {
      get {
        var newFiber = StartWithAction(
          fiber => {
            fiber.running = true;
            if (fiber.action?.Previous == null) { ReturnFromCallee(fiber); }
            else { (fiber.action = fiber.action.Previous).Item(fiber); }
          });
        newFiber.actions = Cache<ActionList>.Instance;
        newFiber.action  = null;
        newFiber.running = false;
        return newFiber;
      }
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
      Cache<ActionList>.Dispose(actions);
      waitingOnCallee?.Dispose();
//      idler?.Dispose();
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
    public Fiber Do(Action nextAction) {
      actions.Add(nextAction);    // No there is at least one on the list
      return PrepareFiberToRun(); // sets action and always return this
    }

    private Fiber PrepareFiberToRun() {
      // add an empty start action so that action.Previous points to last valid action
      if (action == null) action = actions.Add(DoNothing).MoveToEndOf(actions);
      return this;
    }

    private static void DoNothing(Fiber fiber) { }

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
    public void Break() => action = actions.First;

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
      void nothing(Fiber _) { }
      if (actions.Count <= 2) Do(nothing); // otherwise termination gets in before activation
      Do(endCalleeAction);
      return caller ?? this;
    }

    private static void Repeat(Fiber fiber) {
      fiber.repeats.Next();
      if (!fiber.repeats.Reached(0)) { ToBegin(fiber); } // loop back
      else { ReturnFromCallee(fiber); }                  // all done
    }

    private static void ToBegin(Fiber fiber) => fiber.action = fiber.actions.Last;

    private Fiber       caller, idler;
    private CounterFifo repeats = CounterFifo.Instance;

//    /// <a href=""></a>
//    public Fiber Idle => Emitter(idler ?? (idler = Askowl.Emitter.Instance));

    private Fiber StartCall {
      get {
        running = false;
        return Emitter(waitingOnCallee ?? (waitingOnCallee = Askowl.Emitter.Instance));
      }
    }

//    private Emitter idler;
    private Emitter waitingOnCallee;

    private static void ReturnFromCallee(Fiber callee) {
      var caller = callee.caller;
      if (caller == null) return;

      callee.caller = null;
      caller.waitingOnCallee.Fire();
      callee.node.Recycle();
    }

    /// <a href=""></a>
    public IEnumerator AsCoroutine() {
      bool done;

      void allDone(Fiber fiber) => done = true;

      Do(allDone);
      for (done = false; done != true;) yield return null;
    }
    #endregion

    /// <a href=""></a> //#TBD#// <inheritdoc />
    public override string ToString() => $"{node.Owner.Name}-{actions.Name}";
  }
}