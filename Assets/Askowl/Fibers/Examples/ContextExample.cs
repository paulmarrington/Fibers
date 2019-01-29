// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR && AskowlFibers
namespace Askowl.Examples {
  public sealed class ContextExample {
    private class FiberContext : IDisposable {
      public int  Number;
      public void Dispose() => Number = 0;
    }

    [UnityTest] public IEnumerator Context() {
      var fiberContext = new FiberContext {Number = 12};
      Fiber.Start.Context(fiberContext).WaitFor(seconds: 0.1f).Do(
        fiber => {
          var context = fiber.Context<FiberContext>();
          Assert.AreEqual(12, context.Number);
        });
      yield return new WaitForSeconds(0.2f);
      Debug.Log($"*** Context ''"); //#DM#// 
      // proving that the context is also disposed
      Assert.AreEqual(0, fiberContext.Number);
    }
  }
}
#endif