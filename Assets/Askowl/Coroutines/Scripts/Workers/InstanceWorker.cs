using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public static partial class Cue {
    public class InstanceWorker : Worker<Instance> {
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
    }
  }
}