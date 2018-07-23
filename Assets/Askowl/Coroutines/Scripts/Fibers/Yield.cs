//using System;
//
//namespace Askowl.Fibers {
//  public interface Yield {
//    Worker Worker { get; }
//  }
//
//  public struct Yield<T> : Yield {
//    public Worker     Worker { get; internal set; }
//    public Func<bool> EndRepeatCondition;
//    public T          Data { get { return data; } set { data = value; } }
//
//    private T data;
//
//    public Yield(Worker worker, T data) {
//      Worker             = worker;
//      this.data          = data;
//      EndRepeatCondition = () => true;
//    }
//
//    public Yield<T> Repeat(int countdown) {
//      EndRepeatCondition = () => (countdown-- == 0);
//      return this;
//    }
//
//    public Yield<T> Until(Func<bool> condition) {
//      EndRepeatCondition = condition;
//      return this;
//    }
//
//    public Yield<T> While(Func<bool> condition) {
//      EndRepeatCondition = () => !condition();
//      return this;
//    }
//  }
//}

