using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    private static MonoBehaviour controller;

    public static Coroutine Updates(Func<IEnumerator> fiberGenerator, Fibers.Node parentNode = null) =>
      StartInstance(fiberGenerator, () => new FiberWorker(fiberGenerator, FiberController.UpdateWorkers), parentNode);

    public static Coroutine Updates(Action action, Fibers.Node parentNode = null) =>
      StartInstance(action, () => new FiberWorker(FiberGenerator(action), FiberController.UpdateWorkers), parentNode);

    public static Coroutine Updates(Func<object> func, Fibers.Node parentNode = null) =>
      StartInstance(func, () => new FiberWorker(FiberGenerator(func), FiberController.UpdateWorkers), parentNode);

    public static Coroutine LateUpdates(Func<IEnumerator> generator, Fibers.Node parentNode = null) =>
      StartInstance(generator, () => new FiberWorker(generator, FiberController.LateUpdateWorkers), parentNode);

    public static Coroutine FixedUpdates(Func<IEnumerator> generator, Fibers.Node parentNode = null) =>
      StartInstance(generator, () => new FiberWorker(generator, FiberController.FixedUpdateWorkers), parentNode);

    #region Private Support Code
    private static Coroutine StartInstance(object key, Func<FiberWorker> newGenerator, Fibers.Node parentNode) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      if (!WorkerGenerators.ContainsKey(key)) WorkerGenerators[key] = newGenerator();

      var node = WorkerGenerators[key].StartInstance(parentNode);
      return new Coroutine {Worker = WorkerGenerators[key], Node = node, ParentNode = parentNode};
    }

    private static Func<IEnumerator> FiberGenerator(Action fiberAction) {
      return delegate {
        fiberAction();
        return EmptyCoroutine();
      };
    }

    private static Func<IEnumerator> FiberGenerator(Func<object> fiberAction) {
      return delegate {
        fiberAction();
        return EmptyCoroutine();
      };
    }

    private static IEnumerator EmptyCoroutine() { yield break; }
    #endregion

    public static readonly Dictionary<object, FiberWorker> WorkerGenerators = new Dictionary<object, FiberWorker>();
  }

  public static class MonoBehaviourExtensions {
    public static Coroutine Fiber(this MonoBehaviour _, Func<IEnumerator> generator) => WaitFor.Updates(generator);
    public static Coroutine Fiber(this MonoBehaviour _, Action            generator) => WaitFor.Updates(generator);
  }
}