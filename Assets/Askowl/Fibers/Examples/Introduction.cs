// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

using Askowl;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
/// Servicing "Introduction to Fibers" scene
public class Introduction : MonoBehaviour {
  [SerializeField] private Toggle testsAvailable = default;

  private void Start() {
    #if AskowlFibers
    testsAvailable.isOn = true;
    #else
    testsAvailable.isOn = false;
    #endif
  }

  /// Checkbox to enable or disable tests
  public void EnableTests() {
    DefineSymbols.AddOrRemoveDefines(testsAvailable.isOn, "AskowlFibers;Fibers");
    AssetDatabase.Refresh();
  }

  /// When "Home Page" button is pressed
  public void HomePageButton() => Application.OpenURL("http://www.askowl.net/unity-fibers");

  /// When "Home Page" button is pressed
  public void DocumentationButton() =>
    Application.OpenURL("https://paulmarrington.github.io/Unity-Documentation/Fibers/");

  /// When "Home Page" button is pressed
  public void VideosButton() =>
    Application.OpenURL("https://www.youtube.com/playlist?list=PLufDQjKXLJJpT2Vkz6LJPyrBkm8HNL4UP");

  /// When "Home Page" button is pressed
  public void DoxygenButton() => Application.OpenURL(
    "https://paulmarrington.github.io/Unity-Documentation/Fibers/Doxygen/html/annotated.html");

  /// When "Home Page" button is pressed
  public void SupportButton() => Application.OpenURL("https://www.patreon.com/paulmarrington");
}