using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Seconds(float  seconds) => SecondsWorker.Instance.Yield(seconds);
    public static Yield Seconds(double seconds) => SecondsWorker.Instance.Yield((float) seconds);
  }

  public class SecondsWorker : Worker<float> {
    public static      SecondsWorker Instance = new SecondsWorker();
    protected override bool          AddToUpdate => true;

    protected override bool InRange(Fibers.Node node) {
      float time = Parameter(node);
      time -= Time.deltaTime;
      node.Item.Yield.Parameter(value: time);
      return time > 0;
    }
  }
}