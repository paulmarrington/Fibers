// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if AskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class ContextExample {
    private class FiberContext : IDisposable {
      public int  Number;
      public void Dispose() => Number = 0;
    }

    [UnityTest] public IEnumerator Context() {
      var fiberContext = new FiberContext {Number = 12};
      var fibre = Fiber.Start.Context(fiberContext).Context("name here", "a string").WaitFor(seconds: 0.1f).Do(
        fiber => {
          var context = fiber.Context<FiberContext>();
          Assert.AreEqual(12,         context.Number);
          Assert.AreEqual("a string", fiber.Context<string>("name here"));
        });
      yield return new WaitForSeconds(0.2f);
      // proving that the context is also disposed
      Assert.IsNull(fibre.Context<FiberContext>());
    }
  }
}
#endif