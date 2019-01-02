// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

using System;
using UnityEngine;

namespace Askowl {
  public partial class Fiber {
    /// <a href=""></a> //#TBD#// 
    public Fiber SkipFrames(int framesToSkip) =>
      AddAction(_ => FrameWorker.Instance.Load(fiber: this, data: Time.frameCount + framesToSkip));

    /// <a href=""></a> //#TBD#//
    public Fiber SkipFrames(Func<Fiber, int> getter) => AddAction(_ => SkipFrames(getter(this)));

    private class FrameWorker : Worker<int> {
      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      FrameWorker Instance  => Cache<FrameWorker>.Instance;
      protected override void        Recycle() => Cache<FrameWorker>.Dispose(this);

      protected override int CompareTo(Worker other) => Seed.CompareTo((other as FrameWorker)?.Seed);

      public override bool NoMore => (Seed - 3) >= Time.frameCount;

      public override void Step() => Dispose();

      protected override bool Prepare() => true;
    }
  }
}