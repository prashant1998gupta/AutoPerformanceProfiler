using System;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AutoPerformanceProfiler.Runtime
{
    [DefaultExecutionOrder(-10000)]
    public class PerformanceTracker : MonoBehaviour
    {
        public static PerformanceTracker Instance { get; private set; }

        [Header("Tracker Settings")]
        public bool recordOnAwake = true;
        public bool saveReportOnStop = true;
        
        [Header("Advanced Features")]
        public bool takeScreenshotsOnSpike = true;
        public bool showInGameHUD = true;
        public bool trackObjectLeaks = true;
        public float objectSnapshotInterval = 10f; 
        
        [Tooltip("If true, runs a deep hierarchy scan on StopRecording to flag unoptimized scripts/components.")]
        public bool runSceneScannerOnStop = true;
        
        [Header("Bottleneck Thresholds")]
        [Tooltip("If Left Null, defaults to these values. If populated, overrides with platform budgets.")]
        public HardwareBudgetProfile activeHardwareBudget;
        
        public float fallbackFpsThreshold = 30f;
        public long fallbackGcThresholdBytes = 100 * 1024;
        public float fallbackCpuTimeSpikeMs = 33f;
        
        [Header("Wireless Profiling")]
        public bool enableWirelessProfiler = false;
        public string editorIP = "192.168.1.100";
        public int editorPort = 8080;

        private System.Net.Sockets.TcpClient tcpClient;
        private System.Net.Sockets.NetworkStream networkStream;
        private StreamWriter streamWriter;
        
        private float CurrentFpsThreshold => activeHardwareBudget != null ? activeHardwareBudget.fpsThreshold : fallbackFpsThreshold;
        private long CurrentGcThreshold => activeHardwareBudget != null ? activeHardwareBudget.gcThresholdBytes : fallbackGcThresholdBytes;
        private float CurrentCpuSpikeMs => activeHardwareBudget != null ? activeHardwareBudget.cpuTimeSpikeMs : fallbackCpuTimeSpikeMs;

        private bool isRecording;
        private float sessionStartTime;
        private int framesCount;

        private ProfilerRecorder mainThreadRecorder;
        private ProfilerRecorder renderThreadRecorder;
        private ProfilerRecorder scriptsRecorder;
        private ProfilerRecorder physicsRecorder;
        private ProfilerRecorder animRecorder;

        private ProfilerRecorder memoryRecorder;
        private ProfilerRecorder gcAllocRecorder;
        private ProfilerRecorder textureMemoryRecorder;
        private ProfilerRecorder meshMemoryRecorder;
        
        private ProfilerRecorder drawCallsRecorder;
        private ProfilerRecorder batchesRecorder;
        private ProfilerRecorder trianglesRecorder;
        private ProfilerRecorder verticesRecorder;

        private List<FrameData> recordedFrames = new List<FrameData>();
        private List<string> recordedWarnings = new List<string>();
        private List<ObjectSnapshot> objectSnapshots = new List<ObjectSnapshot>();
        private List<ObjectOffender> objectOffenders = new List<ObjectOffender>();

        private float minFPS = float.MaxValue;
        private float maxFPS = float.MinValue;
        private float totalFPS = 0f;
        
        private float totalMainThreadMs;
        private float totalScriptsMs;
        private float totalRenderMs;
        private float totalPhysicsMs;
        private long totalTris;
        private long totalBatches;

        private long highestMemory = 0;
        private long highestTextureMemory = 0;
        private long totalGCAlloc = 0;
        
        private float startBattery;
        private float nextObjectSnapshotTime;

        private string screenshotsFolder;
        private float lastScreenshotTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (recordOnAwake)
                StartRecording();
        }

        public void StartRecording()
        {
            if (isRecording) return;

            mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
            renderThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Render Thread");
            scriptsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "BehaviourUpdate");
            physicsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Physics, "Physics processing");
            animRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Animation, "Director.Update");

            memoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");
            gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            textureMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory");
            meshMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Mesh Memory");
            
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");

            recordedFrames.Clear();
            recordedWarnings.Clear();
            objectSnapshots.Clear();
            objectOffenders.Clear();
            
            sessionStartTime = Time.unscaledTime;
            framesCount = 0;
            totalFPS = totalMainThreadMs = totalScriptsMs = totalRenderMs = totalPhysicsMs = 0f;
            totalTris = totalBatches = highestMemory = highestTextureMemory = totalGCAlloc = 0;
            
            minFPS = float.MaxValue;
            maxFPS = float.MinValue;

            startBattery = SystemInfo.batteryLevel;
            nextObjectSnapshotTime = Time.unscaledTime + objectSnapshotInterval;

            screenshotsFolder = Path.Combine(Application.persistentDataPath, "ProfilerScreenshots", DateTime.Now.ToString("yyyyMMdd_HHmm"));
            if (takeScreenshotsOnSpike && !Directory.Exists(screenshotsFolder))
            {
                Directory.CreateDirectory(screenshotsFolder);
            }

            if (enableWirelessProfiler)
            {
                try {
                    tcpClient = new System.Net.Sockets.TcpClient();
                    tcpClient.BeginConnect(editorIP, editorPort, new AsyncCallback(OnWirelessConnected), tcpClient);
                } catch(Exception e) {
                    Debug.LogWarning("[AutoPerformanceProfiler] Wireless Stream Init Failed: " + e.Message);
                }
            }

            isRecording = true;
            Debug.Log("[AutoPerformanceProfiler] Started tracking granular system metrics.");
        }

        public void StopRecording()
        {
            if (!isRecording) return;
            isRecording = false;

            mainThreadRecorder.Dispose();
            renderThreadRecorder.Dispose();
            scriptsRecorder.Dispose();
            physicsRecorder.Dispose();
            animRecorder.Dispose();

            if (streamWriter != null) { streamWriter.Close(); streamWriter = null; }
            if (networkStream != null) { networkStream.Close(); networkStream = null; }
            if (tcpClient != null) { tcpClient.Close(); tcpClient = null; }

            memoryRecorder.Dispose();
            gcAllocRecorder.Dispose();
            textureMemoryRecorder.Dispose();
            meshMemoryRecorder.Dispose();
            
            drawCallsRecorder.Dispose();
            batchesRecorder.Dispose();
            trianglesRecorder.Dispose();
            verticesRecorder.Dispose();

            if (runSceneScannerOnStop)
            {
                AnalyzeSceneForOffenders();
            }

            if (saveReportOnStop)
            {
                SaveReport();
            }
        }

        private void OnWirelessConnected(IAsyncResult ar)
        {
            try {
                System.Net.Sockets.TcpClient client = (System.Net.Sockets.TcpClient)ar.AsyncState;
                client.EndConnect(ar);
                networkStream = client.GetStream();
                streamWriter = new StreamWriter(networkStream, System.Text.Encoding.UTF8);
                Debug.Log("[AutoPerformanceProfiler] Wireless Streaming Connected to " + editorIP);
            } catch (Exception) {
                Debug.LogWarning("[AutoPerformanceProfiler] TCP Stream Refused. Did you hit 'Start Listening Server' in the Editor?");
            }
        }

        private void OnDestroy() => StopRecording();

        private void Update()
        {
            if (!isRecording) return;

            float dt = Time.unscaledDeltaTime;
            float fps = dt > 0 ? 1f / dt : 0f;
            
            float mainMs = mainThreadRecorder.LastValue / 1000000f; 
            float scriptsMs = scriptsRecorder.LastValue / 1000000f;
            float renderMs = renderThreadRecorder.LastValue / 1000000f;
            float physMs = physicsRecorder.LastValue / 1000000f;
            float animMs = animRecorder.LastValue / 1000000f;
            
            long usedMemMB = memoryRecorder.LastValue / (1024 * 1024);
            long texMemMB = textureMemoryRecorder.LastValue / (1024 * 1024);
            long meshMemMB = meshMemoryRecorder.LastValue / (1024 * 1024);
            long gcAllocThisFrame = gcAllocRecorder.LastValue;
            
            long draws = drawCallsRecorder.LastValue;
            long batches = batchesRecorder.LastValue;
            long tris = trianglesRecorder.LastValue;
            long verts = verticesRecorder.LastValue;

            minFPS = Mathf.Min(minFPS, fps);
            maxFPS = Mathf.Max(maxFPS, fps);
            totalFPS += fps;
            totalMainThreadMs += mainMs;
            totalScriptsMs += scriptsMs;
            totalRenderMs += renderMs;
            totalPhysicsMs += physMs;
            
            totalTris += tris;
            totalBatches += batches;

            highestMemory = Math.Max(highestMemory, usedMemMB);
            highestTextureMemory = Math.Max(highestTextureMemory, texMemMB);
            totalGCAlloc += gcAllocThisFrame;
            
            framesCount++;

            bool isSpike = fps < CurrentFpsThreshold || mainMs > CurrentCpuSpikeMs || gcAllocThisFrame > CurrentGcThreshold;
            string screenshotPath = "";

            if (isSpike && takeScreenshotsOnSpike && Time.unscaledTime - lastScreenshotTime > 1.5f) 
            {
                lastScreenshotTime = Time.unscaledTime;
                screenshotPath = Path.Combine(screenshotsFolder, $"Spike_{framesCount}.png");
                ScreenCapture.CaptureScreenshot(screenshotPath);
            }

            var fd = new FrameData
            {
                time = Time.unscaledTime - sessionStartTime,
                fps = fps,
                mainThreadTimeMs = mainMs,
                scriptsTimeMs = scriptsMs,
                renderThreadTimeMs = renderMs,
                physicsTimeMs = physMs,
                animTimeMs = animMs,
                allocatedMemoryMB = usedMemMB,
                textureMemoryMB = texMemMB,
                meshMemoryMB = meshMemMB,
                gcAllocatedInFrameBytes = gcAllocThisFrame,
                drawCalls = draws,
                batches = batches,
                triangles = tris,
                vertices = verts,
                batteryLevel = SystemInfo.batteryLevel,
                isSpikeFrame = isSpike,
                screenshotPath = screenshotPath
            };

            recordedFrames.Add(fd);

            if (enableWirelessProfiler && streamWriter != null)
            {
                try {
                    string json = JsonUtility.ToJson(fd);
                    streamWriter.WriteLine(json);
                    streamWriter.Flush();
                } catch {
                    streamWriter = null; // Connection broke
                }
            }

            if (trackObjectLeaks && Time.unscaledTime >= nextObjectSnapshotTime)
            {
                TakeObjectSnapshot();
                nextObjectSnapshotTime = Time.unscaledTime + objectSnapshotInterval;
            }

            if (framesCount > 60)
            {
                if (fps < CurrentFpsThreshold && fps > 0.1f)
                {
                    string culprit = "Unknown";
                    if (scriptsMs > mainMs * 0.4f) culprit = "Heavy Update Scripts";
                    else if (renderMs > mainMs * 0.4f) culprit = "Slow Rendering (GPU Prep)";
                    else if (physMs > mainMs * 0.3f) culprit = "Complex Physics Collisions";

                    recordedWarnings.Add($"[{Time.time:F1}s] FPS dropped to {fps:F0}. Culprit: {culprit}.");
                }
                
                if (gcAllocThisFrame > CurrentGcThreshold)
                    recordedWarnings.Add($"[{Time.time:F1}s] GC Spike: {gcAllocThisFrame / 1024} KB.");
            }
        }

        private void TakeObjectSnapshot()
        {
            var gos = FindObjectsByType<GameObject>(FindObjectsSortMode.None); 
            var monos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            objectSnapshots.Add(new ObjectSnapshot {
                time = Time.unscaledTime - sessionStartTime,
                totalGameObjects = gos.Length,
                totalMonoBehaviours = monos.Length
            });
        }

        /// <summary>
        /// Highly valuable check that scans the active live scene for GameObjects containing notoriously unoptimized setups.
        /// </summary>
        private void AnalyzeSceneForOffenders()
        {
            objectOffenders.Clear();

            // 1. Check for Meshes with massive triangle counts
            var meshFilters = FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh != null && mf.sharedMesh.vertexCount > 30000)
                {
                    objectOffenders.Add(new ObjectOffender {
                        gameObjectName = mf.gameObject.name,
                        componentName = "MeshFilter",
                        severity = "High",
                        issueDescription = $"Extremely high geometry count: {mf.sharedMesh.vertexCount} vertices on a single object.",
                        recommendedFix = "Decimate the mesh in a 3D modeling tool or implement Unity LODGroup to reduce GPU draw logic."
                    });
                }
            }

            // 2. Check for expensive Realtime Point/Spot Lights with Shadows
            var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if ((light.type == LightType.Point || light.type == LightType.Spot) && light.shadows != LightShadows.None)
                {
                    objectOffenders.Add(new ObjectOffender {
                        gameObjectName = light.gameObject.name,
                        componentName = "Light",
                        severity = "High",
                        issueDescription = "Point/Spot Light is casting real-time dynamic shadows.",
                        recommendedFix = "Set shadows to 'None', or bake the light into the lightmap. Real-time shadows on local lights cause massive render thread spikes."
                    });
                }
            }

            // 3. Check for Animators always updating off-screen
            var animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (var anim in animators)
            {
                if (anim.cullingMode == AnimatorCullingMode.AlwaysAnimate)
                {
                    objectOffenders.Add(new ObjectOffender {
                        gameObjectName = anim.gameObject.name,
                        componentName = "Animator",
                        severity = "Medium",
                        issueDescription = "Animator culling mode is set to 'Always Animate'.",
                        recommendedFix = "Change Culling Mode to 'Cull Update Transforms' or 'Cull Completely' so it pauses when off-screen."
                    });
                }
            }

            // 4. Check for 'God Objects' (Too many MonoBehaviour scripts)
            var allGameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allGameObjects)
            {
                var scripts = go.GetComponents<MonoBehaviour>();
                if (scripts.Length > 10)
                {
                    objectOffenders.Add(new ObjectOffender {
                        gameObjectName = go.name,
                        componentName = "Multiple Scripts (Architecture)",
                        severity = "Medium",
                        issueDescription = $"Object has {scripts.Length} MonoBehaviour scripts attached at once.",
                        recommendedFix = "This creates a 'God Object' and causes significant overhead during Unity's native-to-managed Update callbacks. Consider refactoring and splitting logic."
                    });
                }
            }

            // 5. Check Particle Systems without culling or with massive emission
            var particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
            foreach (var ps in particleSystems)
            {
                if (ps.main.maxParticles > 5000)
                {
                    objectOffenders.Add(new ObjectOffender {
                        gameObjectName = ps.gameObject.name,
                        componentName = "ParticleSystem",
                        severity = "High",
                        issueDescription = $"Max particles allowed is excessively high ({ps.main.maxParticles}).",
                        recommendedFix = "Lower the Max Particles limit. Over-emitting particles destroys CPU render prep time and Fill-Rate."
                    });
                }
            }
        }

        private void OnGUI()
        {
            if (!isRecording || !showInGameHUD) return;
            if (recordedFrames.Count == 0) return;
            var lastFrame = recordedFrames[recordedFrames.Count - 1];

            GUI.color = lastFrame.fps >= 30 ? Color.green : Color.yellow;
            if (lastFrame.fps < 20) GUI.color = Color.red;

            GUI.Box(new Rect(10, 10, 260, 140), "Auto Profiler HUD (Live)");

            GUI.color = Color.white;
            GUI.Label(new Rect(20, 35, 240, 20), $"FPS: {lastFrame.fps:F0} | CPU: {lastFrame.mainThreadTimeMs:F1} ms");
            GUI.Label(new Rect(20, 55, 240, 20), $"RAM: {lastFrame.allocatedMemoryMB} MB | VRAM: {lastFrame.textureMemoryMB} MB");
            GUI.Label(new Rect(20, 75, 240, 20), $"Batches: {lastFrame.batches} | Tris: {lastFrame.triangles / 1000}k");
            GUI.Label(new Rect(20, 95, 240, 20), $"Graph Ops: {lastFrame.drawCalls} Draw Calls");
            
            float battery = SystemInfo.batteryLevel;
            string batStr = battery < 0 ? "Plugged IN/PC" : $"{(battery * 100):F0}%";
            GUI.Label(new Rect(20, 115, 240, 20), $"Battery: {batStr}");
        }

        public bool IsRecording() => isRecording;

#if UNITY_EDITOR
        public void SaveReport()
        {
            ProfilerReport report = ScriptableObject.CreateInstance<ProfilerReport>();
            report.sessionName = "Session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            report.timestamp = DateTime.Now.ToString("g");
            report.sceneName = SceneManager.GetActiveScene().name;
            report.deviceModel = SystemInfo.deviceModel;
            report.osVersion = SystemInfo.operatingSystem;
            
            report.duration = Time.unscaledTime - sessionStartTime;
            report.totalFramesRecorded = framesCount;

            report.averageFPS = framesCount > 0 ? (totalFPS / framesCount) : 0f;
            report.minFPS = minFPS < float.MaxValue ? minFPS : 0;
            report.maxFPS = maxFPS > float.MinValue ? maxFPS : 0;
            
            report.averageMainThreadMs = framesCount > 0 ? (totalMainThreadMs / framesCount) : 0f;
            report.averageScriptsMs = framesCount > 0 ? (totalScriptsMs / framesCount) : 0f;
            report.averageRenderMs = framesCount > 0 ? (totalRenderMs / framesCount) : 0f;
            report.averagePhysicsMs = framesCount > 0 ? (totalPhysicsMs / framesCount) : 0f;
            
            report.averageBatches = framesCount > 0 ? (totalBatches / framesCount) : 0;
            report.averageTriangles = framesCount > 0 ? (totalTris / framesCount) : 0;

            report.maxMemoryMB = highestMemory;
            report.maxTextureMemoryMB = highestTextureMemory;
            report.totalGCAllocationsMB = totalGCAlloc / (1024 * 1024);

            report.startBatteryLevel = startBattery;
            report.endBatteryLevel = SystemInfo.batteryLevel;

            report.frames = new List<FrameData>(recordedFrames);
            report.warnings = new List<string>(recordedWarnings);
            report.objectSnapshots = new List<ObjectSnapshot>(objectSnapshots);
            report.offenders = new List<ObjectOffender>(objectOffenders);

            report.suggestions = GenerateFinalSuggestions(report);

            string folderPath = "Assets/AutoPerformanceProfiler/Reports";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/AutoPerformanceProfiler", "Reports");
            }

            string reportPath = $"{folderPath}/{report.sessionName}.asset";
            AssetDatabase.CreateAsset(report, reportPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AutoPerformanceProfiler] Advanced metrics saved to {reportPath}");
            EditorGUIUtility.PingObject(report);
        }

        private List<string> GenerateFinalSuggestions(ProfilerReport r)
        {
            List<string> suggestions = new List<string>();
            
            float targetFps = CurrentFpsThreshold;
            long targetGc = CurrentGcThreshold;
            long maxBatches = activeHardwareBudget != null ? activeHardwareBudget.batchesWarningLimit : 2000;
            long targetMem = activeHardwareBudget != null ? activeHardwareBudget.maxTextureMemoryMB : 500;

            if (r.minFPS < targetFps) suggestions.Add($"Target Platform: Dropping below {targetFps}FPS is unacceptable for this profile. Consider disabling shadows or post-processing.");
            if (r.averageScriptsMs > 10f) suggestions.Add("Code Architecture: Scripts are using too much CPU overhead on average.");
            if (r.totalGCAllocationsMB > targetGc / 1024 / 1024 + 5) suggestions.Add("Memory Leaks (GC): You are creating a lot of Garbage inside loops. Use object pooling and avoid LINQ in Update().");
            if (r.averageBatches > maxBatches) suggestions.Add($"Rendering: Average batches ({r.averageBatches}) exceeds budget ({maxBatches}). Enable 'Static Batching' and 'GPU Instancing'.");
            if (r.maxTextureMemoryMB > targetMem) suggestions.Add($"VRAM Bottleneck: Texture Memory ({r.maxTextureMemoryMB}MB) exceeded Hardware Budget ({targetMem}MB). Downscale massive textures.");
            
            if (r.objectSnapshots.Count >= 2)
            {
                var first = r.objectSnapshots[0];
                var last = r.objectSnapshots[r.objectSnapshots.Count - 1];
                if (last.totalGameObjects > first.totalGameObjects * 1.5f && last.totalGameObjects - first.totalGameObjects > 80)
                {
                    suggestions.Add($"⚠️ CRITICAL MEMORY LEAK: GameObjects increased from {first.totalGameObjects} to {last.totalGameObjects} over the session.");
                }
            }

            if (r.startBatteryLevel > 0 && r.endBatteryLevel > 0 && r.startBatteryLevel - r.endBatteryLevel > 0.03f)
            {
                suggestions.Add($"🔋 Battery Drain Warning: Lost {(r.startBatteryLevel - r.endBatteryLevel)*100:F1}% battery during this session.");
            }

            if (suggestions.Count == 0) suggestions.Add("Everything runs optimally! Outstanding architecture.");

            return suggestions;
        }
#endif
    }
}
