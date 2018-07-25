using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield SecondsRealtime(float seconds) => RealtimeWorker.Instance.Yield(seconds);
  }

  public class RealtimeWorker : Worker<float> {
    protected override bool AddToUpdate => true;

    protected override bool InRange(Fiber fiber) => Parameter(fiber) > Time.realtimeSinceStartup;

    protected override float SetRange(float seconds) => Time.realtimeSinceStartup + seconds;
  }
}