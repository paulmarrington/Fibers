// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if UNITY_EDITOR && Fibers

// ReSharper disable MissingXmlDoc

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine.TestTools;

  public class BeginAgainExample {
    private int counter;

    [UnityTest] public IEnumerator BeginEnd() {
      counter = 0;
      yield return Fiber.Start.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(3, counter);
    }

    [UnityTest] public IEnumerator BeginBreakAgain() {
      counter = 0;
      yield return Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginAgainExit() {
      counter = 0;
      yield return Fiber.Start
                        .Begin.Do(IncrementCounter).Do(Exit).Again
                        .Do(IncrementCounter).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

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
#endif