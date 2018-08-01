using System;
using UnityEngine;

namespace Askowl.Fibers {
  public partial class Fiber {
    public Fiber NextFrame()                  => FrameWorker.Load(fiber: this, data: 0);
    public Fiber NextUpdate()                 => FrameWorker.Load(fiber: this, data: 0);
    public Fiber SkipFrames(int framesToSkip) => FrameWorker.Load(fiber: this, data: framesToSkip);
  }

  public class FrameWorker : Worker<int> {
    static FrameWorker() { new FrameWorker().Prepare("Fiber.Frame Worker"); }
    protected override int  Parse(int framesToSkip) => Time.frameCount + framesToSkip;
    protected override bool InRange()               => data >= Time.frameCount;
  }
}