using System;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Emitter(Emitter       emitter) => EmitterWorker.Instance.Yield(emitter);
    public static Yield Emitter<T>(Emitter<T> emitter) => EmitterWorker<T>.Instance.Yield(emitter);
  }

  public class EmitterWorker : Worker<Emitter> {
//    public static EmitterWorker Instance = new EmitterWorker {processOnUpdate = false};

    protected internal override bool OnYield(Fiber fiber) {
      var emitter = Parameter(fiber);
      fiber.Node.MoveTo(Fibers);
      emitter.Subscribe(new Observer {Worker = this, Fiber = fiber});
      return true;
    }

    private struct Observer : IObserver {
      public EmitterWorker Worker;
      public Fiber         Fiber;

      public void OnNext() {
        if (Fiber.Yield.EndYieldCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Fiber); }
    }
  }

  public class EmitterWorker<T> : Worker<Emitter<T>> {
//    public static EmitterWorker<T> Instance = new EmitterWorker<T> {processOnUpdate = false};

    protected internal override bool OnYield(Fiber fiber) {
      var emitter = Parameter(fiber);
      fiber.Node.MoveTo(Fibers);
      emitter.Subscribe(new Observer {Worker = this, Fiber = fiber});
      return true;
    }

    private struct Observer : IObserver<T> {
      public EmitterWorker<T> Worker;
      public Fiber            Fiber;

      public void OnError(Exception error) { }

      public void OnNext(T value) {
        Fiber.Result(value);
        if (Fiber.Yield.EndYieldCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Fiber); }
    }
  }
}