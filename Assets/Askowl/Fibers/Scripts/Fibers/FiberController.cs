// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Fibers {
  using UnityEngine;

  /// <a href=""></a>
  /// <inheritdoc />
  public class FiberController : MonoBehaviour {
    private void Start() { DontDestroyOnLoad(gameObject); }

    private void Update() { UpdateAllWorkers(Fiber.UpdateQueues); }

    private void LateUpdate() { UpdateAllWorkers(Fiber.LateUpdateQueues); }

    private void FixedUpdate() { UpdateAllWorkers(Fiber.FixedUpdateQueues); }

    private static void UpdateAllWorkers(Fiber.FiberQueues queue) {
      for (var fibers = queue.First; fibers != null; fibers = fibers.Next) {
        for (var fiber = fibers.Item.First; fiber != null; fiber = fiber.Next) fiber.Item.OnUpdate();
      }
    }
  }
}