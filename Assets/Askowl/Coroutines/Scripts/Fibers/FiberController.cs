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
      for (var workerNode = WaitFor.Workers.First; workerNode != null; workerNode = workerNode.Next) {
        var fibers = workerNode.Item.Fibers;

        var fiberNode = fibers.First;

        while (fiberNode?.InRange == true) {
          var next = fiberNode.Next;
          workerNode.Item.OnUpdate(fiberNode.Item);
          fiberNode = next;
        }
      }
    }
  }
}