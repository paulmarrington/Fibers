using System;
using System.Collections;

namespace Askowl.Fibers {
  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private Fibers waiting = new Fibers();

    static IEnumeratorWorker() { Register(new IEnumeratorWorker()); }

    protected internal override bool OnYield(Fiber fiber) {
      fiber.Node.MoveTo(waiting); // moved back when InstanceWorker is done
      WaitFor.Coroutine(fiberGenerator: Parameter(fiber), parentNode: fiber.Node);
      return true;
    }
  }
}