// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using System.Collections;
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber Coroutine(int framesBetweenChecks, IEnumerator enumerator) =>
      IEnumeratorWorker.Instance.Load(this, LoadPayload(enumerator, framesBetweenChecks));

    private static IEnumeratorWorker.Payload LoadPayload(IEnumerator e, int f) => new IEnumeratorWorker.Payload {
      Enumerator = e, SkipFrames = Time.frameCount + f
    };

    /// <a href=""></a>
    public Fiber Coroutine(IEnumerator enumerator) => IEnumeratorWorker.Instance.Load(this, LoadPayload(enumerator, 0));

    /// <a href=""></a> <inheritdoc />
    private class IEnumeratorWorker : Worker<IEnumeratorWorker.Payload> {
      public static      IEnumeratorWorker Instance  => Cache<IEnumeratorWorker>.Instance;
      protected override void              Recycle() { Cache<IEnumeratorWorker>.Dispose(this); }

      /// <a href=""></a>
      public struct Payload {
        internal IEnumerator Enumerator;
        internal int         SkipFrames;
      }

      protected override void Prepare() { }

      protected override int CompareTo(Worker other) =>
        Seed.SkipFrames.CompareTo((other as IEnumeratorWorker)?.Seed.SkipFrames);

      public override bool NoMore => Seed.SkipFrames > Time.frameCount;

      public override void Step() {
        if (Seed.Enumerator.MoveNext()) {
          switch (Seed.Enumerator.Current) {
            case IEnumerator coroutine:
              Fiber.Coroutine(coroutine);
              break;
            case float seconds:
              Fiber.WaitForSeconds(seconds);
              break;
            case int frames:
              Fiber.SkipFrames(frames);
              break;
            case null: break; // step again on next frame
          }
        }
        else { Dispose(); }
      }
    }
  }
}