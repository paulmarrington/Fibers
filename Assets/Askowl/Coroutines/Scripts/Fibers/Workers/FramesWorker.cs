using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Frames(int framesToSkip) => FramesWorker.Instance.Yield(framesToSkip);
  }

  public class FramesWorker : Worker<int> {
    protected override bool AddToUpdate => true;

    protected override bool InRange(Fiber fiber) => Parameter(fiber) > Time.frameCount;

    protected override int SetRange(int framesToSkip) => Time.frameCount + framesToSkip;
  }
}