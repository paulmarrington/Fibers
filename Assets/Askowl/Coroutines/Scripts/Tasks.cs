#if (!NET_2_0 && !NET_2_0_SUBSET)
using System.Threading.Tasks;
#endif

namespace Askowl {
  using JetBrains.Annotations;

  [UsedImplicitly]
  public sealed class Tasks {
#if (!NET_2_0 && !NET_2_0_SUBSET)
  public static IEnumerator WaitFor(Task task, Action<string> error = null) {
    Task done = null;
    task.ContinueWith(result => done = result);
    while (done == null)
      yield return null;
    SendError(error, done);
  }

  public static bool SendError(Action<string> error, Task task) {
    if (task.IsCanceled) {
      SendError(error, "Cancelled");
      return true;
    } else if (task.IsFaulted) {
      string fault = "";
      foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
        fault += exception.ToString() + "\n";
      }
      SendError(error, fault);
      return true;
    }
    return false;
  }

  public static bool SendError(Action<string> error, string message) {
    if (error == null)
      error = (msg => Debug.LogError(msg));
    error(message);
    return true;
  }

  public static IEnumerator WaitFor<T>(Task<T> task, Action<T> action, Action<string> error =
 null) {
    Task<T> done = null;
    task.ContinueWith(result => done = result);
    while (done == null)
      yield return null;
    if (!SendError(error, done))
      action(done.Result);
  }
  #endif
  }
}