// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl.Examples {
  using System.Collections;
  using UnityEngine.TestTools;

  /// Using <see cref="Askowl.Fibers" />
  /// <inheritdoc />
  public class WaitForSecondsExample : PlayModeTests {
    private string SceneName = "";

    /// <a href="">Using <see cref="Fiber.WaitForSeconds"/></a>
    [UnityTest]
    public IEnumerator ExternalExample() {
      yield return LoadScene(SceneName);
      yield return Fiber
                  .Start(StepOne, StepTwo)
                  .WaitForSeconds(0.2f)
                  .Do(StepThree)
                  .AsCoroutine();
    }

    private void StepOne(Fiber fiber) { }

    private void StepTwo(Fiber   fiber) { }
    private void StepThree(Fiber fiber) { }

    /// <a href="">Using <see cref="Fiber.WaitForSeconds"/></a>
    [UnityTest]
    public IEnumerator InternalExample() {
      yield return LoadScene(SceneName);
      yield return WaitForSecondsInClass.Start().AsCoroutine();
    }

    private class WaitForSecondsInClass : Fiber {
      public static Fiber Start(params Action[] actions) {
        var fiber = Fiber.Start(actions);
      }
    }
  }
}