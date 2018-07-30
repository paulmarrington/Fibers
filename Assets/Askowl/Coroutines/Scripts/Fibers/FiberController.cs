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
        var worker        = workerNode.Item;
        var coroutineNode = worker.Coroutines.First;

        if (coroutineNode != null) {
          while (coroutineNode?.InRange == true) {
            var next = coroutineNode.Next;
            Debug.Log($"**** FiberController:25 fiberNode={coroutineNode.Owner}"); //#DM#//
            worker.OnUpdate(coroutineNode.Item);
            coroutineNode = next;
          }
        }
      }
    }

    public static readonly Workers UpdateWorkers      = new Workers() {Name = "Update Workers"};
    public static readonly Workers LateUpdateWorkers  = new Workers() {Name = "LateUpdate Workers"};
    public static readonly Workers FixedUpdateWorkers = new Workers() {Name = "FixedUpdate Workers"};
  }
}