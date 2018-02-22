using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

#if (!NET_2_0 && !NET_2_0_SUBSET)
using System.Threading.Tasks;
#endif

public class TasksExample {
  #if (!NET_2_0 && !NET_2_0_SUBSET)

  int counter = 0;

  // Start an asynchronous task that completes after a time in milliseconds
  Task Delay(int ms, string msg) {
    return Task.Run(async () => {
        Debug.Log(msg);
        await Task.Delay(ms);
        counter++;
      });
  }

  [UnityTest]
  public IEnumerator TasksExampleWithEnumeratorPasses() {
    Task task = Delay(500, "1. Wait for task to complete");
    yield return Tasks.WaitFor(task);
    Assert.AreEqual(counter, 1);

    task = Delay(500, "2. Wait for task to complete with error processing");
    yield return Tasks.WaitFor(task, (msg) => Debug.Log(msg));
    Assert.AreEqual(counter, 2);

    Debug.Log("3. All Done");
  }
  #else
  [UnityTest]
  public IEnumerator TasksExampleWithEnumeratorPasses() {
    Debug.LogWarning("Switch player settings to .NET version 4.0 or better");
  yield return null;
  }
  #endif
}
