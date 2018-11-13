// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if UNITY_EDITOR && AskowlFibers
namespace Askowl.Examples {
  using System.Collections;
  using Askowl;
  using NUnit.Framework;
  using UnityEngine.TestTools;
  using System.Threading.Tasks;

  /// <a href="">Samples for using `Tasks` - also behaving as runtime tests</a> //#TBD#//
  public sealed class TasksExample {
    private int counter;

    // Start an asynchronous task that completes after a time in milliseconds
    private Task Delay(int ms) {
      return Task.Run(
        async () => {
          await Task.Delay(ms);
          counter++;
        });
    }

    /// <a href="">Check that tasks will cause matching coroutines to wait until they are done</a> //#TBD#//
    [UnityTest, Timeout(10000)] public IEnumerator EmitOnComplete() {
      counter = 0;
      Task task = Delay(500);
      yield return Fiber.Start.WaitFor(task).AsCoroutine();

      Assert.AreEqual(counter, 1);
    }
  }
}
#endif