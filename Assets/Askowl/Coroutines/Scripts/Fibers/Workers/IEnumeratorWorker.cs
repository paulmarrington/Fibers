using System;
using System.Collections;

namespace Askowl.Fibers {
  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private Instances waiting = new Instances();

    static IEnumeratorWorker() { Register(new IEnumeratorWorker()); }

    protected override bool OnYield(Yield<Func<IEnumerator>> returnedResult, Instances.Node node) {
      node.MoveTo(waiting); // moved back when InstanceWorker is done
      WaitFor.Coroutine(fiberGenerator: returnedResult.Data, parentNode: node);
      return true;
    }
  }
}