using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public sealed class After {
  [UsedImplicitly]
  public sealed class Delay {
    private static readonly Dictionary<int, WaitForSeconds> MsCache =
      new Dictionary<int, WaitForSeconds>();

    // ReSharper disable once InconsistentNaming
    public static IEnumerator ms(int ms) {
      if (!MsCache.ContainsKey(key: ms)) {
        MsCache[key: ms] = new WaitForSeconds(seconds: ms / 1000.0f);
      }

      yield return MsCache[key: ms];
    }

    private static readonly Dictionary<int, WaitForSeconds> SecondsCache =
      new Dictionary<int, WaitForSeconds>();

    // ReSharper disable once InconsistentNaming
    public static IEnumerator seconds(int seconds) {
      if (!SecondsCache.ContainsKey(key: seconds)) {
        SecondsCache[key: seconds] = new WaitForSeconds(seconds: seconds);
      }

      yield return SecondsCache[key: seconds];
    }

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static IEnumerator minutes(int minutes) { return seconds(seconds: minutes * 60); }
  }

  [UsedImplicitly]
  public sealed class Realtime {
    // ReSharper disable once InconsistentNaming
    public static IEnumerator ms(int ms) {
      yield return new WaitForSecondsRealtime(time: ms / 1000.0f);
    }

    // ReSharper disable once InconsistentNaming
    public static IEnumerator seconds(int seconds) {
      yield return new WaitForSecondsRealtime(time: seconds);
    }

    // ReSharper disable once InconsistentNaming
    [UsedImplicitly]
    public static IEnumerator minutes(int minutes) { return seconds(seconds: minutes * 60); }

    public static TimerClass Timer(int secondsTimeout) { return new TimerClass(secondsTimeout); }

    public sealed class TimerClass {
      private readonly float endOfTime;

      public TimerClass(int secondsTimeout) {
        endOfTime = Time.realtimeSinceStartup + secondsTimeout;
      }

      public bool Running { get { return (Time.realtimeSinceStartup < endOfTime); } }
    }
  }
}