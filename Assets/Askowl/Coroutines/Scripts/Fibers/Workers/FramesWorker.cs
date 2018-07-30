using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Frames(int framesToSkip) => FramesWorker.Instance.Yield(framesToSkip);
    public static Yield NextFrame()              => FramesWorker.Instance.Yield(0);
    public static Yield NextUpdate()             => FramesWorker.Instance.Yield(0);
  }

  public class FramesWorker : Worker<int> {
    public static FramesWorker Instance = new FramesWorker();
    protected override bool AddToUpdate => true;

    protected override bool InRange(Fibers.Node node) => Parameter(node) > Time.frameCount;

    protected override int SetRange(int framesToSkip) => Time.frameCount + framesToSkip;
  }
}