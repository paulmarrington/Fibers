using UnityEngine.TestTools;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;

namespace Askowl.Fibers {
  public class NewTestScript {
    private int counter;

    [UnityTest]
    public IEnumerator CueNewCoroutine() {
      counter = 0;
      WaitFor.Coroutine(SimpleFiber);
      Assert.AreEqual(counter, 0);
      yield return null;

      Assert.AreEqual(counter, 1);
      Assert.IsTrue(WaitFor.WorkerGenerators.ContainsKey(SimpleFiber));
      var simpleFiberWorker = WaitFor.WorkerGenerators[SimpleFiber];
      Assert.AreEqual(simpleFiberWorker.Fibers.Count, 0);
      Assert.AreEqual(InstanceWorker.Recycled.Count,  1);
    }

    private IEnumerator SimpleFiber() {
      counter = 1;
      yield break;
    }
  }
}