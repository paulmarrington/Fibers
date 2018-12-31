// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

#if UNITY_EDITOR && Fibers

// ReSharper disable MissingXmlDoc

namespace Askowl.Examples {
  public class EmitterExample {
    private bool         emitterFired;
    private Emitter      emitter;
    private Emitter<int> emitterInt;
    private int          emitterFiredValue;

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
      Fiber.Debugging = true;
      emitterFired    = false;
      var idlingFiber = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      Assert.IsFalse(emitterFired);
      yield return Fiber.Start.Do(Wait300ms).AsCoroutine();

      Assert.IsFalse(emitterFired); // telling another Fiber to do something leaves others idling
      idlingFiber.Restart();        // telling it to do something kicks it out of idling
      yield return null;

      Assert.IsTrue(emitterFired);
    }

    [UnityTest] public IEnumerator Emitter() {
      using (emitter = Askowl.Emitter.Instance) {
        emitterFired = false;
        Fiber.Start.WaitFor(emitter).Do(SetEmitterFiredFlag);
        yield return Fiber.Start.WaitRealtime(0.2f).Do(Fire).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    private void SetEmitterFiredFlag(Fiber fiber) => emitterFired = true;
    private void Fire(Fiber                fiber) => emitter.Fire();
  }
}
#endif