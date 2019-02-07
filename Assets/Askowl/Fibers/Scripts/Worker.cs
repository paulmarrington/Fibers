// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable StaticMemberInGenericType

using System;
using UnityEngine;

namespace Askowl {
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href="http://bit.ly/2Ptbf6V">List of workers associated with Fiber actions</a>
    public readonly Fifo<Worker> Workers = Fifo<Worker>.Instance;

    /// <a href="http://bit.ly/2Ptbf6V">Abstract code to implement a worker</a>
    public abstract class Worker : IDisposable {
      /// <a href="http://bit.ly/2Ptbf6V">Fiber that owns this worker instance</a>
      protected Fiber Fiber;
      /// <a href="http://bit.ly/2Ptbf6V">Queue that Fiber came from and will return to after worker is done</a>
      internal Queue From;

      private protected static void Deactivate(LinkedList<Fiber>.Node node) => node.MoveTo(node.Item.Workers.Top.From);

      private protected static int Compare(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
        left.Item.Workers.Top?.CompareTo(right.Item.Workers.Top) ?? ((right.Item.Workers.Top == null) ? 0 : -1);

      /// <a href="http://bit.ly/2Ptbf6V">Implement for worker queue order</a>
      protected virtual int CompareTo(Worker other) => 0;

      /// <a href="http://bit.ly/2Ptbf6V">Implement to stop scanning based on sort order</a>
      public virtual bool NoMore => false;

      /// <a href="http://bit.ly/2Ptbf6V">Implement on processing worker (usually calls dispose)</a>
      public virtual void Step() { }

      /// <a href="http://bit.ly/2Ptbf6V">Name from worker class name</a>
      public string Name;

      /// <a href="http://bit.ly/2Ptbf6V">Return worker name</a>
      public override string ToString() => Name;

      /// <a href="http://bit.ly/2Ptbf6V">Do any preparation to payload here</a>
      protected abstract bool Prepare();

      /// <a href="http://bit.ly/2Ptbf6V">Implement anything needed before worker is placed in recycle bin</a>
      protected abstract void Recycle();

      /// <a href="http://bit.ly/2Ptbf6V">Move Fiber back to queue it came from</a> <inheritdoc />
      public virtual void Dispose() {
        Fiber.node.MoveTo(From);
        Fiber.Workers.Pop();
        Recycle();
      }
    }

    /// <a href="http://bit.ly/2Ptbf6V">Worker with payload</a> <inheritdoc />
    public abstract class Worker<T> : Worker {
      /// <a href="http://bit.ly/2Ptbf6V">Payload</a>
      public T Seed;

      /// <a href="http://bit.ly/2Ptbf6V">Load happens when we are building up a list of actions</a>
      public Fiber Load(Fiber fiber, T data) {
        if (NeedsUpdates) Instance.Go(onUpdate); // hook into update if this worker needs it
        NeedsUpdates = false;

        Name  = $"{GetType()}-{Uid += 1}";
        Seed  = data;
        Fiber = fiber;
        // ActivateWorker happens when we are executing all the actions in sequence
        if (fiber.Running) { ActivateWorker(fiber); } else { fiber.AddAction(ActivateWorker); }
        return fiber;
      }

      private protected static int Uid;

      /// <a href="http://bit.ly/2Ptbf6V">Move fiber to worker queue and start processing</a>
      protected void ActivateWorker(Fiber fiber) {
        if (!Prepare()) return;
        Debug.Log($"*** ActivateWorker '{Seed}'"); //#DM#// 
        fiber.Workers.Push(this);
        From = (Queue) fiber.node.Owner;
        fiber.node.MoveTo(Queue);
      }

      internal static readonly Queue Queue = new Queue {CompareItem = Compare, DeactivateItem = Deactivate};

      /// <a href="http://bit.ly/2Ptbf6V">Set to false for workers that do not need calls on frame update</a>
      protected static bool NeedsUpdates = true;

      // ReSharper disable once MemberHidesStaticFromOuterClass
      private static readonly Action onUpdate = (fiber) => {
        var node = Queue.First;
        while (node?.Item.Workers.Top?.NoMore == false) {
          var next = node.Next; // save in case Step() calls dispose
          node.Item.Workers.Top.Step();
          node = next;
        }
      };
    }
  }
}