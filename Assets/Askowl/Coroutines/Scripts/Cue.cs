using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public static partial class Cue {
    private static MonoBehaviour controller;

    public static Yield NewCoroutine(Func<IEnumerator> fiberGenerator,
                                     Instances.Node    parentNode = null) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      if (!WorkerGenerators.ContainsKey(fiberGenerator)) {
        WorkerGenerators[fiberGenerator] = new InstanceWorker {GeneratorFunction = fiberGenerator};
      }

      return new Yield<int> {Worker = WorkerGenerators[fiberGenerator].StartInstance(parentNode)};
    }

    public static readonly Dictionary<Func<IEnumerator>, InstanceWorker> WorkerGenerators
      = new Dictionary<Func<IEnumerator>, InstanceWorker>();

    public static readonly Workers Workers = new Workers();
  }
}