using System;
using System.Collections;
using System.Collections.Generic;

namespace Askowl.Fibers {
  public class Workers : LinkedList<Worker> { }

  public abstract class Worker {
    public Instances Fibers = new Instances(), Recycled = new Instances();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    public void Process(Instances.Node node) {
      if (!Step(node)) {
        OnFinished(node);
        node.MoveTo(Recycled);
      }
    }

    private bool Step(Instances.Node node) {
      var coroutine = node.Item.Coroutine;
      if (!coroutine.MoveNext()) return false;

      object returnedResult = coroutine.Current;
      var    result         = returnedResult as Yield;
      if (result != null) return result.Worker.OnYield(result, node);

      var returnedResultType = returnedResult?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(returnedResult, node);
      }

      return OnYield(returnedResult, node);
    }

    protected abstract bool OnYield(object returnedResult, Instances.Node node);

    protected virtual void OnFinished(Instances.Node node) { }
  }

  public abstract class Worker<T> : Worker {
    private static Worker<T> workerInstance;

    protected static void Register(Worker<T> me) {
      workerInstance      = me;
      OnYields[typeof(T)] = me;
      Cue.Workers.Add(me);
    }

    internal static Yield<T> Instance(T value) =>
      new Yield<T> {Worker = workerInstance, Value = workerInstance.SetRange(value)};

    protected virtual T SetRange(T value) => value;

    protected T Data(Instance       instance)          => (T) instance.Data;
    protected T Data(Instance       instance, T value) => (T) (instance.Data = value);
    protected T Data(Instances.Node node)          => (T) node.Item.Data;
    protected T Data(Instances.Node node, T value) => (T) (node.Item.Data = value);

    internal Worker() { Fibers.InRange = InRange; }

    protected virtual bool InRange(Instance instance) => false;

    protected override bool OnYield(object returnedResult, Instances.Node node) =>
      OnYield(((Yield<T>) returnedResult).Value, node);

    protected virtual bool OnYield(T returnedResult, Instances.Node node) {
      node.MoveTo(Fibers);
      Data(node, returnedResult);
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
    public Worker Worker { get; internal set; }
    public T      Value;
  }
}