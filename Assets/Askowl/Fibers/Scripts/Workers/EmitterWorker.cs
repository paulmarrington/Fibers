// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

using System;
using UnityEngine;

namespace Askowl {
  public partial class Fiber {
    /// <a href="http://bit.ly/2Rb9pbs">Wait for an emitter to fire</a>
    public Fiber WaitFor(Emitter emitter, string name = null) =>
      AddAction(_ => EmitterWorker.Instance.Load(this, emitter), name ?? "WaitFor(Emitter)");

    /// <a href="http://bit.ly/2Rb9pbs">Wait for an emitter passed by function return to fire</a>
    public Fiber WaitFor(Func<Fiber, Emitter> getEmitter, string name = null) =>
      AddAction(_ => EmitterWorker.Instance.Load(this, getEmitter(this)), name ?? "WaitFor(Emitter)");

    /// <a href="http://bit.ly/2BeoK0X">Fire an emitter at this point in the Fiber sequence</a>
    public Fiber Fire(Emitter emitter) {
      AddSameFrameAction(_ => emitter.Fire());
      return this;
    }

    /// <a href="http://bit.ly/2BeoK0X">Fire an emitter at this point in the Fiber sequence</a>
    public Fiber Fire(Func<Fiber, Emitter> getEmitter) {
      AddSameFrameAction(_ => getEmitter(this).Fire());
      return this;
    }

    /// <a href="http://bit.ly/2B9DZrU">Cancel/Abort/Exit current fiber if an emitter fires</a>
    public Fiber CancelOn(Emitter emitter) {
      if (cancelOnFired == default) cancelOnFired = ExitOnFire;
      emitter.Listen(cancelOnFired, once: true);
      cancelOnEmitter = emitter;
      return this;
    }
    private Emitter        cancelOnEmitter;
    private Emitter.Action cancelOnFired;
    private void           ExitOnFire(Emitter emitter) => Exit();
    private void CancelOnAborted() {
      cancelOnEmitter?.Remove(cancelOnFired);
      cancelOnEmitter = default;
    }

    private class EmitterWorker : Worker<Emitter> {
      static EmitterWorker() => NeedsUpdates = false;

      public EmitterWorker() : base() => onNext = OnNext;

      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      EmitterWorker Instance  => Cache<EmitterWorker>.Instance;
      protected override void          Recycle() => Cache<EmitterWorker>.Dispose(this);

      protected override bool Prepare() {
        if (Seed != null) {
          Seed.Listen(onNext, once: true);
          return true;
        }
        Recycle();
        return false;
      }

      private readonly Emitter.Action onNext;
      private          void           OnNext(Emitter emitter) => Dispose();
    }
  }
}