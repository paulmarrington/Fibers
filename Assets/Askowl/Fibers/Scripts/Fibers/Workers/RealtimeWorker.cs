using UnityEngine;

namespace Askowl.Fibers {
  public partial class Fiber {
    public Fiber WaitForSecondsRealtime(float seconds) => RealtimeWorker.Load(fiber: this, data: seconds);

    public class RealtimeWorker : Worker<float> {
      static RealtimeWorker() { new RealtimeWorker().Prepare("Fiber.WaitForRealtimeSeconds Worker"); }
      protected override float Parse(float seconds) => Time.realtimeSinceStartup + seconds;
      protected override bool  InRange()            => data >= Time.realtimeSinceStartup;
    }
  }
}