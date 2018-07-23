using System.Collections;

namespace Askowl.Fibers {
  public struct Fiber {
    public IEnumerator Coroutine;
    public Fibers.Node Node;
    public Fibers.Node ParentNode;

    public T Data<T>() {
      try {
        return (T) data;
      } catch {
        return default(T);
      }
    }

    public void Data(object value) => data = value;

    private object data;
  }

  public class Fibers : LinkedList<Fiber> { }
}