// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR && AskowlFibers
namespace Askowl.Examples {
  public sealed class TimeoutExample {
    [UnityTest] public IEnumerator Timeout() {
      var start = Time.realtimeSinceStartup;
      yield return Fiber.Start.Timeout(seconds: 0.2f).Begin.WaitFor(seconds: 0.5f).AsCoroutine();
      var elapsed = Time.realtimeSinceStartup - start;
      Assert.AreEqual(0.2f, elapsed, 0.05f);
    }
  }
}
#endif