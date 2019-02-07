// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using UnityEngine;

namespace Askowl {
  /// <a href="http://bit.ly/2B6jpZl">Cached C# Action instances using the observer pattern</a>
  public class Emitter : IDisposable {
    #region Instantiate
    /// <a href="http://bit.ly/2B6jpZl">Retrieve an emitter from recycling or new</a>
    public static Emitter Instance => Cache<Emitter>.Instance;

    /// <a href="http://bit.ly/2B6jpZl">Returns false to remove listener from future firings</a>
    public delegate void Action(Emitter emitter);

    /// <a href="http://bit.ly/2B8WXPC">Fire once then dispose of the emitter</a>
    public static Emitter SingleFireInstance {
      get {
        var emitter = Instance;
        emitter.isSingleFire = true;
        return emitter;
      }
    }
    private bool isSingleFire;
    #endregion

    #region Context
    /// <a href="http://bit.ly/2RUcL2S">The base context retriever returns the context as an object</a>
    public object Context() => context;

    /// <a href="http://bit.ly/2RUcL2S">Retrieve the context as a class type - null for none or wrong type</a>
    public T Context<T>() where T : class => context as T;

    /// <a href="http://bit.ly/2RUcL2S">Set the context to an instance of a type</a>
    public Emitter Context<T>(T value) where T : class {
      context = value;
      return this;
    }
    private object context;
    #endregion

    private readonly LinkedList<Action> listeners = new LinkedList<Action>();

    /// <a href="http://bit.ly/2B6jpZl">The owner shoots and all the listeners hear</a>
    public void Fire() {
      Firings++;
      LinkedList<Action>.Node next;
      for (node = listeners.First; node != null; node = next) {
        next = node.Next;
        node.Item(this);
      }
      if (isSingleFire) {
        Firings      = 0;
        isSingleFire = false;
        Dispose();
      }
    }

    /// <a href="http://bit.ly/2B6jpZl">Count of the number of times an emitter has fired</a>
    public int Firings;

    /// <a href="http://bit.ly/2B6jpZl">Ask an emitter to tell me too</a>
    public Emitter Listen(Action action) {
      listeners.Add(action);
      return this;
    }

    /// <a href="http://bit.ly/2TpBXuR">Remove a listener if it is in the list</a>
    public Emitter Remove(Action action) {
      for (node = listeners.First; node != null; node = node.Next) {
        if (node.Item != action) continue;
        node.Recycle();
        break;
      }
      return this;
    }

    /// <a href="http://bit.ly/2Tn8xxq">Remove the listener currently being acted on</a>
    public Emitter StopListening() {
      node.Recycle();
      return this;
    }
    private LinkedList<Action>.Node node;

    /// <a href="http://bit.ly/2B6jpZl">Removes all listeners</a>
    public void RemoveAllListeners() {
      listeners.Dispose();
      Firings = 0;
    }

    /// <a href="http://bit.ly/2B6jpZl">Return true if we have one or more listeners registered</a>
    public bool Waiting => listeners.Count > 0;

    /// <a href="http://bit.ly/2B6jpZl">Call when we are done with this emitter.</a> <inheritdoc />
    public void Dispose() {
      listeners.Dispose();
      (context as IDisposable)?.Dispose();
      Cache<Emitter>.Dispose(this);
    }

    public override string ToString() => $"Firings: {Firings}, Listeners: {listeners.Count}, Context: {context}";
  }
}