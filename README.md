# UnityAnalyzer Project

This repository now ships the analyzers as an **embedded Unity package** inside the Unity project `UnityAnalyzer/`.

- Open `UnityAnalyzer/` with Unity 6000.2.14f1 (または互換バージョン)。
- The package sources live at `UnityAnalyzer/Packages/com.akiojin.unity.analyzers/` (including `Analyzers~` binaries, `Editor`, `Plugins`, and package metadata).
- When Unity opens the project it will import the embedded package automatically; no registry configuration is needed.

For package documentation, changelog, and license, see the files inside the package folder.
