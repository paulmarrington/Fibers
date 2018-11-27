// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

#if UNITY_EDITOR && AskowlFibers
namespace Askowl.Examples {
  using System.Collections;
  using System.Threading.Tasks;
  using NUnit.Framework;
  using UnityEngine.TestTools;

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

    [UnityTest, Timeout(10000)] public IEnumerator EmitOnComplete() {
      counter = 0;
      Task task = Delay(500);
      yield return Fiber.Start.WaitFor(task).AsCoroutine();

      Assert.AreEqual(counter, 1);
    }
  }
}
#endif