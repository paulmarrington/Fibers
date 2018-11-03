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

    private void Wait300ms(Fiber fiber) => fiber.WaitForSecondsRealtime(0.3f);

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleRestart() {
      using (var idleFiber = Fiber.Start) {
        emitterFired = false;
        idlingFiber  = idleFiber.Idle.Do(SetEmitterFiredFlag);
        Assert.IsFalse(emitterFired);
        using (var restartFiber = Fiber.Start) yield return restartFiber.Do(Wait300ms).Do(RestartIdle).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleDo() {
      Fiber.Debugging = true;
      emitterFired    = false;
      var idleFiber = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      Assert.IsFalse(emitterFired);
      yield return Fiber.Start.Do(Wait300ms).AsCoroutine();

      Assert.IsFalse(emitterFired);           // telling another Fiber to do something leaves others idling
      idleFiber.Restart.Do(Nothing);          // telling it to do something kicks it out of idling
      yield return Fiber.Start.AsCoroutine(); // another way to wait for a frame

      Assert.IsTrue(emitterFired);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator Emitter() {
      using (emitter = Askowl.Emitter.Instance) {
        emitterFired = false;
        Fiber.Start.Emitter(emitter).Do(SetEmitterFiredFlag);
        yield return Fiber.Start.WaitForSecondsRealtime(0.2f).Do(Fire).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator EmitterT() {
      using (emitterInt = Emitter<int>.Instance) {
        emitterFiredValue = 0;
        Fiber.Start.Emitter(emitterInt, SetEmitterInt);
        yield return Fiber.Start.WaitForSecondsRealtime(0.2f).Do(FireInt).AsCoroutine();

        Assert.AreEqual(22, emitterFiredValue);
      }
    }

    private void SetEmitterInt(int         value) => emitterFiredValue = value;
    private void SetEmitterFiredFlag(Fiber fiber) => emitterFired = true;
    private void Fire(Fiber                fiber) => emitter.Fire();
    private void FireInt(Fiber             fiber) => emitterInt.Fire(22);
    private void RestartIdle(Fiber         fiber) => idlingFiber.Restart.Do(Nothing);
    private void Nothing(Fiber             fiber) { }
  }
}