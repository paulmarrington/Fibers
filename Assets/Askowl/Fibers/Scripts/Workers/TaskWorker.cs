// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;

namespace Askowl {
  using System.Threading.Tasks;

  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href="http://bit.ly/2RcQM7a">Convert Task activities to Coroutines to behave well with the rest of Unity</a>
    public Fiber WaitFor(Task task) =>
      AddAction(
        _ => {
          var emitter = Emitter.SingleFireInstance;
          void action(Task __) => emitter.Fire();
          task.ContinueWith(action);
          EmitterWorker.Instance.Load(this, emitter);
        }, "WaitFor(Task)");

    /// <a href="http://bit.ly/2RcQM7a">Convert Task activities to Coroutines to behave well with the rest of Unity - value passed by function return</a>
    public Fiber WaitFor(Func<Fiber, Task> getter) =>
      AddAction(
        _ => {
          var emitter = Emitter.SingleFireInstance;
          void action(Task __) => emitter.Fire();
          getter(this).ContinueWith(action);
          EmitterWorker.Instance.Load(this, emitter);
        }, "WaitFor(Task)");
  }
}