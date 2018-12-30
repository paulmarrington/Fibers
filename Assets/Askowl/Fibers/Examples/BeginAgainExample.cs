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
      beginEndGo        = Fiber.Instance.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter);
      beginBreakAgainGo = Fiber.Instance.Begin.Do(_ => counter++).Do(Escape).Again.Do(_ => counter++);
      Debug.Log($"BeginAgainExample {beginEndGo}\n{beginBreakAgainGo}"); //#TBD#//
    }

    [UnityTest] public IEnumerator BeginEnd() {
      counter = 0;
      var fiber = Fiber.Start.Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter);
      Debug.Log($"BeginEnd {fiber}");
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(3, counter);
    }

    [UnityTest] public IEnumerator BeginEndGo() {
      counter = 0;
      Debug.Log($"BeginEndGo {beginEndGo}");
      yield return beginEndGo.Go().AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(3, counter);
    }

    [UnityTest] public IEnumerator BeginBreakAgain() {
      counter = 0;
      var fiber = Fiber.Start.Begin.Do(IncrementCounter).Do(Escape).Again.Do(IncrementCounter);
      Debug.Log($"BeginBreakAgain {fiber}");
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
      counter = 0;
      var fiber = Fiber.Start.Begin.Do(IncrementCounter).Do(Exit).Again.Do(IncrementCounter);
      Debug.Log($"BeginAgainExit {fiber}");
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);

      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator BeginRepeat() {
      counter = 0;
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

    private void IncrementCounter(Fiber fiber) {
      counter++;
      Debug.Log($"{counter} IncrementCounter {fiber}");
    }
  }
}
#endif