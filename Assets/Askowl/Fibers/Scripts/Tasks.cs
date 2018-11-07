// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;
  using System.Collections;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using UnityEngine;

  /// <a href="">Convert Task activities to Coroutines to behave well with the rest of Unity</a> //#TBD#//
  public static class Tasks {
    /// <a href="">WaitFor is a coroutines step that will complete when the task is complete</a> //#TBD#//
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
      }
      else if (task.IsFaulted) {
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

    /// <a href="">WaitFor is a coroutines step that will complete when <see cref="Task{TResult}"/> is complete.</a> //#TBD#//
    public static IEnumerator
      WaitFor<T>(Task<T> task, Action<T> action, Action<string> error = null) {
      Task<T> done = null;
      task.ContinueWith(result => done = result);

      while (done == null) yield return null;

      if (!SendError(error, done)) action(done.Result);
    }
  }
}