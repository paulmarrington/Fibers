namespace Askowl.Examples {
  using System.Collections;
  using NUnit.Framework;
  using UnityEngine.TestTools;

  /// <a href=""></a>
  public class UpdatesExample {
    private int counter;

    /// <a href=""></a>
    [UnityTest] public IEnumerator StartExample() {
      counter = 0;
      yield return Fiber.Start.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator OnUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnLateUpdates.OnUpdates.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator OnLateUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnLateUpdates.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    /// <a href=""></a>
    [UnityTest] public IEnumerator OnFixedUpdatesExample() {
      counter = 0;
      yield return Fiber.Start.OnFixedUpdates.Do(Check).AsCoroutine();

      Assert.AreEqual(1, counter);
    }

    private void Check(Fiber fiber) => counter = 1;
  }
}