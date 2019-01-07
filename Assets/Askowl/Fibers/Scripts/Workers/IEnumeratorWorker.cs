// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

using System;
using System.Collections;
using UnityEngine;

namespace Askowl {
  public partial class Fiber {
    /// <a href="http://bit.ly/2CSVnCt">Wait for a C# IEnumerator/Unity Coroutine to take a step</a>
    public Fiber WaitFor(IEnumerator enumerator) =>
      AddAction(_ => LoadWithPayload(enumerator, 0), "WaitFor(IEnumerator)");

    /// <a href="http://bit.ly/2CSVnCt">Wait for a C# IEnumerator/Unity Coroutine to take a step - value passed by function return</a>
    public Fiber WaitFor(Func<Fiber, IEnumerator> getter) =>
      AddAction(_ => WaitFor(getter(this)), "WaitFor(IEnumerator)");

    /// <a href=""></a>
    public Fiber WaitFor(int framesBetweenChecks, IEnumerator enumerator) =>
      AddAction(_ => LoadWithPayload(enumerator, framesBetweenChecks));

    private Fiber LoadWithPayload(IEnumerator enumerator, int skipFrames) {
      var payload = new EnumeratorWorker.Payload {Enumerator = enumerator, SkipFrames = skipFrames};
      return EnumeratorWorker.Instance.Load(this, payload);
    }

    /// <a href=""></a> <inheritdoc />
    private class EnumeratorWorker : Worker<EnumeratorWorker.Payload> {
//      static EnumeratorWorker() => NeedsUpdates = false;
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      EnumeratorWorker Instance  => Cache<EnumeratorWorker>.Instance;
      protected override void             Recycle() => Cache<EnumeratorWorker>.Dispose(this);

      /// <a href=""></a>
      public struct Payload {
        internal IEnumerator Enumerator;
        internal int         SkipFrames;
      }

      private int nextStepFrame;

      protected override bool Prepare() => true;

      protected override int CompareTo(Worker other) =>
        Seed.SkipFrames.CompareTo((other as EnumeratorWorker)?.Seed.SkipFrames);

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
            case YieldInstruction _:
              Log.Error($"YieldInstruction {Seed.Enumerator.Current.GetType()} only works with Unity coroutines");
              break;
            case null: break; // step again on next frame
          }
        } else { Dispose(); }
      }
    }
  }
}