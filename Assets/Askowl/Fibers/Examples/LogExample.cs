// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;
#if AskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class LogExample : PlayModeTests {
    [UnityTest] public IEnumerator FiberLog() {
      yield return Fiber.Start.Log("Ordinary Log Message").AsCoroutine();
      LogAssert.Expect(LogType.Log, "Ordinary Log Message");
    }
    [UnityTest] public IEnumerator FiberLogWarning() {
      yield return Fiber.Start.Log("Warning Log Message", warning: true).AsCoroutine();
      LogAssert.Expect(LogType.Warning, new Regex("Warning Log Message.*"));
    }
  }
}
#endif