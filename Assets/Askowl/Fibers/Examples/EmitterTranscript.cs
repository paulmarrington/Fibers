//- Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- The Emitter class has been moved from Able to Fibers and given a bit of an overhaul. As the primary inter-fiber communications method it holds more prominence in release 2.0 of fibers.
using System;
using NUnit.Framework;
#if UNITY_EDITOR && Fibers
// ReSharper disable MissingXmlDoc

namespace Askowl.Transcripts {
  public class EmitterTranscript {
    //- On the face of it an emitter has one function. Accept listeners and call them when fire is called.
    [Test] public void Basic() {
      bool emitterFired = false;
      //- A listener function is called when the emitter is fired. It is given a reference to the emitter in case it is called out of context. If it returns false it is removed from the list and never called again.
      bool emitterAction(Emitter _) {
        emitterFired = true;
        return false;
      }
      //- An emitter is cached and disposable. In practice a `using` statement is unlikely in an asynchronous world.
      using (var emitter = Emitter.Instance.Listen(emitterAction)) {
        Assert.IsFalse(emitterFired);
        emitter.Fire();
        Assert.IsTrue(emitterFired);
      }
    }

    //- There is a problem with the example above. Emitters need to be efficient as they may be called hundreds of times a seconds. The problem here is `Listen(emitterAction)`. Whenever a function is turned into a delegate an anonymous class is created, exercising the garbage collector. Any form of delegate acts the same - lambdas, inner functions, instance or static members, anything. Unless you are absolutely certain that Listen will not be called often it is best to cache the delegate.
    private static bool emitterFiredStatic;
    //- The anonymous class is only instantiated once when the class is first loaded, then reused as needed.
    private static readonly Emitter.Action emitterActionStatic = emitter => {
      emitterFiredStatic = true;
      return false;
    };

    [Test] public void StaticListener() {
      using (var emitter = Emitter.Instance.Listen(emitterActionStatic)) {
        emitter.Fire();
        Assert.IsTrue(emitterFiredStatic);
      }
    }

    //- `emitterFiredInstance` is static because otherwise we would have to create the listener in the constructor. I often do it that way.
    private bool emitterFiredInstance;
    //- The anonymous class is only instantiated once when the class is first loaded, then reused as needed.
    private Emitter.Action emitterActionInstance;

    [Test] public void InstanceListener() {
      if (emitterActionInstance == default) {
        emitterActionInstance = emitter => {
          emitterFiredInstance = true;
          return false;
        };
      }
      using (var emitter = Emitter.Instance.Listen(emitterActionInstance)) {
        emitter.Fire();
        Assert.IsTrue(emitterFiredInstance);
      }
    }

    //- That works if the data is class-centric, but what if it needs to be specific to the method? Fortunately an emitter can hold context. By making this context disposable, it is possible to cache and reuse it rather than relying on garbage collection. WHen an emitter is disposed of, it's context is also - returning the data object to the cache for reuse.
    private class TransferData : Cached<TransferData> {
      public bool EmitterFired;
    }
    private static readonly Emitter.Action emitterActionWithContext = emitter => {
      emitter.Context<TransferData>().EmitterFired = true;
      return false;
    };
    [Test] public void ContextListener() {
      var data = Cache<TransferData>.Instance;
      data.EmitterFired = false;
      using (var emitter = Emitter.Instance.Listen(emitterActionWithContext)) {
        emitter.Context(data);
        emitter.Fire();
        Assert.IsTrue(data.EmitterFired);
      }
    }
  }
}
#endif