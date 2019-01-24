// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;

namespace Askowl {
  /// <a href="http://bit.ly/2OzDM9D">Cached C# Action instances using the observer pattern</a>
  public class Emitter : IDisposable {
    #region Instantiate
    /// <a href="http://bit.ly/2OzDM9D">Retrieve an emitter from recycling or new</a>
    public static Emitter Instance => Cache<Emitter>.Instance;

    /// <a href=""></a> //#TBD#//
    public static Emitter SingleFireInstance {
      get {
        var emitter = Instance;
        if (emitter.releaseEmitterAction == default) { // fiber.dispose calls context/emitter.dispose
          // can't put it in constructor 'cause fiber calls emitter...
          Fiber releaseEmitterFiber = Fiber.Instance.SkipFrames(2).Do(fiber => fiber.Dispose());
          emitter.releaseEmitterAction = () => releaseEmitterFiber.Go();
        }
        emitter.Context(emitter).Subscribe(emitter.releaseEmitterAction);
        return emitter;
      }
    }
    private Action releaseEmitterAction;
    #endregion

    #region Context
    /// <a href=""></a> //#TBD#//
    public T Context<T>() where T : class => context as T;

    /// <a href=""></a> //#TBD#//
    public Emitter Context<T>(T value) where T : class {
      context = value;
      return this;
    }
    private object context;
    #endregion

    /// <a href=""></a> //#TBD#//
    private readonly Fifo<Action> listeners = new Fifo<Action>();

    /// <a href="http://bit.ly/2OzDM9D">The owner shoots and all the listeners hear</a>
    public void Fire() {
      Firings++;
      for (var idx = 0; idx < listeners.Count; idx++) listeners[idx]();
    }

    /// <a href=""></a> //#TBD#//
    public int Firings;

    /// <a href="http://bit.ly/2OzDM9D">Ask an emitter to tell me too</a>
    public Emitter Subscribe(IObserver observer) => Subscribe(observer.OnNext);

    /// <a href="http://bit.ly/2OzDM9D">Ask an emitter to tell me too</a>
    public Emitter Subscribe(Action action) {
      listeners.Push(action);
      return this;
    }

    /// <a href=""></a> //#TBD#//
    public void RemoveAllListeners() {
      listeners.Count = 0;
      Firings         = 0;
    }

    /// <a href=""></a> //#TBD#//
    public bool Waiting => listeners.Count > 0;

    /// <a href="http://bit.ly/2OzDM9D">Call when we are done with this emitter.</a> <inheritdoc />
    public void Dispose() {
      listeners.Dispose();
      (context as IDisposable)?.Dispose();
      Cache<Emitter>.Dispose(this);
    }
  }

  /// <a href="http://bit.ly/2OzDM9D">Observer pattern without a payload</a>
  public interface IObserver {
    /// <a href="http://bit.ly/2OzDM9D">Get the next listener</a>
    void OnNext();

    /// <a href="http://bit.ly/2OzDM9D">Called when the emitter is discarded</a>
    void OnCompleted();
  }
}