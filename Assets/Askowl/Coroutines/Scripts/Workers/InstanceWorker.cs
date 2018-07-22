using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class InstanceWorker : Worker<Instance> {
    public static Instances Recycled = new Instances();

    internal Func<IEnumerator> GeneratorFunction;

    internal InstanceWorker() { Register(this); }

    public Worker StartInstance(Instances.Node parentNode) {
      if (Recycled.Empty) {
        var instance = new Instance();
        Recycled.Add(instance);
        instance.Coroutine = FiberMonitor(GeneratorFunction(), instance);
      }

      var node = Recycled.First.MoveTo(Fibers);
      node.Item.parentNode = parentNode;
      node.Item.ownerNode  = node;
      return this;
    }

    private IEnumerator FiberMonitor(IEnumerator fiber, Instance instance) {
      try {
        while (fiber.MoveNext()) yield return fiber.Current;
      } finally {
        instance.ownerNode.MoveTo(Recycled);
        instance.parentNode?.MoveBack();
      }
    }

    protected internal override void OnUpdate(Instances.Node node) {
      if (!Step(node)) {
        OnFinished(node);
        node.MoveTo(Recycled);
      }
    }

    private bool Step(Instances.Node node) {
      var coroutine = node.Item.Coroutine;
      if (!coroutine.MoveNext()) return false;

      var yieldResult = coroutine.Current as Yield;
      if (yieldResult != null) return yieldResult.Worker.OnYield(yieldResult, node);

      var returnedResultType = coroutine.Current?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(coroutine.Current, node);
      }

      return OnYield(coroutine.Current, node);
    }
  }
}