// ReSharper disable MissingXmlDoc

using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

#if UNITY_EDITOR && Fibers

namespace Askowl.Examples {
  public class UpdatesExample {
    private int counter;

    [UnityTest] public IEnumerator StartExample() {
      counter = 0;
      yield return Fiber.Start.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator OnUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnLateUpdates.OnUpdates.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator OnLateUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnLateUpdates.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    [UnityTest] public IEnumerator OnFixedUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnFixedUpdates.Do(Check).AsCoroutine();
      Assert.AreEqual(1, counter);
    }

    private void Check(Fiber fiber) => counter = 1;
  }
}
#endif