using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class Workers : LinkedList<Worker> { }

  public abstract class Worker {
    public Fibers Fibers = new Fibers();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    protected internal abstract void OnUpdate(Fiber fiber);

    protected internal abstract bool OnYield(object returnedResult, Fiber fiber);

    protected virtual void OnFinished(Fiber fiber) { }
  }

  public class Worker<T> : Worker {
    private static Worker<T> workerInstance;

    protected static void Register(Worker<T> me, bool processOnUpdate = true) {
      workerInstance      = me;
      OnYields[typeof(T)] = me;
      if (processOnUpdate) WaitFor.Workers.Add(me);
    }

    internal static Yield<T> Instance(T value) =>
      new Yield<T>(worker: workerInstance, data: workerInstance.SetRange(value));

    protected virtual T SetRange(T value) => value;

//    protected T        Data(Instance<T>    instance)          => ((Yield<T>) instance.Data).Value;
//    protected void     Data(Instance<T>    instance, T value) => ((Yield<T>) instance.Data).Value = value;
//    protected Yield<T> Yield(Instance<T>   instance)             => (Yield<T>) instance.Data;
//    protected Yield<T> Data(Instances.Node node)                 => ((Yield<T>) node.Item.Data);
//    protected void     Data(Instances.Node node, Yield<T> value) => node.Item.Data = value;

    internal Worker() { Fibers.InRange = InRange; }

    protected virtual bool InRange(Fiber fiber) => false;

    protected internal override void OnUpdate(Fiber fiber) {
      if (repeatsdone) OnFinished(fiber);
    }

    protected virtual bool OnYield(Yield<T> yield, Fiber fiber) {
      fiber.Node.MoveTo(Fibers);
      Data(node, yield);
      return true;
    }

    protected internal override bool OnYield(object returnedResult, Fiber fiber) =>
      OnYield((Yield<T>) returnedResult, fiber);

    protected override void OnFinished(Fiber fiber) => fiber.Node.MoveBack();
  }
}