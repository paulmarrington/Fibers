using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static Worker<T> Instance = new Worker<T>();

    protected Worker() {
      Fibers.InRange      = InRange;
      Fibers.Name         = $"{GetType().Name}:{typeof(T).Name}";
      OnYields[typeof(T)] = this;
      if (AddToUpdate) FiberController.UpdateWorkers.Add(this);
    }

    protected virtual bool AddToUpdate => false;

    public virtual Yield Yield(T value) =>
      new Yield(worker: this, yieldParam: SetRange(value));

    protected virtual T SetRange(T value) => value;

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