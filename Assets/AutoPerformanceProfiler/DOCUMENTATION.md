<![CDATA[# Auto Performance Profiler Pro — Technical Documentation

Welcome to the comprehensive technical documentation for **Auto Performance Profiler Pro**. This document covers advanced usage, underlying architecture, public API details, data structures, and instructions on how to extend the tool for your studio's specific needs.

---

## 📑 Table of Contents

1. [Installation & Quick Start](#1-installation--quick-start)
2. [Architecture Overview](#2-architecture-overview)
3. [Runtime Module — PerformanceTracker](#3-runtime-module--performancetracker)
4. [Runtime Module — ProfilerReport Data Structures](#4-runtime-module--profilerreport-data-structures)
5. [Runtime Module — Hardware Budget Profiles](#5-runtime-module--hardware-budget-profiles)
6. [Visual Diagnostics & Spike Snapshots](#6-visual-diagnostics--spike-snapshots)
7. [Offline Scene Analyzer (30+ Rules)](#7-offline-scene-analyzer-30-rules)
8. [The Auto-Fix Pipeline](#8-the-auto-fix-pipeline)
9. [Optimization Advisor (Project-Wide Scanner)](#9-optimization-advisor-project-wide-scanner)
10. [Analytics Parsing & AI Suggestions](#10-analytics-parsing--ai-suggestions)
11. [Enterprise: CI/CD Pre-Build Guardian](#11-enterprise-cicd-pre-build-guardian)
12. [Enterprise: Headless CI/CD CLI](#12-enterprise-headless-cicd-cli)
13. [Enterprise: Wireless Mobile Telemetry](#13-enterprise-wireless-mobile-telemetry)
14. [Enterprise: A/B Report Comparator](#14-enterprise-ab-report-comparator)
15. [Enterprise: Studio Integrations](#15-enterprise-studio-integrations)
16. [Advanced Editor: Build-Size Explorer & Treemap](#16-advanced-editor-build-size-explorer--treemap)
17. [Advanced Editor: Shader & Overdraw Heatmaps](#17-advanced-editor-shader--overdraw-heatmaps)
18. [Advanced Editor: Ghost Memory Scanner](#18-advanced-editor-ghost-memory-scanner)
19. [Advanced Editor: Deep Project Asset Scanner](#19-advanced-editor-deep-project-asset-scanner)
20. [Advanced Editor: Duplicate Asset Finder](#20-advanced-editor-duplicate-asset-finder)
21. [Advanced Editor: Mega-Batcher](#21-advanced-editor-mega-batcher)
22. [Advanced Editor: Quick Optimize Context Menus](#22-advanced-editor-quick-optimize-context-menus)
23. [Advanced Editor: Scene View Overlay](#23-advanced-editor-scene-view-overlay)
24. [Advanced Editor: Performance Report Card](#24-advanced-editor-performance-report-card)
25. [Data Export: CSV / JSON / Flame Graph](#25-data-export-csv--json--flame-graph)
26. [API Reference](#26-api-reference)
27. [Performance Overhead Constraints](#27-performance-overhead-constraints)
28. [Troubleshooting & FAQ](#28-troubleshooting--faq)

---

## 1. Installation & Quick Start

### Installation
1. Import the `AutoPerformanceProfilerPro.unitypackage` into your Unity project.
2. The package will automatically compile under the `AutoPerformanceProfiler` namespace.
3. *Note: Ensure your project uses Unity 2021.3 LTS or newer to fully support the `Unity.Profiling` unmanaged API.*

### The 3-Step Workflow ⏱️
1. **Analyze Offline:** Open via `Window → Analysis → Auto Performance Profiler Pro`. Click **Run Offline Scene Scan** to find 30+ architectural mistakes before pressing Play.
2. **Auto-Fix Bottlenecks:** Review the generated offender list. Click **✨ Auto-Fix** on each or **🛠️ MAGIC FIX** to batch-resolve all scene issues.
3. **Capture Runtime Data:** Enter Play Mode → Click **⏺ Record** → Play for 60s → Click **⏹ Stop** → Read the AI-generated analysis.

---

## 2. Architecture Overview

Auto Performance Profiler Pro is divided into two precompiled Assembly Definitions to ensure zero overhead in production builds:

```
┌──────────────────────────────────────────────────────┐
│                    EDITOR ASSEMBLY                    │
│        AutoPerformanceProfiler.Editor.asmdef          │
│                                                      │
│  ProfilerWindow.cs ────── Main 13-tab Dashboard      │
│  ProfilerAnalyzerExtensions.cs ── Offline Scanner    │
│  OptimizationAdvisor.cs ──── Project-wide Advisor    │
│  ProfilerBuildGuardian.cs ── IPreprocessBuild Guard  │
│  ProfilerCLI.cs ──────────── Headless CI/CD Runner   │
│  ProfilerSceneOverlay.cs ── Scene View Overlay       │
│  QuickOptimizeMenu.cs ───── Right-click Menus        │
│  MegaBatcher.cs ──────────── Mesh Combiner           │
│  DuplicateAssetFinder.cs ── MD5 Duplicate Scanner    │
│  PerformanceReportCardGenerator.cs ── HTML Reports   │
│  AICodeDoctor.cs ─────────── LLM Script Refactor     │
│  StudioIntegrations.cs ───── Slack Webhooks          │
│  WelcomeWindow.cs ────────── First-time Setup        │
│                                                      │
│  Platform: Editor Only                               │
│  Dependencies: AutoPerformanceProfiler.Runtime       │
└──────────────────────┬───────────────────────────────┘
                       │ references
┌──────────────────────┴───────────────────────────────┐
│                   RUNTIME ASSEMBLY                    │
│       AutoPerformanceProfiler.Runtime.asmdef          │
│                                                      │
│  PerformanceTracker.cs ── MonoBehaviour Collector     │
│  ProfilerReport.cs ────── ScriptableObject + Structs │
│  HardwareBudgetProfile.cs ── Budget Presets (SO)     │
│                                                      │
│  Platform: All (PC, Mobile, Console, WebGL)           │
│  Dependencies: None                                   │
└──────────────────────────────────────────────────────┘
```

This separation ensures that `UnityEditor` API calls never break mobile/console builds. The Runtime assembly introduces zero third-party dependencies.

---

## 3. Runtime Module — PerformanceTracker

**File:** `Runtime/PerformanceTracker.cs`  
**Namespace:** `AutoPerformanceProfiler.Runtime`  
**Type:** `MonoBehaviour` (Singleton, `DontDestroyOnLoad`)

### Component Inspector Fields

| Header | Field | Type | Default | Description |
|---|---|---|---|---|
| **Tracker Settings** | `recordOnAwake` | `bool` | `true` | Automatically start recording when the scene loads |
| | `saveReportOnStop` | `bool` | `true` | Save a `ProfilerReport` ScriptableObject asset when recording stops |
| **Advanced Features** | `takeScreenshotsOnSpike` | `bool` | `true` | Capture Game View screenshots when spikes occur |
| | `showInGameHUD` | `bool` | `true` | Show the live FPS/RAM/VRAM overlay during recording |
| | `trackObjectLeaks` | `bool` | `true` | Periodically snapshot GameObject/MonoBehaviour counts |
| | `objectSnapshotInterval` | `float` | `10f` | Seconds between leak detection snapshots |
| | `runSceneScannerOnStop` | `bool` | `true` | Run a deep hierarchy scan when recording stops |
| **Bottleneck Thresholds** | `activeHardwareBudget` | `HardwareBudgetProfile` | `null` | Optional ScriptableObject defining platform-specific limits |
| | `fallbackFpsThreshold` | `float` | `30f` | Minimum FPS before marking a frame as a spike (if no budget) |
| | `fallbackGcThresholdBytes` | `long` | `102400` | GC allocation threshold in bytes (if no budget) |
| | `fallbackCpuTimeSpikeMs` | `float` | `33f` | CPU time threshold in ms (if no budget) |
| **Wireless Profiling** | `enableWirelessProfiler` | `bool` | `false` | Enable TCP streaming to the Editor |
| | `editorIP` | `string` | `"192.168.1.100"` | IP address of the PC running Unity Editor |
| | `editorPort` | `int` | `8080` | TCP port for the wireless connection |

### Monitored Subsystems via ProfilerRecorder

The tracker uses Unity's zero-allocation `Unity.Profiling.ProfilerRecorder` C++ struct API:

| Recorder | Category | Marker Name | Unit |
|---|---|---|---|
| `mainThreadRecorder` | `ProfilerCategory.Internal` | `"Main Thread"` | Nanoseconds → ms |
| `renderThreadRecorder` | `ProfilerCategory.Render` | `"Render Thread"` | Nanoseconds → ms |
| `scriptsRecorder` | `ProfilerCategory.Scripts` | `"BehaviourUpdate"` | Nanoseconds → ms |
| `physicsRecorder` | `ProfilerCategory.Physics` | `"Physics processing"` | Nanoseconds → ms |
| `animRecorder` | `ProfilerCategory.Animation` | `"Director.Update"` | Nanoseconds → ms |
| `memoryRecorder` | `ProfilerCategory.Memory` | `"Total Used Memory"` | Bytes → MB |
| `gcAllocRecorder` | `ProfilerCategory.Memory` | `"GC Allocated In Frame"` | Bytes |
| `textureMemoryRecorder` | `ProfilerCategory.Memory` | `"Texture Memory"` | Bytes → MB |
| `meshMemoryRecorder` | `ProfilerCategory.Memory` | `"Mesh Memory"` | Bytes → MB |
| `drawCallsRecorder` | `ProfilerCategory.Render` | `"Draw Calls Count"` | Count |
| `batchesRecorder` | `ProfilerCategory.Render` | `"Batches Count"` | Count |
| `trianglesRecorder` | `ProfilerCategory.Render` | `"Triangles Count"` | Count |
| `verticesRecorder` | `ProfilerCategory.Render` | `"Vertices Count"` | Count |

### Public API

```csharp
// Singleton access
PerformanceTracker.Instance

// Control recording
void StartRecording()       // Begin capturing frame data
void StopRecording()        // Stop capturing and optionally save report
bool IsRecording()          // Check current recording state

// Manual report save (Editor only)
void SaveReport()           // Create and save a ProfilerReport ScriptableObject
```

### Spike Detection Logic

Each frame, the tracker evaluates three conditions against the active hardware budget (or fallback values):

```csharp
bool isSpike = fps < CurrentFpsThreshold 
            || mainMs > CurrentCpuSpikeMs 
            || gcAllocThisFrame > CurrentGcThreshold;
```

If a spike is detected and `takeScreenshotsOnSpike` is enabled, a cooldown of 1.5 seconds prevents screenshot flooding.

### Object Leak Detection

Every `objectSnapshotInterval` seconds, the tracker snapshots `GameObject` and `MonoBehaviour` counts. If the final count exceeds the first by >50% **and** >80 objects over a 5-minute session, it classifies the session as having a **Critical Memory Leak**.

### In-Game HUD

When `showInGameHUD` is enabled, a live `OnGUI` overlay displays:
- FPS (color-coded: green ≥30, yellow ≥20, red <20)
- CPU frame time (ms)
- RAM allocation (MB)
- VRAM texture memory (MB)
- Draw Batches and Triangle count
- Battery level (or "Plugged IN/PC")

---

## 4. Runtime Module — ProfilerReport Data Structures

**File:** `Runtime/ProfilerReport.cs`  
**Namespace:** `AutoPerformanceProfiler.Runtime`

### ProfilerReport (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "NewProfilerReport", 
                 menuName = "Performance Profiler/Report")]
public class ProfilerReport : ScriptableObject
```

| Field | Type | Description |
|---|---|---|
| `sessionName` | `string` | Auto-generated session identifier |
| `timestamp` | `string` | Human-readable datetime |
| `sceneName` | `string` | Active scene at recording time |
| `deviceModel` | `string` | `SystemInfo.deviceModel` |
| `osVersion` | `string` | `SystemInfo.operatingSystem` |
| `duration` | `float` | Total recording time in seconds |
| `totalFramesRecorded` | `int` | Number of frames captured |
| `averageFPS` | `float` | Mean FPS across all frames |
| `minFPS` / `maxFPS` | `float` | Extreme FPS values |
| `averageMainThreadMs` | `float` | Mean main thread CPU time |
| `averageScriptsMs` | `float` | Mean C# script execution time |
| `averageRenderMs` | `float` | Mean render thread CPU time |
| `averagePhysicsMs` | `float` | Mean physics processing time |
| `maxMemoryMB` | `long` | Peak system RAM usage |
| `maxTextureMemoryMB` | `long` | Peak texture VRAM usage |
| `totalGCAllocationsMB` | `long` | Cumulative GC allocations |
| `averageTriangles` / `averageBatches` | `long` | Mean rendering counts |
| `startBatteryLevel` / `endBatteryLevel` | `float` | Battery delta tracking |
| `objectSnapshots` | `List<ObjectSnapshot>` | Periodic leak detection data |
| `frames` | `List<FrameData>` | Per-frame metric history |
| `warnings` | `List<string>` | Event-driven warning logs |
| `suggestions` | `List<string>` | AI-generated optimization suggestions |
| `offenders` | `List<ObjectOffender>` | Detected performance offenders |

### FrameData (Struct)

Captured every frame during recording:

```csharp
[Serializable]
public struct FrameData
{
    public float time;                    // Time since session start
    public float fps;                     // Instantaneous FPS
    public float mainThreadTimeMs;        // Main thread CPU time
    public float scriptsTimeMs;           // C# BehaviourUpdate time
    public float renderThreadTimeMs;      // Render thread CPU time
    public float physicsTimeMs;           // Physics processing time
    public float animTimeMs;              // Animation/Director time
    public long allocatedMemoryMB;        // Total used memory
    public long gcAllocatedInFrameBytes;  // GC allocation this frame
    public long drawCalls;                // Draw call count
    public long batches;                  // Batch count
    public long triangles;                // Triangle count
    public long vertices;                 // Vertex count
    public long textureMemoryMB;          // Texture VRAM
    public long meshMemoryMB;             // Mesh VRAM
    public float batteryLevel;            // Device battery (0-1)
    public bool isSpikeFrame;             // Did this frame exceed thresholds?
    public string screenshotPath;         // Path to spike screenshot (if any)
}
```

### ObjectSnapshot (Struct)

Captured periodically for leak detection:

```csharp
[Serializable]
public struct ObjectSnapshot
{
    public float time;                // Time since session start
    public int totalGameObjects;      // FindObjectsByType<GameObject> count
    public int totalMonoBehaviours;   // FindObjectsByType<MonoBehaviour> count
}
```

### ObjectOffender (Struct)

Generated by offline scene analysis and runtime hierarchy scanning:

```csharp
[Serializable]
public struct ObjectOffender
{
    public string gameObjectName;     // Name of the offending object
    public string componentName;      // Type of issue (e.g., "Light", "MeshFilter")
    public string severity;           // "High", "Medium", or "Low"
    public string issueDescription;   // Human-readable problem description
    public string recommendedFix;     // Suggested solution
}
```

---

## 5. Runtime Module — Hardware Budget Profiles

**File:** `Runtime/HardwareBudgetProfile.cs`  
**Create via:** Right-click → `Create → Auto Profiler → Hardware Budget Profile`

```csharp
[CreateAssetMenu(fileName = "New Hardware Budget Profile", 
                 menuName = "Auto Profiler/Hardware Budget Profile")]
public class HardwareBudgetProfile : ScriptableObject
```

| Field | Type | Default | Description |
|---|---|---|---|
| `profileName` | `string` | `"Mobile Native"` | Display name for the profile |
| `description` | `string` | *(textarea)* | Description of target hardware |
| `fpsThreshold` | `float` | `30f` | Minimum acceptable FPS |
| `cpuTimeSpikeMs` | `float` | `33f` | Maximum CPU frame time before flagging |
| `maxTextureMemoryMB` | `long` | `512` | Maximum acceptable VRAM for textures |
| `maxTotalRAMMB` | `long` | `1024` | Maximum acceptable system RAM |
| `gcThresholdBytes` | `long` | `102400` | GC allocation spike threshold |
| `batchesWarningLimit` | `long` | `1000` | Maximum acceptable draw batches |
| `trisWarningLimit` | `long` | `1000000` | Maximum acceptable triangles |
| `maxActiveGameObjects` | `int` | `4000` | Maximum scene object count |

### Recommended Presets

| Profile | FPS | CPU Spike | VRAM | RAM | Batches | Triangles |
|---|---|---|---|---|---|---|
| **Mobile Low-End** | 30 | 33ms | 256 MB | 512 MB | 500 | 300,000 |
| **Mobile Native** | 30 | 33ms | 512 MB | 1024 MB | 1,000 | 1,000,000 |
| **PC Standard** | 60 | 16.6ms | 2048 MB | 4096 MB | 3,000 | 5,000,000 |
| **Console 4K** | 60 | 16.6ms | 4096 MB | 8192 MB | 5,000 | 10,000,000 |
| **VR (90fps)** | 90 | 11.1ms | 1024 MB | 2048 MB | 1,500 | 2,000,000 |

### Runtime Assignment

```csharp
[SerializeField] private HardwareBudgetProfile mobileLowEndProfile;
[SerializeField] private PerformanceTracker profiler;

void Start()
{
    if (SystemInfo.systemMemorySize < 4000)
    {
        profiler.activeHardwareBudget = mobileLowEndProfile;
    }
}
```

---

## 6. Visual Diagnostics & Spike Snapshots

When `takeScreenshotsOnSpike` is enabled, the tracker monitors frame times against active thresholds.

### Screenshot Capture Flow

```
Frame Update
  ├── Calculate FPS, CPU, GC
  ├── Check: isSpike?
  │     ├── FPS < threshold
  │     ├── CPU ms > spike threshold  
  │     └── GC bytes > GC threshold
  ├── If spike AND cooldown elapsed (1.5s):
  │     ├── ScreenCapture.CaptureScreenshot()
  │     └── Store path in FrameData.screenshotPath
  └── Continue frame
```

**Storage Location:** `Application.persistentDataPath/ProfilerScreenshots/YYYYMMDD_HHmm/`

**Performance Note:** Writing a PNG to disk is inherently slow (~10ms). The tool enforces a 1.5-second cooldown between captures to prevent the profiler itself from causing lag spikes.

**Viewing Screenshots:** The Editor window loads `.png` files via direct `byte[]` reading and `Texture2D.LoadImage()`, bypassing Unity's import pipeline for instant display.

---

## 7. Offline Scene Analyzer (30+ Rules)

**File:** `Editor/ProfilerAnalyzerExtensions.cs`  
**Public API:** `ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis()`  
**Returns:** `List<Runtime.ObjectOffender>`

### How It Scans

Uses `Object.FindObjectsByType<T>(FindObjectsSortMode.None)` for 3x faster scan times on massive scenes (skips internal InstanceID sorting).

### Severity Classifications

| Severity | Color | Criteria | CI/CD Impact |
|---|---|---|---|
| 🔴 **High** | Red | Will cause crashes, massive FPS drops, or OOM errors | **Blocks builds** |
| 🟡 **Medium** | Yellow | Cumulative "death by a thousand cuts" CPU drag | Warning only |
| ⚪ **Low** | Gray | Micro-optimizations for strict mobile targets | Informational |

### Complete Rule Reference

| # | Rule Name | Target Component | Severity | Detection Logic |
|---|---|---|---|---|
| 1 | Empty C# Update Loops | `MonoBehaviour` | Medium | Reflection check for `Update()` method declared on custom user scripts |
| 2 | GetComponent in Update | `.cs` files | High | Regex scan of C# source for `GetComponent` calls inside `Update()`/`FixedUpdate()` |
| 3 | Missing Static Batching | `MeshRenderer` | Medium | Has MeshRenderer, no Rigidbody/Animator, but not marked Static |
| 4 | Multi-Material Draw Calls | `MeshRenderer` | Medium | More than 2 shared materials on a single mesh |
| 5 | Missing LOD Group | `MeshFilter` | High | Mesh has >4000 vertices but no `LODGroup` component in parent hierarchy |
| 6 | Mesh Read/Write Waste | `MeshFilter` | Low | `sharedMesh.isReadable` is true (doubles RAM) |
| 7 | Texture VRAM Bloat | `TextureImporter` | High | Material textures at 4096px or larger |
| 8 | Audio Decompress Abuse | `AudioImporter` | High | Clip >5s using `DecompressOnLoad` |
| 9 | Audio Load Freeze | `AudioImporter` | High | Clip >20s without `loadInBackground` |
| 10 | Audio 3D Stereo Waste | `AudioImporter` | Medium | 3D spatialized audio imported in stereo |
| 11 | UI Raycast Target Bloat | `UI.Graphic` | High | Raycast enabled but no `Selectable`/`EventTrigger` attached |
| 12 | Legacy UI Text | `UI.Text` | Medium | Using `UnityEngine.UI.Text` instead of `TextMeshPro` |
| 13 | UI MipMap VRAM Waste | `TextureImporter` | Medium | ScreenSpace UI sprite with MipMaps enabled (33% VRAM waste) |
| 14 | Canvas Layout Rebuilds | `Canvas` | Medium | Canvas with >10 LayoutGroups (expensive UI rebuilding) |
| 15 | Canvas Pixel Perfect | `Canvas` | Medium | `pixelPerfect` enabled with animated elements |
| 16 | Realtime Point/Spot Shadows | `Light` | High | Point/Spot lights with realtime shadows (6-pass depth map) |
| 17 | Reflection Probe Every-Frame | `ReflectionProbe` | High | `refreshMode == EveryFrame` (re-renders scene 6x/frame) |
| 18 | Camera Far-Clip Exhaustion | `Camera` | Medium | `farClipPlane > 5000` (Z-buffer precision loss) |
| 19 | Multiple AudioListeners | `AudioListener` | High | More than 1 AudioListener in scene |
| 20 | Duplicate EventSystems | `EventSystem` | High | More than 1 EventSystem in scene |
| 21 | Massive Particle Overdraw | `ParticleSystem` | Medium | `maxParticles > 5000` |
| 22 | Rigidbody Interpolation Waste | `Rigidbody` | Medium | Interpolation enabled on non-player objects |
| 23 | Massive BoxCollider Bounds | `BoxCollider` | Medium | Volume >1,000,000 cubic units |
| 24 | Dynamic Non-Convex Collider | `MeshCollider` | High | Non-convex with dynamic Rigidbody (crash risk) |
| 25 | Missing Object Pool | `Renderer` | High | >20 active `(Clone)` duplicates of same prefab |
| 26 | Mono Scripting Backend | Project Settings | Medium | Using Mono instead of IL2CPP |
| 27 | Mobile VSync Drag | Quality Settings | Medium | VSync enabled on Android/iOS target |
| 28 | God Object | `GameObject` | Medium | More than 10 MonoBehaviour scripts on single object |

---

## 8. The Auto-Fix Pipeline

**File:** `Editor/ProfilerAnalyzerExtensions.cs`  
**Method:** `AutoFixSpecificOffender(ObjectOffender o)`

### Fix Categories

| Fix Type | Scope | Example |
|---|---|---|
| **Global Settings** | Project-wide | Switch Mono → IL2CPP, disable VSync |
| **Asset Modification** | Per-asset | Downscale 4K textures to 2048 via `TextureImporter` |
| **Scene Object** | Per-GameObject | Set Static flag, disable shadows, adjust culling mode |
| **Component Generation** | Additive | Auto-generate `LODGroup` with 3 LOD levels |

### Supported Auto-Fix Actions

| Component | Fix Action |
|---|---|
| `Global Architecture` (Mono) | `PlayerSettings.SetScriptingBackend(IL2CPP)` |
| `Mobile VSync Drag` | `QualitySettings.vSyncCount = 0` |
| `Texture Bloat` | `TextureImporter.maxTextureSize = 2048; SaveAndReimport()` |
| `Optimization Missing (Static)` | `target.isStatic = true` |
| `Animator` culling | `animator.cullingMode = CullUpdateTransforms` |
| `Light` shadows | `light.shadows = LightShadows.None` |
| `ParticleSystem` max | `main.maxParticles = Min(current, 1000)` |
| `UI RaycastTarget Bloat` | `graphic.raycastTarget = false` |
| `Realtime Reflection Probe` | `rp.refreshMode = OnAwake` |
| `Camera Far Clip` | `cam.farClipPlane = 1000f` |
| `Rigidbody Interpolation` | `rb.interpolation = None` |
| `Canvas Pixel Perfect` | `canvas.pixelPerfect = false` |
| `Missing LODGroup` | Adds `LODGroup` + generates 3 LOD child duplicates |

**Safety Note:** The tool will never auto-fix C# code layout issues without manual user intervention to prevent destructive workflow overwrites. It safely targets hardware/component options only.

---

## 9. Optimization Advisor (Project-Wide Scanner)

**File:** `Editor/OptimizationAdvisor.cs`  
**Public API:** `OptimizationAdvisor.RunFullAdvisorScan()`  
**Returns:** `List<OptimizationTip>` sorted by priority (Critical → Info)

### Scan Categories

| Category | Settings Scanned | Auto-Fixable |
|---|---|---|
| **Project Settings** | Scripting Backend, Stripping Level, Incremental GC, Strip Engine Code, GPU Skinning | ✅ All |
| **Quality Settings** | Shadow Distance, Shadow Cascades, Pixel Light Count, Texture Quality, Anisotropic, LOD Bias | ✅ All |
| **Physics Settings** | Fixed Timestep, Auto Sync Transforms, Reuse Collision Callbacks, Solver Iterations | ✅ All |
| **Audio Settings** | DSP Buffer Size, Max Real Voices | ❌ Manual |
| **Texture Imports** | Uncompressed textures, 4K+ oversized, Read/Write enabled, missing MipMaps | ✅ Batch |
| **Audio Imports** | Decompress On Load (long clips), missing Load In Background, 3D stereo waste | ✅ Batch |
| **Mesh Imports** | Read/Write enabled, mesh optimization disabled | ✅ Batch |
| **Scene Hierarchy** | Deep hierarchy (>10 levels), disabled root objects, total count >5000 | ❌ Manual |
| **Lighting** | Realtime shadow count, point light count, missing baked GI | ❌ Manual |

### Tip Priority Levels

| Priority | Impact | Description |
|---|---|---|
| `Critical` | 🔥 Massive | Must fix. Ship-blocking performance impact |
| `High` | ⚡ Significant | Should fix before release |
| `Medium` | 📊 Moderate | Nice to fix for target platform |
| `Low` | 🔧 Minor | Micro-optimization |
| `Info` | 💡 Educational | Informational tip |

### Batch Auto-Fix All

```csharp
// Fix all auto-fixable tips at once
OptimizationAdvisor.AutoFixAllSafe(tips);
```

This iterates through all tips with `canAutoFix == true` and invokes their `autoFixAction` delegates.

---

## 10. Analytics Parsing & AI Suggestions

**Method:** `PerformanceTracker.GenerateFinalSuggestions(ProfilerReport r)` (Editor-only)

### Correlation Algorithms

The engine uses mathematical analysis against session-wide averages:

| Condition | Threshold | Suggestion Category |
|---|---|---|
| `minFPS < targetFps` | Budget-defined | Platform targeting / shadows / post-processing |
| `averageScriptsMs > 10f` | Fixed | C# code architecture overhead |
| `totalGCAllocationsMB > budget + 5` | Budget-relative | GC / LINQ / object pooling |
| `averageBatches > batchesWarningLimit` | Budget-defined | Static Batching / GPU Instancing |
| `maxTextureMemoryMB > budgetVRAM` | Budget-defined | Texture downscaling |
| `gameObjectCount grew >50% over session` | Trend analysis | **⚠️ CRITICAL MEMORY LEAK** |
| `batteryDrain > 3% in < 5min` | Thermal analysis | 🔋 Battery drain warning |

If no issues are found: `"Everything runs optimally! Outstanding architecture."`

---

## 11. Enterprise: CI/CD Pre-Build Guardian

**File:** `Editor/ProfilerBuildGuardian.cs`  
**Interface:** `IPreprocessBuildWithReport` (`callbackOrder = 0`)

### Menu Items

| Path | Action |
|---|---|
| `Window → Analysis → Auto Profiler Guardian → Enable Pre-Build Guardian` | Activates build interception |
| `Window → Analysis → Auto Profiler Guardian → Disable Pre-Build Guardian` | Deactivates |

### Build Interception Flow

```
Developer clicks Build (Ctrl+B)
  └── Unity calls IPreprocessBuildWithReport.OnPreprocessBuild()
        ├── Check EditorPrefs("AutoProfiler_EnableBuildGuardian")
        ├── If disabled: return (build continues)
        ├── Run ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis()
        ├── Filter: offenders.Where(severity == "High")
        ├── If criticalIssues.Count > 0:
        │     ├── LogError each violation
        │     └── throw BuildFailedException  ← BUILD STOPS
        └── If clean: Log "Analysis Passed" → build continues
```

---

## 12. Enterprise: Headless CI/CD CLI

**File:** `Editor/ProfilerCLI.cs`  
**Entry Point:** `ProfilerCLI.RunCIAnalysis()` (static)

### Usage

```bash
# GitHub Actions / Jenkins / GitLab CI
Unity.exe -batchmode -nographics \
  -projectPath /path/to/project \
  -executeMethod AutoPerformanceProfiler.Editor.ProfilerCLI.RunCIAnalysis \
  -quit

# Check exit code
echo $? # 0 = passed, 1 = critical violations
```

### Behavior
1. Runs `RunAdvancedEditorAnalysis()` on the current scene
2. Checks for **High severity** offenders
3. If violations exist: logs errors → `EditorApplication.Exit(1)`
4. If clean: logs success → `EditorApplication.Exit(0)`

---

## 13. Enterprise: Wireless Mobile Telemetry

### Network Architecture

```
┌─────────────────────┐         TCP/8080         ┌─────────────────────┐
│   MOBILE DEVICE     │ ──────────────────────▶  │    UNITY EDITOR     │
│                     │       Wi-Fi LAN           │                     │
│  PerformanceTracker │                           │  TcpListener        │
│  (enableWireless)   │  JSON FrameData/line      │  (ProfilerWindow)   │
│                     │                           │                     │
│  editorIP: x.x.x.x │                           │  IP: 0.0.0.0:8080   │
│  editorPort: 8080   │                           │  Status: 🟢 Stream  │
└─────────────────────┘                           └─────────────────────┘
```

### Setup Steps

1. **Network**: Ensure PC and phone are on the **same Wi-Fi router**
2. **Editor**: Open Auto Profiler → **📱 Wireless** tab → **📡 Start Listening Server**
3. **Scene**: On `PerformanceTracker`:
   - `enableWirelessProfiler = true`
   - `editorIP = <your PC's IP>` (NOT `127.0.0.1`)
   - `editorPort = 8080`
4. **Build**: File → Build And Run to phone
5. **Unplug USB** after install to prevent thermal throttling
6. Watch Editor for `Status: 🟢 Connected & Streaming...`

### Data Format

Each frame sends one JSON line over TCP:
```json
{"time":12.5,"fps":58.2,"mainThreadTimeMs":16.1,"scriptsTimeMs":3.2,...}
```

### Windows Firewall Fix

If the Editor shows `🟡 Awaiting TCP Connection...`:
- Allow Unity Editor through Windows Firewall on both Public and Private networks
- Or manually add an Inbound Rule for TCP Port 8080

---

## 14. Enterprise: A/B Report Comparator

Located in the **⚖️ Compare** tab of the Editor window.

### Usage
1. Drag **Report A** (before optimization) into the first slot
2. Drag **Report B** (after optimization) into the second slot
3. The tool calculates precise delta percentages between all metrics

### Output Example
```
FPS:    +24% (42 → 52 avg)
VRAM:   -31% (823 MB → 567 MB peak)
CPU:    -18% (19.2ms → 15.7ms avg)
GC:     -45% (12 MB → 6.6 MB total)
```

---

## 15. Enterprise: Studio Integrations

### Slack Webhook Alerts

**File:** `Editor/StudioIntegrations.cs`

```csharp
StudioIntegrations.SendSlackAlert(
    "Critical Lag Spike at 12.5s! FPS dropped to 8.",
    "High"
);
```

Configure the Slack Webhook URL in the **🏢 Enterprise** tab of the profiler window. The tool sends a `POST` request with a JSON payload formatted for Slack's Incoming Webhooks API.

### AI Code Doctor

**File:** `Editor/AICodeDoctor.cs`

Requires an OpenAI/LLM API key configured in the Enterprise tab. When triggered on a C# script flagged for `GetComponent` in `Update()`:

1. Displays progress bar: "Connecting to Language Model..."
2. Analyzes the script's garbage allocation patterns
3. Provides refactored code moving `GetComponent` to cached `Awake()` variables
4. Replaces LINQ polling with standard for-loops

---

## 16. Advanced Editor: Build-Size Explorer & Treemap

Located in the **📦 Build Size** tab.

### Scan Process
1. Iterates every file in `/Assets/` via `Directory.GetFiles()`
2. Categorizes by extension: Textures, Audio, Meshes, Shaders, Other
3. Sums byte-sizes per category
4. Renders a proportional 2D Treemap (largest category = left half, remaining in 2×2 grid)

### Auto-Crunch Compress
The **"✨ Auto-Crunch Compress All Textures > 2048px"** button:
1. Finds every `TextureImporter` in the project
2. Checks if `maxTextureSize > 2048` **and** `crunchedCompression == false`
3. Enables Crunch Compression at quality 50
4. Calls `SaveAndReimport()` on each

---

## 17. Advanced Editor: Shader & Overdraw Heatmaps

Located in the **🔥 Heatmaps** tab.

Programmatically switches the Scene View's `DrawCameraMode`:

| Button | Mode | Use Case |
|---|---|---|
| 🔥 Overdraw | `DrawCameraMode.Overdraw` | Highlights transparent pixel stacking killing GPU fill rate |
| 🔧 Wireframe | `DrawCameraMode.Wireframe` | Shows raw mesh topology to identify polygon-heavy objects |
| 🔧 Shaded Wireframe | `DrawCameraMode.TexturedWire` | Combines textures with wireframe overlay |
| 🌳 Restore | `DrawCameraMode.Textured` | Returns to standard rendering |

---

## 18. Advanced Editor: Ghost Memory Scanner

Located in the **🗃️ Leaks** tab.

### Scan Algorithm
1. `Resources.FindObjectsOfTypeAll<Texture2D>()` — find every texture in RAM
2. `Resources.FindObjectsOfTypeAll<AudioClip>()` — find every audio clip in RAM
3. `AssetDatabase.GetDependencies()` — build a dependency graph from active scene
4. Cross-reference: any asset loaded but **not** in the scene's dependency graph → Ghost Object
5. Only flags textures ≥ 2048px (to reduce noise from Editor textures)

### Actions per Ghost Object
- **Ping Object** → `EditorGUIUtility.PingObject()` to locate in Project
- **🧨 Destroy & Free RAM** → `Resources.UnloadAsset()` to reclaim memory

---

## 19. Advanced Editor: Deep Project Asset Scanner

Located in the **🛠️ Deep Scan** tab.

Unlike the Offline Scanner (which checks the active scene), this scans the entire `/Assets/` folder:

| Target | Threshold | Description |
|---|---|---|
| Massive Audio | File size > 5 MB | Silent build size bloat |
| 4K+ Textures | Dimensions ≥ 4096px | VRAM consumption |

Results are sorted by file size (heaviest first). Each item has a **"Ping in Project"** button via `EditorGUIUtility.PingObject()`.

---

## 20. Advanced Editor: Duplicate Asset Finder

**File:** `Editor/DuplicateAssetFinder.cs`

### Hashing Strategy

Uses a **fast hash** approach (not full MD5) for performance on large projects:

```
File Size (8 bytes) + First 8KB of content + Last 8KB of content → MD5 Hash
```

This provides a reliable unique fingerprint while being 100x faster than hashing entire multi-GB asset folders.

### Supported Asset Types

| Category | Extensions |
|---|---|
| Textures | `.png`, `.jpg`, `.tga`, `.psd`, `.bmp`, `.tif`, `.exr` |
| Audio | `.wav`, `.mp3`, `.ogg`, `.aif`, `.flac` |
| Meshes | `.fbx`, `.obj`, `.blend`, `.dae` |
| Materials | `.mat` |
| Shaders | `.shader` |
| Animation | `.anim`, `.controller` |

### Output
Returns `List<DuplicateGroup>` sorted by total wasted bytes (highest first). Each group contains:
- `hash` — The computed fingerprint
- `assetType` — Category label
- `paths` — List of duplicate file paths
- `individualSizeBytes` — Size of each duplicate
- `wastedBytes` — `(count - 1) * size`

---

## 21. Advanced Editor: Mega-Batcher

**File:** `Editor/MegaBatcher.cs`  
**Menu:** `GameObject → Auto Profiler → ✨ Mega-Batch Selected`

### Process
1. Collects all `MeshFilter` components from selected GameObjects (including children)
2. Captures the first `sharedMaterial` found
3. Creates `CombineInstance[]` with world-space transform matrices
4. Generates a new `MegaBatched_Chunk` GameObject
5. Creates a combined mesh with `UInt32` index format (supports >65k vertices)
6. Calls `CombineMeshes(mergeSubMeshes: true, useMatrices: true)`
7. Marks combined object as `BatchingStatic | OccludeeStatic`
8. Disables original GameObjects (non-destructive)

---

## 22. Advanced Editor: Quick Optimize Context Menus

**File:** `Editor/QuickOptimizeMenu.cs`

### Quick Performance Check

Analyzes a single selected GameObject across:
- Mesh vertex count + LODGroup presence
- Mesh Read/Write status
- Static batching eligibility
- Material count (draw call impact)
- Light shadow configuration + range
- Particle max count
- Animator culling mode
- UI Raycast Target + legacy Text
- Rigidbody interpolation + non-convex MeshCollider
- AudioSource load type + duration
- Reflection Probe refresh mode
- Hierarchy depth
- Component count (God Object check)

### Auto-Optimize Selected

Automatically fixes:
- Set Static flag on eligible MeshRenderers
- Change Animator culling to `CullUpdateTransforms`
- Disable UI Raycast Target on non-interactive elements
- Disable Rigidbody Interpolation
- Set Reflection Probe to `OnAwake`
- Disable Light shadows on realtime Point/Spot lights

### Batch Optimize All Selected

Applies Static, Animator culling, and UI Raycast fixes across multiple selected GameObjects simultaneously.

---

## 23. Advanced Editor: Scene View Overlay

**File:** `Editor/ProfilerSceneOverlay.cs`  
**Toggle:** `Window → Analysis → Toggle Performance Overlay`

### Features
- **Draw Calls**: Real-time count from visible renderers (green <150, red >150)
- **Triangles**: From all visible `MeshFilter` components (red >500k)
- **Active Risks**: Count from `RunAdvancedEditorAnalysis()`
- **Mobile Budget Bar**: Progress bar targeting 300 Draw Calls
- **Heatmap Tint**: Colors objects by material count cost
- **Quick Scan**: Opens the full Profiler Window

### Performance
- Refreshes data every 2 seconds (not every frame)
- Uses `InitializeOnLoad` to register with `SceneView.duringSceneGui`
- Glassmorphism styling with semi-transparent background

---

## 24. Advanced Editor: Performance Report Card

**File:** `Editor/PerformanceReportCardGenerator.cs`

### Score Calculation

```csharp
float score = 100;
score -= (60 - averageFPS) * 1.5f;          // FPS penalty
score -= highSeverityCount * 12;             // Per critical issue
score -= mediumSeverityCount * 4;            // Per medium issue
score = Clamp(score, 0, 100);
```

### Grade Scale

| Score | Grade | Color |
|---|---|---|
| ≥ 90 | **A+** | Green (#4caf50) |
| ≥ 80 | **A** | Green (#4caf50) |
| ≥ 70 | **B** | Light Green (#8bc34a) |
| ≥ 60 | **C** | Orange (#ff9800) |
| ≥ 40 | **D** | Deep Orange (#ff5722) |
| < 40 | **F** | Red (#f44336) |

### HTML Output
- Self-contained (inline CSS, no external dependencies)
- Dark gradient background with glassmorphism cards
- Metrics grid, FPS/CPU charts, issue cards, pro tips
- Automatically opens in system file explorer after generation

---

## 25. Data Export: CSV / JSON / Flame Graph

### CSV Structure
```
Time,FPS,CPU_Main_ms,Scripts_ms,Render_ms,Physics_ms,RAM_MB,VRAM_MB,GC_Bytes,DrawCalls,Batches,Triangles,IsSpike
0.02,59.8,16.1,3.2,8.4,1.1,1024,456,0,142,98,284000,false
0.04,58.1,17.2,4.1,9.0,1.3,1024,456,1024,145,101,291000,false
...
```

### JSON Structure
Full `ProfilerReport` ScriptableObject serialized via `JsonUtility.ToJson(report, prettyPrint: true)`.

### HTML Flame Graph
- Standalone HTML with inline CSS
- Two bar chart sections: FPS Over Time + CPU Over Time
- Color-coded bars: Green (>55fps / <16.6ms), Yellow (>30fps), Red (<30fps / >16.6ms)
- Spike frames highlighted in hot red
- Hover tooltips showing per-frame FPS and CPU ms

---

## 26. API Reference

### Core Runtime API

```csharp
namespace AutoPerformanceProfiler.Runtime
{
    // Singleton MonoBehaviour (DontDestroyOnLoad)
    public class PerformanceTracker : MonoBehaviour
    {
        public static PerformanceTracker Instance { get; }
        
        public void StartRecording();
        public void StopRecording();
        public bool IsRecording();
        public void SaveReport();   // Editor-only
    }
}
```

### Core Editor API

```csharp
namespace AutoPerformanceProfiler.Editor
{
    // Offline Scene Analyzer
    public static class ProfilerAnalyzerExtensions
    {
        public static List<ObjectOffender> RunAdvancedEditorAnalysis();
        public static void AutoFixSpecificOffender(ObjectOffender o);
    }

    // Project-Wide Advisor
    public static class OptimizationAdvisor
    {
        public static List<OptimizationTip> RunFullAdvisorScan();
        public static void AutoFixAllSafe(List<OptimizationTip> tips);
        
        // Individual batch fixers
        public static void AutoFixUncompressedTextures();
        public static void AutoFixOversizedTextures();
        public static void AutoFixReadWriteTextures();
        public static void AutoFixAudioLoadTypes();
        public static void AutoFixAudioBackground();
        public static void AutoFixMeshReadWrite();
        public static void AutoFixMeshOptimization();
    }

    // Duplicate Detection
    public static class DuplicateAssetFinder
    {
        public static List<DuplicateGroup> FindAllDuplicates(
            Action<float, string> onProgress = null);
        public static long GetTotalWastedBytes(List<DuplicateGroup> groups);
    }

    // Report Generation
    public static class PerformanceReportCardGenerator
    {
        public static void GenerateReportCard(
            ProfilerReport report, 
            List<ObjectOffender> sceneOffenders = null);
    }

    // CI/CD Integration
    public class ProfilerBuildGuardian : IPreprocessBuildWithReport { }
    public static class ProfilerCLI
    {
        public static void RunCIAnalysis();
    }

    // Studio Integrations
    public static class StudioIntegrations
    {
        public static void SendSlackAlert(string message, string severity);
    }
    
    public static class AICodeDoctor
    {
        public static void RequestOptimization(string scriptName);
    }

    // Context Menus & Tools
    public static class QuickOptimizeMenu { }
    public static class MegaBatcher { }
    public static class ProfilerSceneOverlay { }
    
    // UI
    public class ProfilerWindow : EditorWindow
    {
        public static void ShowWindow();
    }
    public class WelcomeWindow : EditorWindow
    {
        public static void ShowWelcomeWindow();
    }
}
```

### Menu Items Reference

| Path | Class |
|---|---|
| `Window → Analysis → Auto Performance Profiler Pro` | `ProfilerWindow` |
| `Window → Analysis → About Auto Performance Profiler Pro` | `WelcomeWindow` |
| `Window → Analysis → Toggle Performance Overlay` | `ProfilerSceneOverlay` |
| `Window → Analysis → Auto Profiler Guardian → Enable` | `ProfilerBuildGuardian` |
| `Window → Analysis → Auto Profiler Guardian → Disable` | `ProfilerBuildGuardian` |
| `GameObject → Auto Profiler → 🔍 Quick Performance Check` | `QuickOptimizeMenu` |
| `GameObject → Auto Profiler → ✨ Auto-Optimize Selected` | `QuickOptimizeMenu` |
| `GameObject → Auto Profiler → 📊 Batch Optimize All Selected` | `QuickOptimizeMenu` |
| `GameObject → Auto Profiler → ✨ Mega-Batch Selected` | `MegaBatcher` |

### ScriptableObject Create Menus

| Path | Class |
|---|---|
| `Create → Auto Profiler → Hardware Budget Profile` | `HardwareBudgetProfile` |
| `Create → Performance Profiler → Report` | `ProfilerReport` |

---

## 27. Performance Overhead Constraints

| Component | Overhead | Notes |
|---|---|---|
| **Runtime Tracker** | < 0.2ms/frame | `ProfilerRecorder` unmanaged reads (0 GC) |
| **Screenshot Capture** | ~10ms per capture | Only on spike frames, 1.5s cooldown |
| **Object Leak Snapshot** | ~0.5ms every 10s | `FindObjectsByType` polling |
| **TCP Wireless Send** | ~0.1ms/frame | JSON serialization + socket write |
| **In-Game HUD** | ~0.3ms/frame | `OnGUI` drawing (only when enabled) |
| **Build Size Impact** | ~50 KB | Editor assemblies fully stripped |
| **Scene Overlay** (Editor) | ~2ms every 2s | `FindObjectsByType` + cost calculation |

---

## 28. Troubleshooting & FAQ

**Q: I clicked "Run Offline Scene Scan" but nothing happened?**  
A: Open the Unity Console (`Window → General → Console`). Ensure there are no compiler errors preventing Editor code execution.

**Q: The AI text isn't generating after I click Stop.**  
A: The AI parses `report.history` arrays. Ensure your session was longer than 5 seconds for sufficient data points.

**Q: Why did my Build fail randomly?**  
A: Check if the **CI/CD Build Guardian** is active. The Console lists exactly which GameObjects violate conventions. Fix them or disable the Guardian.

**Q: The Screenshots are coming out black!**  
A: Unity's `ScreenCapture` API requires the Game View to be visible. Don't minimize the Editor during recording.

**Q: The Scene Overlay is not appearing.**  
A: Toggle it via `Window → Analysis → Toggle Performance Overlay`. Check that `EditorPrefs` key `APP_ShowSceneOverlay` is `true`.

**Q: Auto-Fix didn't change anything.**  
A: Some fixes only apply to specific configurations. After fixing, click **🔄 Refresh** and re-run the scan to verify the change took effect. Also check the Console for any `[Profiler]` log messages.

**Q: Can I run analysis on a scene that isn't currently loaded?**  
A: The Offline Scanner uses `FindObjectsByType`, which only works on the currently loaded scene. To scan other scenes, open them first or use the **Deep Scan** tab for project-wide asset analysis.

**Q: The tool is causing Editor slowdowns.**  
A: Disable the Scene View Overlay (`Toggle Performance Overlay`) and reduce the object snapshot interval on the `PerformanceTracker`. The overlay refreshes every 2 seconds and runs a full `RunAdvancedEditorAnalysis()`.

**Q: How do I completely remove the tool from my project?**  
A: Delete the `Assets/AutoPerformanceProfiler/` folder. If you had a `PerformanceTracker` in your scene, remove that GameObject as well. No project settings or packages are modified permanently.

---

*End of Documentation.*  
*For support, please reach out to the developer.*
]]>
