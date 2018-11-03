// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local ClassNeverInstantiated.Global

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class CustomWorkerExample {
    /// <a href=""></a>
    [UnityTest] public IEnumerator CustomTypeWorkerExample() {
      CustomTypeWorkerClass.Disposed = false;
        var start = Time.frameCount;
        yield return Fiber.Start.CustomTypeWorker(3).AsCoroutine();

        Assert.AreEqual(4, Time.frameCount - start);
      Assert.IsTrue(CustomTypeWorkerClass.Disposed);
    }

    /// <a href=""></a>
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

  /// <a href=""></a> <inheritdoc />
  public class CustomObjectWorkerClass : Fiber.Worker<CustomObjectWorkerClass.Payload> {
    public static      CustomObjectWorkerClass Instance  => Cache<CustomObjectWorkerClass>.Instance;
    protected override void                    Recycle() { Cache<CustomObjectWorkerClass>.Dispose(this); }

    /// <a href=""></a> <inheritdoc />
    protected override void Prepare() { }

    /// <a href=""></a>
    public class Payload {
      /// <a href=""></a>
      public int A, B;
    }

    public override void Step() {
      if (++Seed.A == 10) Dispose();
    }
  }

  /// <a href=""></a>
  public static class MyCustomFiberExtensions {
    /// <a href=""></a>
    public static Fiber CustomTypeWorker(this Fiber fiber, int seed) =>
      CustomTypeWorkerClass.Instance.Load(fiber, seed);

    /// <a href=""></a>
    public static Fiber CustomObjectWorker(this Fiber fiber, CustomObjectWorkerClass.Payload payload) =>
      CustomObjectWorkerClass.Instance.Load(fiber, payload);
  }
}