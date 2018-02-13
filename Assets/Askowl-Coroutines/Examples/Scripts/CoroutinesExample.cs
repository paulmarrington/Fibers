using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using NUnit.Framework.Internal;

public class CoroutinesExample : MonoBehaviour {
  Coroutines auth = null;
  string result = "";
  bool buttonPressed = false;

  void Start() {
    auth = Coroutines.Sequential(this, InitAuth());
  }

  public void ButtonPressed() {
    if (buttonPressed) {
      Debug.LogWarning("Coroutines Example only works once");
    } else {
      buttonPressed = true;
      auth.Queue(LoginAction(), CheckLoginAction());
    }
  }

  void Update() {
    if (buttonPressed) {
      buttonPressed = false;
      auth.Queue(SendMessageAction(), TestResult());
    }
  }

  IEnumerator TestResult() {
    Debug.Log("5. Check " + result);
    Assert.AreEqual("InitAuth LoginAction CheckLoginAction SendMessageAction", result);
    yield return null;
    Debug.Log("6. Done");
  }

  IEnumerator InitAuth() {
    yield return After.Delay.seconds(1);
    result += "InitAuth ";
    Debug.Log("1. InitAuth");
  }

  IEnumerator LoginAction() {
    yield return After.Delay.seconds(1);
    result += "LoginAction ";
    Debug.Log("2. LoginAction");
  }

  IEnumerator CheckLoginAction() {
    yield return null;
    result += "CheckLoginAction ";
    Debug.Log("3. CheckLoginAction");
  }

  IEnumerator SendMessageAction() {
    yield return null;
    result += "SendMessageAction";
    Debug.Log("4. SendMessageAction");
  }


}
