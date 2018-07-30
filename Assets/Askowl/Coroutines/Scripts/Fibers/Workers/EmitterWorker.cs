using System;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Emitter(Emitter       emitter) => EmitterWorker.Instance.Yield(emitter);
    public static Yield Emitter<T>(Emitter<T> emitter) => EmitterWorker<T>.Instance.Yield(emitter);
  }

  public class EmitterWorker : Worker<Emitter> {
    public static EmitterWorker Instance = new EmitterWorker();

    protected internal override bool OnYield(Coroutine coroutine) {
      var emitter = Parameter(coroutine);
      coroutine.Node.MoveTo(Fibers);
      emitter.Subscribe(new Observer {Worker = this, Coroutine = coroutine});
      return true;
    }

    private struct Observer : IObserver {
      public EmitterWorker Worker;
      public Coroutine     Coroutine;

      public void OnNext() {
        if (Coroutine.Yield.EndYieldCondition(Coroutine)) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Coroutine); }
    }
  }

  public class EmitterWorker<T> : Worker<Emitter<T>> {
    public static EmitterWorker<T> Instance = new EmitterWorker<T>();

    protected internal override bool OnYield(Coroutine coroutine) {
      var emitter = Parameter(coroutine);
      coroutine.Node.MoveTo(Fibers);
      emitter.Subscribe(new Observer {Worker = this, Coroutine = coroutine});
      return true;
    }

    private struct Observer : IObserver<T> {
      public EmitterWorker<T> Worker;
      public Coroutine        Coroutine;

      public void OnError(Exception error) { }

      public void OnNext(T value) {
        Coroutine.Fiber.Result(value);
        if (Coroutine.Yield.EndYieldCondition(Coroutine)) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Coroutine); }
    }
  }
}