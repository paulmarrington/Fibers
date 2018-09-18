// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Askowl.Examples {
  public class WaitForExample : PlayModeTests {
    protected internal static int       counter;
    private                   int       recycled;
    private                   Coroutine coroutine;

    public IEnumerator WaitForUpdates() {
      counter  = 0;
      recycled = 0;
      // Sample - can just use Fiber(ExampleSimpleFiber) in MonoBehaviour
      WaitFor.Updates(ExampleSimpleFiber);
      // ~Sample
      Assert.AreEqual(counter, 0);
      yield return null; // Unity coroutine here

      Assert.AreEqual(counter, 1);
      CheckFiber(ExampleSimpleFiber);
    }

    private void CheckFiber(Func<IEnumerator> fiber) {
      Assert.IsTrue(WaitFor.WorkerGenerators.ContainsKey(fiber));
      var fiberWorker = WaitFor.WorkerGenerators[fiber];
      Assert.AreEqual(fiberWorker.Fibers.Count, 0, "Expecting fiber to have been released");

      Assert.AreEqual(
        fiberWorker.Recycled.Count, recycled + 1,
        "Expecting fiber to be back in Recycled");
    }

    private IEnumerator ExampleSimpleFiber() {
      counter = 1;
      yield break;
    }

    [UnityTest]
    public IEnumerator WaitForActionUpdates() {
      counter  = 0;
      recycled = 0;
      // Sample - can just user Fiber(ExampleSimpleActionFiber) in MonoBehaviour
      WaitFor.Updates(() => counter = 11);
      // ~Sample
      Assert.AreEqual(counter, 0);
      yield return null; // Unity coroutine here

      Assert.AreEqual(counter, 11);
//      CheckFiber(ExampleSimpleActionFiber);
    }

    [UnityTest]
    public IEnumerator WaitForFixedUpdate() {
      recycled = 0;
      // Sample
      WaitFor.FixedUpdates(ExampleFixedUpdateFiber);
      // ~Sample
      counter = 0;
      yield return new WaitForSeconds(0.2f); // Unity coroutine here

      Assert.AreEqual(counter, 2);
      CheckFiber(ExampleFixedUpdateFiber);
    }

    private IEnumerator ExampleFixedUpdateFiber() {
      counter = 2;
      yield break;
    }

    [UnityTest]
    public IEnumerator WaitForLateUpdate() {
      recycled = 0;
      // Sample
      WaitFor.LateUpdates(ExampleLateUpdateFiber);
      // ~Sample
      counter = 0;
      yield return new WaitForSeconds(0.2f); // Unity coroutine here

      Assert.AreEqual(counter, 3);
      CheckFiber(ExampleLateUpdateFiber);
    }

    private IEnumerator ExampleLateUpdateFiber() {
      counter = 3;
      yield break;
    }

    [UnityTest]
    public IEnumerator RunYieldDo() => Updates(YieldDo, 0.2f);

    private IEnumerator YieldDo() {
      yield return WaitFor.NextUpdate().Do(() => counter = 11);

      Assert.AreEqual(11, counter);
    }

    [UnityTest]
    public IEnumerator RunYieldDoNode() => Updates(YieldDoNode, 0.2f);

    private IEnumerator YieldDoNode() {
      yield return WaitFor.NextUpdate().Do((coroutine) => coroutine.Result(counter = 11));

      Assert.AreEqual(11, coroutine.Result<int>());
    }

    [UnityTest]
    public IEnumerator RunYieldRepeating() => Updates(YieldRepeating, 0.5f);

    private IEnumerator YieldRepeating() {
      WaitFor.Updates(KillRepeating);
      float start = Time.realtimeSinceStartup;
      yield return WaitFor.Seconds(0.1).Repeating();

      Assert.IsTrue(Time.realtimeSinceStartup > (start + 0.3f));
    }

    private IEnumerator KillRepeating() {
      Debug.Log($"**** WaitForExample:119 To Be Done -- "); //#DM#// 27 Jul 2018
      yield return WaitFor.SecondsRealtime(0.3);

      Debug.Log($"**** WaitForExample:121 To Be Done -- "); //#DM#// 27 Jul 2018

      coroutine.Stop();
    }

    [UnityTest]
    public IEnumerator RunYieldRepeat() => Updates(YieldRepeat, 0.5f);

    private IEnumerator YieldRepeat() {
      float start = Time.realtimeSinceStartup;
      yield return WaitFor.Seconds(0.1).Repeat(3);

      Assert.IsTrue(Time.realtimeSinceStartup > (start + 0.3f));
    }

    [UnityTest]
    public IEnumerator RunYieldUntil() => Updates(YieldUntil, 0.5f);

    private IEnumerator YieldUntil() {
      float end = Time.realtimeSinceStartup + 0.2f;
      yield return WaitFor.NextUpdate().Until(() => Time.realtimeSinceStartup > end);

      Assert.IsTrue(Time.realtimeSinceStartup > end);
    }

    [UnityTest]
    public IEnumerator RunYieldWhile() => Updates(YieldWhile, 0.5f);

    private IEnumerator YieldWhile() {
      float end = Time.realtimeSinceStartup + 0.2f;
      yield return WaitFor.NextUpdate().While(() => Time.realtimeSinceStartup < end);

      Assert.IsTrue(Time.realtimeSinceStartup > end);
    }

    [UnityTest]
    public IEnumerator RunFiberYield() => Updates(FiberYield, 0.2f);

    private IEnumerator FiberYield() {
      yield return WaitFor.Frames(1);

      Assert.AreEqual(1, coroutine.Yield.Parameter<int>());
    }

    [UnityTest]
    public IEnumerator RunFiberResult() => Updates(FiberResult, 0.2f);

    private IEnumerator FiberResult() {
      counter = 0;
      // Sample
      yield return CustomWorkers.Instance.Yield("Hello");

      // ~Sample
      Assert.AreEqual("The result", coroutine.Result<string>());
    }

    [UnityTest]
    public IEnumerator RunCustomWorker() => Updates(CustomWorker, 0.2f);

    private IEnumerator CustomWorker() {
      counter = 0;
      // Sample
      yield return CustomWorkers.Instance.Yield("Hello");
      // ~Sample

      Assert.AreEqual(9, counter);
    }

    private class CustomWorkers : Worker<string> {
      public static      CustomWorkers Instance = new CustomWorkers();
      protected override bool          AddToUpdate => true;

      protected override bool InRange(Coroutine coroutine) => Parameter(coroutine) == "";

      protected override void OnFinished(Coroutine coroutine) {
        coroutine.Result("The result");
        base.OnFinished(coroutine);
        counter = 9;
      }
    }

    [UnityTest]
    public IEnumerator RunWaitForEmitter() => Updates(WaitForEmitter, 0.2f);

    Emitter                 emitter        = new Emitter();
    private Emitter<string> emitterGeneric = new Emitter<string>();

    private IEnumerator WaitForEmitter() {
      WaitFor.Updates(EmitterTrigger);
      float start = Time.realtimeSinceStartup;
      // Sample
      yield return WaitFor.Emitter(emitter);
      // ~Sample

      Assert.AreNotApproximatelyEqual(Time.realtimeSinceStartup, start);
    }

    private IEnumerator EmitterTrigger() {
      yield return WaitFor.SecondsRealtime(0.1);

      emitter.Fire();
    }

    [UnityTest]
    public IEnumerator RunWaitForEmitterGeneric() => Updates(WaitForEmitterGeneric, 0.2f);

    private IEnumerator WaitForEmitterGeneric() {
      var   coroutine = WaitFor.Updates(EmitterGenericTrigger);
      float start     = Time.realtimeSinceStartup;
      // Sample
      yield return WaitFor.Emitter(emitterGeneric);
      // ~Sample

      Assert.AreEqual("EmitterGenericTrigger", coroutine.Result<string>());
    }

    private IEnumerator EmitterGenericTrigger() {
      yield return WaitFor.SecondsRealtime(0.1);

      emitterGeneric.Fire("EmitterGenericTrigger");
    }

    [UnityTest]
    public IEnumerator RunWaitForFrames() => Updates(WaitForFrames, 0.2f);

    private IEnumerator WaitForFrames() {
      float start = Time.realtimeSinceStartup;
      yield return WaitFor.NextUpdate();

      float frameTime = Time.realtimeSinceStartup - start;

      // Sample
      yield return WaitFor.Frames(10);
      // ~Sample

      Assert.AreApproximatelyEqual(Time.realtimeSinceStartup, start + (frameTime * 11));
    }

    [UnityTest]
    public IEnumerator RunWaitForNextUpdate() => Updates(WaitForNextUpdate, 0.2f);

    private IEnumerator WaitForNextUpdate() {
      float start = Time.realtimeSinceStartup;
      // Sample
      yield return WaitFor.NextUpdate();
      // ~Sample

      Assert.AreNotApproximatelyEqual(Time.realtimeSinceStartup, start);
    }

    [UnityTest]
    public IEnumerator RunWaitForCoroutine() => Updates(CoroutineFiber, 0.5f);

    private IEnumerator CoroutineFiber() {
      counter = 4;
      // Sample
      yield return WaitFor.Coroutine(SecondLevelIEnumeratorFiber);
      // ~Sample

      Assert.AreEqual(expected: 6, actual: counter);
    }

    private IEnumerator SecondLevelIEnumeratorFiber() {
      counter = 5;
      yield return WaitFor.Seconds(0.2f);

      counter = 6;
    }

    [UnityTest]
    public IEnumerator RunWaitForFunction() {
      counter = 0;

      // Sample
      coroutine = WaitFor.Updates(() => { counter = 10; });

      // ~Sample
      yield return new WaitForSeconds(0.2f);

      Assert.AreEqual(expected: 10, actual: counter);
    }

    [UnityTest]
    public IEnumerator RunWaitForSecondsRealtime() => Updates(WaitForSecondsRealtime, 0.7f);

    private IEnumerator WaitForSecondsRealtime() {
      float start = Time.realtimeSinceStartup;

      // Sample
      yield return WaitFor.SecondsRealtime(0.5f);
      // ~Sample

      Assert.AreApproximatelyEqual(expected: start + 0.5f, actual: Time.realtimeSinceStartup);
    }

    [UnityTest]
    public IEnumerator RunWaitForSeconds() => Updates(WaitForSeconds, 0.7f);

    public IEnumerator WaitForSeconds() {
      float start = Time.realtimeSinceStartup;

      // Sample
      yield return WaitFor.Seconds(0.5f);
      // ~Sample

      Assert.AreApproximatelyEqual(expected: start + 0.5f, actual: Time.realtimeSinceStartup);
    }

    private IEnumerator Updates(Func<IEnumerator> coroutine, float secondsDelay) {
      counter = 0;
      // Start up an independently running Fiber coroutine (cooperative multitasking)
      this.coroutine = WaitFor.Updates(coroutine);
      // Use Unity coroutines to wait enough time for the fiber to do it's thing
      yield return new WaitForSeconds(secondsDelay); // This is a standard Unity coroutine

      // make sure fiber has completed
      Assert.IsTrue(
        this.coroutine.Yield.EndYieldCondition(this.coroutine),
        "Fiber did not complete");
    }
  }
}