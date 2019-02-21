//- Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- After some research with the decompiler I worked out that all function references are created equal, no matter if it is a lambda, inner function or class method. An anonymous class is generated and instantiated whenever a reference is created. The best way to reduce the load on garbage collection is to precompile the fibers such that the classes are only instantiated once. Start still have value as we don't need to precompile for fibers that are only used once or monitors that remain in an infinite loop.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if AskowlTests
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Transcripts {
  public class PrecompileTranscript : PlayModeTests {
    private static float start, end;
    private static float secondsToDelay;

    //- Precompiled fibers are created with Instance instead of start
    private readonly Fiber fiber300Ms = Fiber.Instance.WaitFor(seconds: 0.3f);

    //- If we want to change a variable as a parameter we need to wrap it in a function so that the current value is picked up
    private readonly Fiber fiberVarMs = Fiber.Instance.WaitFor(_ => secondsToDelay);

    //- Here is a test for the fixed 300ms precompiled fiber
    [UnityTest] public IEnumerator InstanceGo() {
      start = Time.realtimeSinceStartup;

      //- and they are first run when Go is called explicitly or implicitly with WaitFor or AsCoroutine.
      fiber300Ms.Go(); // in this case superfluous since AsCoroutine() calls it implicitly (as does WaitFor(Fiber))

      //- We can mix fibers with coroutines where it makes sense
      yield return fiber300Ms.AsCoroutine();

      Assert.AreEqual(0.3f, Time.realtimeSinceStartup - start, delta: 0.05f);
    }

    //- This next example shows two advantages of precompiled fibers
    [UnityTest] public IEnumerator WaitForFunc() {
      start = Time.realtimeSinceStartup;
      //- One that we can set variables that will be used in the next fiber run
      secondsToDelay = 0.3f;
      yield return fiberVarMs.Go().AsCoroutine();
      Assert.AreEqual(0.3f, Time.realtimeSinceStartup - start, delta: 0.05f);

      start          = Time.realtimeSinceStartup;
      secondsToDelay = 0.5f;
      //- And two that we can reuse fibers without involving the garbage collector - assuming that we do not need AsCoroutine in production code
      yield return fiberVarMs.Go().AsCoroutine();
      Assert.AreEqual(0.5f, Time.realtimeSinceStartup - start, delta: 0.05f);
    }

    //- One more important use for precompiled fibers is reusable code similar to functions in sequential programming
    [UnityTest] public IEnumerator WaitForFiber() {
      var fiber1 = Fiber.Instance.WaitFor(0.3f);
      //- THe second fiber will wait until the first is done. In real code we may want to wait for one action to complete before starting another.
      var fiber2 = Fiber.Instance.Do(_ => start = Time.realtimeSinceStartup)
                        .WaitFor(fiber1).Do(_ => end = Time.realtimeSinceStartup);
      yield return fiber2.AsCoroutine();
      Assert.AreEqual(0.3f, end - start, 0.09f);
    }
  }
}
#endif