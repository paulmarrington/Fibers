// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public class Worker<T> {
      /// <a href=""></a>
      protected T data;

      /// <a href=""></a>
      protected Fiber fiber;

      /// <a href=""></a>
      protected Queue UpdateQueue = OnUpdatesQueue;

      /// <a href=""></a>
      public void Prepare(string name, Queue updateQueue = null) {
        instance             = this;
        instance.UpdateQueue = updateQueue ?? OnUpdatesQueue;

        queue = new Queue(name) { CompareItem = CompareItem, DeactivateItem = DeactivateItem };
      }

      /// <a href=""></a>
      protected virtual void DeactivateItem(LinkedList<Fiber>.Node node) => node.MoveTo(UpdateQueue);

      /// <a href=""></a>
      protected virtual int CompareItem(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) => 0;

      /// <a href=""></a>
      protected virtual T Parse(T naked) => naked;

      /// <a href=""></a>
      protected virtual T Parse(T naked, object[] more) => naked;

      /// <a href=""></a>
      public static Fiber Load(Fiber fiber, T data, object[] parameters) {
        instance.data  = instance.Parse(data, parameters);
        instance.fiber = fiber;
        fiber.node.MoveTo(queue);
        return fiber;
      }

      /// <a href=""></a>
      protected static Worker<T> instance;

      // ReSharper disable once StaticMemberInGenericType
      private static Queue queue;
    }
  }
}