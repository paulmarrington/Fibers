using System;

namespace Askowl.Fibers {
  public struct Yield {
    public Worker     Worker { get; }
    public Func<bool> EndYieldCondition;

    public T Parameter<T>() {
      try {
        return (T) parameter;
      } catch {
        return default(T);
      }
    }

    public void Parameter(object value) => parameter = value;

    private object parameter;

    public Yield(Worker worker, object yieldParam) {
      Worker            = worker;
      parameter         = yieldParam;
      EndYieldCondition = () => true;
    }

    public Yield Repeating() {
      EndYieldCondition = () => false;
      return this;
    }

    public Yield Repeat(int countdown) {
      EndYieldCondition = () => (countdown-- == 0);
      return this;
    }

    public Yield Until(Func<bool> condition) {
      EndYieldCondition = condition;
      return this;
    }

    public Yield While(Func<bool> condition) {
      EndYieldCondition = () => !condition();
      return this;
    }
  }
}