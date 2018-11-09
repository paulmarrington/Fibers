// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if UNITY_EDITOR && AskowlFibers
using System.Collections;
using Askowl;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Threading.Tasks;

/// <summary>
/// Samples for using `Tasks` - also behaving as runtime tests
/// </summary>
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

  /// <summary>
  /// Check that tasks will cause matching coroutines to wait until they are done.
  /// </summary>
  /// <returns></returns>
  [UnityTest, Timeout(10000)] public IEnumerator EmitOnComplete() {
    counter = 0;
    Task task = Delay(500);
    yield return Fiber.Start.Emitter(Tasks.EmitOnComplete(task)).AsCoroutine();

    Assert.AreEqual(counter, 1);
  }
}
#endif