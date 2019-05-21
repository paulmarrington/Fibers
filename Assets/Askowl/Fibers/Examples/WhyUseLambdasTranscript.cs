//- Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- Parameters to all the fiber steps are set in stone when the fiber is built. This is most relevant to precompiled fibers. I am using immediate fibers here for simplicity.
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
#if !ExcludeAskowlTests
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Transcripts {
  public class WhyUseLambdasTranscript {
    private Emitter emitter;
    //- This example works because the emitter is set before either fiber is built.
    [UnityTest] public IEnumerator FireSuccess() {
      emitter = Emitter.SingleFireInstance;
      Fiber.Start().WaitFor(seconds: 0.1f).Fire(emitter);
      var fiber = Fiber.Start().WaitFor(emitter);
      yield return fiber.AsCoroutine();
      Assert.IsFalse(fiber.Aborted);
    }

    //- On the other hand this one fails because each fiber is using a different emitter.
    [UnityTest] public IEnumerator FireFailure() {
      emitter = Emitter.SingleFireInstance;
      Fiber.Start().WaitFor(seconds: 0.1f).Fire(emitter);
      emitter = Emitter.SingleFireInstance;
      // - This fiber will only exit once the timeout is reached - and the aborted flag set.
      var fiber = Fiber.Start().Timeout(seconds: 0.2f).WaitFor(emitter);
      yield return fiber.AsCoroutine();
      Assert.IsTrue(fiber.Aborted);
    }
    //- The solution is to use a lambda. The emitter reference is used when needed, not when the fiber is compiled.
    [UnityTest] public IEnumerator FireWithLambda() {
      emitter = Emitter.SingleFireInstance;

      Fiber.Start().WaitFor(seconds: 0.1f).Fire(_ => emitter);

      emitter = Emitter.SingleFireInstance;
      var fiber = Fiber.Start().WaitFor(emitter);
      yield return fiber.AsCoroutine();
      Assert.IsFalse(fiber.Aborted);

      //- Almost all Fibers built-in commands have a lambda version in addition to the direct parameter approach. If in doubt, use the lambda.
    }
  }
}
#endif