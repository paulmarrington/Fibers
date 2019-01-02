// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;

namespace Askowl {
  using System.Threading.Tasks;

  /// <a href="">Convert Task activities to Coroutines to behave well with the rest of Unity</a> //#TBD#//
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Task task) =>
      AddAction(
        _ => {
          var emitter = Emitter.Instance;

          void action(Task __) {
            emitter.Fire();
            emitter.Dispose();
          }

          task.ContinueWith(action);
          WaitFor(emitter);
        }, "WaitFor(Task)");

    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Func<Fiber, Task> getter) => AddAction(_ => WaitFor(getter(this)), "WaitFor(Task)");
  }
}