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

    private static  Log.EventRecorder debug = Log.Events("Debug");
    internal static MonoBehaviour     Controller;
    internal        Action            Update;

    #region Fiber Instantiation
    private LinkedList<Fiber>.Node node;

    /// <a href=""></a>
    public static Fiber Start {
      get {
        var newFiber = StartWithAction(
          (fiber) => {
            if (fiber.action.Previous != null) debug(fiber.action.Previous.Item.GetType().Name);
            if (fiber.action.Previous == null) {
              fiber.node.Recycle();
              fiber.Finished();
            }
            else { (fiber.action = fiber.action.Previous).Item(fiber); }
          });
        newFiber.actions = Cache<ActionList>.Instance;
        newFiber.action  = null;
        return newFiber;
      }
    }

    private static Fiber StartWithAction(Action onUpdate) {
      if (Controller == null) Controller = Components.Create<FiberController>("FiberController");

      var node  = Queue.Update.GetRecycledOrNew();
      var fiber = node.Item;
      fiber.node        = node;
      fiber.Update      = onUpdate;
      fiber.repeatCount = -1;
      fiber.from        = null;
      return fiber;
    }
    #endregion

    #region Fiber Action Support
    /// <a href=""></a>
    public delegate void Action(Fiber fiber);

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ActionList : LinkedList<Action> { }

    private ActionList              actions;
    private LinkedList<Action>.Node action;

    /// <a href=""></a>
    public void Dispose() {
      Cache<ActionList>.Dispose(actions);
      idler?.Dispose();
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
      debug("DO");
      actions.Add(nextAction);                    // No there is at least one on the list
      Restart();                                  // In case were were idling
      return (action == null) ? ToBegin() : this; // sets action and always return this
    }

    /// <a href=""></a>
    public Fiber Begin {
      get {
        from = this;
        return Start;
      }
    }

    /// <a href=""></a>
    public Fiber Break() => Finished();

    /// <a href=""></a>
    public Fiber End => Finished();
    /// <a href=""></a>
    public Fiber Again => ToBegin();

    /// <a href=""></a>
    public Fiber Repeat(int count) {
      if (repeatCount        < 0) { repeatCount = count; }
      else if (--repeatCount == 0) return Finished();

      return ToBegin();
    }

    private Fiber ToBegin() {
      action = actions.Last;
      return this;
    }

    private Fiber Finished() => from ?? this;

    private Fiber from;
    private int   repeatCount;

    /// <a href=""></a>
    public Fiber Idle => Emitter(idler ?? (idler = Askowl.Emitter.Instance));

    /// <a href=""></a>
    public void Restart() => idler?.Fire();

    private Emitter idler;

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