// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local ClassNeverInstantiated.Global MissingXmlDoc

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  public class CustomWorkerExample {
    [UnityTest] public IEnumerator CustomTypeWorkerExample() {
      CustomTypeWorkerClass.Disposed = false;
      var start = Time.frameCount;
      yield return Fiber.Start.CustomTypeWorker(3).AsCoroutine();

      Assert.AreEqual(4, Time.frameCount - start);
      Assert.IsTrue(CustomTypeWorkerClass.Disposed);
    }

    [UnityTest] public IEnumerator CustomObjectWorkerExample() {
      CustomObjectWorkerClass.Payload payload = new CustomObjectWorkerClass.Payload { A = 5, B = 6 };
      yield return Fiber.Start.CustomObjectWorker(payload).AsCoroutine();

      Assert.AreEqual(16, payload.A + payload.B);
    }
  }

  internal class CustomTypeWorkerClass : Fiber.Worker<int> {
    public static      CustomTypeWorkerClass Instance  => Cache<CustomTypeWorkerClass>.Instance;
    protected override void                  Recycle() { Cache<CustomTypeWorkerClass>.Dispose(this); }
    public static      bool                  Disposed;

    protected override void Prepare() {
      Disposed = false;
      counter  = Seed;
    }

    private int counter;

    protected override int CompareTo(Fiber.Worker other) =>
      counter.CompareTo((other as CustomTypeWorkerClass)?.counter);

    public override void Step() {
      if (++counter == 5) Dispose();
    }

    public override void Dispose() {
      Disposed = true;
      base.Dispose();
    }
  }

  public class CustomObjectWorkerClass : Fiber.Worker<CustomObjectWorkerClass.Payload> {
    public static      CustomObjectWorkerClass Instance  => Cache<CustomObjectWorkerClass>.Instance;
    protected override void                    Recycle() { Cache<CustomObjectWorkerClass>.Dispose(this); }

    protected override void Prepare() { }

    public class Payload {
      public int A, B;
    }

    public override void Step() {
      if (++Seed.A == 10) Dispose();
    }
  }

  public static class MyCustomFiberExtensions {
    public static Fiber CustomTypeWorker(this Fiber fiber, int seed) =>
      CustomTypeWorkerClass.Instance.Load(fiber, seed);

    public static Fiber CustomObjectWorker(this Fiber fiber, CustomObjectWorkerClass.Payload payload) =>
      CustomObjectWorkerClass.Instance.Load(fiber, payload);
  }
}