using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    private static MonoBehaviour controller;

    public static Fiber Updates(Func<IEnumerator> fiberGenerator, Fibers.Node parentNode = null) =>
      CreateCoroutine(FiberController.UpdateWorkers, fiberGenerator, parentNode);

    public static Fiber LateUpdates(Func<IEnumerator> fiberGenerator, Fibers.Node parentNode = null) =>
      CreateCoroutine(FiberController.LateUpdateWorkers, fiberGenerator, parentNode);

    public static Fiber FixedUpdates(Func<IEnumerator> fiberGenerator, Fibers.Node parentNode = null) =>
      CreateCoroutine(FiberController.FixedUpdateWorkers, fiberGenerator, parentNode);

    #region Common
    private static Fiber CreateCoroutine(Workers     updateQueue, Func<IEnumerator> fiberGenerator,
                                         Fibers.Node parentNode = null) {
      if (controller == null) controller = Components.Create<FiberController>("FiberController");

      if (!WorkerGenerators.ContainsKey(fiberGenerator)) {
        WorkerGenerators[fiberGenerator] = new FiberWorker(fiberGenerator, updateQueue);
      }

      return WorkerGenerators[fiberGenerator].StartInstance(parentNode);
    }
    #endregion

    public static readonly Dictionary<Func<IEnumerator>, FiberWorker> WorkerGenerators
      = new Dictionary<Func<IEnumerator>, FiberWorker>();
  }
}