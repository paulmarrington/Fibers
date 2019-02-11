// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR && Fibers

// ReSharper disable MissingXmlDoc

namespace Askowl.Examples {
  public class EmitterExample {
    private        bool    emitterFired;
    private static Emitter emitter;
    private        int     emitterFiredValue;

    [UnityTest] public IEnumerator EmitterFire() {
      using (emitter = Emitter.Instance) {
        emitterFired = false;
        Fiber.Start.WaitFor(emitter).Do(fiber => emitterFired = true);
        yield return Fiber.Start.WaitRealtime(0.2f).Fire(emitter).AsCoroutine();
        Assert.IsTrue(emitterFired);
      }
    }

    [UnityTest] public IEnumerator WaitForFiber() {
      var sleeper = Fiber.Instance.WaitFor(seconds: 0.1f).Do(_ => sleeperDone = true);
      Fiber.Start.Do(_ => sleeperDone = false).WaitFor(sleeper.OnComplete);
      yield return new WaitForSeconds(0.2f);
      Assert.IsFalse(sleeperDone);
      sleeper.Go();
      yield return new WaitForSeconds(0.2f);
      Assert.IsTrue(sleeperDone);
    }
    private bool sleeperDone;

    [UnityTest] public IEnumerator SingleFireInstance() {
      emitter = Emitter.SingleFireInstance;
      emitter.Fire();
      yield return new WaitForSeconds(0.1f);
      Assert.AreSame(Cache<Emitter>.Entries.RecycleBin, Cache<Emitter>.Entries.ReverseLookup(emitter).Owner);
    }

    [UnityTest] public IEnumerator CancelOn() {
      using (emitter = Emitter.Instance) {
        counter = 0;
        Fiber.Start.CancelOn(emitter).Begin.Do(_ => counter++).WaitFor(seconds: 0.05f).Again.Finish();
        yield return new WaitForSeconds(0.5f);
        Assert.AreNotEqual(0, counter);
        var mark = counter;
        emitter.Fire();
        yield return new WaitForSeconds(0.3f);
        Assert.AreEqual(mark, counter);
      }
    }

    [Test] public void ActionEmitter() {
      counter = 0;
      using (emitter = Emitter.Instance.Listen(incrementCounter).Listen(isSame)) {
        Assert.AreEqual(expected: 0, actual: counter);
        emitter.Fire();
        Assert.AreEqual(expected: 1, actual: counter);
      }
    }

    [Test] public void Remove() {
      counter = 0;
      using (emitter = Emitter.Instance.Listen(incrementCounter).Listen(removeMyself)) {
        emitter.Fire();
        Assert.AreEqual(expected: 2, actual: counter);
        emitter.Remove(incrementCounter);
        emitter.Fire();
        Assert.AreEqual(expected: 2, actual: counter);
      }
    }

    [Test] public void ListenOnce() {
      counter = 0;
      using (emitter = Emitter.Instance.Listen(incrementCounter).Listen(incrementCounter)) {
        Assert.AreEqual(expected: 0, actual: counter);
        emitter.Fire();
        emitter.Fire();
        Assert.AreEqual(expected: 4, actual: counter);
      }
      counter = 0;
      using (emitter = Emitter.Instance.Listen(incrementCounterOnce).Listen(incrementCounterOnce)) {
        Assert.AreEqual(expected: 0, actual: counter);
        emitter.Fire();
        emitter.Fire();
        Assert.AreEqual(expected: 2, actual: counter);
      }
    }

    private static readonly Emitter.Action incrementCounter = _ => counter++;

    private static readonly Emitter.Action removeMyself = emitter => {
      counter++;
      emitter.StopListening();
    };

    private static readonly Emitter.Action incrementCounterOnce = emitter => {
      counter++;
      emitter.StopListening();
    };
    private static          void           Same(Emitter _) => Assert.AreSame(emitter, _);
    private static readonly Emitter.Action isSame = Same;

    private class EmitterContext : IDisposable {
      public int  Number;
      public void Dispose() => Number = 0;
    }

    [Test] public void Context() {
      var emitterContext = new EmitterContext {Number = 12};
      var namedContext   = "named context string reference";
      using (emitter = Emitter.Instance.Context(emitterContext).Context("name here", namedContext)) {
        emitter.Listen(em => Assert.AreEqual(12, em.Context<EmitterContext>().Number));
        emitter.Fire();
        Assert.AreEqual(12,                               emitter.Context<EmitterContext>().Number);
        Assert.AreEqual("named context string reference", emitter.Context<string>("name here"));
      }
      // proving that the context is also disposed
      Assert.IsNull(emitter.Context<EmitterContext>());
    }

    private static int counter;
  }
}
#endif