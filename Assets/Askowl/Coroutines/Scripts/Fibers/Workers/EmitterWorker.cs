using System;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Emitter(Emitter       emitter) => EmitterWorker.Instance(emitter);
    public static Yield Emitter<T>(Emitter<T> emitter) => EmitterWorker<T>.Instance(emitter);
  }

  public class EmitterWorker : Worker<Emitter> {
    static EmitterWorker() { Register(new EmitterWorker(), processOnUpdate: false); }

    protected override bool OnYield(Yield<Emitter> yield, Instances.Node node) {
      var emitter = yield.Data;
      base.OnYield(emitter, node);
      emitter.Subscribe(new Observer {Worker = this, Node = node, Yield = yield});
      return true;
    }

    private struct Observer : IObserver {
      public Yield<Emitter> Yield;
      public EmitterWorker  Worker;
      public Instances.Node Node;

      public void OnNext() {
        if (Yield.EndRepeatCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Node); }
    }
  }

  public class EmitterWorker<T> : Worker<Emitter<T>> {
    static EmitterWorker() { Register(new EmitterWorker<T>(), processOnUpdate: false); }

    protected override bool OnYield(Yield<Emitter<T>> yield, Instances.Node node) {
      var emitter = yield.Data;
      base.OnYield(emitter, node);
      emitter.Subscribe(new Observer<T> {Worker = this, Node = node, Yield = yield});
      return true;
    }

    private struct Observer<T> : IObserver<T> {
      public Yield<Emitter<T>> Yield;
      public EmitterWorker<T>  Worker;
      public Instances.Node    Node;

      public void OnError(Exception error) { }

      public void OnNext(T value) {
        if (Yield.EndRepeatCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Node); }
    }
  }
}