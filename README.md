<![CDATA[# 🚀 Auto Performance Profiler Pro

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-000000?logo=unity&logoColor=white)](https://unity3d.com)
[![C#](https://img.shields.io/badge/C%23-10.0-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-Commercial-blue.svg)](#license)
[![Version](https://img.shields.io/badge/Version-2.0.0-orange.svg)](#changelog)

**The Ultimate Automated Technical Director for Unity** — Stop guessing why your game is lagging. Auto Performance Profiler Pro automatically monitors your game, captures visual proof of lag spikes, tracks VRAM memory leaks, and provides **One-Click Auto-Fix** to optimize your scene in seconds.

---

## 🎯 What Does This Tool Do?

Auto Performance Profiler Pro is a premium Unity Editor extension that transforms complex performance profiling into an intuitive, automated workflow. It translates raw profiler data into **plain-English suggestions** so Artists, Level Designers, and Developers can optimize scenes without deep technical expertise.

### ⚡ At a Glance

| Capability | Description |
|---|---|
| 🪄 **Auto-Fix** | One-click fixes for 20+ performance issues directly on GameObjects |
| ⛰️ **30+ Scan Rules** | Detects RT shadows, particle overdraw, UI bloat, God Objects, and more |
| 📊 **13-Tab Dashboard** | Graphs, data, heatmaps, exports, wireless, advisors — all in one window |
| 🛡️ **Build Guardian** | Blocks builds with unresolved critical performance violations |
| 📱 **Wireless Mobile** | Stream live metrics from phone over Wi-Fi (no USB thermal throttling) |
| 📄 **Report Cards** | Generate beautiful HTML reports with letter grades and charts |
| 💡 **Optimization Advisor** | 40+ project-wide tips with batch auto-fix across all Unity settings |
| 🕵️ **Ghost Scanner** | Find textures/audio loaded in RAM but not referenced by your scene |
| 💾 **Export** | CSV spreadsheets, JSON, and interactive HTML Flame Graphs |

---

## 📁 Project Structure

```
Assets/AutoPerformanceProfiler/
├── package.json                              # UPM manifest
├── README.md                                 # Feature overview & quick start
├── DOCUMENTATION.md                          # Full technical reference (28 sections)
│
├── Runtime/                                  # ← Included in production builds
│   ├── AutoPerformanceProfiler.Runtime.asmdef
│   ├── PerformanceTracker.cs                 # Core MonoBehaviour (zero-GC profiling)
│   ├── ProfilerReport.cs                     # ScriptableObject + FrameData/ObjectOffender structs
│   └── HardwareBudgetProfile.cs              # Platform-specific threshold presets
│
├── Editor/                                   # ← Editor-only (stripped from builds)
│   ├── AutoPerformanceProfiler.Editor.asmdef
│   ├── ProfilerWindow.cs                     # Main 13-tab EditorWindow
│   ├── ProfilerAnalyzerExtensions.cs         # 30+ offline scene analysis rules
│   ├── OptimizationAdvisor.cs                # 40+ project-wide optimization tips
│   ├── ProfilerBuildGuardian.cs              # CI/CD pre-build interceptor
│   ├── ProfilerCLI.cs                        # Headless batch mode for CI/CD
│   ├── ProfilerSceneOverlay.cs               # Scene View performance overlay
│   ├── QuickOptimizeMenu.cs                  # Right-click context menu tools
│   ├── MegaBatcher.cs                        # Multi-mesh combiner
│   ├── DuplicateAssetFinder.cs               # MD5 duplicate detection
│   ├── PerformanceReportCardGenerator.cs     # HTML report card generator
│   ├── AICodeDoctor.cs                       # LLM-powered C# refactoring
│   ├── StudioIntegrations.cs                 # Slack webhook alerts
│   └── WelcomeWindow.cs                      # First-time setup wizard
│
└── Reports/                                  # Auto-generated profiling reports
```

---

## 🚀 Getting Started

### Requirements
- **Unity:** 2021.3 LTS or newer
- **Render Pipeline:** Built-in, URP, or HDRP
- **Platforms:** PC, Mac, Mobile, WebGL, Console

### How to Use

1. **Open the tool:** `Window → Analysis → Auto Performance Profiler Pro`
2. **Offline scan:** Click **"Run Offline Scene Scan"** (no Play Mode needed)
3. **Review & fix:** Navigate to **🎯 Auto-Fix** tab → click **✨ Auto-Fix** on each issue
4. **Runtime profiling:** Inject the tracker → Record → Play → Stop & Analyze

### Quick Links
- 📖 [Full Documentation](Assets/AutoPerformanceProfiler/DOCUMENTATION.md) — 28-section technical reference
- 📋 [Feature README](Assets/AutoPerformanceProfiler/README.md) — Detailed feature list & usage guide

---

## 🏗 Architecture

The tool uses **two precompiled Assembly Definitions** for clean separation:

| Assembly | Platform | Size in Build | Purpose |
|---|---|---|---|
| `AutoPerformanceProfiler.Runtime` | All | ~50 KB | Zero-GC data collection via `ProfilerRecorder` |
| `AutoPerformanceProfiler.Editor` | Editor Only | 0 bytes | All UI, analysis, export, and auto-fix logic |

### Zero-Overhead Profiling

Uses Unity's modern `Unity.Profiling.ProfilerRecorder` unmanaged API:
- **CPU cost:** < 0.2ms per frame
- **GC allocations:** 0 bytes per frame from profiling logic
- **Build size:** ~50 KB (Editor assemblies fully stripped)

---

## 🪟 Dashboard Tabs

| # | Tab | Description |
|---|---|---|
| 1 | 📊 Dashboard | Health grade, optimization score, key metrics, CPU breakdown, roadmap |
| 2 | 📈 Graphs | FPS, CPU, and VRAM line graphs over time |
| 3 | 📝 Data | Frame-by-frame data table with spike filtering and screenshot viewer |
| 4 | 🎯 Auto-Fix | Offender list with severity badges and per-item fix buttons |
| 5 | 📦 Build Size | Asset size treemap with auto-crunch compress |
| 6 | 📱 Wireless | TCP server for wireless mobile telemetry |
| 7 | 🔥 Heatmaps | Scene View mode switching (Overdraw, Wireframe) |
| 8 | 🗃️ Leaks | Ghost memory scanner, object snapshots, AI suggestions |
| 9 | 💾 Export | CSV, JSON, and HTML Flame Graph pipelines |
| 10 | ⚖️ Compare | A/B delta comparator for two reports |
| 11 | 🛠️ Deep Scan | Full `/Assets/` scan for oversized audio/textures |
| 12 | 🏢 Enterprise | Slack webhooks, AI Code Doctor, API configuration |
| 13 | 💡 Advisor | 40+ project-wide optimization tips with batch auto-fix |

---

## 🔄 CI/CD Integration

### Pre-Build Guardian
```
Window → Analysis → Auto Profiler Guardian → Enable
```
Blocks builds if any High-severity violations exist.

### Headless CLI
```bash
Unity.exe -batchmode -projectPath /path/to/project \
  -executeMethod AutoPerformanceProfiler.Editor.ProfilerCLI.RunCIAnalysis \
  -quit
# Exit code: 0 = passed, 1 = violations
```

---

## 📄 License

Auto Performance Profiler Pro is a commercial Unity Asset. All rights reserved.

---

## 📞 Support

- 📖 [Technical Documentation](Assets/AutoPerformanceProfiler/DOCUMENTATION.md)
- 📋 [Feature Reference](Assets/AutoPerformanceProfiler/README.md)

---

*Made for the Unity Asset Store — Zero third-party dependencies. Precompiled Assembly Definitions for fast compile times.*
]]>
