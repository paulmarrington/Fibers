using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class Workers : LinkedList<Worker> { }

  public abstract class Worker {
    public Fibers Fibers = new Fibers();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    protected internal abstract void OnUpdate(Fiber fiber);

    protected internal abstract bool OnYield(Fiber fiber);

    protected internal virtual void OnFinished(Fiber fiber) { }
  }

  public class Worker<T> : Worker {
    private static Worker<T> workerInstance;

    protected static void Register(Worker<T> me, bool processOnUpdate = true) {
      me.Fibers.Name      = $"{me.GetType().Name}:{typeof(T).Name}";
      workerInstance      = me;
      OnYields[typeof(T)] = me;
      if (processOnUpdate) FiberController.UpdateWorkers.Add(me);
    }

    internal static Yield Instance(T value) =>
      new Yield(worker: workerInstance, yieldParam: workerInstance.SetRange(value));

    protected virtual T SetRange(T value) => value;

    internal Worker() { Fibers.InRange = InRange; }

    protected virtual bool InRange(Fiber fiber) => true;

    protected internal override void OnUpdate(Fiber fiber) {
      if (fiber.Yield.EndYieldCondition()) OnFinished(fiber);
    }

    protected internal override bool OnYield(Fiber fiber) {
      fiber.Node.MoveTo(Fibers);
      return true;
    }

    protected T Parameter(Fiber fiber) { return fiber.Yield.Parameter<T>(); }

    protected internal override void OnFinished(Fiber fiber) => fiber.Node.MoveBack();
  }
}