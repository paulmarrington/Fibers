using System;
using System.Collections;
using System.Collections.Generic;
using Askowl.Fibers;
using UnityEngine;
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
                                     Instances.Node    parent = null) {
      if (!Generators.ContainsKey(fiberGenerator)) {
        Generators[fiberGenerator] = new InstanceWorker {GeneratorFunction = fiberGenerator};
      }

      return new Yield<int> {Worker = Generators[fiberGenerator].StartInstance(parent)};
    }

    public static Yield<int> Frames(int framesToSkip) {
      var frames = FramesWorker.Instance(framesToSkip);
      return frames;
    }

    internal static readonly Dictionary<Func<IEnumerator>, InstanceWorker> Generators
      = new Dictionary<Func<IEnumerator>, InstanceWorker>();

    internal static readonly Workers Workers = new Workers();
  }

  public class Instance {
    public IEnumerator    Coroutine;
    public Instances.Node parent;
  }

  public class Instances : LinkedList<Instance> { }

  public class Workers : LinkedList<Worker> { }

  public interface Yield {
    Worker Worker { get; }
  }

  public struct Yield<T> : Yield {
    public Worker Worker { get; internal set; }
    public T      Value;
  }

  #region Workers
  #region Worker Support
  public abstract class Worker {
    public Instances Fibers = new Instances(), Recycled = new Instances();

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    public void Process(Instances.Node instance) {
      if (!Step(instance)) {
        AllDone();
        instance.MoveTo(Recycled);
      }
    }

    private bool Step(Instances.Node instance) {
      var coroutine = instance.Item.Coroutine;
      if (!coroutine.MoveNext()) return false;

      object returnedResult = coroutine.Current;
      var    result         = returnedResult as Yield;
      if (result != null) return result.Worker.OnYield(result, instance);

      var returnedResultType = returnedResult?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(returnedResult, instance);
      }

      return OnYield(returnedResult, instance);
    }

    protected abstract bool OnYield(object returnedResult, Instances.Node instance);

    protected virtual void AllDone() { }
  }

  public abstract class Worker<T> : Worker {
    protected static void Register(Worker<T> me) {
      OnYields[typeof(T)] = me;
      Cue.Workers.Add(me);
    }

    protected override bool OnYield(object returnedResult, Instances.Node instance) =>
      OnYield(((Yield<T>) returnedResult).Value, instance);

    protected abstract bool OnYield(T returnedResult, Instances.Node instance);
  }
  #endregion

  #region Workers
  internal class InstanceWorker : Worker<Instance> {
    internal Func<IEnumerator> GeneratorFunction;

    private Instances.Node instance;

    internal InstanceWorker() { Register(this); }

    public Worker StartInstance(Instances.Node parent) {
      if (Recycled.Empty) {
        Recycled.Add(new Instance() {
          Coroutine = FiberMonitor(GeneratorFunction())
        });
      }

      instance             = Recycled.First.MoveTo(Fibers);
      instance.Item.parent = parent;
      return this;
    }

    private IEnumerator FiberMonitor(IEnumerator fiber) {
      try {
        while (fiber.MoveNext()) yield return fiber.Current;
      } finally {
        instance.MoveTo(Recycled);
        instance.Item.parent?.MoveTo(instance.Item.parent.lastOwner);
      }
    }

    protected override bool OnYield(Instance returnedResult, Instances.Node instance) =>
      true;
  }

  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private Instances waiting = new Instances();

    static IEnumeratorWorker() { Register(new IEnumeratorWorker()); }

    protected override bool OnYield(Func<IEnumerator> returnedResult, Instances.Node instance) {
      instance.MoveTo(waiting);
      Cue.NewCoroutine(fiberGenerator: returnedResult, parent: instance);
      return true;
    }
  }

  public class FramesWorker : Worker<int> {
    private static FramesWorker framesWorker = new FramesWorker();
    static FramesWorker() { Register(framesWorker); }

    public FramesWorker() {
//      Fibers.InRange = ()
    }

    public static Yield<int> Instance(int framesToSkip) =>
      new Yield<int> {Worker = framesWorker, Value = framesToSkip};

    protected override bool OnYield(int framesToSkip, Instances.Node instance) {
      instance.MoveTo(Fibers);

      Debug.Log(
        $"**** FiberController:166 To Be Done -- Add comparator and linked list value"); //#DM#// 19 Jul 2018

      return true;
    }
  }
  #endregion
  #endregion
}