using System;
using System.Collections;
using UnityEngine;

namespace Askowl.Fibers {
  public partial class Fiber {
    public Fiber FIXME_Coroutine(Func<IEnumerator> coroutine, int framesBetweenChecks = 1) =>
      IEnumeratorWorker.Load(this, coroutine, framesBetweenChecks);
  }

  public class IEnumeratorWorker : Worker<Func<IEnumerator>> {
    private int  framesBetweenChecks; //#TBD#
    private bool coroutineDone;

    static IEnumeratorWorker() { new IEnumeratorWorker().Prepare("Fiber.IEnumerator Worker"); }

    protected override Func<IEnumerator> Parse(Func<IEnumerator> data, object[] more) {
      framesBetweenChecks = (int) more[0];
      coroutineDone       = false;
      Fiber.controller.StartCoroutine(Wrapper());
      return data;
    }

    IEnumerator Wrapper() {
      yield return data();

      coroutineDone = true;
    }

    protected override bool InRange() => !coroutineDone;
  }
}