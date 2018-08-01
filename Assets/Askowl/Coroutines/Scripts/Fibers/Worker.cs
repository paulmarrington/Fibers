namespace Askowl.Fibers {
  public class Worker<T> {
    protected T     data;
    protected Fiber fiber;

    protected Fibers updateQueue = Fiber.OnUpdatesQueue;
//    protected Func<object, T> ParseData;

    public void Prepare(string name, Fibers updateQueue = null) {
      instance             = this;
      instance.updateQueue = Fiber.OnUpdatesQueue;

      fibers = new Fibers {
        Name       = name,
        InRange    = (t) => InRange(),
        OnComplete = (t) => OnComplete()
      };
    }

    protected virtual T    Parse(T naked)                => naked;
    protected virtual T    Parse(T naked, object[] more) => (T) naked;
    protected virtual bool InRange()    => true;
    protected virtual void OnComplete() => fiber.Node.MoveTo(updateQueue);

    public static Fiber Load(Fiber fiber, T data, params object[] parameters) {
      instance.data  = instance.Parse(data, parameters);
      instance.fiber = fiber;
      fiber.Node.MoveTo(fibers);
      return fiber;
    }

    protected static Worker<T> instance;
    private static   Fibers    fibers;
  }
}