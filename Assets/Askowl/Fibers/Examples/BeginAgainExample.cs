// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if !ExcludeAskowlTests

// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Examples {
  public class BeginAgainExample {
    private int counter;

    private readonly Fiber beginEndGo, beginBreakAgainGo;

    public BeginAgainExample() {
      beginEndGo = Fiber.Instance().Do(IncrementCounter).Begin.NextFrame.Do(IncrementCounter).End.Do(IncrementCounter);

      Fiber.Action incrementCounter = _ => counter++;
      beginBreakAgainGo = Fiber.Instance().Begin.Do(incrementCounter).NextFrame.Do(Escape).Again.Do(incrementCounter);
    }

    [UnityTest] public IEnumerator BeginEnd() {
      counter = 0;
      var fiber = Fiber.Start().Do(IncrementCounter).Begin.Do(IncrementCounter).End.Do(IncrementCounter);
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
      counter = 0;
      var fiber = Fiber.Start().Begin.Do(IncrementCounter).Do(Escape).Again.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BreakIf() {
      counter = 0;
      var fiber = Fiber.Start().Begin.Do(IncrementCounter).BreakIf(_ => counter > 5).Again.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginUntil() {
      counter = 0;
      var fiber = Fiber.Start().Begin.Do(IncrementCounter).Until(_ => counter > 5).Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginBreakAgainGo() {
      counter = 0;
      yield return beginBreakAgainGo.Go().AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(7, counter);
    }

    [UnityTest] public IEnumerator BeginAgainExit() {
      counter = 0;
      var fiber = Fiber.Start().Begin.Do(IncrementCounter).Do(Exit).Again.Do(IncrementCounter);
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);
      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator ExitAnotherFiber() {
      counter = 0;
      var mainFiber = Fiber.Instance();
      mainFiber.Begin.Do(_ => counter++).WaitFor(seconds: 0.2f).Do(_ => counter++).Go();
      yield return Fiber.Start().WaitFor(seconds: 0.1f).Exit(mainFiber).WaitFor(0.2f).AsCoroutine();
      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator BeginRepeat() {
      counter = 0;
      var fiber = Fiber.Start().Begin.Do(IncrementCounter).Repeat(5).Do(IncrementCounter);
      Debug.Log($"BeginRepeat {fiber}");
      yield return fiber.AsCoroutine();
      yield return new WaitForSeconds(0.3f);

      Assert.AreEqual(7, counter);
    }

    private void Escape(Fiber fiber) {
      if (counter > 5) fiber.Break();
    }

    private void Exit(Fiber fiber) {
      Debug.Log($"Exit {fiber}");
      fiber.Exit();
    }

    private void IncrementCounter(Fiber fiber) => counter++;
  }
}
#endif