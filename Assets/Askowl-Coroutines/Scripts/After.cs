using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class After {
  public class Delay {
    static Dictionary<int,WaitForSeconds> msCache = new Dictionary<int,WaitForSeconds> ();

    public static IEnumerator ms(int ms) {
      if (!msCache.ContainsKey(ms)) {
        msCache [ms] = new WaitForSeconds (ms / 1000.0f);
      }
      yield return msCache [ms];
    }

    static Dictionary<int,WaitForSeconds> secondsCache = new Dictionary<int,WaitForSeconds> ();

    public static IEnumerator seconds(int seconds) {
      if (!secondsCache.ContainsKey(seconds)) {
        secondsCache [seconds] = new WaitForSeconds ((float)seconds);
      }
      yield return secondsCache [seconds];
    }

    public static IEnumerator minutes(int minutes) {
      return seconds(minutes * 60);
    }
  }

  public class Realtime {

    public static IEnumerator ms(int ms) {
      yield return new WaitForSecondsRealtime (ms / 1000.0f);
    }

    public static IEnumerator seconds(int seconds) {
      yield return new WaitForSecondsRealtime ((float)seconds);
    }

    public static IEnumerator minutes(int minutes) {
      return seconds(minutes * 60);
    }
  }
}
