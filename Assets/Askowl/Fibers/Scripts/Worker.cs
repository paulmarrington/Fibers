// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable StaticMemberInGenericType

namespace Askowl {
  using System;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public class Worker : IDisposable {
      /// <a href=""></a>
      protected Fiber Fiber;
      /// <a href=""></a>
      internal Queue From;

      private protected static void Deactivate(LinkedList<Fiber>.Node node) => node.MoveTo(node.Item.worker.From);

      private protected static int Compare(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
        left.Item.worker.CompareTo(right.Item.worker);

      /// <a href=""></a>
      protected virtual int CompareTo(Worker other) => 0;

      /// <a href=""></a>
      public virtual void Dispose() { }

      /// <a href=""></a>
      public virtual bool NoMore => false;

      /// <a href=""></a>
      public virtual void Step() { }
    }

    public Worker worker;

    /// <a href=""></a> <inheritdoc />
    public abstract class Worker<T> : Worker {
      /// <a href=""></a>
      public T Seed;

      /// <a href="">Load happens when we are building up a list of actions</a>
      public Fiber Load(Fiber fiber, T data) {
        From  = (Queue) fiber.node.Owner;
        Seed  = data;
        Fiber = fiber;
        // ActivateWorker happens when we are executing all the actions in sequence
        return fiber.Do(ActivateWorker);
      }

      private void ActivateWorker(Fiber fiber) {
        fiber.worker = this;
        Prepare();
        fiber.node.MoveTo(Queue);
      }

      /// <a href=""></a>
      protected abstract void Prepare();

      /// <a href=""></a>
      protected abstract void Recycle();

      /// <a href=""></a> <inheritdoc />
      public override void Dispose() {
        Fiber.node.MoveTo(From);
        Recycle();
      }

      // ReSharper disable once StaticMemberInGenericType
      internal static readonly Queue Queue = new Queue { CompareItem = Compare, DeactivateItem = Deactivate };

      /// <a href=""></a>
      protected static bool NeedsUpdates = true;

      static Worker() {
        if (!NeedsUpdates) return;

        StartWithAction(
          fiber => {
            var node = Queue.First;
            while (node?.Item.worker.NoMore == false) {
              var next = node.Next; // save in case Step() calls dispose
              node.Item.worker.Step();
              node = next;
            }
          });
      }
    }
  }
}