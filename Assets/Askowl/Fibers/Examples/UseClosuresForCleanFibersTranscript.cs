// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- It turns out that precompiling fibers is not enough. A fiber will, but it's very nature, survive long after the code that called it has moved on. In OO a class instance is the closure. Fortunately, C# 7 has syntax that can make it concise with a minimum of visible scaffolding. If you use the the following pattern you can use fibers any time and anywhere without worrying about the typical multi-tasking concerns.
using System;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
#if AskowlTests
// ReSharper disable MissingXmlDoc

namespace Askowl.Fibers.Transcripts {
  public class UseClosuresForCleanFibersTranscript {
    //- I create an inner class to provide a closure for the fiber to run in. It inherits from the Fiber.Closure generic. The first generic argument is the new class so that underlying code can create it or reuse it from a cache. The second generic argument can be a struct, class or tuple. The latter is the most convenient. Like a struct it does not use the heap or garbage collector, but it doesn't need to be defined separately. A tuple is created with a list of items in brackets.
    private class SampleClosure : Fiber.Closure<SampleClosure, (int number, string text)> {
      //- Add and fill in the one abstract method
      protected override void Activities(Fiber fiber) =>
        //- And top up the already created fiber instance with the work you want to perform. The parameters provided when the fiber starts are available in Scope. Fiber step actions can be lambdas or references to methods. In both cases it takes a single parameter fiber and does not have a return value.
        fiber.Begin.WaitFor(seconds: 0.1f).Do(_ => Scope.number++).Repeat(5).Do(SetText);

      private void SetText(Fiber fiber) => Scope.text = $"Is {Scope.number}";
    }
    //- And that is it. For a simple fiber this only takes 4 lines

    [UnityTest] public IEnumerator FiberClosureTest() {
      //- Calling a fiber is as simple as calling the static Go routine. Note the double brackets. The inner one builds a tuple struct to send.
      var sampleClosure = SampleClosure.Go((33, ""));
      //- For code clarity you can add the tuple component names
      // sampleClosure = SampleClosure.Go((number: 33, text: ""));

      //- A fiber closure exposes the the OnComplete emitter and the fiber that was run. For WaitFor, neither is needed as there is a closure override.
      yield return Fiber.Start.WaitFor(sampleClosure).AsCoroutine();

      //- If we need the results later we will need to take a copy of the scope. Because a tuple is a struct, the assignment is a copy, not a reference.
      var scope = sampleClosure.Scope;
      Assert.AreEqual(39,      scope.number);
      Assert.AreEqual("Is 39", scope.text);
    }
  }

  //- I'll finish up with a real-world example. It is production code taken from the CustomAsset package for calling external asynchronous services. 
  public class Services<TS, TC> {
    public class Service {
      public Boolean Error = false;
      public Emitter Reset() => default;
    }
    public TS Next<T>()                    => default;
    public TI Instance<TI>() where TI : TS => default;

    //- The above is just placeholders for code in the service class. CallService below is the only public method needed to call the service.
    public Emitter CallService(Service service) => ServiceFiber.Go((this, Instance<TS>(), service)).OnComplete;

    //- The precompiled fiber wrapper is of type DelayedCache so that is can return itself to the recycling queue after completing it's task.
    private class ServiceFiber : Fiber.Closure<ServiceFiber, (Services<TS, TC> manager, TS server, Service service)> {
      //- We create the precompiled fiber in the Activities called from the constructor. Since this class is cached, there will only be as many copies as the maximum number of concurrent calls.
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
  }
}
#endif