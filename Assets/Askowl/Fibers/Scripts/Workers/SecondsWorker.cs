// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using UnityEngine;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitForSeconds(float seconds) => SecondsWorker.Load(fiber: this, data: seconds);

    /// <a href=""></a>
    public Fiber WaitForSecondsRealtime(float seconds) => RealtimeWorker.Load(fiber: this, data: seconds);

    private class SecondsWorker : BaseTimeWorker {
      static SecondsWorker() { new SecondsWorker().Prepare("Fiber.WaitForSeconds Worker"); }
      protected override float TimeNow => Time.time;
    }

    private class RealtimeWorker : BaseTimeWorker {
      static RealtimeWorker() { new RealtimeWorker().Prepare("Fiber.WaitForRealtimeSeconds Worker"); }
      protected override float TimeNow => Time.realtimeSinceStartup;
    }

    private abstract class BaseTimeWorker : Worker<float> {
      protected abstract float TimeNow { get; }

      protected override float Parse(float seconds) => TimeNow + seconds;

      protected override int CompareItem(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
        data.CompareTo(TimeNow);
    }
  }
}