using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR && Fibers
// ReSharper disable MissingXmlDoc

namespace Askowl.Example.FiberInterruptTranscript {
  public class FiberInterruptTranscript {
    [UnityTest] public IEnumerator TimeoutExample() {
      var start = Time.realtimeSinceStartup;
      yield return Fiber.Start.Timeout(seconds: 0.2f).WaitFor(seconds: 0.5f).AsCoroutine();
      var elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(expected: 0.2f, actual: elapsed, delta: 0.05f);
    }
    [UnityTest] public IEnumerator ExitAnotherFiber() {
      int counter      = 0;
      var mainFiber    = Fiber.Instance;
      var timeoutFiber = Fiber.Instance.WaitFor(seconds: 0.1f).Exit(mainFiber);
      mainFiber.Do(_ => counter++).WaitFor(seconds: 1.0f).Do(_ => counter++).Exit(timeoutFiber).Go();
      yield return timeoutFiber.WaitFor(seconds: 0.2f).AsCoroutine();
      Assert.AreEqual(expected: 1, actual: counter);
    }
    [UnityTest] public IEnumerator CancelOnExample() {
      using (var timeoutEmitter = Emitter.Instance) {
        bool aborted = false;

        Fiber.Start.WaitFor(seconds: 0.1f).Fire(timeoutEmitter).Do(_ => aborted = true);

        Fiber.Start.CancelOn(timeoutEmitter).Begin.WaitFor(seconds: 0.05f).Again.Finish();

        yield return new WaitForSeconds(0.3f);
        Assert.IsTrue(aborted);
      }
    }
    [UnityTest] public IEnumerator AbortedExample() {
      var fiber = Fiber.Instance.Timeout(seconds: 0.2f).Begin.WaitFor(seconds: 0.05f).Again;
      yield return new WaitForSeconds(0.4f);
      Assert.IsTrue(fiber.Aborted);
    }
  }
}
#endif