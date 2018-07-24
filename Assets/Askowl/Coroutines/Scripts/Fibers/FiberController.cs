using System;
using System.Collections;
using Askowl.Fibers;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using Object = System.Object;

namespace Askowl.Fibers {
  public partial class FiberController : MonoBehaviour {
    private void Start() { DontDestroyOnLoad(gameObject); }

    private void Update() { UpdateAllWorkers(UpdateWorkers); }

    private void LateUpdate() { UpdateAllFibers(LateUpdateFibers); }

    private void FixedUpdate() { UpdateAllFibers(FixedUpdateFibers); }

    private static void UpdateAllWorkers(Workers workers) {
      for (var workerNode = workers.First; workerNode != null; workerNode = workerNode.Next) {
        UpdateAllFibers(workerNode.Item.Fibers, workerNode.Item.OnUpdate);
      }
    }

    private static void UpdateAllFibers(Fibers fibers, Action<Fiber> onUpdate) {
      var fiberNode = fibers.First;

      while (fiberNode?.InRange == true) {
        var next  = fiberNode.Next;
        var fiber = fiberNode.Item;
        onUpdate(fiber);
        fiberNode = next;
      }
    }

    private static void UpdateAllFibers(Fibers fibers) { UpdateAllFibers(fibers, OnUpdate); }

    private static void OnUpdate(Fiber fiber) {
      if (fiber.Yield.EndYieldCondition()) fiber.Node.MoveTo(fiber.Worker.Fibers);
    }

    public static readonly Workers UpdateWorkers     = new Workers();
    public static readonly Fibers  LateUpdateFibers  = new Fibers();
    public static readonly Fibers  FixedUpdateFibers = new Fibers();
  }
}