// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

using System;
using UnityEngine;

namespace Askowl {
  public partial class Fiber {
    /// <a href="http://bit.ly/2RdEKtZ">Wait the specified time in game-seconds</a>
    public Fiber WaitFor(float seconds) =>
      AddAction(_ => SecondsWorker.Instance.Load(fiber: this, data: seconds), "WaitFor(Seconds)");

    /// <a href="http://bit.ly/2RdEKtZ">Wait the specified time in game-seconds - value passed by function return</a>
    public Fiber WaitFor(Func<Fiber, float> getter) => AddAction(_ => WaitFor(getter(this)), "WaitFor(Seconds)");

    /// <a href="http://bit.ly/2RdEKtZ">Wait the specified time in real-world seconds</a>
    public Fiber WaitRealtime(float seconds) =>
      AddAction(_ => RealtimeWorker.Instance.Load(fiber: this, seconds), "WaitFor(Realtime Seconds)");

    /// <a href="http://bit.ly/2RdEKtZ">Wait the specified time in real-world seconds - value passed by function return</a>
    public Fiber WaitRealtime(Func<Fiber, float> getter) =>
      AddAction(_ => WaitRealtime(getter(this)), "WaitFor(Realtime Seconds)");

    private class SecondsWorker : BaseTimeWorker {
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      BaseTimeWorker Instance            => Cache<SecondsWorker>.Instance;
      protected override void           Recycle()           => Cache<SecondsWorker>.Dispose(this);
      protected override float          TimeNow             => Time.time;
      protected override float          CalculatedEndTime() => (Seed + Time.time) - (2 * Time.deltaTime);
    }

    private class RealtimeWorker : BaseTimeWorker {
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      BaseTimeWorker Instance  => Cache<RealtimeWorker>.Instance;
      protected override void           Recycle() => Cache<RealtimeWorker>.Dispose(this);
      protected override float          TimeNow   => Time.realtimeSinceStartup;

      protected override float CalculatedEndTime() => (Seed + Time.realtimeSinceStartup) - (2 * Time.unscaledDeltaTime);
    }

    private abstract class BaseTimeWorker : Worker<float> {
      private float endTime;

      protected abstract float CalculatedEndTime();

      protected override bool Prepare() {
        endTime = CalculatedEndTime();
        return true;
      }

      protected abstract float TimeNow { get; }

      protected override int CompareTo(Worker other) => Seed.CompareTo((other as BaseTimeWorker)?.Seed);

      public override bool NoMore => endTime > TimeNow;

      public override void Step() => Dispose();
    }
  }
}