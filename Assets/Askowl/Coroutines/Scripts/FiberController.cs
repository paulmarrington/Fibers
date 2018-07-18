using System;
using System.Collections;
using System.Collections.Generic;
using Askowl.Fibers;
using UnityEngine;

namespace Askowl {
  public class FiberController : MonoBehaviour {
    private void Update() { UpdateAllWorkers(); }

    private static void UpdateAllWorkers() {
      for (var worker = Cue.Workers.First; Cue.Workers.InRange; worker = Cue.Workers.Next) {
        ProcessFibersInRange(worker: worker);
      }
    }

    private static void ProcessFibersInRange(Worker worker) {
      for (var fiber = worker.Fibers.First; worker.Fibers.InRange; fiber = worker.Fibers.Next) {
        worker.Process(fiber);
      }
    }
  }
}

namespace Askowl.Fibers {
  public static class Cue {
    private static InstanceWorker instanceWorker = new InstanceWorker();

    public static Worker Start(Func<IEnumerator> fiberGenerator, Instance parent = null) {
      if (!Generators.ContainsKey(fiberGenerator)) {
        Generators[fiberGenerator] = new Generator {GeneratorFunction = fiberGenerator};
      }

      Generators[fiberGenerator].StartInstance(parent);
      return instanceWorker;
    }

    internal class GeneratorsClass : Dictionary<Func<IEnumerator>, Generator> { }

    internal static readonly GeneratorsClass    Generators = new GeneratorsClass();
    internal static readonly LinkedList<Worker> Workers    = new LinkedList<Worker>(false);
  }

  #region Workers
  #region Worker Support
  public abstract class Worker {
    public LinkedList<Instance> Fibers;
    public LinkedList<Instance> Recycled;

    protected static Dictionary<Type, Worker> OnYields = new Dictionary<Type, Worker>();

    public void Process(Instance instance) {
      Debug.Log($"**** FiberController:53 To Be Done!!! Fibers nor Recycled set from ready");//#DM#// 18 Jul 2018
      if (instance.running && !Step(instance)) {
        AllDone();
        instance.running = false;
      }
    }

    private bool Step(Instance fiber) {
      if (!fiber.Coroutine.MoveNext()) return false;

      object returnedResult = fiber.Coroutine.Current;
      var    worker         = returnedResult as Worker;
      if (worker != null) return OnYield(worker);

      var returnedResultType = returnedResult?.GetType();

      if ((returnedResultType != null) && OnYields.ContainsKey(returnedResultType)) {
        return OnYields[returnedResultType].OnYield(returnedResult);
      }

      return OnYield(returnedResult);
    }

    protected virtual bool OnYield(Worker returnedResult) {
      Debug.Log($"**** FiberController:71 To Be Done!!!"); //#DM#// 18 Jul 2018
      return true;
    }

    protected abstract bool OnYield(object returnedResult);

    protected virtual void AllDone() { }
  }

  public abstract class Worker<T> : Worker {
    protected static void Register(Worker<T> me) { OnYields[typeof(T)] = me; }

    protected override bool OnYield(object returnedResult) => OnYield((T) returnedResult);

    protected abstract bool OnYield(T returnedResult);
  }
  #endregion

  #region Workers
  internal class InstanceWorker : Worker<Instance> {
    protected override bool OnYield(Instance returnedResult) => true;
  }
  #endregion

  #region Special Workers
  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private LinkedList<Instance> waiting = new LinkedList<Instance>(false);

    static IEnumeratorWorker() { Register(new IEnumeratorWorker()); }

    protected override bool OnYield(Func<IEnumerator> returnedResult) {
      Fibers.MoveTo(waiting);
      var lastDone = Fibers.Previous;
      Cue.Start(returnedResult);

      Debug.Log(
        $"**** FiberController:110 To Be Done -- Restore waiting thread when done!!!"); //#DM#// 18 Jul 2018

      return true;
    }
  }
  #endregion
  #endregion

  #region Generators
  internal class Generator {
    internal Func<IEnumerator> GeneratorFunction;

    private LinkedList<Instance> ready = new LinkedList<Instance>(false);

    public Instance StartInstance(Instance parent) {
      if (ready.Empty) {
        ready.Add(new Instance() {
          Coroutine = FiberMonitor(GeneratorFunction(), ready.Mark, parent)
        });
      }

      var instance = ready.Unlink();
      Debug.Log($"**** FiberController:135 To Be Done -- save in Cue.Fibers so it can run");//#DM#// 18 Jul 2018
      return instance;
    }

    private IEnumerator FiberMonitor(IEnumerator fiber, object link, Instance parent) {
      try {
        while (fiber.MoveNext()) yield return fiber.Current;
      } finally {
        Debug.Log($"**** FiberController:142 To Be Done!!! Remove from Fibers");//#DM#// 18 Jul 2018
        ready.Link(link);
        if (parent!=null)
      }
    }
  }

  public class Instance {
    public IEnumerator Coroutine;
    public bool        running;
  }
  #endregion
}