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

    private void LateUpdate() { UpdateAllWorkers(LateUpdateWorkers); }

    private void FixedUpdate() { UpdateAllWorkers(FixedUpdateWorkers); }

    private static void UpdateAllWorkers(Workers workers) {
      for (var workerNode = workers.First; workerNode != null; workerNode = workerNode.Next) {
        var fiberNode = workerNode.Item.Fibers.First;

        if (fiberNode != null) {
          while (fiberNode?.InRange == true) {
            var next  = fiberNode.Next;
            var fiber = fiberNode.Item;
            workerNode.Item.OnUpdate(fiber);
            fiberNode = next;
          }
        }
      }
    }

    public static readonly Workers UpdateWorkers     = new Workers() {Name = "Update Workers"};
    public static readonly Workers LateUpdateWorkers  = new Workers() {Name  = "LateUpdate Workers"};
    public static readonly Workers FixedUpdateWorkers = new Workers() {Name  = "FixedUpdate Workers"};
  }
}