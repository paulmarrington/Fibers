// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;
  using System.Collections;
  using UnityEngine;

  /// <a href=""></a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    #region Queues
    /// <a href=""></a><inheritdoc />
    public class Queue : LinkedList<Fiber> {
      /// <inheritdoc />
      public Queue(string name = null) : base(name) { }
    }

    /// <a href=""></a><inheritdoc />
    public class FiberQueues : LinkedList<Queue> {
      /// <a href=""></a><inheritdoc />
      public FiberQueues(string name = null) : base(name) { }
    }

    /// <a href=""></a>
    public static Queue Enqueue(string name) => Enqueue(name, UpdateQueues);

    private static Queue Enqueue(string name, FiberQueues fiberQueues) {
      var fibers = new Queue(name);
      fiberQueues.Add(fibers);
      return fibers;
    }

    // Linked list of queues that are linked lists of fibers - accessed by FiberController OnUpdate
    internal static readonly FiberQueues UpdateQueues      = new FiberQueues("Update Fibers");
    internal static readonly FiberQueues LateUpdateQueues  = new FiberQueues("LateUpdate Fibers");
    internal static readonly FiberQueues FixedUpdateQueues = new FiberQueues("FixedUpdate Fibers");

    // Linked list of fibers
    internal static readonly Queue OnUpdatesQueue      = Enqueue("Update Fibers",      UpdateQueues);
    internal static readonly Queue OnLateUpdatesQueue  = Enqueue("LateUpdate Fibers",  LateUpdateQueues);
    internal static readonly Queue OnFixedUpdatesQueue = Enqueue("FixedUpdate Fibers", FixedUpdateQueues);
    #endregion

    #region Controller Support
    /// <a href="">Reference to Unity Fiber Controller</a>
    public static MonoBehaviour Controller;

    /// <a href="">Called by FiberController on each Unity Update</a>
    protected internal void OnUpdate() {
      if (currentAction >= actionCount) {
        if (currentActionList == actionListCount) {
          node.Recycle();
          return;
        }

        currentActionList = (currentActionList + 1) % actions.Length;
        currentAction     = 0;
        actionCount       = actions[currentActionList].Length;
      }

      actions[currentActionList][currentAction++](this);
    }
    #endregion

    #region Fiber Instantiation
    /// <a href=""></a>
    public static Fiber Start(Action[] actions) => Start(OnUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnUpdates(Action[] actions) => Start(OnUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnLateUpdates(Action[] actions) => Start(OnLateUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnFixedUpdates(Action[] actions) => Start(OnFixedUpdatesQueue, actions);

    /// <a href=""></a>
    private Queue.Node node;

    /// <a href=""></a>
    public Queue UpdateQueue;

    private static Fiber Start(Queue updateQueue, Action[] actions) {
      if (Controller == null) Controller = Components.Create<FiberController>("FiberController");

      var node  = updateQueue.Fetch();
      var fiber = node.Item;
      fiber.UpdateQueue = updateQueue;
      fiber.node        = node;
      fiber.Do(actions);
      return fiber;
    }
    #endregion

    #region Fiber Action Support
    /// <a href=""></a>
    public delegate void Action(Fiber fiber);

    private readonly Action[][] actions = new Action[128][];

    private int currentAction, actionCount, currentActionList, actionListCount;
    #endregion

    #region Things we can do with Fibers
    /// <a href=""></a>
    public Fiber Do(Action[] moreActions) {
      if (moreActions.Length == 0) return this;

      if (actionListCount >= actions.Length) {
        throw new OverflowException(
          $"More that {actions.Length} action lists for Fiber on {node.Owner.Name}");
      }

      actions[actionListCount++] = moreActions;
      return this;
    }

    /// <a href=""></a>
    public IEnumerator AsCoroutine() {
      yield return null; //#TBD#//
    }
    #endregion
  }
}