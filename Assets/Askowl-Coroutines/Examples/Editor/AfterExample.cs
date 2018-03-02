using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class AfterExample {
  [UnityTest]
  public IEnumerator AfterExampleWithEnumeratorPasses() {
    float start = Time.time;

    yield return After.Delay.seconds(seconds: 2);

    Assert.AreEqual(expected: 2, actual: (int)(Time.time - start));

    yield return After.Delay.ms(ms: 1000);

    Assert.AreEqual(expected: 3, actual: (int)(Time.time - start));

    yield return After.Realtime.seconds(seconds: 1);

    Assert.AreEqual(expected: 4, actual: (int)(Time.time - start));

    yield return After.Realtime.ms(ms: 1000);

    Assert.AreEqual(expected: 5, actual: (int)(Time.time - start));
  }
}
