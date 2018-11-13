// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class BeginAgainExample {
    private int counter;

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginEnd() {
      counter = 0;
      yield return Fiber.Start.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(3, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginBreakAgain() {
      counter = 0;
      yield return Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(7, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginAgainExit() {
      counter = 0;
      yield return Fiber.Start
                        .Begin.Do(IncrementCounter).Do(Exit).Again
                        .Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginRepeat() {
      counter = 0;
      yield return Fiber.Start.Begin.Do(IncrementCounter).Repeat(5).Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(7, counter);
    }

    private void Escape(Fiber fiber) {
      if (counter > 5) fiber.Break();
    }

    private void Exit(Fiber fiber) { fiber.Exit(); }

    private void IncrementCounter(Fiber fiber) => counter++;
  }
}