using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl {
  public class Cue : MonoBehaviour {
    public class Fiber : IEnumerable {
      internal Func<IEnumerator> generator;

      private LinkedList<IEnumerator> ready = new LinkedList<IEnumerator>();
      private LinkedList<IEnumerator> inUse = new LinkedList<IEnumerator>();

      public IEnumerator GetEnumerator() {
        if (!ready.Empty) {
          return ready.MoveTo(inUse);
        }
      }

      private IEnumerator NewFiber {
        get {
          IEnumerator fiber = generator();
        }
      }
    }

    private class Generators : Dictionary<Func<IEnumerator>, Fiber> { }

    private static Generators generators = new Generators();

    public static void New(Func<IEnumerator> fiberGenerator) {
      if (!generators.ContainsKey(fiberGenerator)) {
        generators[fiberGenerator] = new Fiber {generator = fiberGenerator};
      }

      var fiber = generators[fiberGenerator];
    }
  }
}
/*
 * yield return Cue.New(MyCoroutine);
 *
 * IEnumerator MyCoroutine() {
 *   yield return Cue.Frames(5);
 *   yield return Cue.Seconds(5.3);
 * }
*/