// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- It turns out that precompiled fibers is not enough. A fiber will, but it's very nature, survive long after the code that called it has moved on. Any context it keeps must have a lifetime of it's own. Using methods as closures as we would in a dynamic language doesn't work with C# since the data is in a struct frozen at the time of fiber creation. In OO a class instance is the closure. Fortunately C# 7 has syntax that can make it concise with a minimum of visible scaffolding. This example is production code taken from the CustomAsset package for calling external asynchronous services.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
#if AskowlTests
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Transcripts {
  public class Services<TS, TC> {
    public class Service {
      public Boolean Error = false;
      public Emitter Reset() => default;
    }
    public TS Next<T>()                    => default;
    public TI Instance<TI>() where TI : TS => default;

    //- The above is just placeholders for code in the service class. CallService is the only public method needed to call the service. Note the double-brackets around the parameters. This creates a single C#7 tuple parameter. A tuple is a struct, so no subtle class creation or destruction is happening.
    public Emitter CallService(Service service) =>
      CallServiceFiber.Go((this, Instance<TS>(), service)).OnComplete; //#TBD#//

    //- The precompiled fiber wrapper is of type DelayedCache so that is can return itself to the recycling queue after completing it's task.
    private class
      CallServiceFiber : Fiber.Closure<CallServiceFiber, (Services<TS, TC> manager, TS server, Service
        service)> {
      //- We create the precompiled fiber in the Activities called from the constructor. Create once and use many times.
      protected override void Activities(Fiber fiber) =>
        fiber.Begin
             .WaitFor(
                _ => MethodCache.Call(
                       Scope.server, "Call", new object[] {
                         Scope.service.Reset()
                       }) as Emitter)
             .Until(_ => !Scope.service.Error || ((Scope.server = Scope.manager.Next<TS>()) == null));

      //- The single static method Go gets a precompiled fiber from the cache, prepares it and runs it in one fluid movement. It returns a reference to the CallServiceFiber instance we are using, providing access to the OnComplete emitter and the scope.
    }
    //- In case you are interested, MethodCache calls a method overload based in the data type of the single parameter. It returns null for synchronous and an emitter for asynchronous functions. WaitFor continues immediately if the emitter is null.
    
    //- Once more quick example. A copy of the scope is kept in the closure instance. If it is used for request and response, and the response is itself not an object, the we need to access the scope to review the results. You have 10 frames after OnComplete fires to access anything you need from the scope.
    private class SampleClosure : Fiber.Closure<SampleClosure, (int number, string text)> {
      protected override void Activities(Fiber fiber) =>
        fiber.Begin.WaitFor(seconds: 0.1f)
             .Do(_ => Scope.number++).Repeat(5).Do(_ => Scope.text = $"Is {Scope.number}");
    }
    [UnityTest] public IEnumerator FiberClosure() {
      var sampleClosure1 = SampleClosure.Go((0, ""));
      var fiber1 = Fiber.Start
                        .WaitFor(sampleClosure1.OnComplete)
                        .Do(_ => Assert.AreEqual(6, sampleClosure1.Scope.number));

      var fiber2 = Fiber.Start.WaitFor(SampleClosure.Go((12, "")));

      yield return fiber1.AsCoroutine();

      yield return new WaitForSeconds(0.2f);

      var sampleClosure3 = SampleClosure.Go((33, ""));
      yield return Fiber.Start
                        .WaitFor(sampleClosure3.OnComplete)
                        .Do(_ => Assert.AreEqual(39, sampleClosure3.Scope.number))
                        .AsCoroutine();

      yield return fiber2.AsCoroutine();
    }
  }
}
#endif