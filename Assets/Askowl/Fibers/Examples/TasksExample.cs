// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if UNITY_EDITOR && AskowlCoroutines
using System.Collections;
using Askowl;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if (!NET_2_0 && !NET_2_0_SUBSET)
using System.Threading.Tasks;

#endif

/// <summary>
/// Samples for using `Tasks` - also behaving as runtime tests
/// </summary>
public sealed class TasksExample {
#if (!NET_2_0 && !NET_2_0_SUBSET)
  private int counter;

  // Start an asynchronous task that completes after a time in milliseconds
  private Task Delay(int ms, string msg) {
    return Task.Run(async () => {
      Debug.Log(msg);
      await Task.Delay(ms);
      counter++;
    });
  }

  /// <summary>
  /// Check that tasks will cause matching coroutines to wait until they are done.
  /// </summary>
  /// <returns></returns>
  [UnityTest, Timeout(10000)]
  public IEnumerator TestTasksExampleWithEnumeratorPasses() {
    counter = 0;
    Task task = Delay(500, "1. Wait for task to complete");
    yield return Tasks.WaitFor(task);

    Assert.AreEqual(counter, 1);

    task = Delay(500, "2. Wait for task to complete with error processing");
    yield return Tasks.WaitFor(task, Debug.Log);

    Assert.AreEqual(counter, 2);

    Debug.Log("3. All Done");
  }
#else
  [UnityTest]
  public IEnumerator TasksExampleWithEnumeratorPasses() {
    Debug.LogWarning(message: "Switch player settings to .NET version 4.0 or better");
    yield return null;
  }
  #endif
}
#endif