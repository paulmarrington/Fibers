// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local ClassNeverInstantiated.Global

namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class CustomWorkerExample {
    /// <a href=""></a>
    [UnityTest] public IEnumerator CustomTypeWorkerExample() {
      CustomTypeWorkerClass.Disposed = false;
      using (var fiber = Fiber.Start) {
        yield return fiber.CustomTypeWorker(3).UpdateCustomType().UpdateCustomType().AsCoroutine();

        Assert.AreEqual(5, ((CustomTypeWorkerClass) fiber.worker).Data);
        Assert.AreEqual(5, fiber.GetCustomValue());
      }
      Assert.IsTrue(CustomTypeWorkerClass.Disposed);
    }
  }

  internal class CustomTypeWorkerClass : Fiber.Worker<int> {
    public static      CustomTypeWorkerClass Instance  => Cache<CustomTypeWorkerClass>.Instance;
    protected override void                  Recycle() { Cache<CustomTypeWorkerClass>.Dispose(this); }
    public static      bool                  Disposed;
    protected override void                  Prepare() { }

    protected override int CompareTo(Fiber.Worker other) => Data.CompareTo((other as CustomTypeWorkerClass)?.Data);

    public override bool NoMore => Data >= 5;

    public override void Step() { Data++; }

    public override void Dispose() {
      Data = 0;
      base.Dispose();
    }

    // // // // // // // // // // // // // // // // // // // // // // // // //

    /// <a href=""></a>
    [UnityTest] public IEnumerator CustomObjectWorkerExample() {
      CustomObjectWorkerClass.Payload payload;
      using (var fiber = Fiber.Start) {
        var seed = new CustomObjectWorkerClass.Payload { A = 5, B = 6 };
        yield return fiber.CustomObjectWorker(seed).AsCoroutine();

        payload = ((CustomObjectWorkerClass) fiber.worker).Data;
      }
      Assert.AreEqual(11, payload.A + payload.B);
    }
  }

  /// <a href=""></a> <inheritdoc />
  public class CustomObjectWorkerClass : Fiber.Worker<CustomObjectWorkerClass.Payload> {
    public static      CustomObjectWorkerClass Instance  => Cache<CustomObjectWorkerClass>.Instance;
    protected override void                    Recycle() { Cache<CustomObjectWorkerClass>.Dispose(this); }

    /// <a href=""></a> <inheritdoc />
    protected override void Prepare() { }

    /// <a href=""></a>
    public struct Payload {
      /// <a href=""></a>
      public int A, B;
    }
  }

  /// <a href=""></a>
  public static class CustomFiberExtensions {
    /// <a href=""></a>
    public static Fiber CustomTypeWorker(this Fiber fiber, int seed) =>
      CustomTypeWorkerClass.Instance.Load(fiber, seed);

    /// <a href=""></a>
    public static Fiber UpdateCustomType(this Fiber fiber) {
      ((CustomTypeWorkerClass) fiber.worker).Data++;
      return fiber;
    }

    /// <a href=""></a>
    public static int GetCustomValue(this Fiber fiber) => ((CustomTypeWorkerClass) fiber.worker).Data;

    /// <a href=""></a>
    public static Fiber CustomObjectWorker(this Fiber fiber, CustomObjectWorkerClass.Payload payload) =>
      CustomObjectWorkerClass.Instance.Load(fiber, payload);
  }
}