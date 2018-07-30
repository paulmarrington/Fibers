using System;
using System.Collections;
using UnityEngine;

namespace Askowl.Fibers {
  public class FiberWorker : Worker<Fiber> {
    public Fibers Active   = new Fibers() {Name = "Active Fibers"};
    public Fibers Recycled = new Fibers() {Name = "Recycled Fibers"};

    internal readonly Func<IEnumerator> GeneratorFunction;

    internal FiberWorker(Func<IEnumerator> generatorFunction, Workers updateQueue) : base() {
      GeneratorFunction = generatorFunction;
      Coroutines.Name   = $"{Coroutines.Name}:{GeneratorFunction.Method.Name}";
      Recycled.Name     = $"Recycler for {Coroutines.Name}";
      updateQueue.Add(this);
    }

    public Coroutine StartInstance(Coroutine parent) {
      if (Recycled.Empty) {
        Recycled.Add(new Fiber {Worker = this, Enumerator = GeneratorFunction()});
      }

      var node = Recycled.First.MoveTo(Active);
      coroutine.Node       = node;
      coroutine.ParentNode = parent;
      return coroutine;
    }

    private IEnumerator FiberMonitor(IEnumerator enumerator, Fibers.Node node) {
      try {
        while (enumerator.MoveNext()) yield return enumerator.Current;
      } finally {
        node.MoveTo(Recycled);
        coroutine.ParentNode?.MoveBack();
      }
    }

    protected internal override void OnUpdate(Coroutines.Node node) {
      if (!Step(node)) OnFinished(node);
    }

    protected internal override void OnFinished(Coroutine coroutine) { }

    private bool Step(Coroutines.Node node) {
      if (!node.Item.Coroutine.MoveNext()) return false;

      node.Item.YieldValue = node.Item.Coroutine.Current;

      if (node.Item.Coroutine.Current is Yield) {
        return node.Item.Yield.Worker.OnYield(node);
      }

      var returnedResultType = node.Item.Coroutine.Current?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(node);
      }

      Debug.LogWarning("It is bad form to use `Yield null` to skip a frame");
      return OnYield(node);
    }
  }
}