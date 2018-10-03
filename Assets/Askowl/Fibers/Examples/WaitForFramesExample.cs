// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class WaitForFramesExample {
    private int frame;

    /// <a href=""></a>
    [UnityTest] public IEnumerator WaitForFrames() {
      int start = frame = Time.frameCount;
      yield return Fiber.Start.NextFrame.Do(Check).NextUpdate.Do(Check).SkipFrames(5).Do(Check).AsCoroutine();

      Assert.AreEqual(start + 7, Time.frameCount);
    }

    private void Check(Fiber fiber) {
      Assert.Greater(Time.frameCount, frame);
      frame = Time.frameCount;
    }
  }
}