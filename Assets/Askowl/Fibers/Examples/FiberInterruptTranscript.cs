//- Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- There are times when you will need to prematurely terminate a running fiber. It could be a service that is taking too long, or it could be your program has moved on and not longer needs a result a fiber has been waiting for.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if !ExcludeAskowlTests
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Transcripts {
  public class FiberInterruptTranscript {
    //- The first option is a simple timeout. Once the time has expired the calling fiber is terminated.
    [UnityTest] public IEnumerator TimeoutExample() {
      var start = Time.realtimeSinceStartup;
      yield return Fiber.Start.Timeout(seconds: 0.2f).WaitFor(seconds: 0.5f).AsCoroutine();
      var elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(expected: 0.2f, actual: elapsed, delta: 0.05f);
    }

    //- Often we need to know when a fiber has terminated rather than completed it's allotted task. For this we need two fibers.
    [UnityTest] public IEnumerator ExitAnotherFiber() {
      int counter      = 0;
      var mainFiber    = Fiber.Instance;
      var timeoutFiber = Fiber.Instance.WaitFor(seconds: 0.1f).Exit(mainFiber);
      //- The main fiber will do it's thing then cause the timeout fiber to exit once it is no longer needed.
      mainFiber.Do(_ => counter++).WaitFor(seconds: 1.0f).Do(_ => counter++).Exit(timeoutFiber).Go();
      //- The timeout monitor fiber will force the main fiber to exit early.
      yield return timeoutFiber.WaitFor(seconds: 0.2f).AsCoroutine();
      //- If it worked the second counter update will not occur
      Assert.AreEqual(expected: 1, actual: counter);
    }

    //- Another way to do the same thing is using an emitter.
    [UnityTest] public IEnumerator CancelOnExample() {
      using (var timeoutEmitter = Emitter.Instance) {
        bool aborted = false;

        Fiber.Start.WaitFor(seconds: 0.1f).Fire(timeoutEmitter).Do(_ => aborted = true);

        Fiber.Start.CancelOn(timeoutEmitter).Begin.WaitFor(seconds: 0.05f).Again.Finish();

        yield return new WaitForSeconds(0.3f);
        Assert.IsTrue(aborted);
      }
    }

    //- Now that I have shown three ways to abort a running fiber, I can tell you that the second and third are not necessary. From release 2.0.2 and above, Fibers includes an Aborted flag, so we can use `Timeout`, `Exit` or `CancelOn` and it will provide us with the information we need.
    [UnityTest] public IEnumerator AbortedExample() {
      var fiber = Fiber.Instance.Timeout(seconds: 0.2f).Begin.WaitFor(seconds: 0.05f).Again;
      yield return new WaitForSeconds(0.4f);
      Assert.IsTrue(fiber.Aborted);
    }
  }
}
#endif