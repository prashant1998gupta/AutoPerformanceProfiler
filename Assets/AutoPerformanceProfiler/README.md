# Auto Performance Profiler Pro
*The Ultimate Automated Technical Director for Unity*

**Version:** 1.0.0
**Compatibility:** Unity 2021.3+ & 2023+ (Built-in, URP, HDRP)
**Render Pipelines:** All
**Platform:** PC, Mac, Mobile, WebGL, Console

---

## 🚀 Overview

Stop guessing why your game is lagging and stop digging through complicated raw Unity Profiler graphs. **Auto Performance Profiler Pro** acts as an AI Technical Director that automatically monitors your game, captures visual proof of lag spikes, tracks true VRAM memory leaks, and provides **One-Click Magic Fixes** directly to your Scene Hierarchy.

It translates raw data into plain English suggestions so Artists, Level Designers, and Developers can optimize scenes without needing a degree in computer science.

## ✨ Premium Features (Why this Tool is Elite)

1. **Auto-Fix Scene Optimization** 🪄
   - Features a **"✨ Auto-Fix"** button that structurally changes problematic GameObjects instantly. Instead of just telling you a light is bad, it configures it for you.
2. **CI/CD Pre-Build Guardian** 🛡️ (**NEW Enterprise Feature**)
   - Injects an `IPreprocessBuildWithReport` interceptor. If a developer accidentally clicks "Build & Run" while leaving a serious unoptimized artifact in the scene (like a God Object or 4k texture bloat), this tool will physically block the build compilation and print the exact error to the console. Ensures your release branch stays locked down.
3. **A/B Report Comparator** ⚖️ (**NEW Enterprise Feature**)
   - Load two separate profiling session reports (Before vs After) and automatically calculate percentage gains. See exact readouts like *"Freed 114MB of Texture Memory (12% decrease)"* to definitively prove optimization results.
4. **Hardware Budget Profiles** 📊 (**NEW**)
   - Stop guessing if 60ms CPU time is "good". Define custom ScriptableObject Budget Profiles (e.g. 'Mobile Native', 'Console 4K') that cap your acceptable FPS, GC bytes, and Triangle limits dynamically based on the target platform.
5. **Wireless Mobile Telemetry (Live TCP Streaming)** 📱 (**NEW**)
   - Connect your physical iOS/Android device over Wi-Fi and stream live performance metrics directly back into the Unity Editor UI. No more testing with USB cables that cause false thermal-throttling or skew your profiling results!
5. **Global Project Validator** ⚙️ 
   - Scans your global Unity settings for architecture flaws (like deploying without IL2CPP, or leaving Mobile VSync on natively causing input drag). With one click, it automatically switches script backends and rebuilds graphics settings for performance!
6. **Texture Asset Bloat Scanner** 🖼️ 
   - Plunges into material depths analyzing underlying texture assets. Finds hidden 4K/8K images destroying VRAM silently and intelligently downscales the hardware asset cap backwards to 2048 without destroying original art.
7. **The Ultimate Hierarchy Scanner (30+ Deep Checks)** ⛰️ 
   - Automatically detects: Realtime Point Light Shadows, Particle System Overdraw (5000+), Duplicate EventSystems, Empty C# Update Loops, Missing Object Pools, UI MipMap Memory Waste, Audio Load Freezes, Multiple AudioListeners, Legacy UI Text, Dynamic Non-Convex Mesh Colliders, Broadphase BoxCollider Bloat, Missing LODs, UI Raycast Target Bloat, Reflection Probes updating every frame, extreme Camera Far Clip depth exhausts, Rigidbody Interpolation wastes, Audio Stereo wastes, Multi-material Draw Call killers, and more!
8. **Interactive Build-Size Explorer** 📦 (**NEW**)
   - Scans your entire `/Assets/` folder, calculates real disk byte-sizes per category (Textures, Audio, Meshes, Shaders, Other), and draws a proportional 2D Treemap. The **Auto-Crunch Compress** button actually enables `crunchedCompression` on every oversized TextureImporter, genuinely shrinking your APK.
9. **Shader Complexity & Overdraw Heatmaps** 🔥 (**NEW**)
   - One-Click Scene View visualization swap. Programmatically switches your Scene View between Overdraw, Wireframe, or Shaded Wireframe modes via Unity's `DrawCameraMode` API to visually pinpoint GPU-killing geometry and transparent overdraw in real time.
10. **Data Exporting (JSON / CSV / Flame Graph)** 💾
    - Export profiling sessions to `.csv` spreadsheets, `.json` for CI/CD pipelines, or generate a standalone **interactive HTML Flame Graph** with color-coded FPS and CPU charts that you can open in any browser and email directly to stakeholders.
11. **Visual Frame Snapshots (Spike Capture)** 📸
    - When FPS drops below 30 or massive Garbage Collection fires, it takes a micro-screenshot of the Game View. See what caused the lag visually instead of reading numbers.
12. **Ghost Memory & Object Leak Scanner** 🕵️
    - Real-time detection of "Ghost Objects" — textures and audio clips loaded in RAM but not referenced by the active scene. Scan, ping, and destroy orphaned assets with one click to reclaim memory.
13. **Deep Project Asset Scanner** 🛠️ (**NEW**)
    - Scans your entire `/Assets/` folder for massive Audio files (>5MB) and 4K+ Textures that silently bloat your builds, even if they aren't placed in any scene. Results sorted by size with one-click Ping to Project.

---

## 📘 Quick Start Guide

### How to use the Editor Window
1. Open via top menu: **Window > Analysis > Auto Performance Profiler Pro**.
2. Click **"Inject System"** and press **"Start Rec"** to begin recording data.
3. **Play your game** and trigger the heavy actions (combat, loading a level).
4. Click **"Stop & Analyze"**.
5. The window will immediately populate with beautifully formatted data cards, line graphs, and action items.

### How to use the Auto-Fix Scanner (No Play-Mode Required)
1. Open the Profiler Window via **Window > Analysis > Auto Performance Profiler Pro**.
2. Under the *Awaiting Profiler Data* placeholder, click **"Run Offline Scene Scan"**.
3. The tool parses your scene across massive severe performance metrics. 
4. Go to the **🎯 Targets to Optimize** tab and review high/medium risks.
5. Click **✨ Auto-Fix** next to any warning (Global settings or scene objects) to have it completely resolved.

---

## 🔍 Architecture & Modularity

- **Runtime Assembly**: Contains `PerformanceTracker`, designed with zero-allocation `ProfilerRecorder` unmanaged memory pointers. Generates *0 bytes of GC* overhead whilst monitoring.
- **Editor Assembly**: Dark-mode GUI layout wrapped directly in high speed `FindObjectsByType` offline evaluators for instant hierarchy parsing. Compatible inherently with the newest `NamedBuildTarget` Unity pipelines.

**Made for the Unity Asset Store.**
*Code architecture strictly adheres to standard C# naming conventions, uses strict precompiled Assembly Definitions to drop C# compile times to 0, and introduces one-click asset downscaling.*
