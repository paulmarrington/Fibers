using System;
using System.Collections;
using System.Collections.Generic;
using Askowl.Fibers;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleEnums;
using Object = System.Object;

namespace Askowl {
  public class FiberController : MonoBehaviour {
    private void Update() { UpdateAllWorkers(); }

    private static void UpdateAllWorkers() {
      for (var worker = Cue.Workers.First; worker != null; worker = worker.Next) {
        var fibers = worker.Item.Fibers;

        var fiber = fibers.First;

        while (fiber?.InRange == true) {
          var next = fiber.Next;
          worker.Item.Process(fiber);
          fiber = next;
        }
      }
    }
  }
}

namespace Askowl.Fibers {
  public static class Cue {
    public static Yield NewCoroutine(Func<IEnumerator> fiberGenerator,
                                     Instances.Node    parentNode = null) {
      if (!Generators.ContainsKey(fiberGenerator)) {
        Generators[fiberGenerator] = new InstanceWorker {GeneratorFunction = fiberGenerator};
      }

      return new Yield<int> {Worker = Generators[fiberGenerator].StartInstance(parentNode)};
    }

    public static Yield<int> Frames(int framesToSkip) {
      var frames = FramesWorker.Instance(framesToSkip);
      return frames;
    }

    internal static readonly Dictionary<Func<IEnumerator>, InstanceWorker> Generators
      = new Dictionary<Func<IEnumerator>, InstanceWorker>();

    internal static readonly Workers Workers = new Workers();
  }

  public struct Instance {
    public IEnumerator    Coroutine;
    public Instances.Node ownerNode;
    public Instances.Node parentNode;

    public T Data<T>() => (T)data;
    public void Data<T>(T value) { data = value;}
    private object         data;
  }

  public class Instances : LinkedList<Instance> { }

  public class Workers : LinkedList<Worker> { }

  public interface Yield {
    Worker Worker { get; }
  }

  public struct Yield<T> : Yield {
    public Worker Worker { get; internal set; }
    public T Value;
  }

  #region Workers
  #region Worker Support
  public abstract class Worker {
    public Instances Fibers = new Instances(), Recycled = new Instances();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    public void Process(Instances.Node node) {
      if (!Step(node)) {
        AllDone();
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

    protected virtual void AllDone() { }
  }

  public abstract class Worker<T> : Worker {
    protected static void Register(Worker<T> me) {
      OnYields[typeof(T)] = me;
      Cue.Workers.Add(me);
    }

    protected override bool OnYield(object returnedResult, Instances.Node node) =>
      OnYield(((Yield<T>) returnedResult).Value, node);

    protected virtual bool OnYield(T returnedResult, Instances.Node node) => true;
  }
  #endregion

  #region Workers
  internal class InstanceWorker : Worker<Instance> {
    internal Func<IEnumerator> GeneratorFunction;

    internal InstanceWorker() { Register(this); }

    public Worker StartInstance(Instances.Node parentNode) {
      if (Recycled.Empty) {
        var instance = new Instance();
        Recycled.Add(instance);
        instance.Coroutine = FiberMonitor(GeneratorFunction(), instance);
      }

      var node = Recycled.First.MoveTo(Fibers);
      node.Item.parentNode = parentNode;
      node.Item.ownerNode  = node;
      return this;
    }

    private IEnumerator FiberMonitor(IEnumerator fiber, Instance instance) {
      try {
        while (fiber.MoveNext()) yield return fiber.Current;
      } finally {
        instance.ownerNode.MoveTo(Recycled);
        instance.parentNode?.MoveTo(instance.parentNode.lastOwner);
      }
    }
  }

  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private Instances waiting = new Instances();

    static IEnumeratorWorker() { Register(new IEnumeratorWorker()); }

    protected override bool OnYield(Func<IEnumerator> returnedResult, Instances.Node node) {
      node.MoveTo(waiting); // moved back when InstanceWorker is done
      Cue.NewCoroutine(fiberGenerator: returnedResult, parentNode: node);
      return true;
    }
  }

  public class FramesWorker : Worker<int> {
    private static FramesWorker framesWorker = new FramesWorker();
    static FramesWorker() { Register(framesWorker); }

    public FramesWorker() { Fibers.InRange = (instance) => instance.Data<int>() > Time.frameCount; }

    public static Yield<int> Instance(int framesToSkip) =>
      new Yield<int> {Worker = framesWorker, Value = Time.frameCount + framesToSkip};

    protected override bool OnYield(int framesToSkip, Instances.Node node) {
      node.MoveTo(Fibers);
      node.Item.Data(framesToSkip);

      Debug.Log(
        $"**** FiberController:166 To Be Done -- Add comparator and linked list value"); //#DM#// 19 Jul 2018

      return true;
    }
  }
  #endregion
  #endregion
}