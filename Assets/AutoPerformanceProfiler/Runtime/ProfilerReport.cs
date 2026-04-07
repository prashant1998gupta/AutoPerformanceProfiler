using UnityEngine;
using System.Collections.Generic;

namespace AutoPerformanceProfiler.Runtime
{
    [System.Serializable]
    public struct FrameData
    {
        public float time;
        public float fps;
        public float mainThreadTimeMs;
        public float scriptsTimeMs;
        public float renderThreadTimeMs;
        public float physicsTimeMs;
        public float animTimeMs;
        
        public long allocatedMemoryMB;
        public long gcAllocatedInFrameBytes;
        
        public long drawCalls;
        public long batches;
        public long triangles;
        public long vertices;

        // VRAM & Assets tracking
        public long textureMemoryMB;
        public long meshMemoryMB;

        // Thermal & Battery
        public float batteryLevel;
        
        // Spike & Media
        public bool isSpikeFrame;
        public string screenshotPath;
    }

    [System.Serializable]
    public struct ObjectSnapshot
    {
        public float time;
        public int totalGameObjects;
        public int totalMonoBehaviours;
    }

    [System.Serializable]
    public struct ObjectOffender
    {
        public string gameObjectName;
        public string componentName;
        public string severity; // "High", "Medium", "Low"
        public string issueDescription;
        public string recommendedFix;
    }

    /// <summary>
    /// ScriptableObject definition used to save and serialize performance tracking logs.
    /// Used by both Runtime PerformanceTracker and Editor ProfilerWindow.
    /// </summary>
    [CreateAssetMenu(fileName = "NewProfilerReport", menuName = "Performance Profiler/Report", order = 1)]
    public class ProfilerReport : ScriptableObject
    {
        public string sessionName;
        public string timestamp;
        public string sceneName;
        public string deviceModel;
        public string osVersion;
        public float duration;
        public int totalFramesRecorded;
        
        [Header("Global Metrics (Averages)")]
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        
        public float averageMainThreadMs;
        public float averageScriptsMs;
        public float averageRenderMs;
        public float averagePhysicsMs;

        public long maxMemoryMB;
        public long maxTextureMemoryMB;
        public long totalGCAllocationsMB;
        public long averageTriangles;
        public long averageBatches;

        public float startBatteryLevel;
        public float endBatteryLevel;

        [Header("Periodic Snapshots")]
        public List<ObjectSnapshot> objectSnapshots = new List<ObjectSnapshot>();

        [Header("Frame Data Map")]
        public List<FrameData> frames = new List<FrameData>();
        
        [Header("Bottlenecks Detected")]
        public List<string> warnings = new List<string>();
        
        [Header("Optimization Suggestions")]
        public List<string> suggestions = new List<string>();

        [Header("Specific GameObjects Found Unoptimized")]
        public List<ObjectOffender> offenders = new List<ObjectOffender>();
    }
}
