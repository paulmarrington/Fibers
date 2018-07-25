using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Seconds(float seconds) { return SecondsWorker.Instance.Yield(seconds); }
  }

  public class SecondsWorker : Worker<float> {
    protected override bool AddToUpdate => true;

    protected override bool InRange(Fiber fiber) {
      float time = Parameter(fiber);
      time -= Time.deltaTime;
      fiber.Yield.Parameter(value: time);
      return time > 0;
    }
  }
}