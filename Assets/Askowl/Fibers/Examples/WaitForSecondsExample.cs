// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if AskowlTests
// ReSharper disable InconsistentNaming
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Examples {
  public class WaitForSecondsExample : PlayModeTests {
    private float start;

    [UnityTest] public IEnumerator WaitForSeconds() {
      void checkElapsed(float seconds) {
        Assert.AreEqual(seconds + Time.deltaTime, Time.timeSinceLevelLoad - start, 0.05f);
        start = Time.timeSinceLevelLoad + Time.deltaTime;
      }

      void setStartTime(Fiber fiber) => start = Time.timeSinceLevelLoad;
      void check300ms(Fiber   fiber) => checkElapsed(0.3f);

      yield return Fiber.Start.Begin.Do(setStartTime).WaitFor(seconds: 0.3f).Do(check300ms).Repeat(5).AsCoroutine();
    }

    [UnityTest] public IEnumerator WaitForSecondsRealtime() {
      void checkElapsed(float seconds) {
        Assert.AreEqual(seconds + Time.unscaledDeltaTime, Time.realtimeSinceStartup - start, 0.05f);
        start = Time.realtimeSinceStartup;
      }

      void setStartTime(Fiber fiber) => start = Time.realtimeSinceStartup;
      void check300ms(Fiber   fiber) => checkElapsed(0.3f);

      yield return Fiber.Start.Begin
                        .Do(setStartTime).WaitRealtime(seconds: 0.3f).Do(check300ms)
                        .Repeat(5).AsCoroutine();
    }
  }
}
#endif