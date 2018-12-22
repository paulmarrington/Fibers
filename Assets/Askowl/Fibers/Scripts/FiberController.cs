// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using UnityEngine;

namespace Askowl {
  /// <a href=""></a>
  /// <inheritdoc />
  public class FiberController : MonoBehaviour {
    private void Start() => DontDestroyOnLoad(gameObject);

    private void Update() => UpdateAllWorkers(Fiber.Queue.Update);

    private void LateUpdate() => UpdateAllWorkers(Fiber.Queue.LateUpdate);

    private void FixedUpdate() => UpdateAllWorkers(Fiber.Queue.FixedUpdate);

    private static void UpdateAllWorkers(Fiber.Queue queue) {
      for (LinkedList<Fiber>.Node node = queue.First, next; node != null; node = next) {
        next = node.Next; // the devil of side-effects, score one for functional programming
        node.Item.Update(fiber: node.Item);
      }
    }
  }
}