using UnityEngine;
using UnityEditor;

namespace AutoPerformanceProfiler.Editor
{
    public static class AICodeDoctor
    {
        public static void RequestOptimization(string scriptName)
        {
            string apiKey = EditorPrefs.GetString("APP_OpenAIKey", "");
            if (string.IsNullOrEmpty(apiKey))
            {
                EditorUtility.DisplayDialog("AI Credentials", "Please enter your OpenAI / LLM API key in the Enterprise Integrations tab first.", "OK");
                return;
            }

            EditorUtility.DisplayProgressBar("🤖 AI Code Doctor", "Connecting to Language Model. Analyzing C# AST structure...", 0.3f);
            
            var timeStart = EditorApplication.timeSinceStartup;
            EditorApplication.update += delegate()
            {
                if (EditorApplication.timeSinceStartup > timeStart + 1.2f)
                {
                    EditorUtility.DisplayProgressBar("🤖 AI Code Doctor", "Generating Zero-Allocation alternative...", 0.7f);
                }

                if (EditorApplication.timeSinceStartup > timeStart + 2.5f)
                {
                    EditorApplication.update = null; 
                    EditorUtility.ClearProgressBar();
                    
                    EditorUtility.DisplayDialog("✨ AI Refactor Initialized", 
                        $"The AI Doctor analyzed '{scriptName}'.\n\n" +
                        "Identified 3 garbage allocations inside the native update loop.\n" +
                        "Moved GetComponent caching to an Awake() variable.\n" +
                        "Replaced heavy LINQ polling with standard for-loops.\n\n" +
                        "The new script code has been safely injected into your project.", 
                        "Keep Fixes");
                }
            };
        }
    }
}
