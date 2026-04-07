using UnityEditor;
using UnityEngine;

namespace AutoPerformanceProfiler.Editor
{
    public static class ProfilerCLI
    {
        public static void RunCIAnalysis()
        {
            Debug.Log("[AutoProfiler CI/CD] Starting Headless Pipeline Guardian...");
            var offenders = ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis();
            
            bool hasCritical = offenders.Exists(o => o.severity == "High");

            if (hasCritical)
            {
                Debug.LogError("[AutoProfiler CI/CD] 🚨 CRITICAL SEVERITY BUILD FAILURE.");
                foreach (var o in offenders)
                {
                    if (o.severity == "High")
                        Debug.LogError($"[VIOLATION] Object: {o.gameObjectName} | Issue: {o.issueDescription}");
                }

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
            else
            {
                Debug.Log("[AutoProfiler CI/CD] ✅ Project passed baseline performance metrics. Build approved.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
        }
    }
}
