using System;
using System.Collections;

namespace Askowl.Fibers {
  /// Used by all fiber workers to keep a list of fibers of interest.
  public class Coroutines : LinkedList<Coroutine> { }

  public class Fibers : LinkedList<Fiber> { }

  /// <summary>
  /// Core information when a fiber is registered. Put in a recycling queue
  /// when not actively being used. Lives while programming is running.
  /// </summary>
  public struct Fiber {
    public IEnumerator Enumerator;
    public FiberWorker Worker;

//    public object      YieldValue;
//
//    public Yield Yield => (Yield) YieldValue;
//
//    public override string ToString() => $"{Node}";
  }

  /// Generated and returned by WaitFor.Updates and friends.
  /// Lives until enumeration is done (end of coroutine)
  public struct Coroutine {
    public Fiber           Fiber      { get; set; }
    public Coroutines.Node Node       { get; set; }
    public Coroutines.Node ParentNode { get; set; }
    public Yield           Yield      { get; }

    public T Result<T>() {
      try {
        return (T) result;
      } catch {
        return default(T);
      }
    }

    public object Result(object value) => result = value;

//      public Yield Stop() => Yield.Stop();

    private object result;
  }

  /// Generated for every "return yield" from a coroutine.
  /// Lives until the next yield.
  public struct Yield {
    public Coroutine               Coroutine { get; }
    public Func<Coroutine, bool>   EndYieldCondition;
    public Func<Coroutine, object> Action;

    public T Parameter<T>() {
      try {
        return (T) parameter;
      } catch {
        return default(T);
      }
    }

    public void Parameter(object value) => parameter = value;

    private object parameter;

    public Yield(Coroutine coroutine, object yieldParam) {
      Coroutine         = coroutine;
      parameter         = yieldParam;
      EndYieldCondition = (coro) => true;
      Action            = (coro) => null;
    }

    public Yield Repeating() {
      EndYieldCondition = (coroutine) => false;
      return this;
    }

    public Yield Repeat(int countdown) {
      EndYieldCondition = (coroutine) => (countdown-- <= 0);

      return this;
    }

    public Yield Until(Func<Coroutine, bool> condition) {
      EndYieldCondition = condition;
      return this;
    }

    public Yield Until(Func<bool> condition) {
      EndYieldCondition = (coroutine) => condition();
      return this;
    }

    public Yield While(Func<Coroutine, bool> condition) {
      EndYieldCondition = (coroutine) => !condition(coroutine);
      return this;
    }

    public Yield While(Func<bool> condition) {
      EndYieldCondition = (coroutine) => !condition();
      return this;
    }

    public Yield Do(Func<Coroutine, object> action) {
      Action = action;
      return this;
    }

    public Yield Do(Func<object> action) {
      Action = (coroutine) => action();
      return this;
    }

    public Yield Stop() {
      EndYieldCondition = (coroutine) => true;
      return this;
    }
  }
}