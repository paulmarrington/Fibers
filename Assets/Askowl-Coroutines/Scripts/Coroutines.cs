using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coroutines {
  Queue<IEnumerator> actions = new Queue<IEnumerator> ();
  MonoBehaviour owner;

  public static Coroutines Sequential(MonoBehaviour owner, params IEnumerator[] actions) {
    return new Coroutines ().start(owner, actions);
  }

  Coroutines start(MonoBehaviour owner, params IEnumerator[] actions) {
    this.owner = owner;
    Queue(actions);
    return this;
  }

  public void Queue(params IEnumerator[] actions) {
    bool restartProcess = (this.actions.Count == 0);
    foreach (IEnumerator action in actions) {
      this.actions.Enqueue(action);
    }
    if (restartProcess) {
      owner.StartCoroutine(Process());
    }
  }

  public IEnumerator Completed() {
    while (actions.Count > 0) {
      yield return null;
    }
  }

  IEnumerator Process() {
    while (actions.Count > 0) {
      yield return owner.StartCoroutine(actions.Peek());
      actions.Dequeue();
    }
  }
}
