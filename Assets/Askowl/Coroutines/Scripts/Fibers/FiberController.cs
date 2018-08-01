using System;
using System.Collections;
using Askowl.Fibers;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using Object = System.Object;

namespace Askowl.Fibers {
  public partial class FiberController : MonoBehaviour {
    private void Start() { DontDestroyOnLoad(gameObject); }

    private void Update() { UpdateAllWorkers(Fiber.UpdateQueues); }

    private void LateUpdate() { UpdateAllWorkers(Fiber.LateUpdateQueues); }

    private void FixedUpdate() { UpdateAllWorkers(Fiber.FixedUpdateQueues); }

    private static void UpdateAllWorkers(Fiber.FiberQueues queue) {
      queue.Walk((fibersNode) => fibersNode.Item.Walk((node) => node.Item.OnUpdate()));
    }
  }
}