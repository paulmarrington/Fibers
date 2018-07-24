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
        WorkerGenerators[fiberGenerator] = new FiberWorker(fiberGenerator);
      }

      return WorkerGenerators[fiberGenerator].StartInstance(parentNode);
    }

    public static Yield NextUpdate() { return UpdateWorker.Instance(FiberController.UpdateWorkers); }

    public static Yield NextLateUpdate() => UpdateFiber.Instance(FiberController.LateUpdateFibers);

    public static Yield NextFixedUpdate() => UpdateFiber.Instance(FiberController.FixedUpdateFibers);

    public static readonly Dictionary<Func<IEnumerator>, FiberWorker> WorkerGenerators
      = new Dictionary<Func<IEnumerator>, FiberWorker>();
  }

  public class UpdateFiber : Worker<Fibers> {

    protected internal override bool OnYield(Fiber fiber) {
      fiber.Node.MoveTo(Parameter(fiber));
      return true;
    }

    protected internal override void OnFinished(Fiber fiber) => fiber.Node.MoveTo(fiber.Worker.Fibers);
  }

  public class UpdateWorker : Worker<Workers> {

    protected internal override bool OnYield(Fiber fiber) {
      fiber.Node.MoveTo(fiber.Worker.Fibers);
      return true;
    }

    protected internal override void OnFinished(Fiber fiber) => fiber.Node.MoveTo(fiber.Worker.Recycled);
  }
}