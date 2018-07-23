using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class FiberWorker : Worker<Fiber> {
    public static Fibers Recycled = new Fibers();

    internal Func<IEnumerator> GeneratorFunction;

    internal FiberWorker() { Register(this); }

    public Fiber StartInstance(Fibers.Node parentNode) {
      if (Recycled.Empty) {
        var fiber = new Fiber();
        Recycled.Add(fiber);
        fiber.Coroutine = FiberMonitor(GeneratorFunction(), fiber);
      }

      var node = Recycled.First.MoveTo(Fibers);
      node.Item.Node       = node;
      node.Item.ParentNode = parentNode;
      return node.Item;
    }

    private IEnumerator FiberMonitor(IEnumerator coroutine, Fiber fiber) {
      try {
        while (coroutine.MoveNext()) yield return coroutine.Current;
      } finally {
        fiber.Node.MoveTo(Recycled);
        fiber.ParentNode?.MoveBack();
      }
    }

    protected internal override void OnUpdate(Fiber fiber) {
      if (!Step(fiber)) {
        OnFinished(fiber);
        fiber.Node.MoveTo(Recycled);
      }
    }

    private bool Step(Fiber fiber) {
      if (!fiber.Coroutine.MoveNext()) return false;

      var yield = fiber.Coroutine.Current as Yield;
      if (yield != null) return yield.Worker.OnYield(yield, fiber);

      var returnedResultType = fiber.Coroutine.Current?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(fiber.Coroutine.Current, fiber);
      }

      return OnYield(fiber.Coroutine.Current, fiber);
    }
  }
}