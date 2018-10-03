// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  using System;

  public partial class Fiber {
    /// <a href=""></a>
    public Fiber Emitter(Emitter emitter) => EmitterWorker.Instance(this, emitter);

    /// <a href=""></a>
    public Fiber Emitter<T>(Emitter<T> emitter, Action<T> actionOnResult) => EmitterWorker<T>.Instance(
      this, new EmitterWorker<T>.Payload { Emitter = emitter, OnResult = actionOnResult });

    private class EmitterWorker : Worker<Emitter> {
      static EmitterWorker() => NeedsUpdates = false;
      private            IDisposable subscription;
      protected override void        Prepare() { subscription = Data.Subscribe(new Observer { Owner = this }); }

      private struct Observer : IObserver {
        public EmitterWorker Owner;
        public void          OnNext()      => Owner.Unload();
        public void          OnCompleted() { }
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
      private IDisposable subscription;

      protected override void Prepare() {
        subscription = Data.Emitter.Subscribe(new Observer { Owner = this, ActionOnResult = Data.OnResult });
      }

      private struct Observer : IObserver<T> {
        public EmitterWorker<T> Owner;
        public Action<T>        ActionOnResult;
        public void             OnError(Exception error) { }
        public void             OnCompleted()            { }

        public void OnNext(T value) {
          ActionOnResult(value);
          Owner.Unload();
        }
      }

      public override void Dispose() {
        subscription.Dispose();
        base.Dispose();
      }
    }
  }
}