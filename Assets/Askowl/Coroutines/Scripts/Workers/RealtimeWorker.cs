using UnityEngine;

namespace Askowl.Fibers {
  public static partial class Cue {
    public static Yield SecondsRealtime(float seconds) { return RealtimeWorker.Instance(seconds); }
  }

  public class RealtimeWorker : Worker<float> {
    static RealtimeWorker() { Register(new RealtimeWorker()); }

    protected override bool InRange(Instance ins) => Data(ins) > Time.realtimeSinceStartup;

    protected override float SetRange(float seconds) => Time.realtimeSinceStartup + seconds;
  }
}