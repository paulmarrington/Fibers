// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

namespace Askowl {
  using System;
  using System.Collections;

  /// <a href=""></a>
  // ReSharper disable once ClassNeverInstantiated.Global
  public partial class Fiber {
    /// <a href=""></a>
    /// <inheritdoc />
    public class Queue : LinkedList<Fiber> {
      /// <inheritdoc />
      public Queue(string name = null) : base(name) { }
    }

    /// <a href=""></a>
    public delegate void Action(Fiber fiber);

    /// <a href=""></a>
    public LinkedList<Fiber>.Node Node;

    /// <a href=""></a>
    public Queue UpdateQueue;

//    /// <a href=""></a>
//    public Func<bool> EndCondition;
    private readonly Action[][] actions = new Action[128][];

    private int currentAction, actionCount, currentActionList, actionListCount;

    /// <a href=""></a>
    public Fiber Do(params Action[] moreActions) {
      if (moreActions.Length == 0) return this;

      if (actionListCount >= actions.Length) {
        throw new OverflowException(
          $"More that {actions.Length} action lists for Fiber on {Node.Owner.Name}");
      }

      actions[actionListCount++] = moreActions;
      return this;
    }

    /// <a href=""></a>
    public IEnumerator AsCoroutine() {
      yield return null; //#TBD#//
    }

    /// <a href=""></a>
    protected internal void OnUpdate() {
      if (currentAction >= actionCount) {
        if (currentActionList == actionListCount) {
          Node.Recycle();
          return;
        }

        currentActionList = (currentActionList + 1) % actions.Length;
        currentAction     = 0;
        actionCount       = actions[currentActionList].Length;
      }

      actions[currentActionList][currentAction++](this);
    }
  }
}