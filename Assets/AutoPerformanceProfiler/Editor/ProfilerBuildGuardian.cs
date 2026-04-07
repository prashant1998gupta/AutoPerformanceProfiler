using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using AutoPerformanceProfiler.Runtime;
using System.Linq;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// Enterprise feature. Hooks into the Unity Build pipeline before compilation.
    /// Scans the project for severe performance bottlenecks (like Mono backends, excessive texture bloat, or missing LODs)
    /// and throws a build error if the active hardware budget constraints are fundamentally violated.
    /// </summary>
    public class ProfilerBuildGuardian : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string PrefKey = "AutoProfiler_EnableBuildGuardian";

        [MenuItem("Window/Analysis/Auto Profiler Guardian/Enable Pre-Build Guardian")]
        public static void EnableGuardian()
        {
            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("[Profiler Guardian] CI/CD Pre-Build Guardian ENABLED. Your builds will be scanned for critical performance flaws before compiling.");
        }

        [MenuItem("Window/Analysis/Auto Profiler Guardian/Disable Pre-Build Guardian")]
        public static void DisableGuardian()
        {
            EditorPrefs.SetBool(PrefKey, false);
            Debug.Log("[Profiler Guardian] CI/CD Pre-Build Guardian DISABLED.");
        }

        [MenuItem("Window/Analysis/Auto Profiler Guardian/Enable Pre-Build Guardian", true)]
        public static bool ValidateEnable() => !EditorPrefs.GetBool(PrefKey, false);
        
        [MenuItem("Window/Analysis/Auto Profiler Guardian/Disable Pre-Build Guardian", true)]
        public static bool ValidateDisable() => EditorPrefs.GetBool(PrefKey, false);

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorPrefs.GetBool(PrefKey, false)) return;

            Debug.Log("[Profiler Guardian] Initiating Pre-Build Analysis...");

            // Run the deep offline analyzer on current scene/project
            var offenders = ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis();

            // Filter for only the highest severity blockers that legitimately ruin builds
            var criticalIssues = offenders.Where(o => o.severity == "High").ToList();

            if (criticalIssues.Count > 0)
            {
                Debug.LogError($"[Profiler Guardian] Failed: Found {criticalIssues.Count} CRITICAL unoptimized assets/settings. Fix these before building! See specific errors below:");
                
                foreach(var issue in criticalIssues)
                {
                    Debug.LogError($"[Guardian BLOCKED]: {issue.componentName} on {issue.gameObjectName} -> {issue.issueDescription}");
                }

                // Actually stop the Unity Build process
                throw new BuildFailedException("Auto Performance Profiler intercepted the build due to severe optimization violations. Check the console or disable the Guardian.");
            }

            Debug.Log("[Profiler Guardian] Analysis Passed! Commencing Unity Build...");
        }
    }
}
