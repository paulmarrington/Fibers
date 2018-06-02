// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Askowl {
  /// <summary>
  /// Coroutines are great for making independant actions work efficiently with the minimum of waiting.
  /// But what if an action is made up of one or more coroutines that must be run sequentially.
  /// You could write code with lines of `yield return stepCoroutine()` or you could use this helper class.
  /// </summary>
  /// <remarks><a href="http://coroutines.marrington.net#coroutines-1">More...</a></remarks>
  public sealed class Coroutines {
    private readonly Queue<IEnumerator> queue = new Queue<IEnumerator>();
    private          MonoBehaviour      owner;

    /// <summary>
    /// Entry point to start and prime a sequential coroutines stream.
    /// </summary>
    /// <remarks><a href="http://coroutines.marrington.net#coroutinessequential">More...</a></remarks>
    /// <param name="owner">MonoBehaviour that owns the stream</param>
    /// <param name="actions">Zero or more `IEnumerator` returning coroutine components</param>
    /// <returns>Coroutines reference used to add more actions or wait on completion</returns>
    public static Coroutines Sequential(MonoBehaviour owner, params IEnumerator[] actions) {
      return new Coroutines().Start(owner, actions);
    }

    private Coroutines Start(MonoBehaviour owningBehaviour, params IEnumerator[] actionList) {
      owner = owningBehaviour;
      Queue(actionList);
      return this;
    }

    /// <summary>
    /// Queue additional actions to happen sequentially after all previously entered actions complete.
    /// </summary>
    /// <remarks><a href="http://coroutines.marrington.net#coroutinesqueue">More...</a></remarks>
    /// <param name="actions">Zero or more `IEnumerator` returning coroutine components</param>
    public void Queue(params IEnumerator[] actions) {
      bool restartProcess = queue.Count == 0;

      for (var index = 0; index < actions.Length; index++) queue.Enqueue(actions[index]);

      if (restartProcess) owner.StartCoroutine(Process());
    }

    /// <summary>
    /// Wait for all actions enqueued to complete
    /// </summary>
    /// <remarks><a href="http://coroutines.marrington.net#coroutinescompleted">More...</a></remarks>
    /// <returns>IEnumerator</returns>
    
    public IEnumerator Completed() {
      while (queue.Count > 0) yield return null;
    }

    private IEnumerator Process() {
      while (queue.Count > 0) {
        yield return owner.StartCoroutine(queue.Peek());

        queue.Dequeue();
      }
    }
  }
}