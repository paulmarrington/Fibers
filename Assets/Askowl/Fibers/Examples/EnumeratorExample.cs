// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if !ExcludeAskowlTests

// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Examples {
  public class EnumeratorExample {
    private int counter;

    [UnityTest] public IEnumerator Enumerator() {
      counter = 0;
      float start = Time.realtimeSinceStartup;
      yield return Fiber.Start.WaitFor(SampleEnumeratorCoroutine()).AsCoroutine();

      float elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(8 / 60f, elapsed, 0.1f);
      Assert.AreEqual(6,       counter);
    }

    [UnityTest] public IEnumerator EnumeratorSpaced() {
      counter = 0;
      float start = Time.realtimeSinceStartup;
      yield return Fiber.Start.WaitFor(framesBetweenChecks: 5, enumerator: SampleEnumeratorCoroutine()).AsCoroutine();

      float elapsed = Time.realtimeSinceStartup - start;
      // 5 frames - 1 for each count plus 25 frames, 5 for each frames between checks
      Assert.AreEqual((3 / 60f) + (25 / 60f), elapsed, 0.1f);
      Assert.AreEqual(6,                      counter);
    }

    [UnityTest] public IEnumerator EnumeratorFrames() {
      counter = 0;

      IEnumerator sampleFrameEnumeratorCoroutine() {
        while (counter++ < 5) yield return 5;
      }

      float start = Time.realtimeSinceStartup;
      yield return Fiber.Start.WaitFor(sampleFrameEnumeratorCoroutine()).AsCoroutine();

      float elapsed = Time.realtimeSinceStartup - start;
      // 5 * 5 frames
      Assert.AreEqual((3 / 60f) + (25 / 60f), elapsed, 0.1f);
      Assert.AreEqual(6,                      counter);
    }

    [UnityTest] public IEnumerator EnumeratorSeconds() {
      counter = 0;

      IEnumerator sampleSecondsEnumeratorCoroutine() {
        while (counter++ < 5) yield return 0.3f;
      }

      float start = Time.realtimeSinceStartup;
      yield return Fiber.Start.WaitFor(sampleSecondsEnumeratorCoroutine()).AsCoroutine();

      float elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(0.3f * 5, elapsed, 0.2f);
      Assert.AreEqual(6,        counter);
    }

    private IEnumerator SampleEnumeratorCoroutine() {
      while (counter++ < 5) yield return null;
    }
  }
}
#endif