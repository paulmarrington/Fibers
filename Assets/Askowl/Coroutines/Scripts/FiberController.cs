using System.Collections;
using Askowl.Fibers;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using Object = System.Object;

namespace Askowl.Fibers {
  public partial class FiberController : MonoBehaviour {
    private void Start() { DontDestroyOnLoad(gameObject); }

    private void Update() { UpdateAllWorkers(); }

    private static void UpdateAllWorkers() {
      for (var worker = Cue.Workers.First; worker != null; worker = worker.Next) {
        var fibers = worker.Item.Fibers;

        var fiber = fibers.First;

        while (fiber?.InRange == true) {
          var next = fiber.Next;
          worker.Item.Process(fiber);
          fiber = next;
        }
      }
    }
  }
}