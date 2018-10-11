﻿// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class WaitForFramesExample {
    /// <a href=""></a>
    [UnityTest] public IEnumerator WaitForFrames() {
      var frame = 0;
      void reset(Fiber _) => frame = Time.frameCount + 1;

      void check(int expected) {
        int elapsedFrames = Time.frameCount - frame;
        Assert.AreEqual(expected, elapsedFrames);
        frame = Time.frameCount;
      }

      void check0(Fiber  _) => check(0);
      void check10(Fiber _) => check(10);

      yield return Fiber.Start.Do(reset).Do(check0).SkipFrames(10).Do(check10).AsCoroutine();
    }
  }
}