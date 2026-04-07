using UnityEngine;
using UnityEditor;
using AutoPerformanceProfiler.Runtime;
using System.Linq;
using System.IO;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using UnityEditor.SceneManagement;

namespace AutoPerformanceProfiler.Editor
{
    public class ProfilerWindow : EditorWindow
    {
        private ProfilerReport activeReport;
        private Vector2 scrollPos;
        private int selectedTab = 0;
        private string cachedIP = "";
        private readonly string[] tabs = { "📊 Dash", "📈 Graphs", "📝 Data", "🎯 Auto-Fix", "📦 Build Size", "📱 Wireless", "🔥 Heatmaps", "🗃️ Leaks", "💾 Export", "⚖️ Compare", "🛠️ Deep Scan", "🏢 Enterprise", "💡 Advisor" };


        private ProfilerReport compareReportA;
        private ProfilerReport compareReportB;

        private GUIStyle headerStyle;
        private GUIStyle titleStyle;
        private GUIStyle valueStyle;
        private GUIStyle cardStyle;
        private GUIStyle listHeaderStyle;
        private GUIStyle listRowStyle;
        private GUIStyle suggestionStyle;
        private GUIStyle boldLabelCenter;

        private bool showOnlySpikes = false;

        private Texture2D loadedScreenshot;
        private string currentScreenshotPath;

        [MenuItem("Window/Analysis/Auto Performance Profiler Pro")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProfilerWindow>("Auto Profiler Pro");
            window.minSize = new Vector2(950, 650);
            window.Show();
        }

        private void OnEnable() 
        {
            if (EditorPrefs.GetBool("AutoProfilerLiveStream", false)) {
                StartWirelessServer();
            }
        }

        private void OnDisable()
        {
            if (isListening) {
                EditorPrefs.SetBool("AutoProfilerLiveStream", true);
                CloseInternalServer();
            }
        }

        private void OnDestroy()
        {
            EditorPrefs.SetBool("AutoProfilerLiveStream", false);
            CloseInternalServer();
            if (loadedScreenshot != null)
                DestroyImmediate(loadedScreenshot);
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.1f, 0.1f, 0.1f) }
            };

            titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, normal = { textColor = new Color(0.6f, 0.6f, 0.7f) } };
            valueStyle = new GUIStyle(EditorStyles.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.1f, 0.1f, 0.1f) } };
            cardStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(15, 15, 15, 15), margin = new RectOffset(5, 5, 5, 5) };
            
            listHeaderStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.7f, 0.8f, 1.0f) } };
            listRowStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
            
            suggestionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 13,
                wordWrap = true,
                padding = new RectOffset(10, 10, 10, 10),
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.9f, 1.0f) : new Color(0.1f, 0.2f, 0.3f) }
            };

            boldLabelCenter = new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = Color.black } };
        }

        private void OnGUI()
        {
            if (headerStyle == null) InitStyles();

            DrawHeader();
            DrawToolbar();
            EditorGUILayout.Space(10);

            selectedTab = GUILayout.Toolbar(selectedTab, tabs, GUILayout.Height(30));
            EditorGUILayout.Space(5);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            if (activeReport == null && selectedTab != 5 && selectedTab != 10 && selectedTab != 11 && selectedTab != 12)
            {
                DrawEmptyState();
                EditorGUILayout.EndScrollView();
                return;
            }

            switch (selectedTab)
            {
                case 0: DrawDashboardTab(); break;
                case 1: DrawGraphViewTab(); break;
                case 2: DrawDataListTab(); break;
                case 3: DrawTargetsTab(); break;
                case 4: DrawBuildExplorerTab(); break;
                case 5: DrawWirelessMobileTab(); break;
                case 6: DrawHeatmapTab(); break;
                case 7: DrawAnalysisTab(); break;
                case 8: DrawExportsTab(); break;
                case 9: DrawComparatorTab(); break;
                case 10: DrawDeepProjectScanTab(); break;
                case 11: DrawEnterpriseTab(); break;
                case 12: DrawAdvisorTab(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.85f, 0.85f, 0.85f));
            GUI.Label(headerRect, "🚀 Auto Performance Profiler Pro", headerStyle);
            
            Rect lineRect = EditorGUILayout.GetControlRect(GUILayout.Height(3));
            EditorGUI.DrawRect(lineRect, new Color(0.1f, 0.5f, 0.8f));
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Report:", GUILayout.Width(50));
            activeReport = (ProfilerReport)EditorGUILayout.ObjectField(activeReport, typeof(ProfilerReport), false, GUILayout.Width(250));
            GUILayout.FlexibleSpace();

            bool isPlaying = Application.isPlaying;
            EditorGUI.BeginDisabledGroup(!isPlaying);
            if (PerformanceTracker.Instance != null)
            {
                if (PerformanceTracker.Instance.IsRecording())
                {
                    GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                    if (GUILayout.Button("⏹ Stop & Analyze", EditorStyles.toolbarButton, GUILayout.Width(120)))
                    {
                        PerformanceTracker.Instance.StopRecording();
                        
                        // Append Editor-Only deep checks directly to the freshly created report!
                        var freshOffenders = ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis();
                        if (freshOffenders.Count > 0)
                        {
                            var activeInternalReport = Selection.activeObject as ProfilerReport;
                            if (activeInternalReport != null)
                            {
                                activeInternalReport.offenders.AddRange(freshOffenders);
                                EditorUtility.SetDirty(activeInternalReport);
                                AssetDatabase.SaveAssets();
                                activeReport = activeInternalReport;
                            }
                        }
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                    if (GUILayout.Button("⏺ Start Rec", EditorStyles.toolbarButton, GUILayout.Width(90)))
                    {
                        PerformanceTracker.Instance.StartRecording();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f);
                if (GUILayout.Button("⚡ Inject System", EditorStyles.toolbarButton, GUILayout.Width(110)))
                {
                    new GameObject("PerformanceTracker").AddComponent<PerformanceTracker>();
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("🔄 Refresh", EditorStyles.toolbarButton, GUILayout.Width(70))) Repaint();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEmptyState()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            GUIStyle emptyStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, normal = { textColor = Color.gray } };
            GUILayout.Label("📭", new GUIStyle(emptyStyle) { fontSize = 48 });
            GUILayout.Label("Awaiting Profiler Data", emptyStyle);
            
            EditorGUILayout.Space(20);
            if (GUILayout.Button("Run Offline Scene Scan (No Playmode Required)", GUILayout.Height(40)))
            {
                var offenders = ProfilerAnalyzerExtensions.RunAdvancedEditorAnalysis();
                if(offenders.Count == 0) { EditorUtility.DisplayDialog("Clean", "No obvious bad practices found in current scene layout.", "OK"); }
                else
                {
                    // Synthesize a dummy report to hold only offenders
                    activeReport = ScriptableObject.CreateInstance<ProfilerReport>();
                    activeReport.sessionName = "Offline Analysis";
                    activeReport.offenders.AddRange(offenders);
                    selectedTab = 3; // jump to targets tab
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        private void DrawDashboardTab()
        {
            float healthScore = CalculateHealthScore();
            string letterGrade = GetLetterGrade(healthScore);
            Color gradeColor = GetGradeColor(letterGrade);

            EditorGUILayout.BeginHorizontal(cardStyle);
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            var bigGradeStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 72, alignment = TextAnchor.MiddleCenter, normal = { textColor = gradeColor } };
            GUILayout.Label(letterGrade, bigGradeStyle);
            GUILayout.Label("Project Health", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 12 });
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"🌐 Scene: {activeReport.sceneName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"⏱ Duration: {activeReport.duration:F1}s | Frames: {activeReport.totalFramesRecorded}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            float progress = healthScore / 100f;
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(20));
            EditorGUI.DrawRect(r, new Color(0.1f, 0.1f, 0.1f));
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * progress, r.height), gradeColor);
            GUI.Label(r, $"Optimization Score: {healthScore:F0}%", new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.black } });
            
            EditorGUILayout.Space(10);
            if (activeReport.offenders.Count > 0)
            {
                EditorGUILayout.HelpBox($"Found {activeReport.offenders.Count} performance killers. Fixing these will boost your score to 'A'.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("🏆 Perfection! No optimization risks detected in this diagnostic.", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            DrawMetricCard("Avg FPS", $"{activeReport.averageFPS:F0}", GetFPSColor(activeReport.averageFPS), "Smoothness");
            DrawMetricCard("Peak RAM", $"{activeReport.maxMemoryMB} MB", new Color(0.6f, 0.3f, 0.8f), "System Memory");
            DrawMetricCard("Peak VRAM", $"{activeReport.maxTextureMemoryMB} MB", new Color(0.8f, 0.4f, 0.2f), "Texture Memory");
            
            float batteryDrain = activeReport.startBatteryLevel - activeReport.endBatteryLevel;
            string batText = activeReport.startBatteryLevel < 0 ? "N/A" : $"-{batteryDrain*100:F1}%";
            DrawMetricCard("Power Drain", batText, batteryDrain > 0.05f ? Color.red : Color.green, "Thermal Impact");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("CPU Subsystem Cost (Per Frame Avg)", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawMetricCard("Scripts C#", $"{activeReport.averageScriptsMs:F1} ms", activeReport.averageScriptsMs > 8f ? Color.red : Color.cyan, "Logic & Updates");
            DrawMetricCard("Render (CPU)", $"{activeReport.averageRenderMs:F1} ms", activeReport.averageRenderMs > 8f ? Color.red : Color.cyan, "Draw Call Prep");
            DrawMetricCard("Physics", $"{activeReport.averagePhysicsMs:F1} ms", activeReport.averagePhysicsMs > 5f ? Color.red : Color.cyan, "Collisions/Rigid");
            EditorGUILayout.EndHorizontal();

            DrawOptimizationRoadmap();
        }

        private float CalculateHealthScore()
        {
            if (activeReport == null) return 0;
            float score = 100;
            
            // Deduct for low FPS
            if (activeReport.averageFPS < 60) score -= (60 - activeReport.averageFPS) * 1.5f;
            
            // Deduct for offenders
            int highRisks = activeReport.offenders.Count(o => o.severity == "High");
            int medRisks = activeReport.offenders.Count(o => o.severity == "Medium");
            
            score -= highRisks * 15;
            score -= medRisks * 5;
            
            return Mathf.Clamp(score, 0, 100);
        }

        private string GetLetterGrade(float score)
        {
            if (score >= 90) return "A+";
            if (score >= 80) return "A";
            if (score >= 70) return "B";
            if (score >= 60) return "C";
            if (score >= 40) return "D";
            return "F";
        }

        private Color GetGradeColor(string grade)
        {
            if (grade.StartsWith("A")) return new Color(0.3f, 0.8f, 0.4f);
            if (grade == "B") return new Color(0.7f, 0.8f, 0.2f);
            if (grade == "C") return new Color(0.9f, 0.7f, 0.2f);
            if (grade == "D") return new Color(0.9f, 0.5f, 0.2f);
            return new Color(0.9f, 0.3f, 0.3f);
        }

        private void DrawOptimizationRoadmap()
        {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("🗺️ YOUR OPTIMIZATION ROADMAP", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(cardStyle);
            
            if (activeReport == null || activeReport.offenders.Count == 0)
            {
                GUILayout.Label("✨ All systems optimal. No actions required.", EditorStyles.miniLabel);
            }
            else
            {
                var roadmapItems = activeReport.offenders
                    .OrderByDescending(o => o.severity == "High" ? 2 : 1)
                    .Take(3);

                foreach (var item in roadmapItems)
                {
                    EditorGUILayout.BeginHorizontal();
                    string icon = item.severity == "High" ? "🔴" : "🟡";
                    EditorGUILayout.LabelField($"{icon} {item.issueDescription.Substring(0, Mathf.Min(item.issueDescription.Length, 60))}...", EditorStyles.wordWrappedLabel);
                    if (GUILayout.Button("Go to Fix", GUILayout.Width(80)))
                    {
                        selectedTab = 3; // Jump to targets tab
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }


        private void DrawMetricCard(string title, string value, Color valueColor, string subtitle)
        {
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label(title, titleStyle);
            
            var cachedColor = valueStyle.normal.textColor;
            valueStyle.normal.textColor = valueColor;
            GUILayout.Label(value, valueStyle);
            valueStyle.normal.textColor = cachedColor;
            
            GUILayout.Label(subtitle, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private Color GetFPSColor(float fps) => fps >= 55 ? new Color(0.3f, 0.8f, 0.4f) : (fps >= 30 ? new Color(0.9f, 0.7f, 0.2f) : new Color(0.9f, 0.3f, 0.3f));

        private void DrawGraphViewTab()
        {
            if (activeReport.frames == null || activeReport.frames.Count < 2) return;

            DrawLineGraph("FPS Stability", activeReport.frames.Select(f => f.fps).ToArray(), 0, Mathf.Max(60, activeReport.maxFPS), new Color(0.2f, 0.8f, 0.4f), true);
            EditorGUILayout.Space(20);
            
            float maxMain = activeReport.frames.Max(f => f.mainThreadTimeMs);
            DrawLineGraph("Main Thread CPU (ms)", activeReport.frames.Select(f => f.mainThreadTimeMs).ToArray(), 0, Mathf.Max(16f, maxMain), new Color(0.9f, 0.3f, 0.3f), false);
            EditorGUILayout.Space(20);

            float maxVRAM = activeReport.frames.Max(f => f.textureMemoryMB);
            DrawLineGraph("VRAM Allocated (MB)", activeReport.frames.Select(f => (float)f.textureMemoryMB).ToArray(), 0, Mathf.Max(50f, maxVRAM), new Color(0.8f, 0.4f, 0.2f), false);
        }

        private void DrawLineGraph(string title, float[] data, float minVal, float maxVal, Color graphColor, bool isHigherBetter)
        {
            EditorGUILayout.LabelField($"{(isHigherBetter ? "📈" : "📉")} {title}", EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100), GUILayout.ExpandWidth(true));
            
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
            Handles.DrawLine(new Vector3(rect.x, rect.y + rect.height / 2), new Vector3(rect.xMax, rect.y + rect.height / 2));

            GUI.Label(new Rect(rect.x + 5, rect.y, 50, 20), $"{maxVal:F1}", EditorStyles.miniLabel);
            GUI.Label(new Rect(rect.x + 5, rect.yMax - 15, 50, 20), $"{minVal:F1}", EditorStyles.miniLabel);

            Handles.color = graphColor;
            Vector3[] points = new Vector3[data.Length];
            float xStep = rect.width / (data.Length - 1);
            float valueRange = maxVal - minVal;
            if (valueRange <= 0) valueRange = 1f;

            for (int i = 0; i < data.Length; i++)
            {
                float normalizedValue = (data[i] - minVal) / valueRange;
                points[i] = new Vector3(rect.x + i * xStep, rect.yMax - (normalizedValue * rect.height), 0);
            }
            Handles.DrawAAPolyLine(3.0f, points);
            Handles.color = Color.white;
        }

        private void DrawDataListTab()
        {
            if (activeReport.frames == null || activeReport.frames.Count == 0) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Frame By Frame Feed", EditorStyles.boldLabel);
            showOnlySpikes = EditorGUILayout.ToggleLeft("Show Only Spikes (Lag Frames)", showOnlySpikes, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            if (loadedScreenshot != null)
            {
                EditorGUILayout.BeginVertical(cardStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("📸 Last Opened Screenshot:", EditorStyles.boldLabel);
                if (GUILayout.Button("Close Image", GUILayout.Width(100)))
                {
                    DestroyImmediate(loadedScreenshot);
                    loadedScreenshot = null;
                }
                EditorGUILayout.EndHorizontal();
                
                if (loadedScreenshot != null)
                {
                    float aspect = (float)loadedScreenshot.width / loadedScreenshot.height;
                    float w = Mathf.Min(position.width - 60, 400);
                    float h = w / aspect;
                    Rect texRect = GUILayoutUtility.GetRect(w, h);
                    GUI.DrawTexture(texRect, loadedScreenshot, ScaleMode.ScaleToFit);
                    EditorGUILayout.LabelField(currentScreenshotPath, EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal(cardStyle);
            GUILayout.Label("Time", listHeaderStyle, GUILayout.Width(50));
            GUILayout.Label("FPS", listHeaderStyle, GUILayout.Width(40));
            GUILayout.Label("CPU Main", listHeaderStyle, GUILayout.Width(70));
            GUILayout.Label("Scripts", listHeaderStyle, GUILayout.Width(60));
            GUILayout.Label("Render", listHeaderStyle, GUILayout.Width(60));
            GUILayout.Label("VRAM", listHeaderStyle, GUILayout.Width(60));
            GUILayout.Label("Screenshot", listHeaderStyle, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();

            var renderedFrames = showOnlySpikes ? activeReport.frames.Where(x => x.isSpikeFrame).ToList() : activeReport.frames;

            int displayCount = Mathf.Min(renderedFrames.Count, 300); 
            for (int i = 0; i < displayCount; i++)
            {
                var f = renderedFrames[i];
                Color bgColor = (i % 2 == 0) ? new Color(0f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.15f);
                if (f.isSpikeFrame) bgColor = new Color(0.9f, 0.2f, 0.2f, 0.25f); 
                if (EditorGUIUtility.isProSkin && !f.isSpikeFrame) bgColor = (i % 2 == 0) ? new Color(1f, 1f, 1f, 0.05f) : new Color(1f, 1f, 1f, 0.1f);

                Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(22));
                EditorGUI.DrawRect(r, bgColor);

                GUILayout.Label($"{f.time:F2}", listRowStyle, GUILayout.Width(50));
                
                var fpsStyle = new GUIStyle(listRowStyle);
                fpsStyle.normal.textColor = GetFPSColor(f.fps);
                fpsStyle.fontStyle = f.isSpikeFrame ? FontStyle.Bold : FontStyle.Normal;
                GUILayout.Label($"{f.fps:F0}", fpsStyle, GUILayout.Width(40));
                
                GUILayout.Label($"{f.mainThreadTimeMs:F1}", listRowStyle, GUILayout.Width(70));
                GUILayout.Label($"{f.scriptsTimeMs:F1}", listRowStyle, GUILayout.Width(60));
                GUILayout.Label($"{f.renderThreadTimeMs:F1}", listRowStyle, GUILayout.Width(60));
                GUILayout.Label($"{f.textureMemoryMB}", listRowStyle, GUILayout.Width(60));
                
                if (f.isSpikeFrame && !string.IsNullOrEmpty(f.screenshotPath))
                {
                    if (GUILayout.Button("👁 View", GUILayout.Width(50), GUILayout.Height(18))) LoadScreenshot(f.screenshotPath);
                    if (GUILayout.Button("📢 Slack", GUILayout.Width(60), GUILayout.Height(18)))
                    {
                        StudioIntegrations.SendSlackAlert($"Critical Lag Spike at {f.time:F1}s! FPS dropped to {f.fps:F0}. RAM Peak: {f.textureMemoryMB}MB.", "High");
                    }
                }
                else
                {
                    GUILayout.Label("-", listRowStyle, GUILayout.Width(60));
                }

                EditorGUILayout.EndHorizontal();
            }

            if (renderedFrames.Count > 300) EditorGUILayout.HelpBox($"Displaying first 300 results to keep UI responsive.", MessageType.Warning);
        }

        private void LoadScreenshot(string path)
        {
            if (File.Exists(path))
            {
                if (loadedScreenshot != null) DestroyImmediate(loadedScreenshot);
                byte[] bytes = File.ReadAllBytes(path);
                loadedScreenshot = new Texture2D(2, 2);
                loadedScreenshot.LoadImage(bytes);
                currentScreenshotPath = path;
            }
            else
            {
                Debug.LogWarning($"Screenshot not found at: {path}");
            }
        }

        private void DrawTargetsTab()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🎯 Specific Objects & Scripts to Optimize", EditorStyles.boldLabel);
            if (GUILayout.Button("🛠️ MAGIC FIX: Resolve All Non-Script Issues Automatically", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Magic Fix", "This will automatically change GameObject settings across your scene based on our analysis (E.g Disable Point Light shadows, check 'Static' boxes on unused meshes). Are you sure?", "Yes, Optimize my Scene", "Cancel"))
                {
                    foreach (var offender in activeReport.offenders.ToList())
                    {
                        if (offender.componentName == "C# Roslyn Refactor (GetComponent)") continue;
                        ProfilerAnalyzerExtensions.AutoFixSpecificOffender(offender);
                    }
                    activeReport.offenders.RemoveAll(o => o.componentName != "Multiple Scripts (Architecture)" && o.componentName != "AudioSource/AudioClip" && o.componentName != "C# Roslyn Refactor (GetComponent)");
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("These Explicit items were flagged as Performance Killers.", MessageType.Info);
            EditorGUILayout.Space(10);

            if (activeReport.offenders == null || activeReport.offenders.Count == 0)
            {
                var greenStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 18, normal = { textColor = new Color(0.3f, 0.8f, 0.4f) } };
                GUILayout.Label("✨ No severe Scene Offenders detected! Your Hierarchy is clean.", greenStyle);
                return;
            }

            foreach (var offender in activeReport.offenders)
            {
                EditorGUILayout.BeginVertical(cardStyle);

                EditorGUILayout.BeginHorizontal();
                var color = offender.severity == "High" ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.9f, 0.7f, 0.2f);
                var svStyle = new GUIStyle(EditorStyles.boldLabel); svStyle.normal.textColor = color;
                GUILayout.Label($"[{offender.severity} Risk]", svStyle, GUILayout.Width(90));
                GUILayout.Label($"Object: {offender.gameObjectName}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField($"Component: {offender.componentName}", EditorStyles.miniLabel);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.LabelField($"Issue: {offender.issueDescription}", EditorStyles.wordWrappedLabel);
                
                var fixStyle = new GUIStyle(EditorStyles.wordWrappedLabel); 
                fixStyle.normal.textColor = new Color(0.3f, 0.8f, 0.4f);
                EditorGUILayout.LabelField($"Fix: {offender.recommendedFix}", fixStyle);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                // Build out smart buttons based on issue
                bool isGodObjOrAudio = offender.componentName == "Multiple Scripts (Architecture)" || offender.componentName == "AudioSource/AudioClip";

                if (offender.componentName == "C# Roslyn Refactor (GetComponent)")
                {
                    GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f); // Blue
                    if (GUILayout.Button("✨ AI Code Doctor (Auto-Fix C#)", GUILayout.Width(220), GUILayout.Height(25)))
                    {
                        AICodeDoctor.RequestOptimization(offender.gameObjectName);
                        activeReport.offenders.Remove(offender);
                        GUIUtility.ExitGUI();
                    }
                    GUI.backgroundColor = Color.white;
                }
                else if (offender.componentName == "Missing LODGroup")
                {
                    GUI.backgroundColor = new Color(0.8f, 0.4f, 0.9f); // Purple
                    if (GUILayout.Button("✨ Auto-Generate LODs", GUILayout.Width(180), GUILayout.Height(25)))
                    {
                        ProfilerAnalyzerExtensions.AutoFixSpecificOffender(offender);
                        activeReport.offenders.Remove(offender);
                        GUIUtility.ExitGUI();
                    }
                    GUI.backgroundColor = Color.white;
                }
                else if (!isGodObjOrAudio)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f); // Green Fix Button
                    if (GUILayout.Button("✨ Auto-Fix", GUILayout.Width(100), GUILayout.Height(22)))
                    {
                        ProfilerAnalyzerExtensions.AutoFixSpecificOffender(offender);
                        activeReport.offenders.Remove(offender);
                        GUIUtility.ExitGUI(); // Prevent layout errors during deletion
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button("Find in Scene", GUILayout.Width(120), GUILayout.Height(22)))
                {
                    GameObject go = GameObject.Find(offender.gameObjectName);
                    if (go != null)
                    {
                        EditorGUIUtility.PingObject(go);
                        Selection.activeGameObject = go;
                        SceneView.FrameLastActiveSceneView();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }


        private void DrawAnalysisTab()
        {
            EditorGUILayout.LabelField("🗃️ Ghost Memory & Object Leak Tracker", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Scans all loaded Unity Objects in RAM and compares against active Scene references. Objects loaded but unreferenced are potential memory leaks ('Ghost Assets').", MessageType.Info);
            EditorGUILayout.Space(15);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.3f);
            if (GUILayout.Button("🔍 Scan for Ghost Objects in Memory", GUILayout.Width(300), GUILayout.Height(35)))
            {
                ScanGhostObjects();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            if (ghostResults != null && ghostResults.Count > 0)
            {
                EditorGUILayout.LabelField($"Found {ghostResults.Count} potentially orphaned objects in RAM:", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                foreach (var ghost in ghostResults)
                {
                    EditorGUILayout.BeginVertical(cardStyle);
                    EditorGUILayout.BeginHorizontal();
                    var svStyle = new GUIStyle(EditorStyles.boldLabel); svStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                    GUILayout.Label("[Ghost]", svStyle, GUILayout.Width(60));
                    GUILayout.Label($"'{ghost.name}' ({ghost.GetType().Name})", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Ping Object", GUILayout.Width(120), GUILayout.Height(22)))
                    {
                        EditorGUIUtility.PingObject(ghost);
                    }
                    if (GUILayout.Button("🧨 Destroy & Free RAM", GUILayout.Width(180), GUILayout.Height(22)))
                    {
                        Resources.UnloadAsset(ghost);
                        ghostResults.Remove(ghost);
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
            }
            else if (ghostResults != null)
            {
                var greenStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 16, normal = { textColor = new Color(0.3f, 0.8f, 0.4f) } };
                GUILayout.Label("✅ No ghost objects detected. Memory is clean!", greenStyle);
            }

            EditorGUILayout.Space(15);

            if (activeReport != null && activeReport.suggestions != null && activeReport.suggestions.Count > 0)
            {
                EditorGUILayout.LabelField("💡 Auto-Generated Strategy For Optimization", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                foreach (var suggestion in activeReport.suggestions)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("►", GUILayout.Width(20));
                    EditorGUILayout.LabelField(suggestion, suggestionStyle);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.Space(15);
            
            if (activeReport != null && activeReport.objectSnapshots != null && activeReport.objectSnapshots.Count > 0)
            {
                EditorGUILayout.LabelField("🔍 Memory Leak Detection (Periodic Object Counts)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(cardStyle);
                GUILayout.Label("Time | GameObjects | MonoBehaviours", EditorStyles.boldLabel);
                foreach (var snap in activeReport.objectSnapshots)
                {
                    GUILayout.Label($"[{snap.time:F1}s]   {snap.totalGameObjects}       |  {snap.totalMonoBehaviours}");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(15);
            }

            if (activeReport != null && activeReport.warnings != null && activeReport.warnings.Count > 0)
            {
                EditorGUILayout.LabelField($"🚨 Direct Event Logs ({activeReport.warnings.Count})", EditorStyles.boldLabel);
                int displayCount = Mathf.Min(activeReport.warnings.Count, 300);
                for (int i = 0; i < displayCount; i++)
                {
                    string warning = activeReport.warnings[i];
                    MessageType type = warning.Contains("FPS") || warning.Contains("Spike") ? MessageType.Error : MessageType.Warning;
                    EditorGUILayout.HelpBox(warning, type);
                }
            }
        }

        private System.Collections.Generic.List<Object> ghostResults;

        private void ScanGhostObjects()
        {
            ghostResults = new System.Collections.Generic.List<Object>();
            
            // Find all Texture2D loaded in memory
            Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (var tex in allTextures)
            {
                if (tex.hideFlags != HideFlags.None) continue;
                string path = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(path)) continue;
                if (path.StartsWith("Packages/")) continue;
                
                // Check if any scene object references this texture
                var deps = AssetDatabase.GetDependencies(EditorSceneManager.GetActiveScene().path, true);
                bool isReferenced = false;
                foreach (var dep in deps) {
                    if (dep == path) { isReferenced = true; break; }
                }
                if (!isReferenced && tex.width >= 2048) {
                    ghostResults.Add(tex);
                }
            }

            // Find all AudioClips loaded in memory
            AudioClip[] allClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            foreach (var clip in allClips)
            {
                if (clip.hideFlags != HideFlags.None) continue;
                string path = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(path) || path.StartsWith("Packages/")) continue;
                
                var deps = AssetDatabase.GetDependencies(EditorSceneManager.GetActiveScene().path, true);
                bool isReferenced = false;
                foreach (var dep in deps) {
                    if (dep == path) { isReferenced = true; break; }
                }
                if (!isReferenced) {
                    ghostResults.Add(clip);
                }
            }
        }

        private void DrawExportsTab()
        {
            EditorGUILayout.LabelField("💾 Data Export Pipelines", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Export your performance reports for external analysis, team sharing, or CI/CD dashboards.", MessageType.Info);
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f);
            if (GUILayout.Button("Export to CSV (Spreadsheet)", GUILayout.Width(220), GUILayout.Height(40)))
            {
                ExportCSV();
            }
            
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.8f, 0.5f, 0.2f);
            if (GUILayout.Button("Export to JSON (CI Tools)", GUILayout.Width(220), GUILayout.Height(40)))
            {
                ExportJSON();
            }
            
            GUILayout.Space(10);
            
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.4f);
            if (GUILayout.Button("🔥 Export Flame Graph (.html)", GUILayout.Width(250), GUILayout.Height(40)))
            {
                ExportFlameGraphHTML();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // ── Report Card ──
            EditorGUILayout.LabelField("📄 Premium Report Card", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generate a beautiful interactive HTML Report Card with grades, charts, and issue breakdowns. Share it with your team leads and stakeholders.", MessageType.Info);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.6f, 0.3f, 0.8f);
            if (GUILayout.Button("📄 Generate Performance Report Card (.html)", GUILayout.Width(350), GUILayout.Height(40)))
            {
                PerformanceReportCardGenerator.GenerateReportCard(activeReport, activeReport != null ? activeReport.offenders : null);
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // ── Duplicate Asset Finder ──
            EditorGUILayout.LabelField("🔍 Duplicate Asset Finder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Scan your entire /Assets/ folder for duplicate files (textures, audio, meshes, materials). Duplicates waste build size and confuse your team.", MessageType.Info);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.2f);
            if (GUILayout.Button("🔍 Scan for Duplicate Assets", GUILayout.Width(300), GUILayout.Height(40)))
            {
                RunDuplicateScan();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (duplicateGroups != null && duplicateGroups.Count > 0)
            {
                EditorGUILayout.Space(10);
                long wastedMB = DuplicateAssetFinder.GetTotalWastedBytes(duplicateGroups) / (1024 * 1024);
                EditorGUILayout.LabelField($"⚠️ Found {duplicateGroups.Count} duplicate groups wasting ~{wastedMB} MB of build size!", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                duplicateScroll = EditorGUILayout.BeginScrollView(duplicateScroll, cardStyle, GUILayout.Height(250));
                foreach (var group in duplicateGroups)
                {
                    EditorGUILayout.BeginVertical(cardStyle);
                    EditorGUILayout.BeginHorizontal();
                    var typeStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.9f, 0.5f, 0.2f) } };
                    GUILayout.Label($"[{group.assetType}]", typeStyle, GUILayout.Width(80));
                    GUILayout.Label($"{group.paths.Count} duplicates — wasting {(group.wastedBytes / 1024f / 1024f):F1} MB", EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();

                    foreach (var p in group.paths)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label($"  📁 {p}", EditorStyles.miniLabel);
                        if (GUILayout.Button("Ping", GUILayout.Width(50), GUILayout.Height(16)))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(p);
                            if (obj != null) EditorGUIUtility.PingObject(obj);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
                EditorGUILayout.EndScrollView();
            }
            else if (duplicateGroups != null)
            {
                EditorGUILayout.Space(10);
                var greenStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = { textColor = new Color(0.3f, 0.8f, 0.4f) } };
                GUILayout.Label("✅ No duplicate assets found! Your project is clean.", greenStyle);
            }
        }

        private System.Collections.Generic.List<DuplicateAssetFinder.DuplicateGroup> duplicateGroups;
        private Vector2 duplicateScroll;

        private void RunDuplicateScan()
        {
            duplicateGroups = DuplicateAssetFinder.FindAllDuplicates((progress, fileName) => {
                EditorUtility.DisplayProgressBar("🔍 Scanning for Duplicates", $"Hashing: {fileName}", progress);
            });
            EditorUtility.ClearProgressBar();
        }

        private void ExportCSV()
        {
            if (activeReport == null) return;
            string path = EditorUtility.SaveFilePanel("Export Profiler Data to CSV", "", activeReport.name + "_ProfilerData", "csv");
            if (string.IsNullOrEmpty(path)) return;
            
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("Time,FPS,Main CPU,Scripts CPU,Render CPU,Physics CPU,RAM MB,VRAM MB,GC Alloc Bytes,Draw Calls,Batches,Triangles,Is Spike");
                foreach(var f in activeReport.frames)
                {
                    writer.WriteLine($"{f.time},{f.fps},{f.mainThreadTimeMs},{f.scriptsTimeMs},{f.renderThreadTimeMs},{f.physicsTimeMs},{f.allocatedMemoryMB},{f.textureMemoryMB},{f.gcAllocatedInFrameBytes},{f.drawCalls},{f.batches},{f.triangles},{f.isSpikeFrame}");
                }
            }
            EditorUtility.RevealInFinder(path);
        }

        private void ExportJSON()
        {
            if (activeReport == null) return;
            string path = EditorUtility.SaveFilePanel("Export Profiler Data to JSON", "", activeReport.name + "_ProfilerData", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            string json = JsonUtility.ToJson(activeReport, true);
            File.WriteAllText(path, json);
            EditorUtility.RevealInFinder(path);
        }

        private void ExportFlameGraphHTML()
        {
            if (activeReport == null || activeReport.frames == null || activeReport.frames.Count == 0) return;
            string path = EditorUtility.SaveFilePanel("Export Flame Graph", "", activeReport.name + "_FlameGraph", "html");
            if (string.IsNullOrEmpty(path)) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            sb.AppendLine("<title>Performance Flame Graph - " + activeReport.sessionName + "</title>");
            sb.AppendLine("<style>body{background:#1a1a2e;color:#eee;font-family:sans-serif;padding:20px}");
            sb.AppendLine(".bar{display:inline-block;vertical-align:bottom;margin:1px;min-width:2px;cursor:pointer}");
            sb.AppendLine(".bar:hover{opacity:0.8}.chart{display:flex;align-items:flex-end;height:300px;border-bottom:2px solid #444;margin:20px 0}");
            sb.AppendLine("h1{color:#4fc3f7}h2{color:#aaa;font-weight:normal}.stats{display:flex;gap:30px;margin:20px 0}");
            sb.AppendLine(".stat{background:#16213e;padding:15px 25px;border-radius:8px}.stat h3{margin:0;color:#888;font-size:12px}.stat p{margin:5px 0 0;font-size:24px;font-weight:bold}</style></head><body>");
            sb.AppendLine("<h1>&#128293; Performance Flame Graph</h1>");
            sb.AppendLine("<h2>" + activeReport.sessionName + " | " + activeReport.sceneName + " | " + activeReport.duration.ToString("F1") + "s | " + activeReport.totalFramesRecorded + " frames</h2>");
            sb.AppendLine("<div class='stats'>");
            sb.AppendLine("<div class='stat'><h3>Avg FPS</h3><p>" + activeReport.averageFPS.ToString("F0") + "</p></div>");
            sb.AppendLine("<div class='stat'><h3>Peak RAM</h3><p>" + activeReport.maxMemoryMB + " MB</p></div>");
            sb.AppendLine("<div class='stat'><h3>Peak VRAM</h3><p>" + activeReport.maxTextureMemoryMB + " MB</p></div>");
            sb.AppendLine("<div class='stat'><h3>Avg CPU</h3><p>" + activeReport.averageMainThreadMs.ToString("F1") + " ms</p></div>");
            sb.AppendLine("</div>");

            sb.AppendLine("<h2>FPS Over Time</h2><div class='chart'>");
            float maxFps = activeReport.frames.Max(f => f.fps);
            if (maxFps < 1) maxFps = 60;
            float barW = Mathf.Max(2, 800f / activeReport.frames.Count);
            foreach (var f in activeReport.frames) {
                float h = (f.fps / maxFps) * 280f;
                string color = f.fps >= 55 ? "#4caf50" : (f.fps >= 30 ? "#ff9800" : "#f44336");
                if (f.isSpikeFrame) color = "#ff1744";
                sb.AppendLine("<div class='bar' style='height:" + h.ToString("F0") + "px;background:" + color + ";width:" + barW.ToString("F0") + "px' title='FPS: " + f.fps.ToString("F0") + " | CPU: " + f.mainThreadTimeMs.ToString("F1") + "ms'></div>");
            }
            sb.AppendLine("</div>");

            sb.AppendLine("<h2>Main Thread CPU (ms)</h2><div class='chart'>");
            float maxCpu = activeReport.frames.Max(f => f.mainThreadTimeMs);
            if (maxCpu < 1) maxCpu = 33;
            foreach (var f in activeReport.frames) {
                float h = (f.mainThreadTimeMs / maxCpu) * 280f;
                string color = f.mainThreadTimeMs > 16.6f ? "#f44336" : "#4fc3f7";
                sb.AppendLine("<div class='bar' style='height:" + h.ToString("F0") + "px;background:" + color + ";width:" + barW.ToString("F0") + "px' title='CPU: " + f.mainThreadTimeMs.ToString("F1") + "ms'></div>");
            }
            sb.AppendLine("</div>");

            sb.AppendLine("<p style='color:#666;margin-top:40px'>Generated by Auto Performance Profiler Pro</p>");
            sb.AppendLine("</body></html>");

            File.WriteAllText(path, sb.ToString());
            EditorUtility.RevealInFinder(path);
        }

        private TcpListener tcpServer;
        private TcpClient connectedClient;
        private NetworkStream networkStream;
        private StreamReader streamReader;
        private bool isListening = false;

        private void StartWirelessServer()
        {
            if (isListening) return;
            try {
                tcpServer = new TcpListener(IPAddress.Any, 8080);
                tcpServer.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                tcpServer.Start();
                isListening = true;
                EditorPrefs.SetBool("AutoProfilerLiveStream", true);
                EditorApplication.update -= CheckWirelessNetwork;
                EditorApplication.update += CheckWirelessNetwork;
                
                if (activeReport == null) {
                    activeReport = ScriptableObject.CreateInstance<ProfilerReport>();
                    activeReport.sessionName = "LIVE Stream Session";
                }
            } catch (System.Exception e) { Debug.LogError("TCP Server Error: " + e.Message); }
        }

        private void StopWirelessServer()
        {
            EditorPrefs.SetBool("AutoProfilerLiveStream", false);
            CloseInternalServer();
        }

        private void CloseInternalServer()
        {
            isListening = false;
            EditorApplication.update -= CheckWirelessNetwork;
            if (streamReader != null) { streamReader.Close(); streamReader = null; }
            if (networkStream != null) { networkStream.Close(); networkStream = null; }
            if (connectedClient != null) { connectedClient.Close(); connectedClient = null; }
            if (tcpServer != null) { tcpServer.Stop(); tcpServer = null; }
        }

        private void CheckWirelessNetwork()
        {
            if (!isListening || tcpServer == null) return;

            if (connectedClient == null) {
                if (tcpServer.Pending()) {
                    connectedClient = tcpServer.AcceptTcpClient();
                    networkStream = connectedClient.GetStream();
                    streamReader = new StreamReader(networkStream, System.Text.Encoding.UTF8);
                }
            }
            
            if (connectedClient != null && streamReader != null && networkStream.DataAvailable) {
                try {
                    string payload = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(payload)) {
                        var frame = JsonUtility.FromJson<FrameData>(payload);
                        if (activeReport != null) {
                            activeReport.frames.Add(frame);
                            activeReport.totalFramesRecorded = activeReport.frames.Count;
                            activeReport.duration = frame.time;
                            Repaint();
                        }
                    }
                } catch { 
                    StopWirelessServer(); 
                    StartWirelessServer(); 
                } 
            }
        }

        private void DrawWirelessMobileTab()
        {
            EditorGUILayout.LabelField("📱 Wireless Mobile Device Connector", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Connect your mobile device over WiFi to stream telemetry directly into the Editor without USB thermal-throttling or faking lag spikes.", MessageType.Info);
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.Width(450));
            
            EditorGUILayout.Space(15);
            string ip = GetLocalIPAddress();
            EditorGUILayout.LabelField("Connection Target IP (Enter this in your Build, or 127.0.0.1 for Play Mode):", titleStyle);
            EditorGUILayout.LabelField($"{ip} : 8080", valueStyle);
            
            EditorGUILayout.Space(15);
            
            if (!isListening) {
                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if(GUILayout.Button("📡 Start Listening Server", GUILayout.Height(50))) {
                    StartWirelessServer();
                }
            } else {
                GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
                if(GUILayout.Button("🛑 Stop Server", GUILayout.Height(50))) {
                    StopWirelessServer();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(15);
            string status = isListening ? (connectedClient != null ? "🟢 Connected & Streaming..." : "🟡 Awaiting TCP Connection...") : "🔴 Disconnected";
            EditorGUILayout.LabelField($"Status: {status}", EditorStyles.boldLabel);
            
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private string GetLocalIPAddress()
        {
            if (!string.IsNullOrEmpty(cachedIP)) return cachedIP;
            
            try
            {
                foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.OperationalStatus == OperationalStatus.Up && 
                        (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        foreach (var ip in netInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                cachedIP = ip.Address.ToString();
                                return cachedIP;
                            }
                        }
                    }
                }
            }
            catch (System.Exception) { }

            cachedIP = "127.0.0.1";
            return cachedIP;
        }

        private long textureTotalBytes, audioTotalBytes, meshTotalBytes, shaderTotalBytes, otherTotalBytes;
        private bool hasBuildScanData = false;

        private void DrawBuildExplorerTab()
        {
            EditorGUILayout.LabelField("📦 Interactive Build-Size Explorer (The App-Shrinker)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Scans your entire /Assets/ folder and calculates the real disk footprint of every asset category. The treemap below shows proportional sizes.", MessageType.Info);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.2f, 0.6f, 0.9f);
            if (GUILayout.Button("🔍 Scan Project Asset Sizes", GUILayout.Width(250), GUILayout.Height(35)))
            {
                ScanProjectBuildSizes();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (hasBuildScanData)
            {
                long totalBytes = textureTotalBytes + audioTotalBytes + meshTotalBytes + shaderTotalBytes + otherTotalBytes;
                EditorGUILayout.LabelField($"Total Project Asset Footprint: {(totalBytes / 1024f / 1024f):F1} MB", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                Rect treemapRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(300), GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(treemapRect, new Color(0.1f, 0.1f, 0.1f));

                if (totalBytes > 0)
                {
                    float w = treemapRect.width;
                    float h = treemapRect.height;
                    float tx = treemapRect.x;
                    float ty = treemapRect.y;

                    // Sort categories by size for best visual layout
                    var categories = new System.Collections.Generic.List<System.Tuple<string, long, Color>> {
                        System.Tuple.Create("Textures", textureTotalBytes, new Color(0.8f, 0.3f, 0.3f)),
                        System.Tuple.Create("Other", otherTotalBytes, new Color(0.6f, 0.3f, 0.8f)),
                        System.Tuple.Create("Meshes", meshTotalBytes, new Color(0.3f, 0.8f, 0.4f)),
                        System.Tuple.Create("Audio", audioTotalBytes, new Color(0.3f, 0.5f, 0.8f)),
                        System.Tuple.Create("Shaders", shaderTotalBytes, new Color(0.8f, 0.8f, 0.3f))
                    };
                    categories.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                    // Largest block takes the left column (50% width, full height)
                    float leftW = w * 0.5f;
                    string lbl0 = categories[0].Item1 + " (" + (categories[0].Item2 / 1024f / 1024f).ToString("F0") + " MB)";
                    DrawTreemapBlock(new Rect(tx, ty, leftW, h), categories[0].Item3, lbl0);

                    // Remaining 4 blocks fill the right column in a 2x2 grid
                    float rightX = tx + leftW;
                    float rightW = w - leftW;
                    float halfW = rightW * 0.5f;
                    float halfH = h * 0.5f;

                    for (int ci = 1; ci < categories.Count && ci <= 4; ci++)
                    {
                        float bx = rightX + ((ci - 1) % 2) * halfW;
                        float by = ty + ((ci - 1) / 2) * halfH;
                        string lbl = categories[ci].Item1 + " (" + (categories[ci].Item2 / 1024f / 1024f).ToString("F0") + " MB)";
                        DrawTreemapBlock(new Rect(bx, by, halfW, halfH), categories[ci].Item3, lbl);
                    }
                }

                EditorGUILayout.Space(20);
                var bStyle = new GUIStyle(EditorStyles.miniButton) { fixedHeight = 35 };
                if(GUILayout.Button("✨ Auto-Crunch Compress All Textures > 2048px", bStyle))
                {
                    int compressed = 0;
                    string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets"});
                    foreach(var guid in texGuids) {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer != null && importer.maxTextureSize > 2048 && !importer.crunchedCompression) {
                            importer.crunchedCompression = true;
                            importer.compressionQuality = 50;
                            importer.SaveAndReimport();
                            compressed++;
                        }
                    }
                    EditorUtility.DisplayDialog("Crunch Complete", $"Enabled Crunch Compression on {compressed} textures larger than 2048px. Re-scan to see updated sizes.", "OK");
                    ScanProjectBuildSizes();
                }
            }
        }

        private void ScanProjectBuildSizes()
        {
            textureTotalBytes = audioTotalBytes = meshTotalBytes = shaderTotalBytes = otherTotalBytes = 0;
            string[] allGuids = AssetDatabase.FindAssets("", new[] {"Assets"});
            foreach(var guid in allGuids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path)) continue;
                var info = new FileInfo(path);
                if (!info.Exists) continue;
                long size = info.Length;
                string ext = Path.GetExtension(path).ToLowerInvariant();
                if (ext == ".png" || ext == ".jpg" || ext == ".tga" || ext == ".psd" || ext == ".exr" || ext == ".tif" || ext == ".bmp")
                    textureTotalBytes += size;
                else if (ext == ".wav" || ext == ".mp3" || ext == ".ogg" || ext == ".aif" || ext == ".flac")
                    audioTotalBytes += size;
                else if (ext == ".fbx" || ext == ".obj" || ext == ".blend" || ext == ".dae" || ext == ".3ds")
                    meshTotalBytes += size;
                else if (ext == ".shader" || ext == ".cginc" || ext == ".hlsl" || ext == ".compute" || ext == ".shadergraph")
                    shaderTotalBytes += size;
                else
                    otherTotalBytes += size;
            }
            hasBuildScanData = true;
        }

        private void DrawTreemapBlock(Rect r, Color c, string label)
        {
            EditorGUI.DrawRect(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), c);
            var labelStyle = new GUIStyle() {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            // Draw shadow for readability
            GUI.Label(new Rect(r.x + 1, r.y + 1, r.width, r.height), label, new GUIStyle(labelStyle) { normal = { textColor = new Color(0, 0, 0, 0.6f) } });
            GUI.Label(r, label, labelStyle);
        }

        private bool heatmapActive = false;

        private void DrawHeatmapTab()
        {
            EditorGUILayout.LabelField("🎨 Shader Complexity & Overdraw Heatmap Replacement", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("One-Click Shader Swap. Replace your Scene View with built-in visualization modes to see GPU Overdraw, Wireframe, or Shaded Wireframe in real time.", MessageType.Info);
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("🔥 Enable Overdraw Visualization", GUILayout.Width(250), GUILayout.Height(50)))
            {
                SetSceneViewMode("Overdraw");
                heatmapActive = true;
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.2f);
            if (GUILayout.Button("🔧 Wireframe Mode", GUILayout.Width(200), GUILayout.Height(50)))
            {
                SetSceneViewMode("Wireframe");
                heatmapActive = true;
            }
            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f);
            if (GUILayout.Button("🔧 Shaded Wireframe", GUILayout.Width(200), GUILayout.Height(50)))
            {
                SetSceneViewMode("ShadedWireframe");
                heatmapActive = true;
            }

            GUILayout.Space(20);
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
            if (GUILayout.Button("🌳 Restore Original View", GUILayout.Width(250), GUILayout.Height(50)))
            {
                SetSceneViewMode("Shaded");
                heatmapActive = false;
            }
            
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
            if (heatmapActive)
            {
                EditorGUILayout.HelpBox("⚠️ Heatmap is ACTIVE. Your Scene View is currently rendering in a diagnostic visualization mode. Click 'Restore Original View' when finished.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Scene View is rendering normally.", MessageType.Info);
            }
        }

        private void SetSceneViewMode(string mode)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv == null) return;

            DrawCameraMode drawMode = DrawCameraMode.Textured;
            if (mode == "Overdraw") drawMode = DrawCameraMode.Overdraw;
            else if (mode == "Wireframe") drawMode = DrawCameraMode.Wireframe;
            else if (mode == "ShadedWireframe") drawMode = DrawCameraMode.TexturedWire;

            sv.cameraMode = SceneView.GetBuiltinCameraMode(drawMode);
            sv.Repaint();
        }

        private void DrawComparatorTab()
        {
            EditorGUILayout.LabelField("⚖️ A/B Delta Report Comparator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("Select two historical reports to instantly calculate exactly how much performance you gained (or lost) after optimizing.", MessageType.Info);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Report A (Before):", GUILayout.Width(120));
            compareReportA = (ProfilerReport)EditorGUILayout.ObjectField(compareReportA, typeof(ProfilerReport), false);
            GUILayout.Space(50);
            GUILayout.Label("Report B (After):", GUILayout.Width(120));
            compareReportB = (ProfilerReport)EditorGUILayout.ObjectField(compareReportB, typeof(ProfilerReport), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            if (compareReportA == null || compareReportB == null)
            {
                var grayStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.gray } };
                GUILayout.Label("Assign two reports to compare metrics.", grayStyle);
                return;
            }

            DrawDeltaRow("Average FPS", compareReportA.averageFPS, compareReportB.averageFPS, "Frames", true);
            DrawDeltaRow("Main Thread CPU", compareReportA.averageMainThreadMs, compareReportB.averageMainThreadMs, "ms", false);
            DrawDeltaRow("Peak Used RAM", compareReportA.maxMemoryMB, compareReportB.maxMemoryMB, "MB", false);
            DrawDeltaRow("Peak VRAM Bloat", compareReportA.maxTextureMemoryMB, compareReportB.maxTextureMemoryMB, "MB", false);
            DrawDeltaRow("Avg GC Allocations", compareReportA.totalGCAllocationsMB, compareReportB.totalGCAllocationsMB, "MB", false);
            DrawDeltaRow("Avg GPU Batches", compareReportA.averageBatches, compareReportB.averageBatches, "calls", false);
            
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Architectural Summary", EditorStyles.boldLabel);
            
            float fpsDelta = compareReportB.averageFPS - compareReportA.averageFPS;
            if (fpsDelta > 2f) 
                EditorGUILayout.HelpBox($"🏆 Incredible! You boosted the total framerate by {fpsDelta:F1} FPS.", MessageType.Info);
            else if (fpsDelta < -2f)
                EditorGUILayout.HelpBox($"⚠️ Warning. Performance degraded by {Mathf.Abs(fpsDelta):F1} FPS between these builds.", MessageType.Error);
                
            float vramDelta = compareReportB.maxTextureMemoryMB - compareReportA.maxTextureMemoryMB;
            if (vramDelta < -10)
                EditorGUILayout.HelpBox($"📉 VRAM Optimization Complete: Freed {Mathf.Abs(vramDelta)} MB of Texture Memory.", MessageType.Info);
                
            float patchesDelta = compareReportB.averageBatches - compareReportA.averageBatches;
            if (patchesDelta < -50)
                EditorGUILayout.HelpBox($"📉 Draw Call Optimization Complete: Freed {Mathf.Abs(patchesDelta)} GPU pipeline calls per frame.", MessageType.Info);
        }

        private Vector2 deepScanScroll;
        
        [System.Serializable]
        private class DeepScanResult {
            public string typeName;
            public string assetPath;
            public string message;
            public long sizeBytes;
            public Object assetObject;
        }
        
        private System.Collections.Generic.List<DeepScanResult> deepScanResults = new System.Collections.Generic.List<DeepScanResult>();
        private bool hasScannedDeepProject = false;
        private string scanStatusMessage = "";

        private void DrawDeepProjectScanTab()
        {
            EditorGUILayout.LabelField("🛠️ Unused Asset Deep Scanner (Project View)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("The Offline Scanner only checks your active Scene. This Deep Scan looks at your entire /Assets/ folder for astronomically massive Prefabs, Textures, or Audio files that are bloat, even if they aren't placed in the world.", MessageType.Info);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.8f, 0.4f, 0.9f);
            if (GUILayout.Button("🔍 Run Deep Project Diagnostic", GUILayout.Width(300), GUILayout.Height(40)))
            {
                RunActualDeepScan();
            }
            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            if (hasScannedDeepProject) {
                EditorGUILayout.LabelField(scanStatusMessage, EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                
                deepScanScroll = EditorGUILayout.BeginScrollView(deepScanScroll, cardStyle, GUILayout.Height(300));
                
                for(int i = 0; i < deepScanResults.Count; i++) {
                    var r = deepScanResults[i];
                    EditorGUILayout.BeginHorizontal();
                    
                    var style = new GUIStyle(EditorStyles.boldLabel);
                    if (r.typeName == "Orphan Script") style.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                    
                    string mbStr = r.sizeBytes > 0 ? $" ({(r.sizeBytes / 1024f / 1024f):F1}MB)" : "";
                    GUILayout.Label($"[{r.typeName}] '{Path.GetFileName(r.assetPath)}' {r.message}{mbStr}", style);
                    
                    GUILayout.FlexibleSpace();
                    
                    if(GUILayout.Button("Ping in Project", GUILayout.Width(150))) { 
                        EditorGUIUtility.PingObject(r.assetObject);
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void RunActualDeepScan()
        {
            deepScanResults.Clear();
            float startTime = Time.realtimeSinceStartup;
            string[] allAssets = AssetDatabase.FindAssets("t:Object", new[] {"Assets"});
            scanStatusMessage = $"Scanning {allAssets.Length} assets...";
            int bloatCount = 0;

            foreach (var guid in allAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj == null) continue;

                // Check Audio Clips
                if (obj is AudioClip ac) {
                    var info = new FileInfo(path);
                    if (info.Exists && info.Length > 5 * 1024 * 1024) { // > 5MB
                        deepScanResults.Add(new DeepScanResult {
                            typeName = "Massive Audio",
                            assetPath = path,
                            message = $"is incredibly large. Consider compressing to Vorbis.",
                            sizeBytes = info.Length,
                            assetObject = obj
                        });
                        bloatCount++;
                    }
                }
                
                // Check Raw Textures
                if (obj is Texture2D tex) {
                    if (tex.width >= 4096 || tex.height >= 4096) {
                        var info = new FileInfo(path);
                        deepScanResults.Add(new DeepScanResult {
                            typeName = "4K Texture",
                            assetPath = path,
                            message = $"is a 4K+ uncompressed texture eating VRAM.",
                            sizeBytes = info != null && info.Exists ? info.Length : 0,
                            assetObject = obj
                        });
                        bloatCount++;
                    }
                }
            }
            
            deepScanResults = deepScanResults.OrderByDescending(x => x.sizeBytes).ToList();
            hasScannedDeepProject = true;
            float duration = Time.realtimeSinceStartup - startTime;
            scanStatusMessage = $"Diagnostic Complete! Scanned {allAssets.Length} Assets in {duration:F1}s. Found {bloatCount} extreme bloat items.";
        }

        private void DrawEnterpriseTab()
        {
            EditorGUILayout.LabelField("🏢 Enterprise Automations & API Keys", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Connect external tracking APIs to automatically broadcast critical metrics, crashes, or leverage AI tools without leaving the Unity Editor.", MessageType.Info);
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("🤖 OpenAI (Code Optimization LLM)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            string llmKey = EditorPrefs.GetString("APP_OpenAIKey", "");
            string newLlmKey = EditorGUILayout.PasswordField("API Key (sk-...):", llmKey);
            if(llmKey != newLlmKey) EditorPrefs.SetString("APP_OpenAIKey", newLlmKey);
            GUILayout.Label("Used by 'AI Code Doctor' button in the Analyzer to strictly rewrite internal C# scripts to be allocation-free.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(15);

            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("📢 Slack / Jira / GitHub CI Webhooks", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            string slackUrl = EditorPrefs.GetString("APP_SlackWebhook", "");
            string newUrl = EditorGUILayout.TextField("HTTP Post Webhook:", slackUrl);
            if(slackUrl != newUrl) EditorPrefs.SetString("APP_SlackWebhook", newUrl);
            GUILayout.Label("When active, clicking '📢 Slack' next to a Spike frame instantly generates a bug report in your workspace.", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Send Test Output Header", GUILayout.Width(180))) { StudioIntegrations.SendSlackAlert("Test automated packet initiated.", "Low"); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(20);
            GUILayout.Label("💻 Headless CLI Command String (For Jenkins / Actions)", EditorStyles.boldLabel);
            EditorGUILayout.TextField("-quit -batchmode -executeMethod AutoPerformanceProfiler.Editor.ProfilerCLI.RunCIAnalysis");
        }

        private void DrawDeltaRow(string title, float valA, float valB, string unit, bool higherIsBetter)
        {
            EditorGUILayout.BeginHorizontal(cardStyle);
            GUILayout.Label(title, titleStyle, GUILayout.Width(150));
            
            GUILayout.Label($"{valA:F1} {unit}", valueStyle, GUILayout.Width(120));
            GUILayout.Label("➔", titleStyle, GUILayout.Width(30));
            GUILayout.Label($"{valB:F1} {unit}", valueStyle, GUILayout.Width(120));

            float diff = valB - valA;
            float percent = valA == 0 ? 0 : (diff / valA) * 100f;

            Color goodColor = new Color(0.2f, 0.8f, 0.4f);
            Color badColor = new Color(0.9f, 0.3f, 0.3f);
            Color finalColor = Color.gray;

            bool isPositiveGain = false;
            
            if (diff != 0f)
            {
                 if (higherIsBetter)
                 {
                     if (diff > 0) { finalColor = goodColor; isPositiveGain = true; }
                     else finalColor = badColor;
                 }
                 else
                 {
                     if (diff < 0) { finalColor = goodColor; isPositiveGain = true; }
                     else finalColor = badColor;
                 }
            }

            string sign = diff > 0 ? "+" : "";
            var deltaStyle = new GUIStyle(valueStyle);
            deltaStyle.normal.textColor = finalColor;

            GUILayout.Label($"  {sign}{diff:F1} {unit}", deltaStyle, GUILayout.Width(100));
            
            var pctStyle = new GUIStyle(EditorStyles.miniLabel);
            pctStyle.normal.textColor = finalColor;
            
            if (Mathf.Abs(diff) > 0.05f) 
                GUILayout.Label($"({sign}{percent:F1}%) {(isPositiveGain ? "Improvement" : "Degradation")}", pctStyle);
            
            EditorGUILayout.EndHorizontal();
        }

        // ==========================================
        // 💡 ADVISOR TAB
        // ==========================================
        private System.Collections.Generic.List<OptimizationAdvisor.OptimizationTip> advisorTips;
        private bool hasRunAdvisor = false;
        private Vector2 advisorScroll;
        private int advisorFilterCategory = -1; // -1 = All
        private readonly string[] advisorCategoryLabels = { "All", "Project", "Quality", "Physics", "Audio Cfg", "Textures", "Audio Import", "Meshes", "Shaders", "Scene", "Scripts", "Lighting", "UI" };

        private void DrawAdvisorTab()
        {
            EditorGUILayout.LabelField("💡 Intelligent Optimization Advisor", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "The Advisor scans your entire project: Settings, Textures, Audio, Meshes, Physics, Lighting, Scene Hierarchy — and generates prioritized smart tips with one-click auto-fixes. " +
                "This is your checklist to a perfectly optimized game.", MessageType.Info);
            EditorGUILayout.Space(10);

            // ── Header Buttons ──
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.9f);
            if (GUILayout.Button("🔍 Run Full Advisor Scan", GUILayout.Width(250), GUILayout.Height(40)))
            {
                advisorTips = OptimizationAdvisor.RunFullAdvisorScan();
                hasRunAdvisor = true;
            }

            GUILayout.Space(10);

            if (hasRunAdvisor && advisorTips != null && advisorTips.Count > 0)
            {
                int fixableCount = advisorTips.Count(t => t.canAutoFix && !t.isFixed);
                GUI.backgroundColor = fixableCount > 0 ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
                EditorGUI.BeginDisabledGroup(fixableCount == 0);
                if (GUILayout.Button($"⚡ AUTO-FIX ALL ({fixableCount} items)", GUILayout.Width(250), GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("Auto-Fix All",
                        $"This will automatically apply {fixableCount} safe optimizations to your project settings and asset imports.\n\n" +
                        "This includes changing Project Settings, compressing textures, optimizing audio, and fixing mesh imports.\n\n" +
                        "Are you absolutely sure?",
                        "Yes, Optimize Everything", "Cancel"))
                    {
                        OptimizationAdvisor.AutoFixAllSafe(advisorTips);
                        advisorTips = OptimizationAdvisor.RunFullAdvisorScan();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!hasRunAdvisor || advisorTips == null)
            {
                EditorGUILayout.Space(30);
                var emptyStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, normal = { textColor = Color.gray } };
                GUILayout.Label("Click 'Run Full Advisor Scan' to generate optimization tips.", emptyStyle);
                return;
            }

            // ── Stats Summary ──
            EditorGUILayout.Space(10);
            int criticals = advisorTips.Count(t => t.priority == OptimizationAdvisor.TipPriority.Critical && !t.isFixed);
            int highs = advisorTips.Count(t => t.priority == OptimizationAdvisor.TipPriority.High && !t.isFixed);
            int fixed_count = advisorTips.Count(t => t.isFixed);
            int total = advisorTips.Count;

            EditorGUILayout.BeginHorizontal();
            DrawAdvisorStatBox("Total Tips", total.ToString(), new Color(0.3f, 0.6f, 0.9f));
            DrawAdvisorStatBox("Critical", criticals.ToString(), criticals > 0 ? new Color(0.9f, 0.2f, 0.2f) : new Color(0.3f, 0.8f, 0.4f));
            DrawAdvisorStatBox("High", highs.ToString(), highs > 0 ? new Color(0.9f, 0.5f, 0.2f) : new Color(0.3f, 0.8f, 0.4f));
            DrawAdvisorStatBox("Fixed ✅", fixed_count.ToString(), new Color(0.3f, 0.8f, 0.4f));
            EditorGUILayout.EndHorizontal();

            // ── Score Bar ──
            EditorGUILayout.Space(5);
            float advisorScore = total > 0 ? ((float)fixed_count / total) * 100f : 100f;
            Rect scoreR = EditorGUILayout.GetControlRect(GUILayout.Height(22));
            EditorGUI.DrawRect(scoreR, new Color(0.1f, 0.1f, 0.1f));
            Color barColor = advisorScore >= 80 ? new Color(0.3f, 0.8f, 0.4f) : (advisorScore >= 50 ? new Color(0.9f, 0.7f, 0.2f) : new Color(0.9f, 0.3f, 0.3f));
            EditorGUI.DrawRect(new Rect(scoreR.x, scoreR.y, scoreR.width * (advisorScore / 100f), scoreR.height), barColor);
            GUI.Label(scoreR, $"  Completion: {advisorScore:F0}% ({fixed_count}/{total} optimizations applied)", new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });

            // ── Category Filter ──
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", EditorStyles.miniLabel, GUILayout.Width(40));
            int newFilter = GUILayout.Toolbar(advisorFilterCategory + 1, advisorCategoryLabels, EditorStyles.miniButton, GUILayout.Height(20)) - 1;
            if (newFilter != advisorFilterCategory) advisorFilterCategory = newFilter;
            EditorGUILayout.EndHorizontal();

            // ── Tips List ──
            EditorGUILayout.Space(5);
            advisorScroll = EditorGUILayout.BeginScrollView(advisorScroll, GUILayout.ExpandHeight(true));

            var filteredTips = advisorFilterCategory < 0 
                ? advisorTips 
                : advisorTips.Where(t => (int)t.category == advisorFilterCategory).ToList();

            foreach (var tip in filteredTips)
            {
                DrawAdvisorTipCard(tip);
            }

            if (filteredTips.Count == 0)
            {
                var g = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.3f, 0.8f, 0.4f) }, fontSize = 14 };
                GUILayout.Label("✨ No issues in this category! Everything is optimized.", g);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAdvisorStatBox(string label, string value, Color color)
        {
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.ExpandWidth(true));
            GUILayout.Label(label, new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 10 });
            GUILayout.Label(value, new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 22, normal = { textColor = color } });
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvisorTipCard(OptimizationAdvisor.OptimizationTip tip)
        {
            Color borderColor = GetPriorityColor(tip.priority);
            if (tip.isFixed) borderColor = new Color(0.3f, 0.8f, 0.4f, 0.3f);

            EditorGUILayout.BeginVertical(cardStyle);

            // ── Title Row ──
            EditorGUILayout.BeginHorizontal();

            string priorityIcon = GetPriorityIcon(tip.priority);
            var prioStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = borderColor } };
            GUILayout.Label($"{priorityIcon} [{tip.priority}]", prioStyle, GUILayout.Width(100));

            string categoryIcon = GetCategoryIcon(tip.category);
            GUILayout.Label($"{categoryIcon} {tip.category}", EditorStyles.miniLabel, GUILayout.Width(120));

            if (tip.isFixed)
            {
                GUILayout.Label("✅ FIXED", new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = new Color(0.3f, 0.8f, 0.4f) } });
            }
            else
            {
                GUILayout.Label(tip.title, EditorStyles.boldLabel);
            }

            EditorGUILayout.EndHorizontal();

            if (!tip.isFixed)
            {
                // ── Description ──
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField(tip.description, EditorStyles.wordWrappedLabel);

                // ── Impact ──
                EditorGUILayout.Space(3);
                var impactStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.9f, 0.7f, 0.2f) }, fontStyle = FontStyle.Bold };
                EditorGUILayout.LabelField($"Impact: {tip.estimatedImpact}", impactStyle);

                // ── How to Fix ──
                var fixLabelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.5f, 0.8f, 1.0f) } };
                EditorGUILayout.LabelField($"📖 {tip.howToFix}", fixLabelStyle);

                // ── Action Buttons ──
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (tip.canAutoFix && tip.autoFixAction != null)
                {
                    GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                    if (GUILayout.Button("✨ Auto-Fix Now", GUILayout.Width(140), GUILayout.Height(24)))
                    {
                        tip.autoFixAction.Invoke();
                        tip.isFixed = true;
                    }
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
                    GUILayout.Label("  Manual Fix Required  ", new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleCenter });
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField(tip.title, new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic });
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private Color GetPriorityColor(OptimizationAdvisor.TipPriority p)
        {
            switch (p)
            {
                case OptimizationAdvisor.TipPriority.Critical: return new Color(0.9f, 0.2f, 0.2f);
                case OptimizationAdvisor.TipPriority.High: return new Color(0.9f, 0.5f, 0.2f);
                case OptimizationAdvisor.TipPriority.Medium: return new Color(0.9f, 0.7f, 0.2f);
                case OptimizationAdvisor.TipPriority.Low: return new Color(0.5f, 0.7f, 0.9f);
                default: return Color.gray;
            }
        }

        private string GetPriorityIcon(OptimizationAdvisor.TipPriority p)
        {
            switch (p)
            {
                case OptimizationAdvisor.TipPriority.Critical: return "🔴";
                case OptimizationAdvisor.TipPriority.High: return "🟠";
                case OptimizationAdvisor.TipPriority.Medium: return "🟡";
                case OptimizationAdvisor.TipPriority.Low: return "🔵";
                default: return "⚪";
            }
        }

        private string GetCategoryIcon(OptimizationAdvisor.TipCategory c)
        {
            switch (c)
            {
                case OptimizationAdvisor.TipCategory.ProjectSettings: return "⚙️";
                case OptimizationAdvisor.TipCategory.QualitySettings: return "🎮";
                case OptimizationAdvisor.TipCategory.PhysicsSettings: return "💥";
                case OptimizationAdvisor.TipCategory.AudioSettings: return "🔊";
                case OptimizationAdvisor.TipCategory.TextureImport: return "🖼️";
                case OptimizationAdvisor.TipCategory.AudioImport: return "🎵";
                case OptimizationAdvisor.TipCategory.MeshImport: return "📐";
                case OptimizationAdvisor.TipCategory.ShaderOptimization: return "💎";
                case OptimizationAdvisor.TipCategory.SceneHierarchy: return "🗂️";
                case OptimizationAdvisor.TipCategory.ScriptArchitecture: return "📜";
                case OptimizationAdvisor.TipCategory.Lighting: return "💡";
                case OptimizationAdvisor.TipCategory.UICanvas: return "🖥️";
                default: return "❓";
            }
        }
    }
}
