// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using System.Collections;
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber Coroutine(int framesBetweenChecks, IEnumerator enumerator) => IEnumeratorWorker.Instance(
      this
    , new IEnumeratorWorker.Payload { Enumerator = enumerator, SkipFrames = Time.frameCount + framesBetweenChecks });

    /// <a href=""></a>
    public Fiber Coroutine(IEnumerator enumerator) => IEnumeratorWorker.Instance(
      this, new IEnumeratorWorker.Payload { Enumerator = enumerator, SkipFrames = Time.frameCount });

    /// <a href=""></a> <inheritdoc />
    private class IEnumeratorWorker : Worker<IEnumeratorWorker.Payload> {
      /// <a href=""></a>
      public struct Payload {
        internal IEnumerator Enumerator;
        internal int         SkipFrames;
      }

      protected override int CompareTo(Worker other) =>
        Data.SkipFrames.CompareTo((other as IEnumeratorWorker)?.Data.SkipFrames);

      public override bool NoMore => Data.SkipFrames > Time.frameCount;

      public override void Step() {
        if (Data.Enumerator.MoveNext()) {
          switch (Data.Enumerator.Current) {
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
        else { Unload(); }
      }
    }
  }
}