// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if !ExcludeAskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class IfElseThenExample {
    [UnityTest, Timeout(10000)] public IEnumerator IfThen() {
      int mark = 0;
      // ReSharper disable once AccessToModifiedClosure
      var fiber = Fiber.Instance().If(_ => mark == 1).Do(_ => mark = 2).Then;
      fiber.Go();
      yield return new WaitForSeconds(0.2f);
      Assert.AreEqual(0, mark);

      mark = 1;
      fiber.Go();
      yield return new WaitForSeconds(0.2f);
      Assert.AreEqual(2, mark);
    }

    [UnityTest, Timeout(10000)] public IEnumerator IfElseThen() {
      int mark = 0;
      // ReSharper disable once AccessToModifiedClosure
      var fiber = Fiber.Instance().If(_ => mark == 1).Do(_ => mark = 2).Else.Do(_ => mark = 3).Then;

      mark = 0;
      fiber.Go();
      yield return new WaitForSeconds(0.2f);
      Assert.AreEqual(3, mark);

      mark = 1;
      fiber.Go();
      yield return new WaitForSeconds(0.2f);
      Assert.AreEqual(2, mark);
    }
  }
}
#endif