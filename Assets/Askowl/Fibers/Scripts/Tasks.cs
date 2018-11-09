// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System.Threading.Tasks;

  /// <a href="">Convert Task activities to Coroutines to behave well with the rest of Unity</a> //#TBD#//
  public static class Tasks {
    public static Emitter EmitOnComplete(Task task) {
      var emitter = Emitter.Instance;

      void action(Task _) {
        emitter.Fire();
        emitter.Dispose();
      }

      task.ContinueWith(action);
      return emitter;
    }
  }
}