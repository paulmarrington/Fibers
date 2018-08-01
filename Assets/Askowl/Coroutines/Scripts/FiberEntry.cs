using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;

namespace Askowl.Fibers {
  public partial class Fiber {
    public static MonoBehaviour controller;

    public static Fiber Start(params Action<Fiber>[] actions) => Start(OnUpdatesQueue, actions);

    public static Fiber OnUpdates(params Action<Fiber>[] actions) => Start(OnUpdatesQueue, actions);

    public static Fiber OnLateUpdates(params Action<Fiber>[] actions) => Start(OnLateUpdatesQueue, actions);

    public static Fiber OnFixedUpdates(params Action<Fiber>[] actions) => Start(OnFixedUpdatesQueue, actions);

    private static Fiber Start(Fibers updateQueue, params Action<Fiber>[] actions) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      if (Recycled.Empty) Recycled.Add(new Fiber());

      var node  = Recycled.MoveTo(updateQueue);
      var fiber = node.Item;
      fiber.UpdateQueue = updateQueue;
      fiber.Node        = node;
      fiber.Do(actions);
      return fiber;
    }

    private static readonly  Fibers Recycled            = new Fibers {Name = "Idle Fibers"};
    internal static readonly Fibers OnUpdatesQueue      = Enqueue("Update Fibers",      UpdateQueues);
    internal static readonly Fibers OnLateUpdatesQueue  = Enqueue("LateUpdate Fibers",  LateUpdateQueues);
    internal static readonly Fibers OnFixedUpdatesQueue = Enqueue("FixedUpdate Fibers", FixedUpdateQueues);

    public static Fibers Enqueue(string name) => Enqueue(name, UpdateQueues);

    private static Fibers Enqueue(string name, FiberQueues fiberQueues) {
      var fibers = new Fibers {Name = name};
      fiberQueues.Add(fibers);
      return fibers;
    }

    public class FiberQueues : LinkedList<Fibers> { }

    internal static readonly FiberQueues UpdateQueues      = new FiberQueues {Name = "Update Fibers"};
    internal static readonly FiberQueues LateUpdateQueues  = new FiberQueues {Name = "LateUpdate Fibers"};
    internal static readonly FiberQueues FixedUpdateQueues = new FiberQueues {Name = "FixedUpdate Fibers"};
  }
}