// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Askowl {
  /// <a href="http://bit.ly/2DF6QHw">lightweight cooperative multi-tasking</a>
  public partial class Fiber : IDisposable {
    #region Instantiate
    ///
    public Emitter OnComplete;

    ///
    public bool Running, Aborted;

    /// <a href="http://bit.ly/2DDZjbO">Method signature for Do(Action) methods</a>
    public delegate void Action(Fiber fiber);

    /// <a href="http://bit.ly/2RAsNy3">Precompile an instance of a fiber command</a>
    public static Fiber Instance {
      get {
        var node = Queue.Waiting.GetRecycledOrNew();
        Queue.Reactivation(node);
        var fiber = node.Item;
        fiber.node = node;
        fiber.id   = ++nextId;
        return fiber;
      }
    }

    /// <a href="http://bit.ly/2DDvnwP">Cleans up Fiber before it goes into the recycling</a>
    public void Dispose() {
      (context as IDisposable)?.Dispose();
      actions.Dispose();
      node.Recycle();
      if (resetOnError) onError = Debug.LogError;
    }

    /// <a href="http://bit.ly/2DDvnwP">Prepare a Fiber and place it on the Update queue</a>
    public static Fiber Start {
      get {
        if (controller == null) {
          controller = Components.Create<FiberController>("FiberController");
        }
        var fiber = Instance;
        fiber.Update = FirstUpdate;
        fiber.node.MoveTo(Queue.Update);
        fiber.disposeOnComplete = true;
        return fiber;
      }
    }
    private static void FirstUpdate(Fiber fiber) { // called on first Update
      fiber.Go(NextAction);
      NextAction(fiber);
    }

    /// <a href="http://bit.ly/2Rb9oEq">Start a fiber if it is not already running</a>
    public Fiber Go() => Go(NextAction);

    /// <a href="http://bit.ly/2Rb9oEq">Start a fiber if it is not already running</a>
    public Fiber Go(Action updater) {
      if (Running) return this;
      if (controller == null) {
        controller = Components.Create<FiberController>("FiberController");
      }
      Update  = updater;
      Running = true;
      SetAction(actions.Last);
      node.MoveTo(Queue.Update);
      return this;
    }
    #endregion

    #region Closures
    /// <a href="http://bit.ly/2NjSGNX">Interface used by WaitFor(IClosure)</a>
    public interface IClosure {
      /// <a href="http://bit.ly/2NjSGNX">A reference to closure.Fiber.OnComplete</a>
      Emitter OnComplete { get; }
      Fiber Fiber { get; }
    }

    /// <a href="http://bit.ly/2NjSGNX">Closure super-class that does all the smarts</a>
    public abstract class Closure<TS, TTuple> : DelayedCache<Closure<TS, TTuple>>, IClosure
      where TS : DelayedCache<Closure<TS, TTuple>> {
      /// <a href="http://bit.ly/2NjSGNX">Scope is available for 10 frames after OnComplete in case it holds response data</a>
      public TTuple Scope;
      /// <a href="http://bit.ly/2NjSGNX">Emitter that is fired when the fiber completes all actions</a>
      public Emitter OnComplete => onComplete;
      private Emitter onComplete;

      protected Closure() {
        fiber = Fiber.Instance;
        // ReSharper disable once VirtualMemberCallInConstructor
        Activities(Fiber);
        Fiber.Do(_ => Dispose());
      }
      /// <a href="http://bit.ly/2NjSGNX">Fiber that will run / is running / has run in the context of this closure</a>
      public Fiber Fiber => fiber;
      private readonly Fiber fiber;

      /// <a href="http://bit.ly/2NjSGNX">Add all the steps you need to this override. It is called by the constructor.</a>
      protected abstract void Activities(Fiber fiberToUpdate);

      /// <a href="http://bit.ly/2NjSGNX">Calling this static will fetch a prepared fiber, add scope and run it.</a>
      public static Closure<TS, TTuple> Go(TTuple scope) {
        var instance = Cache<TS>.Instance as Closure<TS, TTuple>;
        // ReSharper disable once PossibleNullReferenceException
        instance.Scope      = scope;
        instance.onComplete = instance.Fiber.OnComplete;
        instance.Fiber.Go();
        return instance;
      }
    }

    /// <a href="http://bit.ly/2NjSGNX">Helper that is the same as fiber.WaitFor(closure.OnComplete)</a>
    public Fiber WaitFor(IClosure closure) => WaitFor(closure.Fiber);
    #endregion

    #region Context
    /// <a href="http://bit.ly/2RUcL2S">Retrieve the context as a class type - null for none or wrong type</a>
    public T Context<T>() where T : class => context[typeof(T)].Value as T;

    /// <a href="http://bit.ly/2RUcL2S">Set the context to an instance of a type</a>
    public Fiber Context<T>(T value) where T : class {
      (context[typeof(T)].Value as IDisposable)?.Dispose();
      context.Add(typeof(T), value);
      return this;
    }
    /// <a href="http://bit.ly/2RUcL2S">Retrieve the context as a class type - null for none or wrong type</a>
    public T Context<T>(string name) where T : class => context[name].Value as T;

    /// <a href="http://bit.ly/2RUcL2S">Set the context to an instance of a type</a>
    public Fiber Context<T>(string name, T value) where T : class {
      (context[name].Value as IDisposable)?.Dispose();
      context.Add(name, value);
      return this;
    }
    private readonly Map context = Map.Instance;
    #endregion

    #region Queues
    /// <a href="http://bit.ly/2Pqv2Ub">Return Fiber processing to frame Update queue</a>
    public Fiber OnUpdates => AddAction(_ => node.MoveTo(Queue.Update), "OnUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to FixedUpdate queue</a>
    public Fiber OnFixedUpdates => AddAction(_ => node.MoveTo(Queue.FixedUpdate), "OnFixedUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to LateUpdate queue</a>
    public Fiber OnLateUpdates => AddAction(_ => node.MoveTo(Queue.LateUpdate), "OnLateUpdates");

    /// <a href="http://bit.ly/2DBVWCe">Abort fiber processing immediately, cleaning up as we go</a>
    public Fiber Exit() {
      Aborted = true;
      action  = actions.First;
      return this;
    }

    /// <a href="http://bit.ly/2DBVWCe">Force another fiber to exit immediately</a>
    public Fiber Exit(Fiber fiber) {
      AddAction(
        _ => {
          fiber.Aborted = true;
          fiber.action  = fiber.actions.First;
          node.MoveTo(Queue.Update);
          Update(fiber);
        });
      return this;
    }

    /// <a href="http://bit.ly/2Rf0dD4">Complete a Fiber.Start statement where needed (no action)</a>
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
    public Fiber Repeat(int count) => Repeat(_ => count);

    /// <a href="http://bit.ly/2DDvp7V">Begin/Repeat loop for a specific number of times</a>
    public Fiber Repeat(Func<Fiber, int> countLambda) {
      var count   = 0;
      int counter = 0;
      return AddAction(
        _ => {
          if (count == 0) count = countLambda(this) + 1;
          if (count == 0) return;
          var begin                            = blockStack.Top;
          if ((++counter % count) != 0) action = begin;
        }, "Repeat").End;
    }

    /// <a href="http://bit.ly/2CT634f">Loop until a value function returns true</a>
    public Fiber Until(Func<Fiber, bool> isTrue) =>
      AddAction(
        _ => {
          var begin                 = blockStack.Top;
          if (!isTrue(this)) action = begin;
        }, "Until").End;

    /// <a href="http://bit.ly/2RDN05W">Break out of any block if a value function returns true</a>
    public Fiber BreakIf(Func<Fiber, bool> isBreak) =>
      AddAction(
        _ => {
          if (isBreak(this)) Break();
        }, "BreakIf");

    /// <a href="http://bit.ly/2DDvlFd">Break a Begin/End/Repeat/Again block</a>
    public void Break() {
      while ((action?.Previous != null) && (action.Previous.Item.Actor != NextAction)) action = action.Previous;
    }

    ///
    public void Break(int after) {
      Skip(after);
      Break();
    }

    ///
    public void Skip(int after) {
      for (int i = 0; (i < after) && (action != null); i++) action = action.Previous;
    }
    #endregion

    #region If Else Then
    /// <a href="http://bit.ly/2CU6Vp6">Standard If // Else // Then branch</a>
    public Fiber If(Func<Fiber, bool> isTrue) =>
      AddAction(
        _ => {
          if (!isTrue(this)) Break();
        }, "If");

    /// <a href="http://bit.ly/2CU6Vp6">Standard If // Else // Then branch</a>
    public Fiber Else => AddAction(_ => Break(2), "Else").AddAction(NextAction);

    /// <a href="http://bit.ly/2CU6Vp6">Standard If // Else // Then branch</a>
    public Fiber Then => AddAction(NextAction, "Then");
    #endregion

    /// <a href="http://bit.ly/2DDZjbO">Business logic activation step</a>
    public Fiber Do(Action nextAction, string name = null) => AddAction(nextAction, name);

    #region Fiber Control and Monitoring
    /// <a href="http://bit.ly/2DB3wgx">Return an IEnumerator to use with a yield in a Coroutine</a>
    public IEnumerator AsCoroutine() {
      if (!Running) Go();
      while (Running) yield return null;
    }

    /// <a href="http://bit.ly/2CV0RNn">Wait for another fiber to complete, starting it if needed</a>
    public Fiber WaitFor(Fiber anotherFiber) {
      WaitFor(PrepareAnotherFiber(anotherFiber).OnComplete, "WaitFor(Fiber)");
      Do(_ => anotherFiber.Go());
      return this;
    }

    /// <a href="http://bit.ly/2RWQrpp">Exit later fiber operations if the time supplied is exceeded</a>
    public Fiber Timeout(float seconds) {
      secondsTimeout = seconds;
      if (timeoutFiber == null) timeoutFiber = Instance.WaitFor(_ => secondsTimeout).Exit(this);
      timeoutFiber.Go();
      return this;
    }
    private float secondsTimeout;
    private Fiber timeoutFiber;

    /// <a href="http://bit.ly/2CV0RNn">Wait for another fiber to complete, starting it if needed - value set by return value of a function</a>
//    public Fiber WaitFor(Func<Fiber, Fiber> getFiber) => AddAction(_ => WaitFor(getFiber(this)));
    public Fiber WaitFor(Func<Fiber, Fiber> getFiber) => AddAction(
      _ => {
        var anotherFiber = PrepareAnotherFiber(getFiber(this)).Go();
        EmitterWorker.Instance.Load(this, anotherFiber.OnComplete);
      });

    private Fiber PrepareAnotherFiber(Fiber anotherFiber) {
      if (anotherFiber         == null) return null;
      if (anotherFiber.onError == globalOnError) anotherFiber.onError = onError;
      return anotherFiber;
    }
    #endregion

    #region Error Management
    /// <a href="http://bit.ly/2NsjMml">Set a global (app-wide) error catch lambda. All fibers without a local override will come here. The default is to write to the Unity console.</a>
    public Fiber GlobalOnError(Action<string> actor) {
      onError      = globalOnError = actor;
      resetOnError = true;
      return this;
    }
    /// <a href="http://bit.ly/2NlxMy3">The catch lambda will be called for any exceptions from this fiber or any fibers called with WaitFor</a>
    public Fiber OnError(Action<string> actor) {
      onError = actor;
      return this;
    }
    /// <a href="http://bit.ly/2Nn2xCx">Exceptions in this fiber will cause the fiber to exit</a>
    public Fiber ExitOnError {
      get {
        exitOnError = true;
        return this;
      }
    }
    /// <a href="http://bit.ly/2NjXMJZ"></a> //#TBD#//
    public Fiber Error(string message) {
      onError(message);
      return this;
    }
    /// <a href="http://bit.ly/2NjXMJZ"></a> //#TBD#//
    public Fiber Error(Func<Fiber, string> messageLambda) {
      onError(messageLambda(this));
      return this;
    }
    private        bool           resetOnError;
    private        Action<string> onError       = msg => globalOnError(msg);
    private static Action<string> globalOnError = msg => Debug.LogError($"onError: {msg}");
    private        bool           exitOnError;
    #endregion

    #region Support
    /// <a href="http://bit.ly/2DF6QHw">Container for different update queues</a> <inheritdoc />
    internal class Queue : LinkedList<Fiber> {
      // Deactivate only used for Start when it is not an infinite loop - in other words hardly ever
      internal static Action<Node> Deactivation = (node) => {
        var fiber = node.Item;
        fiber.actions.Dispose();
        fiber.blockStack.Dispose();
        fiber.OnComplete.Dispose();
        fiber.CancelOnAborted();
      };

      internal static void Reactivation(Node node) {
        var fiber = node.Item;
        fiber.OnComplete = Emitter.Instance;
        fiber.actions    = Cache<ActionList>.Instance;
        fiber.AddAction(_ => { }, "Start");
        fiber.blockStack = Fifo<LinkedList<ActionItem>.Node>.Instance;
        fiber.Running    = fiber.Aborted = fiber.resetOnError = fiber.disposeOnComplete = fiber.exitOnError = false;
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
    #endregion

    #region Action
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
        try {
          fiber.SetAction(fiber.action.Previous).Item.Actor(fiber);
        } catch (Exception e) {
          fiber.onError(e.ToString());
          if (fiber.exitOnError) fiber.Exit();
        }
      } else {
        #if UNITY_EDITOR
        if (fiber.Debugging) Debug.Log($"OnComplete: for {fiber.node}");
        #endif
        fiber.OnComplete.Fire();
        fiber.Running = false;
        fiber.node.MoveTo(Queue.Waiting);
        if (fiber.disposeOnComplete) fiber.Dispose();
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
    private static int                               nextId;
    private        LinkedList<Fiber>.Node            node;
    internal       Action                            Update;

    private Fiber AddAction(Action newAction, string name = null) {
      actions.Add(new ActionItem {Name = name, Actor = newAction});
      return this;
    }

    private LinkedList<ActionItem>.Node SetAction(LinkedList<ActionItem>.Node nextAction) {
      action = nextAction;
      #if UNITY_EDITOR
      if (Debugging) Debug.Log($"Run: {ActionName(action.Item),10} for {this}");
      #endif
      return action;
    }
    #endregion

    #region Debugging Mode
    /// <a href="http://bit.ly/2DDvmZN">Displays Do() and action events on Unity console</a>
    public bool Debugging = false;

    /// <a href=""></a> //#TBD#//
    public Fiber DebugLog(bool on = true) {
      Debugging = @on;
      return this;
    }

    /// <a href="http://bit.ly/2DDvmZN">Return Fiber contents and current state</a><inheritdoc />
    public override string ToString() => $"Id: {id} // Actions: {ActionNames} // Queue: {node?.Owner}";

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

    /// <a href="http://bit.ly/2NjzIHg">Write to the Unity console (optionally as a warning entry)</a>
    public Fiber Log(string message, bool warning = false) {
      AddAction(
        _ => {
          var msg = $"{message}\n{this}";
          if (warning) {
            Debug.LogWarning(msg);
          } else {
            Debug.Log(msg);
          }
        });
      return this;
    }

    /// <a href=""></a> //#TBD#//
    public Fiber Log(Func<Fiber, string> messageLambda, bool warning = false) {
      AddAction(
        _ => {
          var msg = $"{messageLambda(_)}\n{this}";
          if (warning) {
            Debug.LogWarning(msg);
          } else {
            Debug.Log(msg);
          }
        });
      return this;
    }
    #endregion
  }
}