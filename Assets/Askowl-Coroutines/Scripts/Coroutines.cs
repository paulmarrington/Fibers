using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public sealed class Coroutines {
  private readonly Queue<IEnumerator> actions = new Queue<IEnumerator>();
  private          MonoBehaviour      owner;

  [NotNull]
  public static Coroutines Sequential(MonoBehaviour owner, [NotNull] params IEnumerator[] actions) {
    return new Coroutines().Start(owningBehaviour: owner, actionList: actions);
  }

  [NotNull]
  private Coroutines Start(MonoBehaviour                  owningBehaviour,
                           [NotNull] params IEnumerator[] actionList) {
    owner = owningBehaviour;
    Queue(actionList: actionList);
    return this;
  }

  public void Queue([NotNull] params IEnumerator[] actionList) {
    bool restartProcess = actions.Count == 0;

    foreach (IEnumerator action in actionList) {
      actions.Enqueue(item: action);
    }

    if (restartProcess) {
      owner.StartCoroutine(routine: Process());
    }
  }

  [UsedImplicitly]
  public IEnumerator Completed() {
    while (actions.Count > 0) {
      yield return null;
    }
  }

  private IEnumerator Process() {
    while (actions.Count > 0) {
      yield return owner.StartCoroutine(routine: actions.Peek());

      actions.Dequeue();
    }
  }
}