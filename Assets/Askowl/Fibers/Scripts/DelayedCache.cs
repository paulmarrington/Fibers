// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using Askowl;
/// <a href=""></a> //#TBD#//
public class DelayedCache<T> : IDisposable where T : DelayedCache<T> {
  /// <a href=""></a> //#TBD#//
  public static T Instance => Cache<T>.Instance;

  /// <a href=""></a> //#TBD#//
  public int Frames = 10;

  protected DelayedCache() => disposalFiber = Fiber.Instance.SkipFrames(Frames).Do(_ => Cache<T>.Dispose(this as T));

  private readonly Fiber disposalFiber;

  /// <a href=""></a> //#TBD#//
  public void Dispose() => disposalFiber.Go();
}