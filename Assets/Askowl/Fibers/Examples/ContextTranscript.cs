//- Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

//- Fibers are asynchronous. While Do methods can have access to the surrounding class this causes tight coupling and limited scope. Take service custom assets for example. A service will provide multiple entry points, each wrapped in a method. Most services are asynchronous, so will wait on response with a fiber. Each service method returns different types of information.
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
#if UNITY_EDITOR && Fibers
// ReSharper disable MissingXmlDoc

namespace Askowl.Transcripts {
  public class ContextTranscript {
    //- All service method responses can be assigned in the constructor. The anonymous classes created to generate method references all happen here and hence only once for the containing class instance. If the class is cached, this will only happen once per app execution.
    public ContextTranscript() => serviceMethodResponse = ServiceMethodResponse;

    //- This is a simulated service method where a response occurs after 1/10th of a second.
    [UnityTest] public IEnumerator ServiceMethod() {
      var context = ServiceMethodContext.Instance;
      context.Number = 12;
      yield return Fiber.Start.Context(context).WaitFor(seconds: 0.1f).Do(serviceMethodResponse).AsCoroutine();
    }
    //- Here is a simple context with only one value. Typically it will be data from both the service communications as well as the service if all is well.
    private class ServiceMethodContext : Cached<ServiceMethodContext> {
      public int Number;
    }
    //- This is the method called in the response. It is responsible for all the hard lifting needed with the information provided by the service.
    private void ServiceMethodResponse(Fiber fiber) {
      var context = fiber.Context<ServiceMethodContext>();
      Assert.AreEqual(12, context.Number);
    }
    private readonly Fiber.Action serviceMethodResponse;
  }
}
#endif