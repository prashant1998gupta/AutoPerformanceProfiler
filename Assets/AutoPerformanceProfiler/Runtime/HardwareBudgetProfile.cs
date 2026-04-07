using UnityEngine;

namespace AutoPerformanceProfiler.Runtime
{
    [CreateAssetMenu(fileName = "New Hardware Budget Profile", menuName = "Auto Profiler/Hardware Budget Profile")]
    public class HardwareBudgetProfile : ScriptableObject
    {
        [Header("Target Identity")]
        public string profileName = "Mobile Native";
        [TextArea] public string description = "Standard limits for modern mobile devices to prevent thermal throttling.";

        [Header("FPS & CPU Constraints")]
        public float fpsThreshold = 30f;
        public float cpuTimeSpikeMs = 33f;

        [Header("Memory Constraints (MB)")]
        public long maxTextureMemoryMB = 512;
        public long maxTotalRAMMB = 1024;
        
        [Header("Garbage Collection")]
        public long gcThresholdBytes = 100 * 1024;

        [Header("Graphics & Drawing")]
        public long batchesWarningLimit = 1000;
        public long trisWarningLimit = 1000000;
        
        [Header("Object Limits")]
        public int maxActiveGameObjects = 4000;
    }
}
