using System;
using System.Collections;

namespace Askowl.Fibers {
  public class Fibers : LinkedList<Fiber> { }

  public struct Fiber {
    public IEnumerator Coroutine  { get; set; }
    public FiberWorker Worker     { get; set; }
    public Fibers.Node Node       { get; set; }
    public Fibers.Node ParentNode { get; set; }
    public object      YieldValue;

    public Yield Yield => (Yield) YieldValue;

    public T Result<T>() {
      try {
        return (T) result;
      } catch {
        return default(T);
      }
    }

    public void Result(object value) => result = value;

    private object result;
  }
}