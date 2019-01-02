// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Askowl {
  /// <a href="http://bit.ly/2DF6QHw">lightweight cooperative multi-tasking</a>
  public partial class Fiber : IDisposable {
    #region Instantiate

    /// <a href=""></a> //#TBD#//
    public Emitter OnComplete;

    /// <a href=""></a> //#TBD#//
    public bool Running;

    /// <a href="http://bit.ly/2DDZjbO">Method signature for Do(Action) methods</a>
    public delegate void Action(Fiber fiber);

    /// <a href=""></a> //#TBD#//
    public static Fiber Instance {
      get {
        var node = Queue.Waiting.GetRecycledOrNew();
        Queue.Reactivation(node);
        var fiber = node.Item;
        fiber.disposeOnComplete = false;
        fiber.node              = node;
        fiber.id                = ++nextId;
        return fiber;
      }
    }

    /// <a href="http://bit.ly/2DDvnwP">Cleans up Fiber before it goes into the recycling</a>
    public void Dispose() => node.Recycle();

    /// <a href="http://bit.ly/2DDvnwP">Prepare a Fiber and place it on the Update queue</a>
    public static Fiber Start {
      get {
        var fiber = Instance.Go();
        fiber.disposeOnComplete = true;
        return fiber;
      }
    }

    /// <a href=""></a> //#TBD#//
    public Fiber Go() => Go(NextAction);

    /// <a href=""></a> //#TBD#//
    public Fiber Go(Action updater) {
      if (controller == null) {
        controller = Components.Create<FiberController>("FiberController");
      }
      Running = true;
      Update  = updater;
      SetAction(actions.Last);
      node.MoveTo(Queue.Update);
      return this;
    }

    #endregion

    #region Queues

    /// <a href="http://bit.ly/2Pqv2Ub">Return Fiber processing to frame Update queue</a>
    public Fiber OnUpdates => AddAction(_ => node.MoveTo(Queue.Update), "OnUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to FixedUpdate queue</a>
    public Fiber OnFixedUpdates => AddAction(_ => node.MoveTo(Queue.FixedUpdate), "OnFixedUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to LateUpdate queue</a>
    public Fiber OnLateUpdates => AddAction(_ => node.MoveTo(Queue.LateUpdate), "OnLateUpdates");

    /// <a href="http://bit.ly/2DBVWCe">Abort fiber processing, cleaning up as we go</a>
    public Fiber Exit() {
      while ((action?.Previous != null) && (action.Previous.Item.Actor != Yielding)) action = action.Previous;
      return this;
    }

    /// <a href=""></a> //#TBD#//
    public void Finish() { }

    #endregion

    #region Blocks and Loops

    /// <a href="http://bit.ly/2DDvnNl">Loops and Blocks - Begin/End, Begin/Again, Begin-Repeat</a>
    public Fiber Begin {
      get {
        AddAction(_ => blockStack.Push(action?.Previous ?? actions.First), "Begin");
        AddAction(NextAction);
        return this;
      }
    }

    /// <a href="http://bit.ly/2PtbezT">Begin/End block - use Break() to create an `if`</a>
    public Fiber End => AddAction(NextAction).AddAction(_ => blockStack.Pop(), "End");

    /// <a href="http://bit.ly/2DDvnNl">Begin/Again repeating operations. Use Break() or Exit() to leave</a>
    public Fiber Again => AddAction(_ => action = blockStack.Top).AddAction(NextAction).End;

    /// <a href="http://bit.ly/2DDvp7V">Begin/Repeat loop for a specific number of times</a>
    public Fiber Repeat(int count) {
      count += 1;
      int counter = 0;
      return AddAction(
        _ => {
          var begin                            = blockStack.Top;
          if ((++counter % count) != 0) action = begin;
        }, "Repeat").End;
    }

    /// <a href=""></a> //#TBD#//
    public Fiber Until(Func<Fiber, bool> isTrue) =>
      AddAction(
        _ => {
          var begin                 = blockStack.Top;
          if (!isTrue(this)) action = begin;
        }, "Until").End;

    /// <a href=""></a> //#TBD#//
    public Fiber BreakIf(Func<Fiber, bool> isBreak) =>
      AddAction(
        _ => {
          if (isBreak(this)) Break();
        }, "BreakIf");

    /// <a href="http://bit.ly/2DDvlFd">Break a Begin/End/Repeat/Again block</a>
    public void Break() {
      while ((action?.Previous != null) && (action.Previous.Item.Actor != NextAction)) action = action.Previous;
    }

    /// <a href=""></a> //#TBD#//
    public void Break(int after) {
      Skip(after);
      Break();
    }

    /// <a href=""></a> //#TBD#//
    public void Skip(int after) {
      for (int i = 0; (i < after) && (action != null); i++) action = action.Previous;
    }

    #endregion

    #region If Else Then

    /// <a href=""></a> //#TBD#//
    public Fiber If(Func<Fiber, bool> isTrue) =>
      AddAction(
        _ => {
          if (!isTrue(this)) Break();
        }, "If");

    /// <a href=""></a> //#TBD#//
    public Fiber Else => AddAction(_ => Break(2), "Else").AddAction(NextAction);

    /// <a href=""></a> //#TBD#//
    public Fiber Then => AddAction(NextAction, "Then");

    #endregion

    /// <a href="http://bit.ly/2DDZjbO">Business logic activation step</a>
    public Fiber Do(Action nextAction, string name = null) => AddAction(nextAction, name);

    #region Leave a Fiber Idling

    /// <a href="http://bit.ly/2DDvmcf">Separate Fiber into more than one statement</a>
    public Fiber Idle => WaitFor(idleEmitter, "Idle");

    /// <a href="http://bit.ly/2DDvmcf">Restart a fiber that includes an Idle command</a>
    public Fiber Restart() {
      idleEmitter.Fire();
      return this;
    }

    /// <a href="http://bit.ly/2DDvmcf">Restart another fiber that includes an Idle command</a>
    public Fiber Restart(Fiber fiber) => AddAction(_ => fiber.idleEmitter.Fire(), "Restart");

    /// <a href="http://bit.ly/2DB3wgx">Return an IEnumerator to use with a yield in a Coroutine</a>
    public IEnumerator AsCoroutine() {
      Do(Yielding);
      for (yielding = false; yielding != true;) yield return null;
    }
    private bool yielding;
    private void Yielding(Fiber fiber) => yielding = true;

    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Fiber anotherFiber) =>
      AddAction(
        _ => {
          if (anotherFiber == null) return;
          WaitFor(anotherFiber.OnComplete);
          if (!anotherFiber.Running) anotherFiber.Go();
        });

    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Func<Fiber, Fiber> getFiber) => AddAction(_ => WaitFor(getFiber(this)));

    #endregion

    /// <a href="http://bit.ly/2DDvmZN">Displays Do() and action events on Unity console</a>
    public static bool Debugging = false;

    #region Support

    /// <a href="http://bit.ly/2DF6QHw">Container for different update queues</a> <inheritdoc />
    internal class Queue : LinkedList<Fiber> {
      internal static Action<Node> Deactivation = (node) => {
        var fiber = node.Item;
        fiber.actions.Dispose();
        fiber.blockStack.Dispose();
        fiber.idleEmitter.Dispose();
        fiber.OnComplete.Dispose();
      };

      internal static void Reactivation(Node node) {
        var fiber = node.Item;
        fiber.OnComplete = Emitter.Instance;
        fiber.actions    = Cache<ActionList>.Instance;
        fiber.AddAction(_ => { }, "Start");
        fiber.blockStack  = Fifo<LinkedList<ActionItem>.Node>.Instance;
        fiber.Running     = false;
        fiber.idleEmitter = Emitter.Instance;
      }

      internal static readonly Queue Update = new Queue
        {Name = "Update Fibers", DeactivateItem = Deactivation, ReactivateItem = Reactivation};
      internal static readonly Queue LateUpdate = new Queue
        {Name = "Late Update Fibers", DeactivateItem = Deactivation, ReactivateItem = Reactivation};
      internal static readonly Queue FixedUpdate = new Queue
        {Name = "Fixed Update Fibers", DeactivateItem = Deactivation, ReactivateItem = Reactivation};
      internal static readonly Queue Waiting = new Queue
        {Name = "Fiber Waiting Queue", DeactivateItem = Deactivation, ReactivateItem = Reactivation};
    }

    private static string ActionName(ActionItem actionItem) {
      var   name  = actionItem.Name ?? actionItem.Actor.Method.Name;
      Match match = getRe.Match(name);
      for (int i = 0; i < match.Groups.Count; i++) {
        if (match.Groups[i].Success) name = match.Groups[i].Value;
      }
      if (name == "NextAction") name = "_";
      return name;
    }
    private static readonly Regex getRe = new Regex(@"<get_(.*?)>|<.*?>g__(.*)\|\d+|<(.*?)>");

    private static void NextAction(Fiber fiber) {
      if (fiber.action?.Previous != null) {
        fiber.SetAction(fiber.action.Previous).Item.Actor(fiber);
      } else {
        #if UNITY_EDITOR
        if (Debugging) Log.Debug($"OnComplete: for {fiber.node}");
        #endif
        fiber.OnComplete.Fire();
        fiber.Running = false;
        fiber.node.MoveTo(Queue.Waiting);
        if (fiber.disposeOnComplete) fiber.node.Dispose();
      }
    }

    private struct ActionItem {
      public string Name;
      public Action Actor;
    }

    private class ActionList : LinkedList<ActionItem> { }

    private        LinkedList<ActionItem>.Node       action;
    private        ActionList                        actions;
    private        Fifo<LinkedList<ActionItem>.Node> blockStack;
    private static MonoBehaviour                     controller;
    private        bool                              disposeOnComplete;
    private        int                               id;
    private        Emitter                           idleEmitter;
    private static int                               nextId;
    private        LinkedList<Fiber>.Node            node;
    internal       Action                            Update;

    private Fiber AddAction(Action newAction, string name = null) {
      if (Running) {
        newAction(this);
      } else {
        actions.Add(new ActionItem {Name = name, Actor = newAction});
      }
      return this;
    }

    private LinkedList<ActionItem>.Node SetAction(LinkedList<ActionItem>.Node nextAction) {
      action = nextAction;
      #if UNITY_EDITOR
      if (Debugging) Log.Debug($"Run: {ActionName(action.Item),10} for {this}");
      #endif
      return action;
    }

    #endregion

    #region Debugging

    /// <a href="http://bit.ly/2DDvmZN">Return Fiber contents and current state</a><inheritdoc />
    public override string ToString() => $"Id: {id} // Actions: {ActionNames} // Queue: {node.Owner}";

    //BeginAgainExample.IncrementCounter,BeginAgainExample.IncrementCounter,Fiber.<get_End>b__18_0,<>c.<.ctor>b__75_0,BeginAgainExample.IncrementCounter
    private string ActionNames {
      get {
        var array = new string[actions.Count];
        // ReSharper disable once LocalVariableHidesMember
        var node = actions.Last;

        for (var idx = 0; idx < array.Length; node = node.Previous, idx++) {
          var name = ActionName(node.Item);
          array[idx] = node == action ? $"[{name}]" : name;
        }
        return Csv.ToString(array);
      }
    }

    #endregion
  }
}