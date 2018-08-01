using UnityEngine;

namespace Askowl.Fibers {
  public partial class Fiber {
    public Fiber WaitForSeconds(float seconds) => SecondsWorker.Load(fiber: this, data: seconds);
  }

  public class SecondsWorker : Worker<float> {
    static SecondsWorker() { new SecondsWorker().Prepare("Fiber.WaitForSeconds Worker"); }
    protected override bool InRange() => (data -= Time.deltaTime) > 0;
  }
}