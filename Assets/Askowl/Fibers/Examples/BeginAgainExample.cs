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
      yield return Fiber.Start.Do(IncrementCounter).Begin.Do(IncrementCounter).End.AsCoroutine();

      Assert.AreEqual(2, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator NoBegin() {
      counter = 0;
      yield return Fiber.Start.Do(IncrementCounter).End.AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginBreakEnd() {
      counter = 0;
      yield return Fiber.Start.Begin.Break().Do(IncrementCounter).End.AsCoroutine();

      Assert.AreEqual(1, counter); // Doesn't do one after `Break`
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginBreakAgain() {
      counter = 0;
      yield return Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.AsCoroutine();

      Assert.AreEqual(6, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator BeginRepeat() {
      counter = 0;
      yield return Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.AsCoroutine();

      Assert.AreEqual(6, counter);
    }

    private void Escape(Fiber fiber) {
      if (counter > 5) fiber.Break();
    }

    private void IncrementCounter(Fiber fiber) => counter++;
  }
}