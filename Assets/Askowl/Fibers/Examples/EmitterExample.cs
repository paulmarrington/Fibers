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

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleRestart() {
      emitterFired = false;
      idlingFiber  = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      yield return Fiber.Start.WaitForSecondsRealtime(0.2f).NextUpdate.Do(RestartIdle).AsCoroutine();

      Assert.IsTrue(emitterFired);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator IdleDo() {
      emitterFired = false;
      idlingFiber  = Fiber.Start.Idle.Do(SetEmitterFiredFlag);
      yield return Fiber.Start.WaitForSecondsRealtime(0.2f).AsCoroutine();

      Assert.IsFalse(emitterFired);
      idlingFiber.Do(Nothing);
      yield return Fiber.Start.NextFrame.AsCoroutine();

      Assert.IsTrue(emitterFired);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator Emitter() {
      using (emitter = Askowl.Emitter.Instance) {
        emitterFired = false;
        Fiber.Start.Emitter(emitter).Do(SetEmitterFiredFlag);
        yield return Fiber.Start.WaitForSecondsRealtime(0.2f).NextUpdate.Do(Fire).AsCoroutine();

        Assert.IsTrue(emitterFired);
      }
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator EmitterT() {
      using (emitterInt = Emitter<int>.Instance) {
        emitterFiredValue = 0;
        Fiber.Start.Emitter(emitterInt, SetEmitterInt);
        yield return Fiber.Start.WaitForSecondsRealtime(0.2f).NextUpdate.Do(FireInt).AsCoroutine();

        Assert.AreEqual(22, emitterFiredValue);
      }
    }

    private void SetEmitterInt(int         value) => emitterFiredValue = value;
    private void SetEmitterFiredFlag(Fiber fiber) => emitterFired = true;
    private void Fire(Fiber                fiber) => emitter.Fire();
    private void FireInt(Fiber             fiber) => emitterInt.Fire(22);
    private void RestartIdle(Fiber         fiber) => idlingFiber.Restart();
    private void Nothing(Fiber             fiber) { }
  }
}