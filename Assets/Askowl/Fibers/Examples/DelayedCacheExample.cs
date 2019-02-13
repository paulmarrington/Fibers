// Copyright 2018 (C) paul@marrington.net http://www.askowl.net/unity-packages

// ReSharper disable MissingXmlDoc

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if AskowlTests
namespace Askowl.Fibers.Examples {
  public sealed class DelayedCacheExample {
    private class DelayedCachedData : DelayedCache<DelayedCachedData> {
      public int Number;
    }

    [UnityTest] public IEnumerator DelayedCache() {
      var data = DelayedCachedData.Instance;
      data.Frames = 10;
      data.Number = 11;
      var node       = Cache<DelayedCachedData>.Entries.ReverseLookup(data);
      var recycleBin = node.Owner.RecycleBin;

      data.Dispose();

      yield return new WaitForSeconds(0.1f);
      Assert.IsFalse(recycleBin == node.Owner);

      yield return new WaitForSeconds(0.1f);
      Assert.IsTrue(recycleBin == node.Owner);
    }
  }
}
#endif