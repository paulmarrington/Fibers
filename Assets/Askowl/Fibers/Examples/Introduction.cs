// Copyright 2019 (C) paul@marrington.net http://www.askowl.net/unity-packages

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Askowl.Fibers.Examples {
  /// Servicing "Introduction to Fibers" scene
  public class Introduction : MonoBehaviour {
    [SerializeField] private Toggle testsAvailable = default;

    private bool inStart;

    private void Start() {
      inStart = true;
      #if AskowlTests
      testsAvailable.isOn = true;
      #else
      testsAvailable.isOn = false;
      #endif
      inStart = false;
    }

    /// Checkbox to enable or disable tests
    public void EnableTests() {
      if (inStart) return;
      var value = testsAvailable.isOn ? 1 : 0;
      File.WriteAllText("Assets/Askowl/Askowl.json", $"{{\"EnableTesting\": {value}\n\"Update\": \"{DateTime.Now}\"}}");
      EditorApplication.isPlaying = false;
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

  /// <inheritdoc />
  [InitializeOnLoad] public sealed class AskowlTest : DefineSymbols {
    static AskowlTest() => EditorApplication.playModeStateChanged += OnPlayModeState;
    private static void OnPlayModeState(PlayModeStateChange state) {
      if (state == PlayModeStateChange.EnteredEditMode) {
        using (var json = Json.Instance.Parse(File.ReadAllText("Assets/Askowl/Askowl.json"))) {
          var enableTests = json.Node.To("EnableTesting").Found && (json.Node.Long == 1);
          AddOrRemoveDefines(enableTests, "AskowlTests");
        }
      }
    }
  }
}