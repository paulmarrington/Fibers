// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if UNITY_EDITOR && Able

namespace Askowl.Examples {
  using System;
  using NUnit.Framework;

  public class EmitterExamples {
    [Test] public void ObserverEmitter() {
      counter = 0;

      var emitter = new Emitter();

      emitter.Subscribe(new Observer1());
      emitter.Subscribe(new Observer2());
      Assert.AreEqual(expected: 0, actual: counter);
      emitter.Fire();
      Assert.AreEqual(expected: 3, actual: counter);
      emitter.Fire();
      Assert.AreEqual(expected: 6, actual: counter);
    }

    [Test] public void ActionEmitter() {
      var emitter = new Emitter();
      void action() { counter++; }
      emitter.Subscribe(action);
      Assert.AreEqual(expected: 0, actual: counter);
      emitter.Fire();
      Assert.AreEqual(expected: 1, actual: counter);
    }

    [Test] public void ObserverTEmitter() {
      counter = 0;

      var emitter = new Emitter<int>();

      using (emitter.Subscribe(new Observer3())) {
        Assert.AreEqual(expected: 0, actual: counter);
        emitter.Fire(10);
        Assert.AreEqual(expected: 10, actual: counter);
        emitter.Fire(9);
        Assert.AreEqual(expected: 9, actual: counter);
      }

      // implicit Dispose()
      Assert.AreEqual(expected: 8, actual: counter);
      // Last value broadcast
      Assert.AreEqual(expected: 9, actual: emitter.LastValue);
    }

    private struct Observer1 : IObserver {
      public void OnNext()      { ++counter; }
      public void OnCompleted() { counter--; }
    }

    private struct Observer2 : IObserver {
      public void OnNext()      { counter += 2; }
      public void OnCompleted() { counter--; }
    }

    private struct Observer3 : IObserver<int> {
      public void OnCompleted()            { counter--; }
      public void OnError(Exception error) { throw new NotImplementedException(); }
      public void OnNext(int        value) { counter = value; }
    }

    private static int counter;
  }
}
#endif