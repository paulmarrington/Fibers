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

    private void checkElapsed(float seconds) {
      Assert.AreEqual(seconds + Time.deltaTime, Time.timeSinceLevelLoad - start, 0.05f);
      start = Time.timeSinceLevelLoad + Time.deltaTime;
    }

    [UnityTest] public IEnumerator WaitForFunc() {
      var compiledFiber = Fiber.Instance.Begin.Do(_ => start = Time.timeSinceLevelLoad)
                               .WaitFor(_ => secondsToDelay)
                               .Do(_ => checkElapsed(secondsToDelay)).Repeat(5);

      secondsToDelay = 0.3f;
      yield return compiledFiber.Go().AsCoroutine();

      secondsToDelay = 0.5f;
      yield return compiledFiber.Go().AsCoroutine();
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