using UnityEngine;
using NUnit.Framework;
using System.Collections;

public sealed class CoroutinesExample : MonoBehaviour {
  private Coroutines auth;
  private string     result = "";
  private bool       buttonPressed;

  private void Start() { auth = Coroutines.Sequential(this, InitAuth()); }

  // ReSharper disable once UnusedMember.Global
  public void ButtonPressed() {
    if (buttonPressed) {
      Debug.LogWarning(message: "Coroutines Example only works once");
    } else {
      buttonPressed = true;
      auth.Queue(LoginAction(), CheckLoginAction());
    }
  }

  private void Update() {
    if (!buttonPressed) return;

    buttonPressed = false;
    auth.Queue(SendMessageAction(), TestResult());
  }

  private IEnumerator TestResult() {
    Debug.Log(message: "5. Check " + result);

    Assert.AreEqual(
      expected: "InitAuth LoginAction CheckLoginAction SendMessageAction",
      actual: result);

    yield return null;

    Debug.Log(message: "6. Done");
  }

  private IEnumerator InitAuth() {
    yield return After.Delay.seconds(seconds: 1);

    result += "InitAuth ";
    Debug.Log(message: "1. InitAuth");
  }

  private IEnumerator LoginAction() {
    yield return After.Delay.seconds(seconds: 1);

    result += "LoginAction ";
    Debug.Log(message: "2. LoginAction");
  }

  private IEnumerator CheckLoginAction() {
    yield return null;

    result += "CheckLoginAction ";
    Debug.Log(message: "3. CheckLoginAction");
  }

  private IEnumerator SendMessageAction() {
    yield return null;

    result += "SendMessageAction";
    Debug.Log(message: "4. SendMessageAction");
  }
}