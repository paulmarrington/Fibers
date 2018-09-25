// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  /// Using <see cref="Askowl.Fibers" />
  /// <inheritdoc />
  public class WaitForSecondsExample : PlayModeTests {
    private string SceneName = "Fiber Examples";
    private float  start;

    /// <a href="">Using <see cref="Fiber.WaitForSeconds"/></a>
    [UnityTest]
    public IEnumerator WaitForSeconds() {
      void stepOne(Fiber fiber) { fiber.WaitForSeconds(0.3f); }

      void stepTwo(Fiber fiber) {
        CheckElapsed(0.3f);
        fiber.WaitForSeconds(0.1f);
      }

      void stepThree(Fiber fiber) { CheckElapsed(0.7f); }

      yield return LoadScene(SceneName);

      start = Time.realtimeSinceStartup;

      yield return Fiber
                  .Start(stepOne, stepTwo)
                  .WaitForSeconds(0.2f)
                  .Do(stepThree)
                  .AsCoroutine();

      CheckElapsed(0.7f);
    }

    /// <a href="">Using <see cref="Fiber.WaitForSecondsRealtime"/></a>
    [UnityTest]
    public IEnumerator WaitForSecondsRealtime() {
      void stepOne(Fiber fiber) { fiber.WaitForSecondsRealtime(0.3f); }

      void stepTwo(Fiber fiber) {
        CheckElapsed(0.3f);
      }

      void stepThree(Fiber fiber) { CheckElapsed(0.5f); }

      yield return LoadScene(SceneName);

      start = Time.realtimeSinceStartup;

      yield return Fiber
                  .Start(stepOne, stepTwo)
                  .WaitForSecondsRealtime(0.2f)
                  .Do(stepThree)
                  .AsCoroutine();

      CheckElapsed(0.5f);
    }

    private void CheckElapsed(float seconds) { Assert.AreEqual(seconds, Time.realtimeSinceStartup - start, 0.01f); }
  }
}