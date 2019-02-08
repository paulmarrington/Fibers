using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR && Fibers
// ReSharper disable MissingXmlDoc

namespace Askowl.Example.PrecompileTranscript {
  public class PrecompileTranscript : PlayModeTests {
    private static   float start, end;
    private static   float secondsToDelay;
    private readonly Fiber fiber300Ms = Fiber.Instance.WaitFor(seconds: 0.3f);
    private readonly Fiber fiberVarMs = Fiber.Instance.WaitFor(_ => secondsToDelay);
    [UnityTest] public IEnumerator InstanceGo() {
      start = Time.realtimeSinceStartup;
      fiber300Ms.Go(); // in this case superfluous since AsCoroutine() calls it implicitly (as does WaitFor(Fiber))
      yield return fiber300Ms.AsCoroutine();

      Assert.AreEqual(0.3f, Time.realtimeSinceStartup - start, delta: 0.05f);
    }
    [UnityTest] public IEnumerator WaitForFunc() {
      start          = Time.realtimeSinceStartup;
      secondsToDelay = 0.3f;
      yield return fiberVarMs.Go().AsCoroutine();
      Assert.AreEqual(0.3f, Time.realtimeSinceStartup - start, delta: 0.05f);

      start          = Time.realtimeSinceStartup;
      secondsToDelay = 0.5f;
      yield return fiberVarMs.Go().AsCoroutine();
      Assert.AreEqual(0.3f, Time.realtimeSinceStartup - start, delta: 0.05f);
    }
    [UnityTest] public IEnumerator WaitForFiber() {
      var fiber1 = Fiber.Instance.WaitFor(0.3f);
      var fiber2 = Fiber.Instance.Do(_ => start = Time.realtimeSinceStartup)
                        .WaitFor(fiber1).Do(_ => end = Time.realtimeSinceStartup);
      yield return fiber2.AsCoroutine();
      Assert.AreEqual(0.3f, end - start, 0.05f);
    }
  }
}
#endif