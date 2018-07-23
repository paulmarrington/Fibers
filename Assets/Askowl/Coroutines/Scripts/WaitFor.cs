using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    private static MonoBehaviour controller;

    public static Fiber Coroutine(Func<IEnumerator> fiberGenerator, Fibers.Node parentNode = null) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      if (!WorkerGenerators.ContainsKey(fiberGenerator)) {
        WorkerGenerators[fiberGenerator] = new FiberWorker {GeneratorFunction = fiberGenerator};
      }

      return WorkerGenerators[fiberGenerator].StartInstance(parentNode);
    }

    public static readonly Dictionary<Func<IEnumerator>, FiberWorker> WorkerGenerators
      = new Dictionary<Func<IEnumerator>, FiberWorker>();

    public static readonly Workers Workers = new Workers();
  }
}