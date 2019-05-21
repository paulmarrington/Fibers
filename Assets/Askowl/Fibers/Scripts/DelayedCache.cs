// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using Askowl;
/// <a href="http://bit.ly/2BkDrzH">Disposed does not recycle object immediately</a>
public class DelayedCache<T> : IDisposable where T : DelayedCache<T> {
  /// <a href="http://bit.ly/2BkDrzH">It behaves like Askowl.Cached from Able</a>
  public static T Instance => Cache<T>.Instance;

  /// <a href="http://bit.ly/2BkDrzH">How many Unity frames do we delay for (default 10)</a>
  public int Frames = 10;

  protected DelayedCache() =>
    disposalFiber = Fiber.Instance().SkipFrames(Frames).Do(_ => Cache<T>.Dispose(this as T));

  private readonly Fiber disposalFiber;

  public void Dispose() => disposalFiber.Go();
}