// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable ClassNeverInstantiated.Local, ClassNeverInstantiated.Global

namespace Askowl {
  public partial class Fiber {
    /// <a href=""></a>
    public Fiber WaitFor(Emitter emitter) => EmitterWorker.Instance.Load(this, emitter);

    private class EmitterWorker : Worker<Emitter> {
      static EmitterWorker() => NeedsUpdates = false;

      public static      EmitterWorker Instance  => Cache<EmitterWorker>.Instance;
      protected override void          Recycle() => Cache<EmitterWorker>.Dispose(this);
      protected override void          Prepare() => Seed.Subscribe(Dispose);
    }
  }
}