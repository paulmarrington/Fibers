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
    private bool    emitterFired;
    private Emitter emitter;
    private int     emitterFiredValue;

    private void Wait300ms(Fiber fiber) => fiber.WaitRealtime(seconds: 0.3f);

    [UnityTest] public IEnumerator IdleRestart() {
      Fiber.Debugging = false;
      using (var idleFiber = Fiber.Start.Idle) {
        emitterFired = false;
        idleFiber.Do(SetEmitterFiredFlag);
        Assert.IsFalse(emitterFired);
        yield return Fiber.Start.Restart().Do(Wait300ms).Restart(idleFiber).Do(Wait300ms).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    [UnityTest] public IEnumerator IdleDo() {
      Fiber.Debugging = false;
      emitterFired    = false;
      var idlingFiber = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      Assert.IsFalse(emitterFired);
      yield return Fiber.Start.Do(Wait300ms).AsCoroutine();

      Assert.IsFalse(emitterFired); // telling another Fiber to do something leaves others idling
      idlingFiber.Restart();        // telling it to do something kicks it out of idling
      yield return null;

      Assert.IsTrue(emitterFired);
    }

    [UnityTest] public IEnumerator EmitterFire() {
      using (emitter = Emitter.Instance) {
        emitterFired = false;
        Fiber.Start.WaitFor(emitter).Do(SetEmitterFiredFlag);
        yield return Fiber.Start.WaitRealtime(0.2f).Do(Fire).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    private void SetEmitterFiredFlag(Fiber fiber) => emitterFired = true;
    private void Fire(Fiber                fiber) => emitter.Fire();

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

    [Test] public void ObserverEmitter() {
      counter = 0;

      emitter = Emitter.Instance;

      emitter.Subscribe(new Observer1()).Subscribe(new Observer2());
      Assert.AreEqual(expected: 0, actual: counter);
      emitter.Fire();
      Assert.AreEqual(expected: 3, actual: counter);
      emitter.Fire();
      Assert.AreEqual(expected: 6, actual: counter);
    }

    [Test] public void ActionEmitter() {
      emitter = Emitter.Instance.Subscribe(_ => counter++).Subscribe(_ => Assert.AreSame(emitter, _));
      using (emitter) {
        Assert.AreEqual(expected: 0, actual: counter);
        emitter.Fire();
        Assert.AreEqual(expected: 1, actual: counter);
      }
    }

    private class EmitterContext : IDisposable {
      public int  Number;
      public void Dispose() => Number = 0;
    }

    [Test] public void Context() {
      var emitterContext = new EmitterContext {Number = 12};
      using (emitter = Emitter.Instance.Context(emitterContext)) {
        emitter.Subscribe(em => Assert.AreEqual(12, em.Context<EmitterContext>().Number));
        emitter.Fire();
        Assert.AreEqual(12, emitter.Context<EmitterContext>().Number);
      }
      // proving that the context is also disposed
      Assert.AreEqual(0, emitter.Context<EmitterContext>().Number);
    }

    private struct Observer1 : IObserver {
      public void OnNext()      => ++counter;
      public void OnCompleted() => counter--;
    }

    private struct Observer2 : IObserver {
      public void OnNext()      => counter += 2;
      public void OnCompleted() => counter--;
    }

    private static int counter;
  }
}
#endif