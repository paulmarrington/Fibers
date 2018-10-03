// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class IEnumeratorExample {
    /// <a href=""></a>
    [UnityTest] public IEnumerator IEnumerator() {
      float start = Time.realtimeSinceStartup;
      yield return Fiber.Start.Coroutine(SampleCoroutine()).AsCoroutine();

      float elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(0.3f, elapsed, 1e-5f);
    }

    private IEnumerator SampleCoroutine() { yield return new WaitForSeconds(0.3f); }
  }
}