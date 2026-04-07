<![CDATA[# 🚀 Auto Performance Profiler Pro

**The Ultimate Automated Technical Director for Unity**

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com)
[![License](https://img.shields.io/badge/License-Commercial-green.svg)](#license)
[![Version](https://img.shields.io/badge/Version-2.0.0-orange.svg)](#changelog)

> Stop guessing why your game is lagging. **Auto Performance Profiler Pro** acts as an AI Technical Director that automatically monitors your game, captures visual proof of lag spikes, tracks true VRAM memory leaks, and provides **One-Click Magic Fixes** directly to your Scene Hierarchy.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [System Requirements](#-system-requirements)
- [Installation](#-installation)
- [Quick Start Guide](#-quick-start-guide)
- [Architecture](#-architecture)
- [Editor Window Tabs](#-editor-window-tabs)
- [Right-Click Context Menus](#-right-click-context-menus)
- [Scene View Overlay](#-scene-view-overlay)
- [CI/CD Integration](#-cicd-integration)
- [Data Export Formats](#-data-export-formats)
- [Extending the Tool](#-extending-the-tool)
- [Performance Overhead](#-performance-overhead)
- [Troubleshooting & FAQ](#-troubleshooting--faq)
- [Changelog](#-changelog)
- [License](#-license)
- [Support](#-support)

---

## 🔭 Overview

**Auto Performance Profiler Pro** is a premium Unity Editor extension that transforms complex performance profiling into an intuitive, visual, and automated workflow. It translates raw profiler data into plain-English suggestions so Artists, Level Designers, and Developers can optimize their scenes without needing deep technical profiling expertise.

### What Makes This Tool Different?

| Traditional Unity Profiler | Auto Performance Profiler Pro |
|---|---|
| Raw data graphs you must interpret | AI-generated plain-English summaries |
| Manual bottleneck identification | 30+ automated detection rules |
| No fix suggestions | One-click ✨ Auto-Fix buttons |
| Play Mode only | Offline Scene Scanning (no Play Mode) |
| Desktop only | Wireless mobile TCP streaming |
| No build protection | CI/CD Pre-Build Guardian |
| No visual spike capture | Automatic screenshot on frame drops |
| Single-session analysis | A/B Report Comparison |

---

## ✨ Key Features

### 🪄 Auto-Fix Scene Optimization
Click **✨ Auto-Fix** next to any detected issue and watch the tool structurally reconfigure problematic GameObjects. Instead of just telling you a light is misconfigured, it fixes it for you — setting shadows to `None`, enabling Static batching, adjusting Animator culling modes, capping particle counts, and more.

### 🛡️ CI/CD Pre-Build Guardian (Enterprise)
An `IPreprocessBuildWithReport` interceptor that **physically blocks the Unity Build pipeline** if unresolved High-severity performance violations exist. Prevents unoptimized builds from ever reaching QA or production.

### ⚖️ A/B Report Comparator (Enterprise)
Load two `ProfilerReport` ScriptableObjects side-by-side and automatically calculate percentage deltas. Instantly generate documentation like *"Decreased GPU Batches by 14% and freed 256 MB of VRAM."*

### 📊 Hardware Budget Profiles
Define custom `HardwareBudgetProfile` ScriptableObjects (e.g., *Mobile Low-End*, *Console 4K*, *PC Ultra*) that cap acceptable FPS, GC bytes, triangle counts, and VRAM limits. The profiler dynamically shifts all thresholds and warnings based on the active profile.

### 📱 Wireless Mobile Telemetry (Live TCP)
Stream live performance metrics from a physical iOS/Android device **over Wi-Fi** directly into the Unity Editor UI. No USB cable required — eliminating thermal throttling artifacts that skew profiling data.

### ⚙️ Global Project Validator
Scans your Unity Project Settings for architecture flaws:
- Mono scripting backend instead of IL2CPP
- Mobile VSync causing input drag
- Missing Incremental GC
- Suboptimal managed stripping levels
- GPU Skinning disabled on PC

### 🖼️ Texture Asset Bloat Scanner
Dives deep into material textures to find hidden 4K/8K images silently destroying VRAM. Intelligently downscales the hardware asset cap to 2048 without destroying original art assets.

### ⛰️ Hierarchy Scanner (30+ Deep Checks)
Automatically detects critical performance killers across your entire scene:

| Category | Checks |
|---|---|
| **Lighting** | Realtime Point/Spot shadows, missing baked GI, excessive point lights |
| **Rendering** | Missing LODs, multi-material draw calls, 4K+ texture bloat, mesh Read/Write waste |
| **Physics** | Dynamic non-convex mesh colliders, massive broadphase BoxColliders, Rigidbody Interpolation waste |
| **UI** | Legacy `UI.Text`, Raycast Target bloat, Canvas Pixel Perfect, MipMap VRAM waste, >10 LayoutGroups |
| **Audio** | Decompress On Load abuse, missing Load In Background, stereo 3D waste |
| **Architecture** | God Objects (10+ components), empty C# Update loops, GetComponent in Update, missing object pools |
| **System** | Multiple AudioListeners, duplicate EventSystems, camera far clip exhaustion, Reflection Probe every-frame |

### 📦 Interactive Build-Size Explorer
Scans your entire `/Assets/` folder, categorizes files by type (Textures, Audio, Meshes, Shaders, Other), calculates byte-sizes, and renders a proportional 2D Treemap. The **Auto-Crunch Compress** button enables `crunchedCompression` on every oversized `TextureImporter` to genuinely shrink your APK.

### 🔥 Shader Complexity & Overdraw Heatmaps
One-click Scene View visualization swaps via Unity's `DrawCameraMode` API:
- **Overdraw** → Highlights transparent pixel stacking that kills GPU fill rate
- **Wireframe** → Shows raw mesh topology to spot high-poly objects
- **Shaded Wireframe** → Texture + wireframe overlay

### 💾 Data Export (JSON / CSV / Flame Graph)
- **CSV**: Flattened per-frame spreadsheet with 13 columns
- **JSON**: Full `ProfilerReport` serialization for CI/CD pipelines  
- **HTML Flame Graph**: Standalone interactive bar charts (FPS + CPU) with color-coded bars and hover tooltips

### 📸 Visual Frame Snapshots (Spike Capture)
When FPS drops below threshold or massive GC fires, the tool captures a screenshot of the Game View. See what the player was looking at when the lag spike occurred — not just numbers.

### 🕵️ Ghost Memory & Object Leak Scanner
Real-time detection of "Ghost Objects" — textures and audio clips loaded in RAM but not referenced by the active scene. Scan, ping, and destroy orphaned assets with one click to reclaim memory.

### 🛠️ Deep Project Asset Scanner
Scans your entire `/Assets/` folder for massive audio files (>5 MB) and 4K+ textures that silently bloat builds — even if they aren't placed in any scene.

### 💡 Optimization Advisor
A comprehensive project-wide scanner that generates prioritized optimization tips with one-click auto-fix actions across:
- Project Settings (IL2CPP, Stripping, Incremental GC, GPU Skinning)
- Quality Settings (Shadow distance, cascades, pixel lights, LOD bias)
- Physics Settings (Fixed timestep, Auto Sync Transforms, solver iterations)
- Audio Settings (DSP buffer, max voices)
- Texture/Audio/Mesh Import Settings (Compression, Read/Write, load types)
- Scene Hierarchy (Deep hierarchy, disabled objects, total count)
- Lighting (Realtime shadows, baked GI)

### 🏢 Studio Integrations (Enterprise)
- **Slack Webhook Alerts**: Automatically push performance incident reports to your Slack channel
- **AI Code Doctor**: Analyze and refactor C# scripts with LLM-powered suggestions
- **Duplicate Asset Finder**: Hash-based detection of duplicate textures, audio, meshes, and materials wasting build size

### 📄 Performance Report Card Generator
Generates a beautiful, standalone HTML performance report with:
- Letter grade (A+ through F) and optimization score
- Metrics grid (FPS, RAM, VRAM, CPU, Scripts, Rendering, Physics, Lag Spikes)
- FPS and CPU time series charts
- Detected issues list with severity badges
- Pro optimization tips
- Device information

### 🔍 Right-Click Quick Optimize
Right-click any GameObject → **Auto Profiler** for instant per-object performance analysis:
- **Quick Performance Check**: Full diagnostic of mesh, light, particle, animator, UI, physics, and audio components
- **Auto-Optimize Selected**: One-click fix all auto-fixable issues on the selected object
- **Batch Optimize All Selected**: Multi-select and optimize in bulk

### ✨ Mega-Batcher
Right-click selected GameObjects → **Mega-Batch Selected** to combine multiple meshes into a single draw call. Automatically marks the result as Static and OccludeeStatic.

### 🚀 Scene View Performance Overlay
A glassmorphism-style overlay drawn directly in the Scene View showing:
- Live Draw Call count with color coding
- Triangle count
- Active performance risk count  
- Mobile budget progress bar (300 Draw Calls)
- Heatmap tint button and quick scan access

---

## 💻 System Requirements

| Requirement | Minimum | Recommended |
|---|---|---|
| **Unity Version** | 2021.3 LTS | 2022.3+ LTS |
| **Render Pipeline** | Built-in, URP, or HDRP | Any |
| **OS** | Windows 10, macOS 10.15+ | Any |
| **Target Platforms** | PC, Mac, Mobile, WebGL, Console | All |

### Dependencies
- Unity `com.unity.profiling.core` (included in Unity 2021.3+)
- Unity `com.unity.ugui` (for UI scanning features)
- No third-party dependencies

---

## 📥 Installation

### From Unity Package

1. Import the `AutoPerformanceProfilerPro.unitypackage` into your Unity project via **Assets → Import Package → Custom Package**.
2. The package will compile under the `AutoPerformanceProfiler` namespace automatically.
3. A **Welcome Window** will appear on first import, guiding you through initial platform configuration.

### Manual Installation

1. Copy the `AutoPerformanceProfiler/` folder into your project's `Assets/` directory.
2. Unity will compile the two Assembly Definitions:
   - `AutoPerformanceProfiler.Runtime` — Runtime data collection (included in builds)
   - `AutoPerformanceProfiler.Editor` — Editor UI and analysis tools (stripped from builds)
3. Open the tool via **Window → Analysis → Auto Performance Profiler Pro**.

### Package Manager (Local)

1. In Unity, go to **Window → Package Manager**.
2. Click **+** → **Add package from disk**.
3. Navigate to `Assets/AutoPerformanceProfiler/package.json` and select it.

---

## ⚡ Quick Start Guide

### 3-Step Workflow

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│  1. ANALYZE OFFLINE  │───▶│  2. AUTO-FIX ISSUES  │───▶│  3. CAPTURE RUNTIME  │
│                     │    │                     │    │                     │
│  Run Offline Scan   │    │  Click ✨ Auto-Fix   │    │  Record → Play →    │
│  (No Play Mode)     │    │  on each warning    │    │  Stop & Analyze     │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
```

#### Step 1: Analyze Offline (No Play Mode Required)
1. Open the tool: **Window → Analysis → Auto Performance Profiler Pro**
2. Click **"Run Offline Scene Scan"** in the empty state view
3. The tool parses your scene hierarchy across 30+ performance metrics instantly

#### Step 2: Auto-Fix Bottlenecks
1. Navigate to the **🎯 Auto-Fix** tab
2. Review the generated list of offenders with severity ratings
3. Click **✨ Auto-Fix** next to any item with a fixable icon
4. Or click **"🛠️ MAGIC FIX"** to resolve all non-script issues at once

#### Step 3: Capture Runtime Data
1. Click **"⚡ Inject System"** to add the `PerformanceTracker` to your scene
2. Press **"⏺ Start Rec"** and enter Play Mode
3. Play your game normally for 30-60 seconds
4. Click **"⏹ Stop & Analyze"**
5. Review the dashboard, graphs, and AI-generated suggestions

---

## 🏗 Architecture

```
AutoPerformanceProfiler/
├── package.json                          # Unity Package Manager manifest
├── README.md                             # This file
├── DOCUMENTATION.md                      # Extended technical documentation
│
├── Runtime/                              # Included in production builds
│   ├── AutoPerformanceProfiler.Runtime.asmdef
│   ├── PerformanceTracker.cs             # Core MonoBehaviour data collector
│   ├── ProfilerReport.cs                 # ScriptableObject report + data structs
│   └── HardwareBudgetProfile.cs          # Platform-specific threshold presets
│
├── Editor/                               # Editor-only (stripped from builds)
│   ├── AutoPerformanceProfiler.Editor.asmdef
│   ├── ProfilerWindow.cs                 # Main EditorWindow (13-tab dashboard)
│   ├── ProfilerAnalyzerExtensions.cs     # 30+ offline scene analysis rules
│   ├── OptimizationAdvisor.cs            # Project-wide optimization scanner
│   ├── ProfilerBuildGuardian.cs          # IPreprocessBuildWithReport CI/CD guard
│   ├── ProfilerCLI.cs                    # Headless CI/CD batch mode runner
│   ├── ProfilerSceneOverlay.cs           # Scene View glassmorphism overlay
│   ├── QuickOptimizeMenu.cs              # Right-click context menu optimizations
│   ├── MegaBatcher.cs                    # Multi-mesh combine into single draw call
│   ├── DuplicateAssetFinder.cs           # Hash-based duplicate asset detection
│   ├── PerformanceReportCardGenerator.cs # HTML report card with grades/charts
│   ├── AICodeDoctor.cs                   # LLM-powered C# script refactoring
│   ├── StudioIntegrations.cs             # Slack webhook notifications
│   ├── WelcomeWindow.cs                  # First-time setup wizard
│   └── Resources/                        # Editor resources
│
└── Reports/                              # Auto-generated profiling reports
```

### Assembly Separation

The tool uses strict **Assembly Definition** boundaries to ensure zero overhead in production builds:

| Assembly | Platform | Purpose |
|---|---|---|
| `AutoPerformanceProfiler.Runtime` | All Platforms | `PerformanceTracker` MonoBehaviour, `ProfilerReport` data structures, `HardwareBudgetProfile` ScriptableObjects |
| `AutoPerformanceProfiler.Editor` | Editor Only | All UI, analysis, export, and auto-fix logic. Uses `UnityEditor` APIs. Fully stripped from builds. |

### Zero-GC Data Collection

The runtime tracker uses Unity's modern `Unity.Profiling.ProfilerRecorder` C++ struct API. Unlike older profiler methods that caused GC spikes just by reading data, this tool pulls metrics from Unity's internal unmanaged memory:

- **Runtime Overhead**: < 0.2ms per frame
- **GC Allocations**: 0 bytes per frame (from profiling logic)
- **Build Size Impact**: ~50 KB

---

## 🪟 Editor Window Tabs

The main Profiler window (`Window → Analysis → Auto Performance Profiler Pro`) contains **13 functional tabs**:

| Tab | Name | Description |
|---|---|---|
| 📊 | **Dashboard** | Health grade (A+ to F), optimization score bar, key metrics (FPS/RAM/VRAM/Battery), CPU subsystem breakdown, optimization roadmap |
| 📈 | **Graphs** | Interactive line graphs for FPS stability, Main Thread CPU (ms), and VRAM allocation over time |
| 📝 | **Data** | Frame-by-frame data table with spike filtering, screenshot viewer, and Slack alert integration |
| 🎯 | **Auto-Fix** | Full list of detected offenders with severity badges, per-item Auto-Fix buttons, and Magic Fix All |
| 📦 | **Build Size** | Asset size treemap, category breakdown, Auto-Crunch Compress button |
| 📱 | **Wireless** | TCP server for wireless mobile telemetry, IP configuration, connection status |
| 🔥 | **Heatmaps** | One-click Scene View mode switching (Overdraw, Wireframe, Shaded Wireframe) |
| 🗃️ | **Leaks** | Ghost Memory scanner, object snapshot timeline, event logs, AI suggestions |
| 💾 | **Export** | CSV, JSON, and HTML Flame Graph export pipelines |
| ⚖️ | **Compare** | A/B delta comparator for two ProfilerReport assets |
| 🛠️ | **Deep Scan** | Full project `/Assets/` scanning for oversized audio and textures |
| 🏢 | **Enterprise** | Slack webhooks, AI Code Doctor, LLM API key configuration |
| 💡 | **Advisor** | Project-wide optimization tips with category filters and batch Auto-Fix All |

---

## 🖱 Right-Click Context Menus

Access instant per-object analysis by right-clicking any GameObject in the Hierarchy:

| Menu Path | Action |
|---|---|
| `GameObject → Auto Profiler → 🔍 Quick Performance Check` | Full diagnostic dialog with issue counts and pass counts |
| `GameObject → Auto Profiler → ✨ Auto-Optimize Selected` | One-click fix all auto-fixable issues on the selected object |
| `GameObject → Auto Profiler → 📊 Batch Optimize All Selected` | Fix all selected objects at once |
| `GameObject → Auto Profiler → ✨ Mega-Batch Selected` | Combine meshes into a single draw call |

---

## 🎯 Scene View Overlay

The **Performance Overlay** renders directly in the Scene View (toggle via `Window → Analysis → Toggle Performance Overlay`):

- Displays real-time Draw Call count, Triangle count, and Active Risks
- Shows a mobile budget progress bar (300 Draw Call target)
- Refreshes every 2 seconds to stay responsive without CPU impact
- Includes **Heatmap Tint** and **Quick Scan** buttons
- Glassmorphism-styled with semi-transparent dark background

---

## 🔄 CI/CD Integration

### Pre-Build Guardian

Enable via `Window → Analysis → Auto Profiler Guardian → Enable Pre-Build Guardian`.

When a developer attempts to build (`Ctrl+B` or `File → Build And Run`), the Guardian:
1. Silently executes `RunAdvancedEditorAnalysis()`
2. Filters for **High severity** violations
3. Throws `BuildFailedException` if any critical issues exist
4. The build **stops completely** and prints explicit error messages to the Console

### Headless CI/CD Mode

For server-based CI/CD pipelines (GitHub Actions, Jenkins, GitLab CI), use the CLI runner:

```bash
# Run from command line in batch mode
Unity.exe -batchmode -projectPath /path/to/project \
  -executeMethod AutoPerformanceProfiler.Editor.ProfilerCLI.RunCIAnalysis \
  -quit
```

**Exit Codes:**
- `0` — All checks passed, build approved
- `1` — Critical severity violations detected, build rejected

---

## 💾 Data Export Formats

### CSV Export
Flattened per-frame spreadsheet with columns:
`Time | FPS | CPU Main (ms) | Scripts (ms) | Render (ms) | Physics (ms) | RAM (MB) | VRAM (MB) | GC (bytes) | Draw Calls | Batches | Triangles | Spike`

### JSON Export
Full `ProfilerReport` ScriptableObject serialized via `JsonUtility.ToJson()`.

### HTML Flame Graph
A standalone, self-contained HTML file with:
- Inline CSS (no external dependencies)
- Two interactive bar charts: FPS Over Time + CPU Over Time
- Color-coded bars (green/yellow/red) with hover tooltips
- Per-frame metrics visible on hover
- Can be opened in any browser and emailed to stakeholders

### HTML Report Card
A premium, glassmorphism-styled HTML report featuring:
- Letter grade hero section with optimization score
- Metrics grid with color-coded values
- FPS and CPU time-series bar charts
- Detected issues with severity badges and fix recommendations
- Pro optimization tips grid
- Device information section

---

## 🔧 Extending the Tool

### Adding a Custom Offline Scan Rule

1. Open `Editor/ProfilerAnalyzerExtensions.cs`
2. Locate `RunAdvancedEditorAnalysis()`
3. Add your scan block:

```csharp
// Example: Flag all SphereColliders with radius > 50
var spheres = Object.FindObjectsByType<SphereCollider>(FindObjectsSortMode.None);
foreach (var sphere in spheres)
{
    if (sphere.radius > 50f)
    {
        offenders.Add(new Runtime.ObjectOffender
        {
            gameObjectName = sphere.gameObject.name,
            componentName = "SphereCollider",
            severity = "Medium",
            issueDescription = "Massive hitbox causes intense broadphase physics overlap checks.",
            recommendedFix = "Scale the underlying mesh, or break into smaller compound colliders."
        });
    }
}
```

### Adding Auto-Fix for Your Custom Rule

1. Navigate to `AutoFixSpecificOffender()` in the same file
2. Add an intercept:

```csharp
if (o.componentName == "SphereCollider")
{
    var sc = target.GetComponent<SphereCollider>();
    if (sc != null)
    {
        sc.radius = 5f; // Cap the size
        EditorSceneManager.MarkSceneDirty(target.scene);
    }
}
```

### Adding a Custom Optimization Advisor Tip

1. Open `Editor/OptimizationAdvisor.cs`
2. Create a new scan method or add to an existing one
3. Return an `OptimizationTip` with a `canAutoFix` flag and optional `autoFixAction` lambda

### Creating a Custom Hardware Budget Profile

1. Right-click in the Project window
2. **Create → Auto Profiler → Hardware Budget Profile**
3. Configure:
   - `fpsThreshold` — Minimum acceptable FPS
   - `cpuTimeSpikeMs` — Maximum acceptable CPU frame time
   - `maxTextureMemoryMB` — VRAM cap
   - `maxTotalRAMMB` — System RAM cap
   - `gcThresholdBytes` — GC allocation spike threshold
   - `batchesWarningLimit` — Maximum draw batches
   - `trisWarningLimit` — Maximum triangle count
   - `maxActiveGameObjects` — Scene object count warning

4. Assign the profile to `PerformanceTracker.activeHardwareBudget` in the Inspector or via code:

```csharp
[SerializeField] private HardwareBudgetProfile mobileLowEndProfile;
[SerializeField] private PerformanceTracker profiler;

void Start()
{
    if (SystemInfo.systemMemorySize < 4000)
    {
        profiler.activeHardwareBudget = mobileLowEndProfile;
        Debug.Log("Switched profiler to Mobile Low-End specification.");
    }
}
```

---

## ⚡ Performance Overhead

| Metric | Value | Notes |
|---|---|---|
| **CPU per frame** | < 0.2 ms | Primarily from evaluating threshold booleans |
| **GC per frame** | 0 bytes | Uses `ProfilerRecorder` unmanaged memory API |
| **Build size** | ~50 KB | `UnityEditor` namespace fully stripped from runtime |
| **Screenshot spike** | ~10 ms | Only on the **exact frame** a lag spike triggers; async IO |
| **Editor-only tools** | 0 bytes in build | Entire `Editor/` assembly stripped via Assembly Definition |

---

## ❓ Troubleshooting & FAQ

<details>
<summary><strong>Q: I clicked "Run Offline Scene Scan" but nothing happened</strong></summary>

Open the Unity Console (`Window → General → Console`). Ensure there are no compiler errors in your own scripts preventing Editor code from executing.
</details>

<details>
<summary><strong>Q: The AI text isn't generating after I click Stop</strong></summary>

The AI parses data from `PerformanceTracker` by analyzing array lengths in `report.history`. Ensure your recording session was longer than 5 seconds so enough data points were captured.
</details>

<details>
<summary><strong>Q: Why did my Build fail randomly?</strong></summary>

Check if the **CI/CD Build Guardian** is active (`Window → Analysis → Auto Profiler Guardian`). The Console will list exactly which GameObjects violate architecture conventions. Fix them or disable the Guardian to proceed.
</details>

<details>
<summary><strong>Q: Screenshots are coming out black</strong></summary>

Unity's `ScreenCapture` API requires the Game View to be visible. If you minimize the Editor, the graphics thread stops rasterizing, resulting in black pixels. Keep the Game View visible during recording.
</details>

<details>
<summary><strong>Q: Wireless streaming isn't connecting to my phone</strong></summary>

1. Ensure both PC and phone are on the **exact same Wi-Fi network/router**
2. Check that Windows Firewall allows Unity on port 8080 (allow both Public and Private)
3. Do NOT use `127.0.0.1` for mobile builds — use your PC's local IP address
4. Click **"📡 Start Listening Server"** in the Editor **before** launching the app on your phone
</details>

<details>
<summary><strong>Q: Can I use this with URP/HDRP?</strong></summary>

Yes! The tool is fully compatible with Built-in, URP, and HDRP render pipelines. All profiler metrics come from Unity's internal `ProfilerRecorder` API, which is pipeline-agnostic.
</details>

<details>
<summary><strong>Q: Will this tool affect my game's performance in production builds?</strong></summary>

The `PerformanceTracker` MonoBehaviour adds < 0.2ms overhead. For final release builds, either remove the tracker GameObject or disable `recordOnAwake`. All Editor assemblies are automatically stripped.
</details>

---

## 📜 Changelog

### v2.0.0 (Current)
- ✨ Added **Optimization Advisor** with 40+ project-wide tips and batch auto-fix
- ✨ Added **Duplicate Asset Finder** with MD5 fast-hashing
- ✨ Added **Performance Report Card** HTML generator with letter grades
- ✨ Added **Scene View Performance Overlay** with glassmorphism styling
- ✨ Added **Quick Optimize** right-click context menus
- ✨ Added **Mega-Batcher** for multi-mesh combining
- ✨ Added **CLI headless mode** for CI/CD batch pipelines
- ✨ Added **Welcome Window** with guided platform setup

### v1.0.0
- 🚀 Initial release
- Core PerformanceTracker with zero-GC ProfilerRecorder
- 30+ Offline Scene Analysis rules with Auto-Fix
- Hardware Budget Profiles
- Wireless TCP Mobile Telemetry
- Build-Size Explorer with Treemap
- Shader Overdraw Heatmaps
- Ghost Memory Scanner
- CSV / JSON / Flame Graph exports
- A/B Report Comparator
- CI/CD Pre-Build Guardian

---

## 📄 License

Auto Performance Profiler Pro is a commercial Unity Asset. All rights reserved.

For licensing inquiries, volume discounts, or enterprise features, please contact the developer.

---

## 📞 Support

For bug reports, feature requests, or integration support:
- Check the [DOCUMENTATION.md](DOCUMENTATION.md) for detailed technical reference
- Open an issue on the support channel

---

**Made for the Unity Asset Store** — Code architecture strictly adheres to standard C# naming conventions, uses precompiled Assembly Definitions for fast compile times, and introduces zero third-party dependencies.
]]>
