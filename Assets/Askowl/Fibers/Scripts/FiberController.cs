// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using UnityEngine;

  /// <a href=""></a>
  /// <inheritdoc />
  public class FiberController : MonoBehaviour {
    private void Start() { DontDestroyOnLoad(gameObject); }

    private void Update() { UpdateAllWorkers(Fiber.Queue.Update); }

    private void LateUpdate() { UpdateAllWorkers(Fiber.Queue.LateUpdate); }

    private void FixedUpdate() { UpdateAllWorkers(Fiber.Queue.FixedUpdate); }

    private static void UpdateAllWorkers(Fiber.Queue queue) {
      for (var node = queue.First; node != null; node = node.Next) node.Item.Update(fiber: node.Item);
    }
  }
}