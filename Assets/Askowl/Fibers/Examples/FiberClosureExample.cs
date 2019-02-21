// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
#if AskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class FiberClosureExample {
    private class SampleClosure : Fiber.Closure<SampleClosure, (int number, string text)> {
      protected override void Activities(Fiber fiber) =>
        fiber.Begin.WaitFor(seconds: 0.1f)
             .Do(_ => Scope.number++).Repeat(5).Do(_ => Scope.text = $"Is {Scope.number}");
    }
    [UnityTest] public IEnumerator FiberClosure() {
      var sampleClosure1 = SampleClosure.Go((0, ""));
      var fiber1 = Fiber.Start
                        .WaitFor(sampleClosure1.OnComplete)
                        .Do(_ => Assert.AreEqual(6, sampleClosure1.Scope.number));

      var fiber2 = Fiber.Start.WaitFor(SampleClosure.Go((12, "")));

      yield return fiber1.AsCoroutine();

      yield return new WaitForSeconds(0.2f);

      var sampleClosure3 = SampleClosure.Go((33, ""));
      yield return Fiber.Start
                        .WaitFor(sampleClosure3.OnComplete)
                        .Do(_ => Assert.AreEqual(39, sampleClosure3.Scope.number))
                        .AsCoroutine();

      yield return fiber2.AsCoroutine();
    }
  }
}
#endif