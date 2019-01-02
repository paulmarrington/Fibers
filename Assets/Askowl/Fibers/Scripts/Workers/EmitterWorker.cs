// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

using System;

namespace Askowl {
  public partial class Fiber {
    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Emitter emitter, string name = null) =>
      AddAction(_ => EmitterWorker.Instance.Load(this, emitter), name ?? "WaitFor(Emitter)");

    /// <a href=""></a> //#TBD#//
    public Fiber WaitFor(Func<Fiber, Emitter> getEmitter, string name = null) =>
      AddAction(_ => WaitFor(getEmitter(this)), name ?? "WaitFor(Emitter)");

    private class EmitterWorker : Worker<Emitter> {
      static EmitterWorker() => NeedsUpdates = false;

      // ReSharper disable once MemberHidesStaticFromOuterClass
      public static      EmitterWorker Instance  => Cache<EmitterWorker>.Instance;
      protected override void          Recycle() => Cache<EmitterWorker>.Dispose(this);

      protected override bool Prepare() {
        bool nothingWaiting = Seed.Firings == 0;
        if (nothingWaiting) Seed.Subscribe(OnFire);
        Seed.Firings = 0;
        return nothingWaiting; // drop through if emission already happened
      }

      private void OnFire() {
        Seed.RemoveAllListeners();
        Dispose();
      }
    }
  }
}