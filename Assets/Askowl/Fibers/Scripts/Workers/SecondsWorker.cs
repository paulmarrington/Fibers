// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitForSeconds(float seconds) =>
      SecondsWorker.Instance.Load(fiber: this, data: Time.time + seconds - 2 * Time.deltaTime);

    /// <a href=""></a>
    public Fiber WaitForSecondsRealtime(float seconds) =>
      RealtimeWorker.Instance.Load(this, Time.realtimeSinceStartup + seconds - 2 * Time.unscaledDeltaTime);

    private class SecondsWorker : BaseTimeWorker {
      public static      BaseTimeWorker Instance  => Cache<SecondsWorker>.Instance;
      protected override void           Recycle() => Cache<SecondsWorker>.Dispose(this);
      protected override float          TimeNow   => Time.time;
    }

    private class RealtimeWorker : BaseTimeWorker {
      public static      BaseTimeWorker Instance  => Cache<RealtimeWorker>.Instance;
      protected override void           Recycle() => Cache<RealtimeWorker>.Dispose(this);
      protected override float          TimeNow   => Time.realtimeSinceStartup;
    }

    private abstract class BaseTimeWorker : Worker<float> {
      protected override void Prepare() { }

      protected abstract float TimeNow { get; }

      protected override int CompareTo(Worker other) => Data.CompareTo((other as BaseTimeWorker)?.Data);

      public override bool NoMore => Data > TimeNow;

      public override void Step() { Dispose(); }
    }
  }
}