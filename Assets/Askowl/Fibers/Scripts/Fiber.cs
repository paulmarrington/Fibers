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
            if (fiber.action?.Previous == null) { ReturnFromCallee(fiber); }
            else { (fiber.action = fiber.action.Previous).Item(fiber); }
          });
        newFiber.actions = Cache<ActionList>.Instance;

        Log.Debug($"Start {newFiber.actions}"); //#DM#//

        newFiber.action  = null;
        return newFiber;
      }
    }

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
//    public Fiber Idle => (idler = Begin).caller; // put fiber on call waiting queue
    public Fiber Idle {
      get {
        Log.Debug($"Idle {actions}"); //#DM#//
        idler = Begin;
        return this;
      }
    }

    /// <a href=""></a>
    public Fiber Restart => idler?.EndCallee(ReturnFromCallee);

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

    private Fiber StartCall => Emitter(waitingOnCallee ?? (waitingOnCallee = Askowl.Emitter.Instance));

//    private Emitter idler;
    private Emitter waitingOnCallee;

    private static void ReturnFromCallee(Fiber callee) {
      Log.Debug($"ReturnFromCallee {callee.actions}, caller={callee.caller?.actions}"); //#DM#//

      var caller = callee.caller;
      if (caller == null) return;

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
  }
}