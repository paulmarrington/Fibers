using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class Workers : LinkedList<Worker> { }

  public abstract class Worker {
    public Instances Fibers = new Instances();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    protected internal abstract void OnUpdate(Instances.Node node);

    protected internal abstract bool OnYield(object returnedResult, Instances.Node node);

    protected virtual void OnFinished(Instances.Node node) { }
  }

  public class Worker<T> : Worker {
    private static Worker<T> workerInstance;

    protected static void Register(Worker<T> me, bool processOnUpdate = true) {
      workerInstance      = me;
      OnYields[typeof(T)] = me;
      if (processOnUpdate) WaitFor.Workers.Add(me);
    }

    internal static Yield<T> Instance(T value) =>
      new Yield<T>(worker: workerInstance, value: workerInstance.SetRange(value));

    internal static Yield<T> Instance(Func<bool> endCondition = null) =>
      new Yield<T>(worker: workerInstance, value: default(T), endCondition: endCondition);

    protected virtual T SetRange(T value) => value;

    protected T        Data(Instance       instance)             => ((Yield<T>) instance.Data).Value;
    protected Yield<T> Yield(Instance      instance)             => (Yield<T>) instance.Data;
    protected Yield<T> Data(Instances.Node node)                 => ((Yield<T>) node.Item.Data);
    protected void     Data(Instances.Node node, Yield<T> value) => node.Item.Data = value;

    internal Worker() { Fibers.InRange = InRange; }

    protected virtual bool InRange(Instance instance) => false;

    protected internal override void OnUpdate(LinkedList<Instance>.Node node) {
      if (Data(node).RepeatCondition()) OnFinished(node);
    }

    protected internal override bool OnYield(object returnedResult, Instances.Node node) =>
      OnYield((Yield<T>) returnedResult, node);

    protected virtual bool OnYield(Yield<T> yield, Instances.Node node) {
      node.MoveTo(Fibers);
      Data(node, yield);
      return true;
    }

    protected override void OnFinished(LinkedList<Instance>.Node node) => node.MoveBack();
  }

  public struct Instance {
    public IEnumerator    Coroutine;
    public Instances.Node ownerNode;
    public Instances.Node parentNode;
    public object         Data;
  }

  public class Instances : LinkedList<Instance> { }

  public interface Yield {
    Worker Worker { get; }
  }

  public struct Yield<T> : Yield {
    public Worker     Worker { get; internal set; }
    public T          Value;
    public Func<bool> RepeatCondition, EndCondition;

    public Yield(Worker worker, T value, Func<bool> endCondition = null) {
      Worker       = worker;
      Value        = value;
      EndCondition = RepeatCondition         = () => true;
      if (endCondition != null) EndCondition = endCondition;
    }

    public Yield<T> Repeat(int countdown) {
      RepeatCondition = () => (countdown-- == 0);
      return this;
    }

    public Yield<T> Until(Func<bool> condition) {
      RepeatCondition = condition;
      return this;
    }

    public Yield<T> While(Func<bool> condition) {
      RepeatCondition = () => !condition();
      return this;
    }
  }
}