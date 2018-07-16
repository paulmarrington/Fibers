using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Askowl {
  public class Cue : MonoBehaviour {
    public static IEnumerator Instance(Func<IEnumerator> fiberGenerator) {
      if (!generators.ContainsKey(fiberGenerator)) {
        generators[fiberGenerator] = new FiberGenerator {Generator = fiberGenerator};
      }

      return generators[fiberGenerator].GetEnumerator();
    }

    public class FiberGenerator : IEnumerable {
      internal Func<IEnumerator> Generator;

      private LinkedList<IEnumerator>         ready = new LinkedList<IEnumerator>();
      private Dictionary<IEnumerator, object> inUse = new Dictionary<IEnumerator, object>();

      public IEnumerator GetEnumerator() {
        if (ready.Empty) ready.Add(FiberMonitor(Generator()));
        var fiber = ready.Unlink();
        inUse[fiber] = ready.Mark;
        return fiber;
      }

      private IEnumerator FiberMonitor(IEnumerator fiber) {
        try {
          while (fiber.MoveNext()) yield return fiber.Current;
        } finally {
          ready.Link(inUse[fiber]);
        }
      }
    }

    private class Generators : Dictionary<Func<IEnumerator>, FiberGenerator> { }

    private static Generators generators = new Generators();
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