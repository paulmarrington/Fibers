using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl.Fibers {
  public class Workers : LinkedList<Worker> { }

  public abstract class Worker {
    public Coroutines Coroutines;

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    protected internal abstract void OnUpdate(Coroutine coroutine);

    protected virtual bool InRange(Coroutines.Node node) => InRange(node.Item);

    protected virtual bool InRange(Coroutine coroutine) => true;

    protected internal abstract bool OnYield(Coroutine coroutine);

    protected internal virtual void OnFinished(Coroutine coroutine) { }
  }

  public class Worker<T> : Worker {
    protected Worker() {
      Coroutines          = new Coroutines {Name = $"{GetType().Name}:{typeof(T).Name}", InRange = InRange};
      OnYields[typeof(T)] = this;
      if (AddToUpdate) FiberController.UpdateWorkers.Add(this);
    }

    protected virtual bool AddToUpdate => false;

//    public virtual Yield Yield(T value) =>
//      new Yield(SetRange(value));

    protected virtual T SetRange(T value) => value;

    protected internal override void OnUpdate(Coroutine coroutine) {
      coroutine.Result(coroutine.Yield.Action(coroutine));
      if (coroutine.Yield.EndYieldCondition(coroutine)) OnFinished(coroutine);
    }

    protected internal override bool OnYield(Coroutine coroutine) {
      coroutine.Node.MoveTo(Coroutines);
      return true;
    }

    protected T Parameter(Coroutine coroutine) => coroutine.Yield.Parameter<T>();

    protected void Parameter(Coroutine coroutine, T value) { coroutine.Yield.Parameter(value); }

    protected internal override void OnFinished(Coroutine coroutine) => coroutine.Node.MoveBack();
  }
}