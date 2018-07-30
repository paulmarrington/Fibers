using System;
using System.Collections;
using UnityEngine;

namespace Askowl.Fibers {
  public static partial class WaitFor {
    public static Yield Coroutine(Func<IEnumerator> coroutine) => IEnumeratorWorker.Instance.Yield(coroutine);
    public static Yield Fiber(Func<IEnumerator>     coroutine) => IEnumeratorWorker.Instance.Yield(coroutine);
  }

  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    public static IEnumeratorWorker Instance = new IEnumeratorWorker();
    protected override bool AddToUpdate => true;

    private Fibers waiting = new Fibers();

    protected internal override bool OnYield(Coroutine coroutine) {
      coroutine.Node.MoveTo(waiting); // moved back when InstanceWorker is done
      WaitFor.Updates(fiberGenerator: Parameter(coroutine.Yield), parentNode: coroutine);
      return true;
    }
  }
}