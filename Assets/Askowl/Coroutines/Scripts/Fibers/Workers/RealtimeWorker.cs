using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield SecondsRealtime(float seconds) { return RealtimeWorker.Instance(seconds); }
  }

  public class RealtimeWorker : Worker<float> {
    static RealtimeWorker() { Register(new RealtimeWorker()); }

    protected override bool InRange(Fiber fiber) => Parameter(fiber) > Time.realtimeSinceStartup;

    protected override float SetRange(float seconds) => Time.realtimeSinceStartup + seconds;
  }
}