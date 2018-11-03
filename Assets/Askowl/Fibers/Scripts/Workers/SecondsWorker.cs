// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitForSeconds(float seconds) => SecondsWorker.Instance.Load(fiber: this, data: seconds);

    /// <a href=""></a>
    public Fiber WaitForSecondsRealtime(float seconds) => RealtimeWorker.Instance.Load(fiber: this, seconds);

    private class SecondsWorker : BaseTimeWorker {
      protected override void Prepare() { EndTime = Seed + Time.time - 2 * Time.deltaTime; }

      public static      BaseTimeWorker Instance  => Cache<SecondsWorker>.Instance;
      protected override void           Recycle() => Cache<SecondsWorker>.Dispose(this);
      protected override float          TimeNow   => Time.time;
    }

    private class RealtimeWorker : BaseTimeWorker {
      protected override void Prepare() => EndTime = Seed + Time.realtimeSinceStartup - 2 * Time.unscaledDeltaTime;

      public static      BaseTimeWorker Instance  => Cache<RealtimeWorker>.Instance;
      protected override void           Recycle() => Cache<RealtimeWorker>.Dispose(this);
      protected override float          TimeNow   => Time.realtimeSinceStartup;
    }

    private abstract class BaseTimeWorker : Worker<float> {
      protected float EndTime;

      protected abstract float TimeNow { get; }

      protected override int CompareTo(Worker other) => Seed.CompareTo((other as BaseTimeWorker)?.Seed);

      public override bool NoMore => EndTime > TimeNow;

      public override void Step() => Dispose();
    }
  }
}