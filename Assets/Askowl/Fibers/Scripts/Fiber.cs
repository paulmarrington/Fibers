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

    #region Controller Support
    internal static MonoBehaviour Controller;
    private         Action        onUpdate;

    /// <a href="">Called by FiberController on each Unity Update to work through the list</a>
//    public void OnUpdate() {
//      if (actions.Empty) { node.Recycle(); }
//      else { actions.Pull().Item(this); }
//    }
    #endregion

    #region Fiber Instantiation
    private LinkedList<Fiber>.Node node;

    /// <a href=""></a>
    public static Fiber Start => OnUpdate(Queue.Update);
    /// <a href=""></a>
    public static Fiber OnUpdates => OnUpdate(Queue.Update);
    /// <a href=""></a>
    public static Fiber OnLateUpdates => OnUpdate(Queue.LateUpdate);
    /// <a href=""></a>
    public static Fiber OnFixedUpdates => OnUpdate(Queue.FixedUpdate);

    private static Fiber OnUpdate(Queue updateQueue, Action action) {
      if (Controller == null) Controller = Components.Create<FiberController>("FiberController");

      var node  = updateQueue.GetRecycledOrNew();
      var fiber = node.Item;
      fiber.node     = node;
      fiber.onUpdate = action;
      return fiber;
    }

    private static Fiber OnUpdate(Queue updateQueue) {
      var newFiber = OnUpdate(
        updateQueue, (fiber) => {
          if (fiber.actions.Empty) { fiber.node.Recycle(); }
          else { fiber.actions.Pull().Item(fiber); }
        });
      newFiber.actions = Cache<ActionList>.Instance;
      return newFiber;
    }
    #endregion

    #region Fiber Action Support
    /// <a href=""></a>
    public delegate void Action(Fiber fiber);

    // ReSharper disable once ClassNeverInstantiated.Local
    private class ActionList : LinkedList<Action> { }

    private ActionList actions;

    /// <a href=""></a>
    public void Dispose() { Cache<ActionList>.Dispose(actions); }
    #endregion

    #region Things we can do with Fibers
    /// <a href=""></a>
    public Fiber Do(Action action) {
      actions.Add(action);
      return this;
    }

    /// <a href=""></a>
    public IEnumerator AsCoroutine() {
      yield return null; //#TBD#//
    }
    #endregion
  }
}