#if UNITY_EDITOR && AskowlCoroutines
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using Askowl;
using UnityEngine.TestTools;

/// <summary>`Coroutines` test framework</summary>
public class CoroutinesExample {
  /// <summary>
  /// Generates MonoBehaviour to check `Coroutines`
  /// </summary>
  /// <returns></returns>
  [UnityTest, Timeout(10000)]
  public IEnumerator TestCoroutines() {
    yield return new MonoBehaviourTest<CoroutinesExampleMonoBehaviour>();
  }

  private class CoroutinesExampleMonoBehaviour : MonoBehaviour, IMonoBehaviourTest {
    private Coroutines auth;
    private string     result;
    private int        stage;

    public bool IsTestFinished { get; set; }

    public IEnumerator Start() {
      stage  = 0;
      result = "";

      auth = Coroutines.Sequential(this, InitAuth());

      auth.Queue(LoginAction(), CheckLoginAction());
      yield return auth.Completed();

      auth.Queue(SendMessageAction(), TestResult());
      yield return auth.Completed();

      Assert.AreEqual(6, stage);
      IsTestFinished = true;
    }

    private IEnumerator TestResult() {
      Debug.Log(message: "5. Check " + result);
      stage = 5;

      Assert.AreEqual(
        expected: "InitAuth LoginAction CheckLoginAction SendMessageAction",
        actual: result);

      yield return null;

      Debug.Log(message: "6. Done");
      stage = 6;
    }

    private IEnumerator InitAuth() {
      yield return new WaitForSeconds(1);

      result += "InitAuth ";
      Debug.Log(message: "1. InitAuth");
      stage = 1;
    }

    private IEnumerator LoginAction() {
      yield return new WaitForSeconds(1);

      result += "LoginAction ";
      Debug.Log(message: "2. LoginAction");
      stage = 2;
    }

    private IEnumerator CheckLoginAction() {
      yield return null;

      result += "CheckLoginAction ";
      Debug.Log(message: "3. CheckLoginAction");
      stage = 3;
    }

    private IEnumerator SendMessageAction() {
      yield return null;

      result += "SendMessageAction";
      Debug.Log(message: "4. SendMessageAction");
      stage = 4;
    }
  }
}

#endif