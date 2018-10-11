// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using System;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber Emitter(Emitter emitter) => EmitterWorker.Instance.Load(this, emitter);

    /// <a href=""></a>
    public Fiber Emitter<T>(Emitter<T> emitter, Action<T> actionOnResult) {
      var emitterWorker = EmitterWorker<T>.Instance;
      return emitterWorker.Load(this, new EmitterWorker<T>.Payload { Emitter = emitter, OnResult = actionOnResult });
    }

    private class EmitterWorker : Worker<Emitter> {
      static EmitterWorker() => NeedsUpdates = false;

      public static      EmitterWorker Instance  => Cache<EmitterWorker>.Instance;
      protected override void          Recycle()  => Cache<EmitterWorker>.Dispose(this);
      protected override void          Prepare() => subscription = Data.Subscribe(new Observer { Owner = this });

      private IDisposable subscription;

      private struct Observer : IObserver {
        public EmitterWorker Owner;

        public void OnNext() => Owner.Dispose();

        public void OnCompleted() { }
      }

      public override void Dispose() {
        subscription.Dispose();
        base.Dispose();
      }
    }

    private class EmitterWorker<T> : Worker<EmitterWorker<T>.Payload> {
      /// <a href=""></a>
      public struct Payload {
        internal Emitter<T> Emitter;
        internal Action<T>  OnResult;
      }

      static EmitterWorker() => NeedsUpdates = false;

      public static   EmitterWorker<T> Instance => Cache<EmitterWorker<T>>.Instance;
      protected override void             Recycle() { Cache<EmitterWorker<T>>.Dispose(this); }

      private IDisposable subscription;

      protected override void Prepare() => subscription =
        Data.Emitter.Subscribe(new Observer { Owner = this, ActionOnResult = Data.OnResult });

      private struct Observer : IObserver<T> {
        public EmitterWorker<T> Owner;
        public Action<T>        ActionOnResult;
        public void             OnError(Exception error) { }
        public void             OnCompleted()            { }

        public void OnNext(T value) {
          ActionOnResult(value);
          Owner.Recycle();
        }
      }

      public override void Dispose() {
        subscription.Dispose();
        base.Dispose();
      }
    }
  }
}