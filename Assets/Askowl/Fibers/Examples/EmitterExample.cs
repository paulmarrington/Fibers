// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class EmitterExample {
    private bool         emitterFired;
    private Emitter      emitter;
    private Emitter<int> emitterInt;
    private int          emitterFiredValue;
    private Fiber        idlingFiber;

    private void Wait300ms(Fiber fiber) => fiber.WaitRealtime(seconds: 0.3f);

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleRestart() {
      using (var idleFiber = Fiber.Start) {
        emitterFired = false;
        idlingFiber  = idleFiber.Idle.Do(SetEmitterFiredFlag);
        Assert.IsFalse(emitterFired);
        using (var restartFiber = Fiber.Start) {
          yield return restartFiber.Do(Wait300ms).Restart(idlingFiber).AsCoroutine();
        }

        Assert.IsTrue(emitterFired);
      }
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleDo() {
      Fiber.Debugging = false;
      emitterFired    = false;
      var idleFiber = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      Assert.IsFalse(emitterFired);
      yield return Fiber.Start.Do(Wait300ms).AsCoroutine();

      Assert.IsFalse(emitterFired);           // telling another Fiber to do something leaves others idling
      idleFiber.Restart();                    // telling it to do something kicks it out of idling
      yield return Fiber.Start.AsCoroutine(); // another way to wait for a frame

      Assert.IsTrue(emitterFired);
    }

    /// <a href=""></a>
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