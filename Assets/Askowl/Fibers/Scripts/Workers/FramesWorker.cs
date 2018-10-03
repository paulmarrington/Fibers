// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using UnityEngine;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber NextFrame => FrameWorker.Instance(fiber: this, Time.frameCount);

    /// <a href=""></a>
    public Fiber NextUpdate => FrameWorker.Instance(fiber: this, Time.frameCount);

    /// <a href=""></a>
    public Fiber SkipFrames(int framesToSkip) =>
      FrameWorker.Instance(fiber: this, data: Time.frameCount + framesToSkip);

    private class FrameWorker : Worker<int> {
      protected override int CompareTo(Worker other) => Data.CompareTo((other as FrameWorker)?.Data);

      public override bool NoMore => Data > Time.frameCount;

      public override void Step() { Unload(); }
    }
  }
}