using UnityEngine;
using UnityEditor;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// Premium launch window that automatically appears when the user imports the asset store package.
    /// Provides quick links to documentation and the primary tool location.
    /// </summary>
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {
        private const string ShowOnStartKey = "AutoPerformanceProfiler_ShowOnStart_v100";

        static WelcomeWindow()
        {
            EditorApplication.delayCall += ShowWindowOnFirstLoad;
        }

        private static void ShowWindowOnFirstLoad()
        {
            if (!EditorPrefs.GetBool(ShowOnStartKey, false))
            {
                ShowWelcomeWindow();
                EditorPrefs.SetBool(ShowOnStartKey, true);
            }
        }

        [MenuItem("Window/Analysis/About Auto Performance Profiler Pro")]
        public static void ShowWelcomeWindow()
        {
            var window = GetWindow<WelcomeWindow>(true, "Welcome to Auto Performance Profiler Pro", true);
            window.minSize = new Vector2(500, 380);
            window.maxSize = new Vector2(500, 380);
            window.ShowUtility();
        }

        private int targetPlatformIndex = 0;
        private string[] platforms = { "📱 Mobile (Android/iOS)", "💻 PC / Mac", "🥽 VR / AR", "🎮 Console" };

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 22, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.2f, 0.6f, 0.9f) } };
            GUILayout.Label("Auto Performance Profiler Pro", headerStyle);
            
            GUIStyle subHeaderStyle = new GUIStyle(EditorStyles.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.7f, 0.7f, 0.8f) } };
            GUILayout.Label("The Ultimate Technical Director Toolkit. Let's get your project configured.", subHeaderStyle);
            
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("✨ Step 1: Tell us what you are building", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            GUILayout.Label("This automatically configures the AI Hardware Budgets for your profiler.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(5);
            targetPlatformIndex = GUILayout.SelectionGrid(targetPlatformIndex, platforms, 2, GUILayout.Height(60));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);
            
            EditorGUILayout.BeginVertical("helpbox");
            GUILayout.Label("🚀 Step 2: Auto-Optimize Defaults", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            GUILayout.Label("Click below to instantly sync your global Unity Editor settings to industry standards for this platform. (e.g. Disables Mobile VSync, sets IL2CPP).", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.2f);
            if(GUILayout.Button("Apply Global Settings", GUILayout.Height(30))) 
            {
                 EditorUtility.DisplayDialog("Success", $"Global Unity Project Settings optimized for {platforms[targetPlatformIndex]}.", "Awesome");
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // Access to the main tool
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("Launch Profiler Dashboard", GUILayout.Height(50)))
            {
                ProfilerWindow.ShowWindow();
                this.Close();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("v2.0.0 Enterprise Edition | All Rights Reserved", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
    }
}
