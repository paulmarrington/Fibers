using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Frames(int framesToSkip) => FramesWorker.Instance(framesToSkip);
  }

  public class FramesWorker : Worker<int> {
    static FramesWorker() { Register(new FramesWorker()); }

    protected override bool InRange(Instance instance) => Data(instance) > Time.frameCount;

    protected override int SetRange(int framesToSkip) => Time.frameCount + framesToSkip;
  }
}