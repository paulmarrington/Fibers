using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public class FiberWorker : Worker<Fiber> {
    public Fibers Recycled = new Fibers() {Name = "Recycled Fibers"};

    internal readonly Func<IEnumerator> GeneratorFunction;

    internal FiberWorker(Func<IEnumerator> generatorFunction) {
      GeneratorFunction = generatorFunction;
      Register(this);
      Fibers.Name   = $"{Fibers.Name}:{GeneratorFunction.Method.Name}";
      Recycled.Name = $"Recycler for {Fibers.Name}";
    }

    public Fiber StartInstance(Fibers.Node parentNode) {
      Fibers.Node node;

      if (Recycled.Empty) {
        node                = Recycled.Add(new Fiber());
        node.Item.Coroutine = FiberMonitor(GeneratorFunction(), node);
      }

      node                 = Recycled.First.MoveTo(Fibers);
      node.Item.Node       = node;
      node.Item.ParentNode = parentNode;
      return node.Item;
    }

    private IEnumerator FiberMonitor(IEnumerator coroutine, Fibers.Node node) {
      try {
        while (coroutine.MoveNext()) yield return coroutine.Current;
      } finally {
        node.MoveTo(Recycled);
        node.Item.ParentNode?.MoveBack();
      }
    }

    protected internal override void OnUpdate(Fiber fiber) {
      if (!Step(fiber)) OnFinished(fiber);
    }

    protected internal override void OnFinished(Fiber fiber) { }

    private bool Step(Fiber fiber) {
      if (!fiber.Coroutine.MoveNext()) return false;

      fiber.YieldValue = fiber.Coroutine.Current;

      if (fiber.Coroutine.Current is Yield) {
        return fiber.Yield.Worker.OnYield(fiber);
      }

      var returnedResultType = fiber.Coroutine.Current?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(fiber);
      }

      Debug.LogWarning("It is bad form to use `Yield null` to skip a frame");
      return OnYield(fiber);
    }
  }
}