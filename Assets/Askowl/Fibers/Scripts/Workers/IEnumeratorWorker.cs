// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using System.Collections;
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitFor(IEnumerator enumerator) => LoadWithPayload(enumerator, 0);

    /// <a href=""></a>
    public Fiber WaitFor(int framesBetweenChecks, IEnumerator enumerator) =>
      LoadWithPayload(enumerator, framesBetweenChecks);

    private Fiber LoadWithPayload(IEnumerator enumerator, int skipFrames) {
      var payload = new IEnumeratorWorker.Payload { Enumerator = enumerator, SkipFrames = skipFrames };
      return IEnumeratorWorker.Instance.Load(this, payload);
    }

    /// <a href=""></a> <inheritdoc />
    private class IEnumeratorWorker : Worker<IEnumeratorWorker.Payload> {
//      static IEnumeratorWorker() => NeedsUpdates = false;
      public static      IEnumeratorWorker Instance  => Cache<IEnumeratorWorker>.Instance;
      protected override void              Recycle() { Cache<IEnumeratorWorker>.Dispose(this); }

      /// <a href=""></a>
      public struct Payload {
        internal IEnumerator Enumerator;
        internal int         SkipFrames;
      }

      private int nextStepFrame;

      protected override void Prepare() { }

      protected override int CompareTo(Worker other) =>
        Seed.SkipFrames.CompareTo((other as IEnumeratorWorker)?.Seed.SkipFrames);

      public override void Step() {
        if (Time.frameCount < nextStepFrame) return;

        nextStepFrame = Time.frameCount + Seed.SkipFrames;

        if (Seed.Enumerator.MoveNext()) {
          switch (Seed.Enumerator.Current) {
            case IEnumerator coroutine:
              Fiber.WaitFor(coroutine);
              break;
            case float seconds:
              Fiber.WaitFor(seconds);
              break;
            case int frames:
              nextStepFrame = Time.frameCount + frames;
              break;
            case YieldInstruction yieldInstruction:
              Log.Error($"YieldInstruction {Seed.Enumerator.Current.GetType()} only works with Unity coroutines");
              break;
            case null: break; // step again on next frame
          }
        }
        else { Dispose(); }
      }
    }
  }
}