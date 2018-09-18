// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Fibers {
  using UnityEngine;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public static MonoBehaviour controller;

    /// <a href=""></a>
    public static Fiber Start(params Action[] actions) => Start(OnUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnUpdates(params Action[] actions) => Start(OnUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnLateUpdates(params Action[] actions) => Start(OnLateUpdatesQueue, actions);

    /// <a href=""></a>
    public static Fiber OnFixedUpdates(params Action[] actions) => Start(OnFixedUpdatesQueue, actions);

    private static Fiber Start(Queue updateQueue, params Action[] actions) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      var node  = updateQueue.Fetch();
      var fiber = node.Item;
      fiber.UpdateQueue = updateQueue;
      fiber.Node        = node;
      fiber.Do(actions);
      return fiber;
    }

    // Linked list of queues that are linked lists of fibers - accessed by FiberController OnUpdate
    internal static readonly FiberQueues UpdateQueues      = new FiberQueues("Update Fibers");
    internal static readonly FiberQueues LateUpdateQueues  = new FiberQueues("LateUpdate Fibers");
    internal static readonly FiberQueues FixedUpdateQueues = new FiberQueues("FixedUpdate Fibers");

    // Linked list of fibers
    internal static readonly Queue OnUpdatesQueue      = Enqueue("Update Fibers",      UpdateQueues);
    internal static readonly Queue OnLateUpdatesQueue  = Enqueue("LateUpdate Fibers",  LateUpdateQueues);
    internal static readonly Queue OnFixedUpdatesQueue = Enqueue("FixedUpdate Fibers", FixedUpdateQueues);

    /// <a href=""></a>
    public static Queue Enqueue(string name) => Enqueue(name, UpdateQueues);

    private static Queue Enqueue(string name, FiberQueues fiberQueues) {
      var fibers = new Queue(name);
      fiberQueues.Add(fibers);
      return fibers;
    }

    /// <a href=""></a>
    /// <inheritdoc />
    public class FiberQueues : LinkedList<Queue> {
      /// <a href=""></a>
      /// <inheritdoc />
      public FiberQueues(string name = null) : base(name) { }
    }
  }
}