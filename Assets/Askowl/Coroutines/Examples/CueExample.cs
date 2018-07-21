using UnityEngine.TestTools;
using System.Collections;
using Assert = UnityEngine.Assertions.Assert;

namespace Askowl.Fibers {
  public class NewTestScript {
    private int counter;

    [UnityTest]
    public IEnumerator CueNewCoroutine() {
      counter = 0;
      Cue.NewCoroutine(SimpleFiber);
      Assert.AreEqual(counter, 0);
      yield return null;

      Assert.AreEqual(counter, 1);
      Assert.IsTrue(Cue.WorkerGenerators.ContainsKey(SimpleFiber));
      var simpleFiberWorker = Cue.WorkerGenerators[SimpleFiber];
      Assert.AreEqual(simpleFiberWorker.Fibers.Count,   0);
      Assert.AreEqual(simpleFiberWorker.Recycled.Count, 1);
    }

    private IEnumerator SimpleFiber() {
      counter = 1;
      yield break;
    }
  }
}