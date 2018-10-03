// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitForSeconds(float seconds) => SecondsWorker.Instance(fiber: this, data: Time.time + seconds);

    /// <a href=""></a>
    public Fiber WaitForSecondsRealtime(float seconds) => RealtimeWorker.Instance(
      fiber: this, data: Time.realtimeSinceStartup + seconds);

    private class SecondsWorker : BaseTimeWorker {
      protected override float TimeNow => Time.time;
    }

    private class RealtimeWorker : BaseTimeWorker {
      protected override float TimeNow => Time.realtimeSinceStartup;
    }

    private abstract class BaseTimeWorker : Worker<float> {
      protected abstract float TimeNow { get; }

      protected override int CompareTo(Worker other) => Data.CompareTo((other as BaseTimeWorker)?.Data);

      public override bool NoMore => Data > TimeNow;

      public override void Step() { Unload(); }
    }
  }
}