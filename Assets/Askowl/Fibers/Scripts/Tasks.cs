// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

#if (!NET_2_0 && !NET_2_0_SUBSET)
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;

#endif

namespace Askowl {
  using JetBrains.Annotations;

  /// <summary>
  /// Convert Task activities to Coroutines to behave well with the rest of Unity
  /// </summary>
  /// <remarks><a href="http://coroutines.marrington.net#tasks">More...</a></remarks>
  public sealed class Tasks {
#if (!NET_2_0 && !NET_2_0_SUBSET)
    /// <summary>
    /// WaitFor is a coroutines step that will complete when the task is complete.
    /// </summary>
    /// <remarks><a href="http://coroutines.marrington.net#taskswaitfor">More...</a></remarks>
    /// <param name="task">C# Task</param>
    /// <param name="error">Action to call with a single string parameter on an error in the task</param>
    /// <returns>Coroutine enabler</returns>
    public static IEnumerator WaitFor(Task task, Action<string> error = null) {
      Task done = null;
      task.ContinueWith(result => done = result);

      while (done == null) yield return null;

      SendError(error, done);
    }

    private static bool SendError(Action<string> error, Task task) {
      if (task.IsCanceled) {
        SendError(error, "Cancelled");
        return true;
      } else if (task.IsFaulted) {
        string fault = "";

        // ReSharper disable once PossibleNullReferenceException
        ReadOnlyCollection<Exception> exceptions = task.Exception.Flatten().InnerExceptions;

        for (var index = 0; index < exceptions.Count; index++) fault += exceptions[index] + "\n";

        SendError(error, fault);
        return true;
      }

      return false;
    }

    private static void SendError(Action<string> error, string message) {
      if (error == null) error = Debug.LogError;

      error(message);
    }

    /// <summary>
    /// WaitFor is a coroutines step that will complete when Task&lt;T> is complete.
    /// </summary>
    /// <remarks><a href="http://coroutines.marrington.net#taskswaitfort">More...</a></remarks>
    /// <param name="task">C# Task</param>
    /// <param name="action">Action to call on success, passing result in T</param>
    /// <param name="error">Action to call with a single string parameter on an error in the task</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>Coroutine enabler</returns>
    public static IEnumerator
      WaitFor<T>(Task<T> task, Action<T> action, Action<string> error = null) {
      Task<T> done = null;
      task.ContinueWith(result => done = result);

      while (done == null) yield return null;

      if (!SendError(error, done)) action(done.Result);
    }
#endif
  }
}