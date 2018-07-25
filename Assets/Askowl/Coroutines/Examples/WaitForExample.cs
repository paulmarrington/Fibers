using System;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace Askowl.Fibers {
  public class WaitForExample {
    private int counter;

    [UnityTest]
    public IEnumerator WaitForCoroutine() {
      counter = 0;
      WaitFor.Updates(ExampleSimpleFiber);
      Assert.AreEqual(counter, 0);
      yield return null; // Unity coroutine here

      Assert.AreEqual(counter, 1);
      CheckFiber(ExampleSimpleFiber);
    }

    private void CheckFiber(Func<IEnumerator> fiber) {
      Assert.IsTrue(WaitFor.WorkerGenerators.ContainsKey(fiber));
      var simpleFiberWorker = WaitFor.WorkerGenerators[fiber];
      Assert.AreEqual(simpleFiberWorker.Fibers.Count,   0, "Expecting fiber to have been released");
      Assert.AreEqual(simpleFiberWorker.Recycled.Count, 1, "Expecting fiber to be back in Recycled");
    }

    private IEnumerator ExampleSimpleFiber() {
      counter = 1;
      yield break;
    }

    [UnityTest]
    public IEnumerator WaitForFixedUpdate() {
      WaitFor.FixedUpdates(ExampleFixedUpdateFiber);
      counter = 1;
      yield return new WaitForSeconds(0.2f); // Unity coroutine here

      Assert.AreEqual(counter, 2);
    }

    private IEnumerator ExampleFixedUpdateFiber() {
      counter = 2;
      yield break;
    }
  }
}