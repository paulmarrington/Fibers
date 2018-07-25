using System;
using System.Collections;

namespace Askowl.Fibers {
  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    protected override bool AddToUpdate => true;

    private Fibers waiting = new Fibers();

    protected internal override bool OnYield(Fiber fiber) {
      fiber.Node.MoveTo(waiting); // moved back when InstanceWorker is done
      WaitFor.Updates(fiberGenerator: Parameter(fiber), parentNode: fiber.Node);
      return true;
    }
  }
}