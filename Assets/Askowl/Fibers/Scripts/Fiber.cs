// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
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
    public static Fiber Instance(
      string                  name           = null, [CallerMemberName] string callerName       = ""
    , [CallerFilePath] string callerFilePath = "",   [CallerLineNumber] int    callerLineNumber = 0) {
      var node = Queue.Waiting.GetRecycledOrNew();
      Queue.Reactivation(node);
      var fiber = node.Item;
      fiber.name = BuildName(name, callerName, callerFilePath, callerLineNumber);
      fiber.node = node;
      fiber.id   = ++nextId;
      return fiber;
    }

    /// <a href="http://bit.ly/2DDvnwP">Cleans up Fiber before it goes into the recycling</a>
    public void Dispose() {
      (context as IDisposable)?.Dispose();
      actions.Dispose();
      node.Recycle();
      if (resetOnError) onError = Debug.LogError;
    }

    /// <a href="http://bit.ly/2DDvnwP">Prepare a Fiber and place it on the Update queue</a>
    public static Fiber Start(
      string                  name           = null, [CallerMemberName] string callerName       = ""
    , [CallerFilePath] string callerFilePath = "",   [CallerLineNumber] int    callerLineNumber = 0) {
      if (controller == null) {
        controller = Components.Create<FiberController>("FiberController");
      }
      var fiber = Instance(BuildName(name, callerName, callerFilePath, callerLineNumber));
      fiber.Update = FirstUpdate;
      fiber.node.MoveTo(Queue.Update);
      fiber.disposeOnComplete = true;
      return fiber;
    }
    private static string BuildName(string name, string callerName, string callerFilePath, int callerLineNumber) {
      var fileName = Path.GetFileNameWithoutExtension(callerFilePath);
      if (name == null) return $"{callerName} ({fileName}:{callerLineNumber})";
      if (name.EndsWith(")")) return name;
      return $"{name} ({fileName}.{callerName}:{callerLineNumber})";
    }
    private static void FirstUpdate(Fiber fiber) => fiber.Go();

    /// <a href="http://bit.ly/2Rb9oEq">Start a fiber if it is not already running</a>
    public Fiber Go() {
      Go(NextAction);
      NextAction(this);
      return this;
    }

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
    public abstract class Closure : DelayedCache<Closure> {
      /// <a href="http://bit.ly/2NjSGNX">Fiber that will run / is running / has run in the context of this closure</a>
      public readonly Fiber Fiber;
      /// <a href="http://bit.ly/2NjSGNX">Emitter that is fired when the fiber completes all actions</a><inheritdoc/>
      public Emitter OnComplete => Fiber?.OnComplete;

      protected Closure() {
        Fiber = Fiber.Instance().Context(this);
        // ReSharper disable once VirtualMemberCallInConstructor
        Activities(Fiber);
        Fiber.Do(_ => Dispose());
      }

      /// <a href="http://bit.ly/2NjSGNX">Add all the steps you need to this override. It is called by the constructor.</a>
      protected abstract void Activities(Fiber fiberToUpdate);
    }

    /// <a href="http://bit.ly/2NjSGNX">Closure super-class that does all the smarts</a>
    public abstract class Closure<TS, TScope> : Closure where TS : DelayedCache<Closure> {
      /// <a href="http://bit.ly/2NjSGNX">Scope is available for 10 frames after OnComplete in case it holds response data</a>
      public TScope Scope;
      /// <a href="http://bit.ly/2NjSGNX">Calling this static will fetch a prepared fiber, add scope and run it.</a>
      public static Closure<TS, TScope> Go(TScope scope) {
        var instance = Cache<TS>.Instance as Closure<TS, TScope>;
        // ReSharper disable once PossibleNullReferenceException
        instance.Scope = scope;
        instance.Fiber.Go();
        return instance;
      }
    }

    /// <a href="http://bit.ly/2NjSGNX">Helper that is the same as fiber.WaitFor(closure.OnComplete)</a>
    public Fiber WaitFor(Closure closure) => WaitFor(closure.Fiber);
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
    public T Context<T>(string contextName) where T : class => context[contextName].Value as T;

    /// <a href="http://bit.ly/2RUcL2S">Set the context to an instance of a type</a>
    public Fiber Context<T>(string contextName, T value) where T : class {
      (context[contextName].Value as IDisposable)?.Dispose();
      context.Add(contextName, value);
      return this;
    }
    private readonly Map context = Map.Instance;
    #endregion

    #region Queues
    /// <a href="http://bit.ly/2Pqv2Ub">Return Fiber processing to frame Update queue</a>
    public Fiber OnUpdates => AddSameFrameAction(_ => node.MoveTo(Queue.Update), "OnUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to FixedUpdate queue</a>
    public Fiber OnFixedUpdates => AddSameFrameAction(_ => node.MoveTo(Queue.FixedUpdate), "OnFixedUpdates");
    /// <a href="http://bit.ly/2Pqv2Ub">Move Fiber processing to LateUpdate queue</a>
    public Fiber OnLateUpdates => AddSameFrameAction(_ => node.MoveTo(Queue.LateUpdate), "OnLateUpdates");

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
        AddSameFrameAction(_ => blockStack.Push(action ?? actions.First), "Begin");
        return this;
      }
    }

    /// <a href="http://bit.ly/2PtbezT">Begin/End block - use Break() to create an `if`</a>
    public Fiber End => AddBreakToAction().AddSameFrameAction(_ => blockStack.Pop(), "End");

    /// <a href="http://bit.ly/2DDvnNl">Begin/Again repeating operations. Use Break() or Exit() to leave</a>
    public Fiber Again => AddSameFrameAction(_ => action = blockStack.Top).End;

    /// <a href="http://bit.ly/2DDvp7V">Begin/Repeat loop for a specific number of times</a>
    public Fiber Repeat(int count) => Repeat(_ => count);

    /// <a href="http://bit.ly/2DDvp7V">Begin/Repeat loop for a specific number of times</a>
    public Fiber Repeat(Func<Fiber, int> countLambda) {
      var count   = 0;
      int counter = 0;
      return AddSameFrameAction(
        _ => {
          if (count == 0) count = countLambda(this) + 1;
          if (count == 0) return;
          var begin                            = blockStack.Top;
          if ((++counter % count) != 0) action = begin;
        }, "Repeat").End;
    }

    /// <a href="http://bit.ly/2CT634f">Loop until a value function returns true</a>
    public Fiber Until(Func<Fiber, bool> isTrue) =>
      AddSameFrameAction(
        _ => {
          var begin                 = blockStack.Top;
          if (!isTrue(this)) action = begin;
        }, "Until").End;

    /// <a href="http://bit.ly/2RDN05W">Break out of any block if a value function returns true</a>
    public Fiber BreakIf(Func<Fiber, bool> isBreak) =>
      AddSameFrameAction(
        _ => {
          if (isBreak(this)) Break();
        }, "BreakIf");

    /// <a href="http://bit.ly/2DDvlFd">Break a Begin/End/Repeat/Again block</a>
    public void Break() {
      while ((action?.Previous != null) && (action.Previous.Item.Actor != BreakTo)) action = action.Previous;
    }

    private void BreakTo(Fiber fiber) { }

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
      AddSameFrameAction(
        _ => {
          if (!isTrue(this)) Break();
        }, "If");

    /// <a href="http://bit.ly/2CU6Vp6">Standard If // Else // Then branch</a>
    public Fiber Else => AddSameFrameAction(_ => Break(2), "Else").AddBreakToAction();

    /// <a href="http://bit.ly/2CU6Vp6">Standard If // Else // Then branch</a>
    public Fiber Then => AddBreakToAction("Then");
    #endregion

    /// <a href="http://bit.ly/2DDZjbO">Business logic activation step</a>
    public Fiber Do(Action nextAction, string actionName = null) => AddSameFrameAction(nextAction, actionName);

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
      if (timeoutFiber == null) timeoutFiber = Instance().WaitFor(_ => secondsTimeout).Exit(this);
      timeoutFiber.Go();
      return this;
    }
    private float secondsTimeout;
    private Fiber timeoutFiber;

    /// <a href="http://bit.ly/2CV0RNn">Wait for another fiber to complete, starting it if needed - value set by return value of a function</a>
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
    public Fiber OnError(Action<string> actor, bool exit = false) {
      onError = actor;
      if (exit) exitOnError = true;
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
    private static Action<string> globalOnError = Debug.LogError;
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
        fiber.AddSameFrameAction(_ => { }, "Start");
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
      try {
        while (!fiber.NoMoreActions()) {
          var act = fiber.SetAction(fiber.action.Previous);
          act.Item.Actor(fiber);
          if (!act.Item.sameFrame) return;
        }
      } catch (Exception e) {
        fiber.onError(e.ToString());
        if (fiber.exitOnError) fiber.Exit();
      }
    }

    private bool NoMoreActions() {
      if (action?.Previous != null) return false;
      #if UNITY_EDITOR
      if (Debugging) Debug.Log($"OnComplete: for {node}");
      #endif
      OnComplete.Fire();
      Running = false;
      node.MoveTo(Queue.Waiting);
      if (disposeOnComplete) Dispose();
      return true;
    }

    private struct ActionItem {
      public string Name;
      public Action Actor;
      public bool   sameFrame;
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
    private        string                            name;
    internal       Action                            Update;

    private Fiber AddAction(Action newAction, string actionName = null) {
      actions.Add(new ActionItem {Name = actionName, Actor = newAction});
      return this;
    }

    private Fiber AddSameFrameAction(Action newAction, string actionName = null) {
      actions.Add(new ActionItem {Name = actionName, Actor = newAction, sameFrame = true});
      return this;
    }

    private Fiber AddBreakToAction(string actionName = null) {
      actions.Add(new ActionItem {Name = actionName, Actor = BreakTo, sameFrame = true});
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
    public Fiber Assert(Func<Fiber, bool> isOk, string message = "Assertion Failure") =>
      AddSameFrameAction(
        _ => {
          if (isOk(this)) return;
          Error(message);
          Exit();
        }, "Assert");
    public Fiber Assert(Func<Fiber, bool> isOk, Func<Fiber, string> message) =>
      AddSameFrameAction(
        _ => {
          if (isOk(this)) return;
          Error(message(this));
          Exit();
        }, "Assert");

    /// <a href="http://bit.ly/2DDvmZN">Displays Do() and action events on Unity console</a>
    public bool Debugging = false;

    /// <a href=""></a> //#TBD#//
    public Fiber DebugLog(bool on = true) {
      Debugging = on;
      return this;
    }

    /// <a href="http://bit.ly/2DDvmZN">Return Fiber contents and current state</a><inheritdoc />
    public override string ToString() => $"Name: {name} // Id: {id} // Actions: {ActionNames} // Queue: {node?.Owner}";

    //BeginAgainExample.IncrementCounter,BeginAgainExample.IncrementCounter,Fiber.<get_End>b__18_0,<>c.<.ctor>b__75_0,BeginAgainExample.IncrementCounter
    private string ActionNames {
      get {
        var array = new string[actions.Count];
        // ReSharper disable once LocalVariableHidesMember
        var node = actions.Last;

        for (var idx = 0; idx < array.Length; node = node.Previous, idx++) {
          var actionName = ActionName(node.Item);
          array[idx] = node == action ? $"[{actionName}]" : actionName;
        }
        return Csv.ToString(array);
      }
    }

    public Fiber Name(string fiberName) {
      name = fiberName;
      return this;
    }

    /// <a href="http://bit.ly/2NjzIHg">Write to the Unity console (optionally as a warning entry)</a>
    public Fiber Log(string message, bool warning = false) {
      AddSameFrameAction(
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
      AddSameFrameAction(
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