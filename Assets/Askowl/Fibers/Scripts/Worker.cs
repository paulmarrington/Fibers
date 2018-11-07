// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable StaticMemberInGenericType

namespace Askowl {
  using System;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a> //#TBD#//
    public readonly Fifo<Worker> Workers = Fifo<Worker>.Instance;

    /// <a href=""></a>
    public abstract class Worker : IDisposable {
      /// <a href=""></a>
      protected Fiber Fiber;
      /// <a href=""></a>
      internal Queue From;

//      private protected static void Deactivate(LinkedList<Fiber>.Node node) => node.MoveTo(node.Item.Workers.Top.From);
      private protected static void Deactivate(LinkedList<Fiber>.Node node) => node.MoveTo(node.Item.Workers.Top.From);

      private protected static int Compare(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
        left.Item.Workers.Top.CompareTo(right.Item.Workers.Top);

      /// <a href=""></a>
      protected virtual int CompareTo(Worker other) => 0;

      /// <a href=""></a>
      public virtual bool NoMore => false;

      /// <a href=""></a>
      public virtual void Step() { }

      /// <a href=""></a> //#TBD#//
      public string Name;

      /// <a href=""></a> //#TBD#//
      public override string ToString() => Name;

      /// <a href=""></a>
      protected abstract void Prepare();

      /// <a href=""></a>
      protected abstract void Recycle();

      /// <a href="">Deactivate worker</a> <inheritdoc />
      public virtual void Dispose() {
        Fiber.node.MoveTo(From);
        Fiber.Workers.Pop();
        Recycle();
      }
    }

    /// <a href=""></a> <inheritdoc />
    public abstract class Worker<T> : Worker {
      /// <a href=""></a>
      public T Seed;

      /// <a href="">Load happens when we are building up a list of actions</a>
      public Fiber Load(Fiber fiber, T data) {
        Name  = $"{GetType()}-{Uid += 1}";
        Seed  = data;
        Fiber = fiber;
        // ActivateWorker happens when we are executing all the actions in sequence
        if (fiber.running) { ActivateWorker(fiber); }
        else { fiber.Do(ActivateWorker, Name); }
        return fiber;
      }

      private protected static int Uid;

      /// <a href=""></a> //#TBD#//
      protected void ActivateWorker(Fiber fiber) {
        fiber.Workers.Push(this);
        From = (Queue) fiber.node.Owner;
        Prepare();
        fiber.node.MoveTo(Queue);
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
            while (node?.Item.Workers.Top?.NoMore == false) {
              var next = node.Next; // save in case Step() calls dispose
              node.Item.Workers.Top.Step();
              node = next;
            }
          });
      }
    }
  }
}