// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR && Fibers

// ReSharper disable MissingXmlDoc

namespace Askowl.Examples {
  public class BeginAgainExample {
    private int counter;

    private readonly Fiber beginEndGo, beginBreakAgainGo;

    public BeginAgainExample() {
      beginEndGo = Fiber.Instance.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter);

      Fiber.Action incrementCounter = _ => counter++;
      beginBreakAgainGo = Fiber.Instance.Begin.Do(incrementCounter).Do(Escape).Again.Do(incrementCounter);
    }

    [UnityTest] public IEnumerator BeginEnd() {
      counter = 0;
      var fiber = Fiber.Start.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(3, counter);
    }

    [UnityTest] public IEnumerator BeginEndGo() {
      counter = 0;
      yield return beginEndGo.Go().AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(3, counter);
    }

    [UnityTest] public IEnumerator BeginBreakAgain() {
      Fiber.Debugging = false;
      counter         = 0;
      var fiber = Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginBreakAgainGo() {
      counter = 0;
      Debug.Log($"BeginBreakAgainGo {beginBreakAgainGo}");
      yield return beginBreakAgainGo.Go().AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginAgainExit() {
      Fiber.Debugging = false;
      counter         = 0;
      var fiber = Fiber.Start.Begin.Do(IncrementCounter).Do(Exit).Again.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);

      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator BeginRepeat() {
      Fiber.Debugging = false;
      counter         = 0;
      var fiber = Fiber.Start.Begin.Do(IncrementCounter).Repeat(5).Do(IncrementCounter);
      Debug.Log($"BeginRepeat {fiber}");
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);

      Assert.AreEqual(7, counter);
    }

    private void Escape(Fiber fiber) {
      if (counter > 5) {
        Debug.Log($"Escape {fiber}");
        fiber.Break();
      }
    }

    private void Exit(Fiber fiber) {
      Debug.Log($"Exit {fiber}");
      fiber.Exit();
    }

    private void IncrementCounter(Fiber fiber) => counter++;
  }
}
#endif