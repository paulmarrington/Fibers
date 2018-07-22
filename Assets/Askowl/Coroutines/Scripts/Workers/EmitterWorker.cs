namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Emitter(Emitter emitter) => EmitterWorker.Instance(emitter);
  }

  public class EmitterWorker : Worker<Emitter> {
    static EmitterWorker() { Register(new EmitterWorker(), processOnUpdate: false); }

    protected override bool OnYield(Yield<Emitter> yield, Instances.Node node) {
      var emitter = yield.Value;
      base.OnYield(emitter, node);
      emitter.Subscribe(new Observer {Worker = this, Node = node, Yield = yield});
      return true;
    }

    private struct Observer : IObserver {
      public Yield<Emitter> Yield;
      public EmitterWorker  Worker;
      public Instances.Node Node;

      public void OnNext() {
        if (Yield.RepeatCondition()) OnCompleted();
      }

      public void OnCompleted() { Worker.OnFinished(Node); }
    }
  }
}