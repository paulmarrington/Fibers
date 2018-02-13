using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class AfterExample {
  [UnityTest]
  public IEnumerator AfterExampleWithEnumeratorPasses() {
    float start = Time.time;

    yield return After.Delay.seconds(2);

    Assert.AreEqual(2, (int)(Time.time - start));

    yield return After.Delay.ms(1000);

    Assert.AreEqual(3, (int)(Time.time - start));

    yield return After.Realtime.seconds(1);

    Assert.AreEqual(4, (int)(Time.time - start));

    yield return After.Realtime.ms(1000);

    Assert.AreEqual(5, (int)(Time.time - start));
  }
}
