using System;

namespace Askowl.Fibers {
  public class Fibers : LinkedList<Fiber> { }

  public partial class Fiber {
    public Fibers.Node Node;
    public Fibers      UpdateQueue;
    public Func<bool>  EndCondition;

    private readonly Action<Fiber>[][] actions = new Action<Fiber>[100][];

    private int currentAction, actionCount, currentActionList, actionListCount;

    public Fiber Do(params Action<Fiber>[] moreActions) {
      if (moreActions.Length == 0) return this;

      if (actionListCount >= actions.Length) {
        throw new OverflowException($"More that {this.actions.Length} action lists for Fiber on {Node.Owner.Name}");
      }

      actions[actionListCount++] = moreActions;
      return this;
    }

    protected internal bool OnUpdate() {
      var action = Next();

      if (action == null) { }
      return true;
    }

    private Action<Fiber> Next() {
      if (currentAction >= actionCount) {
        if (currentActionList == actionListCount) {
          Node.MoveTo(Recycled);
          return null;
        }

        currentActionList = (currentActionList + 1) % actions.Length;
        currentAction     = 0;
        actionCount       = actions[currentActionList].Length;
      }

      return actions[currentActionList][currentAction++];
    }
  }
}