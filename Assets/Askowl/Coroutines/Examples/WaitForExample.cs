using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Askowl.Fibers {
  public class NewTestScript {
    private int counter;

    [UnityTest]
    public IEnumerator WaitForCoroutine() {
      counter = 0;
      WaitFor.Coroutine(SimpleFiber);
      Assert.AreEqual(counter, 0);
      yield return null; // Unity coroutine here

      Assert.AreEqual(counter, 1);
      Assert.IsTrue(WaitFor.WorkerGenerators.ContainsKey(SimpleFiber));
      var simpleFiberWorker = WaitFor.WorkerGenerators[SimpleFiber];
      Assert.AreEqual(simpleFiberWorker.Fibers.Count,   0, "Expecting fiber to have been released");
      Assert.AreEqual(simpleFiberWorker.Recycled.Count, 1, "Expecting fiber to be back in Recycled");
    }

    private IEnumerator SimpleFiber() {
      counter = 1;
      yield break;
    }
  }
}