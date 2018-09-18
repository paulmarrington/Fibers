// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using UnityEngine;

namespace Askowl.Fibers {
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitForSeconds(float seconds) => SecondsWorker.Load(fiber: this, data: seconds);
  }

  /// <a href=""></a>
  /// <inheritdoc />
  public class SecondsWorker : Worker<float> {
    static SecondsWorker() { new SecondsWorker().Prepare("Fiber.WaitForSeconds Worker"); }

    /// <inheritdoc />
    protected override int CompareItem(LinkedList<Fiber>.Node left, LinkedList<Fiber>.Node right) =>
      data.CompareTo(Time.time);

    /// <a href="">Set `data` tom game end time in seconds</a>
    /// <inheritdoc />
    protected override float Parse(float seconds) => Time.time + seconds;
  }
}