using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield SecondsRealtime(float  seconds) => RealtimeWorker.Instance.Yield(seconds);
    public static Yield SecondsRealtime(double seconds) => RealtimeWorker.Instance.Yield((float) seconds);
  }

  public class RealtimeWorker : Worker<float> {
    public static      RealtimeWorker Instance = new RealtimeWorker();
    protected override bool           AddToUpdate => true;

    protected override bool InRange(Fibers.Node node) => Parameter(node) > Time.realtimeSinceStartup;

    protected override float SetRange(float seconds) => Time.realtimeSinceStartup + seconds;
  }
}