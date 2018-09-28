// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public abstract class Worker : IDisposable {
      /// <a href=""></a>
      protected Fiber Fiber;
      /// <a href=""></a>
      internal Queue From;

      private protected static void Deactivate(LinkedList<Fiber>.Node node) => node.MoveTo(node.Item.worker.From);

      private protected static int Compare(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
        left.Item.worker.CompareTo(right.Item.worker);

      /// <a href=""></a>
      protected abstract int CompareTo(Worker other);

      /// <a href=""></a>
      public abstract void Dispose();

      /// <a href=""></a>
      public virtual bool NoMore => false;

      /// <a href=""></a>
      public abstract void Update();
    }

    private Worker worker;

    /// <a href=""></a> <inheritdoc />
    public abstract class Worker<T> : Worker {
      /// <a href=""></a>
      public static Fiber Instance(Fiber fiber, T data) => Cache<Worker<T>>.Instance.Load(fiber, data);

      /// <a href=""></a>
      protected T Data;

      /// <a href=""></a>
      public Fiber Load(Fiber fiber, T data) {
        From         = (Queue) fiber.node.Owner;
        Data         = data;
        Fiber        = fiber;
        fiber.worker = this;
        Fiber.node.MoveTo(Queue);
        return fiber;
      }

      /// <a href=""></a> <inheritdoc />
      public override void Dispose() {
        Fiber.node.MoveTo(From);
      }

      // ReSharper disable once StaticMemberInGenericType
      internal static readonly Queue Queue = new Queue { CompareItem = Compare, DeactivateItem = Deactivate };

      static Worker() {
        OnUpdate(
          Queue.Update, (fiber) => {
            for (var node = Queue.First; node?.Item.worker.NoMore == false; node = node.Next) node.Item.worker.Update();
          });
      }
    }
  }
}