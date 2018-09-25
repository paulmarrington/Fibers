using System;

namespace Askowl.Fibers {
  public partial class Fiber {
    public Fiber Emitter(Emitter emitter) => EmitterWorker.Load(this, emitter);

    public Fiber Emitter<T>(Emitter<T> emitter, Action<T> actionOnResult) =>
      EmitterWorker<T>.Load(this, emitter, actionOnResult);
  }

  public class EmitterWorker : Worker<Emitter> {
    static EmitterWorker() { new EmitterWorker().Prepare("Fiber Emitter Worker"); }

    protected override Emitter Parse(Emitter emitter) {
      emitter.Subscribe(new Observer {Worker = this});
      return emitter;
    }

    private struct Observer : IObserver {
      public EmitterWorker Worker;

      public void OnNext() {
        if (Worker.fiber.EndCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnComplete(); }
    }
  }

  public class EmitterWorker<T> : Worker<Emitter<T>> {
    static EmitterWorker() { new EmitterWorker().Prepare("Fiber Emitter T Worker"); }

    protected override Emitter<T> Parse(Emitter<T> emitter, object[] more) {
      emitter.Subscribe(new Observer {Worker = this, actionOnResult = (Action<T>) more[0]});
      return emitter;
    }

    private struct Observer : IObserver<T> {
      public EmitterWorker<T> Worker;
      public Action<T>        actionOnResult;

      public void OnError(Exception error) { }

      public void OnNext(T value) {
        actionOnResult(value);
        if (Worker.fiber.EndCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnComplete(); }
    }
  }
}