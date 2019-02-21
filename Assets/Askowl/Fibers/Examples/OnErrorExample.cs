// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
#if AskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class OnErrorExample {
    private string errorMessage = "";

    [UnityTest] public IEnumerator OnError() {
      yield return Fiber.Start.OnError(msg => errorMessage = msg)
                        .Do(_ => throw new Exception("OnError Exception Thrown"))
                        .AsCoroutine();
      Assert.IsTrue(errorMessage.Contains("OnError Exception Thrown"));
    }

    [UnityTest] public IEnumerator GlobalOnError() {
      yield return Fiber.Start.GlobalOnError(msg => errorMessage = msg)
                        .Do(_ => throw new Exception("Global OnError Thrown"))
                        .AsCoroutine();
      Assert.IsTrue(errorMessage.Contains("Global OnError Thrown"));
    }

    private string note = "";

    [UnityTest] public IEnumerator ExitOnError() {
      note = "unchanged";
      yield return Fiber.Start.OnError(msg => errorMessage = msg)
                        .Do(_ => throw new Exception("ExitOnError Exception"))
                        .Do(_ => note = "reached")
                        .AsCoroutine();
      Assert.AreEqual("reached", note);

      note = "unchanged";
      yield return Fiber.Start.ExitOnError
                        .Do(_ => throw new Exception("ExitOnError Exception"))
                        .Do(_ => note = "reached")
                        .AsCoroutine();
      Assert.AreEqual("unchanged", note);
    }

    [UnityTest] public IEnumerator Error() {
      yield return Fiber.Start.OnError(_ => errorMessage = _).Error(_ => "Error Called").AsCoroutine();
      Assert.IsTrue(errorMessage.Contains("Error Called"));
    }
  }
}
#endif