﻿using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Seconds(float seconds) { return SecondsWorker.Instance(seconds); }
  }

  public class SecondsWorker : Worker<float> {
    static SecondsWorker() { Register(new SecondsWorker()); }

    protected override bool InRange(Instance instance) => Yield(instance).EndCondition();
  }
}