// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable MissingXmlDoc

#if UNITY_EDITOR && Fibers

namespace Askowl.Examples {
  public class Precompile {
    private float start, end;
    private float secondsToDelay;

    private void CheckElapsed(float seconds) {
      Assert.AreEqual(seconds + Time.deltaTime, Time.timeSinceLevelLoad - start, 0.05f);
      start = Time.timeSinceLevelLoad + Time.deltaTime;
    }

    [UnityTest] public IEnumerator InstanceGo() {
      start = Time.realtimeSinceStartup;

      var fiber = Fiber.Instance.WaitFor(seconds: 0.3f);
      fiber.Go(); // in this case superfluous since AsCoroutine() calls it implicitly (as does WaitFor(Fiber))

      yield return fiber.AsCoroutine();
      end = Time.realtimeSinceStartup;
      Assert.AreEqual(0.3f, end - start, 0.05f);
    }

    [UnityTest] public IEnumerator WaitForFunc() {
      var compiledFiber = Fiber.Instance.Begin.Do(_ => start = Time.timeSinceLevelLoad)
                               .WaitFor(_ => secondsToDelay)
                               .Do(_ => CheckElapsed(secondsToDelay)).Repeat(5);

      secondsToDelay = 0.3f;
      yield return compiledFiber.Go().AsCoroutine();

      secondsToDelay = 0.5f;
      yield return compiledFiber.Go().AsCoroutine();
    }

    [UnityTest] public IEnumerator WaitForFiber() {
      var fiber1 = Fiber.Instance.WaitFor(0.3f);
      var fiber2 = Fiber.Instance.Do(_ => start = Time.realtimeSinceStartup, "Start time")
                        .WaitFor(fiber1).Do(_ => end = Time.realtimeSinceStartup, "End time");
      yield return fiber2.AsCoroutine();
      Assert.AreEqual(expected: 0.3f, actual: end - start, delta: 0.05f);
    }
  }
}
#endif