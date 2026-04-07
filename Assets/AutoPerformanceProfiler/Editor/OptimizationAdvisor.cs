using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// The Optimization Advisor is a premium intelligent engine that scans the entire project,
    /// generates prioritized optimization tips, and provides one-click auto-improvement actions
    /// for Project Settings, Quality Settings, Physics, Audio, Textures, Meshes, Shaders, and more.
    /// </summary>
    public static class OptimizationAdvisor
    {
        public enum TipCategory
        {
            ProjectSettings,
            QualitySettings,
            PhysicsSettings,
            AudioSettings,
            TextureImport,
            AudioImport,
            MeshImport,
            ShaderOptimization,
            SceneHierarchy,
            ScriptArchitecture,
            Lighting,
            UICanvas
        }

        public enum TipPriority
        {
            Critical,   // Must fix, massive performance impact
            High,       // Should fix, significant impact
            Medium,     // Nice to fix, moderate impact
            Low,        // Minor optimization
            Info        // Informational tip
        }

        [System.Serializable]
        public class OptimizationTip
        {
            public string title;
            public string description;
            public string howToFix;
            public TipCategory category;
            public TipPriority priority;
            public string estimatedImpact;
            public bool canAutoFix;
            public System.Action autoFixAction;
            public bool isFixed;
        }

        // ==========================================
        // MASTER SCAN: Collects ALL tips
        // ==========================================
        public static List<OptimizationTip> RunFullAdvisorScan()
        {
            var tips = new List<OptimizationTip>();
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Project Settings...", 0.1f);
            tips.AddRange(ScanProjectSettings());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Quality Settings...", 0.2f);
            tips.AddRange(ScanQualitySettings());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Physics Settings...", 0.3f);
            tips.AddRange(ScanPhysicsSettings());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Audio Settings...", 0.4f);
            tips.AddRange(ScanAudioSettings());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Texture Imports...", 0.5f);
            tips.AddRange(ScanTextureImports());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Audio Imports...", 0.6f);
            tips.AddRange(ScanAudioImports());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Mesh Imports...", 0.7f);
            tips.AddRange(ScanMeshImports());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Scene Hierarchy...", 0.8f);
            tips.AddRange(ScanSceneHierarchy());
            
            EditorUtility.DisplayProgressBar("💡 Optimization Advisor", "Scanning Lighting...", 0.9f);
            tips.AddRange(ScanLightingSettings());
            
            EditorUtility.ClearProgressBar();
            
            // Sort: Critical first, Info last
            tips.Sort((a, b) => a.priority.CompareTo(b.priority));
            
            return tips;
        }

        // ==========================================
        // PROJECT SETTINGS SCAN
        // ==========================================
        private static List<OptimizationTip> ScanProjectSettings()
        {
            var tips = new List<OptimizationTip>();
            var buildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildGroup);

            // 1. Scripting Backend
            if (PlayerSettings.GetScriptingBackend(namedTarget) == ScriptingImplementation.Mono2x)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Switch to IL2CPP Scripting Backend",
                    description = "Your project uses Mono scripting. IL2CPP converts C# to native C++ code, delivering 2x-4x faster execution on all platforms.",
                    howToFix = "Edit → Project Settings → Player → Configuration → Scripting Backend → IL2CPP",
                    category = TipCategory.ProjectSettings,
                    priority = TipPriority.Critical,
                    estimatedImpact = "🔥 2x-4x CPU performance boost. Ship-blocking optimization.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        PlayerSettings.SetScriptingBackend(namedTarget, ScriptingImplementation.IL2CPP);
                        Debug.Log("[Advisor] ✅ Switched Scripting Backend to IL2CPP.");
                    }
                });
            }

            // 2. Managed Stripping Level
            if (PlayerSettings.GetManagedStrippingLevel(namedTarget) == ManagedStrippingLevel.Disabled ||
                PlayerSettings.GetManagedStrippingLevel(namedTarget) == ManagedStrippingLevel.Minimal)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Increase Managed Stripping Level",
                    description = "Managed Stripping removes unused C# code from your build, significantly reducing APK/IPA size and improving IL2CPP compile times.",
                    howToFix = "Edit → Project Settings → Player → Configuration → Managed Stripping Level → Medium or High",
                    category = TipCategory.ProjectSettings,
                    priority = TipPriority.High,
                    estimatedImpact = "📦 10-30% smaller build size. Faster IL2CPP compilation.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        PlayerSettings.SetManagedStrippingLevel(namedTarget, ManagedStrippingLevel.Medium);
                        Debug.Log("[Advisor] ✅ Set Managed Stripping Level to Medium.");
                    }
                });
            }

            // 3. Incremental GC
            if (!PlayerSettings.gcIncremental)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Enable Incremental Garbage Collection",
                    description = "Without incremental GC, garbage collection freezes the main thread entirely (up to 100ms spikes). Incremental GC spreads the work across multiple frames.",
                    howToFix = "Edit → Project Settings → Player → Configuration → Use Incremental GC ✓",
                    category = TipCategory.ProjectSettings,
                    priority = TipPriority.Critical,
                    estimatedImpact = "🔥 Eliminates GC frame spikes. Essential for 60fps targets.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        PlayerSettings.gcIncremental = true;
                        Debug.Log("[Advisor] ✅ Enabled Incremental Garbage Collection.");
                    }
                });
            }

            // 4. Strip Engine Code
            if (!PlayerSettings.stripEngineCode)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Enable Strip Engine Code",
                    description = "Unity includes ALL engine modules by default. Stripping removes unused engine components (like AR, VR, Cloth) from the build.",
                    howToFix = "Edit → Project Settings → Player → Strip Engine Code ✓",
                    category = TipCategory.ProjectSettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "📦 5-15% smaller build. Removes dead engine code.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        PlayerSettings.stripEngineCode = true;
                        Debug.Log("[Advisor] ✅ Enabled Strip Engine Code.");
                    }
                });
            }

            // 5. GPU Skinning
            bool isMobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
            if (!PlayerSettings.gpuSkinning && !isMobile)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Enable GPU Skinning (Compute Deformation)",
                    description = "Skeletal mesh deformation is done on CPU by default. GPU Skinning offloads this to the GPU where it's massively parallel and nearly free.",
                    howToFix = "Edit → Project Settings → Player → GPU Skinning ✓",
                    category = TipCategory.ProjectSettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "⚡ 30-50% reduction in skinned mesh CPU cost.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        PlayerSettings.gpuSkinning = true;
                        Debug.Log("[Advisor] ✅ Enabled GPU Skinning.");
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // QUALITY SETTINGS SCAN
        // ==========================================
        private static List<OptimizationTip> ScanQualitySettings()
        {
            var tips = new List<OptimizationTip>();

            // 1. Shadow Distance
            if (UnityEngine.QualitySettings.shadowDistance > 150f)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Reduce Shadow Draw Distance",
                    description = $"Shadow Distance is {UnityEngine.QualitySettings.shadowDistance}m. Shadows beyond 80-100m are invisible to most players but cost enormous GPU fillrate.",
                    howToFix = "Edit → Project Settings → Quality → Shadows → Shadow Distance → 80-100",
                    category = TipCategory.QualitySettings,
                    priority = TipPriority.High,
                    estimatedImpact = "🎮 20-40% GPU savings on shadow rendering.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        UnityEngine.QualitySettings.shadowDistance = 100f;
                        Debug.Log("[Advisor] ✅ Set Shadow Distance to 100m.");
                    }
                });
            }

            // 2. Shadow Cascades
            if (UnityEngine.QualitySettings.shadowCascades > 2)
            {
                bool mobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
                if (mobile)
                {
                    tips.Add(new OptimizationTip
                    {
                        title = "Reduce Shadow Cascades for Mobile",
                        description = $"Currently using {UnityEngine.QualitySettings.shadowCascades} shadow cascades. On mobile, 1-2 cascades is optimal. Each cascade re-renders the entire scene for shadows.",
                        howToFix = "Edit → Project Settings → Quality → Shadows → Shadow Cascades → 1 or 2",
                        category = TipCategory.QualitySettings,
                        priority = TipPriority.High,
                        estimatedImpact = "🎮 50%+ GPU savings per cascade removed on mobile.",
                        canAutoFix = true,
                        autoFixAction = () => {
                            UnityEngine.QualitySettings.shadowCascades = 1;
                            Debug.Log("[Advisor] ✅ Set Shadow Cascades to 1 for mobile target.");
                        }
                    });
                }
            }

            // 3. Pixel Light Count
            if (UnityEngine.QualitySettings.pixelLightCount > 4)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Reduce Pixel Light Count",
                    description = $"Pixel Light Count is {UnityEngine.QualitySettings.pixelLightCount}. Each pixel light forces a full additional rendering pass on every affected object. 2-4 is ideal.",
                    howToFix = "Edit → Project Settings → Quality → Pixel Light Count → 2-4",
                    category = TipCategory.QualitySettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "🎮 Significant Draw Call reduction per extra light removed.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        UnityEngine.QualitySettings.pixelLightCount = 4;
                        Debug.Log("[Advisor] ✅ Capped Pixel Light Count to 4.");
                    }
                });
            }

            // 4. Texture Quality
            if (UnityEngine.QualitySettings.globalTextureMipmapLimit == 0)
            {
                bool mobile = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;
                if (mobile)
                {
                    tips.Add(new OptimizationTip
                    {
                        title = "Use Half-Resolution Textures on Mobile",
                        description = "Global Texture Quality is set to Full Resolution on a mobile target. Most mobile screens are small enough that Half Resolution is indistinguishable, saving 75% VRAM.",
                        howToFix = "Edit → Project Settings → Quality → Texture Quality → Half Res",
                        category = TipCategory.QualitySettings,
                        priority = TipPriority.Medium,
                        estimatedImpact = "📱 75% VRAM savings on textures. Prevents thermal throttling.",
                        canAutoFix = true,
                        autoFixAction = () => {
                            UnityEngine.QualitySettings.globalTextureMipmapLimit = 1;
                            Debug.Log("[Advisor] ✅ Set Global Texture Quality to Half Resolution for mobile.");
                        }
                    });
                }
            }

            // 5. Anisotropic Textures
            if (UnityEngine.QualitySettings.anisotropicFiltering == AnisotropicFiltering.ForceEnable)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Use Per-Texture Anisotropic Filtering",
                    description = "Anisotropic Filtering is force-enabled globally. This applies expensive filtering to ALL textures including UI and particles where it has zero visual benefit.",
                    howToFix = "Edit → Project Settings → Quality → Anisotropic Textures → Per Texture",
                    category = TipCategory.QualitySettings,
                    priority = TipPriority.Low,
                    estimatedImpact = "⚡ Minor GPU savings on fillrate-bound scenes.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        UnityEngine.QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                        Debug.Log("[Advisor] ✅ Set Anisotropic Filtering to Per-Texture.");
                    }
                });
            }

            // 6. LOD Bias
            if (UnityEngine.QualitySettings.lodBias > 2f)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Reduce LOD Bias",
                    description = $"LOD Bias is {UnityEngine.QualitySettings.lodBias:F1}. Values above 2 force Unity to render higher-detail LODs at much greater distances, wasting triangles.",
                    howToFix = "Edit → Project Settings → Quality → LOD Bias → 1.0-2.0",
                    category = TipCategory.QualitySettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "🎮 Renders fewer triangles at distance, boosting FPS.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        UnityEngine.QualitySettings.lodBias = 1.5f;
                        Debug.Log("[Advisor] ✅ Set LOD Bias to 1.5.");
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // PHYSICS SETTINGS SCAN
        // ==========================================
        private static List<OptimizationTip> ScanPhysicsSettings()
        {
            var tips = new List<OptimizationTip>();

            // 1. Fixed Timestep
            if (Time.fixedDeltaTime < 0.02f)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Increase Physics Fixed Timestep",
                    description = $"Fixed Timestep is {Time.fixedDeltaTime}s ({1f/Time.fixedDeltaTime:F0} Hz). Below 0.02s (50Hz), you're wasting CPU cycles on physics precision that players can't perceive.",
                    howToFix = "Edit → Project Settings → Time → Fixed Timestep → 0.02 (50Hz) or 0.033 (30Hz for mobile)",
                    category = TipCategory.PhysicsSettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "⚡ Fewer physics steps per frame = direct CPU savings.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        Time.fixedDeltaTime = 0.02f;
                        Debug.Log("[Advisor] ✅ Set Fixed Timestep to 0.02s (50Hz).");
                    }
                });
            }

            // 2. Auto Sync Transforms
            if (Physics.autoSyncTransforms)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Disable Physics Auto Sync Transforms",
                    description = "Auto Sync Transforms forces the physics engine to synchronize with Transform changes every single time you move an object. This is extremely expensive with many moving objects.",
                    howToFix = "Edit → Project Settings → Physics → Auto Sync Transforms → OFF",
                    category = TipCategory.PhysicsSettings,
                    priority = TipPriority.High,
                    estimatedImpact = "🔥 Major CPU reduction for scenes with many moving Rigidbodies.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        Physics.autoSyncTransforms = false;
                        Debug.Log("[Advisor] ✅ Disabled Physics Auto Sync Transforms.");
                    }
                });
            }

            // 3. Reuse Collision Callbacks
            if (!Physics.reuseCollisionCallbacks)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Enable Reuse Collision Callbacks",
                    description = "Without this, Unity creates new Collision objects every single physics callback (OnCollisionEnter etc.), generating significant garbage collection pressure.",
                    howToFix = "Edit → Project Settings → Physics → Reuse Collision Callbacks ✓",
                    category = TipCategory.PhysicsSettings,
                    priority = TipPriority.Medium,
                    estimatedImpact = "⚡ Reduces GC allocations in physics-heavy games.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        Physics.reuseCollisionCallbacks = true;
                        Debug.Log("[Advisor] ✅ Enabled Reuse Collision Callbacks.");
                    }
                });
            }

            // 4. Default Solver Iterations
            if (Physics.defaultSolverIterations > 8)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Reduce Physics Solver Iterations",
                    description = $"Solver Iterations is {Physics.defaultSolverIterations}. The default of 6 is sufficient for most games. Higher values add CPU cost per Rigidbody contact.",
                    howToFix = "Edit → Project Settings → Physics → Default Solver Iterations → 6",
                    category = TipCategory.PhysicsSettings,
                    priority = TipPriority.Low,
                    estimatedImpact = "⚡ Minor CPU savings per physics step.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        Physics.defaultSolverIterations = 6;
                        Debug.Log("[Advisor] ✅ Set Physics Solver Iterations to 6.");
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // AUDIO SETTINGS SCAN
        // ==========================================
        private static List<OptimizationTip> ScanAudioSettings()
        {
            var tips = new List<OptimizationTip>();

            var audioConfig = AudioSettings.GetConfiguration();

            // 1. DSP Buffer Size
            if (audioConfig.dspBufferSize > 0 && audioConfig.dspBufferSize < 512)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Increase Audio DSP Buffer Size",
                    description = "A small DSP buffer forces the audio engine to process sound in tiny chunks more frequently, increasing CPU overhead. Unless you need ultra-low latency (rhythm games), a larger buffer is fine.",
                    howToFix = "Edit → Project Settings → Audio → DSP Buffer Size → Best Performance",
                    category = TipCategory.AudioSettings,
                    priority = TipPriority.Low,
                    estimatedImpact = "⚡ Reduces audio thread CPU overhead.",
                    canAutoFix = false
                });
            }

            // 2. Max Real Voices
            if (audioConfig.numRealVoices > 32)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Reduce Max Real Audio Voices",
                    description = $"Max Real Voices is {audioConfig.numRealVoices}. Each real voice requires CPU mixing. Most games need 16-32 concurrent sounds at most.",
                    howToFix = "Edit → Project Settings → Audio → Max Real Voices → 32",
                    category = TipCategory.AudioSettings,
                    priority = TipPriority.Low,
                    estimatedImpact = "⚡ Reduces audio CPU mixing cost.",
                    canAutoFix = false
                });
            }

            return tips;
        }

        // ==========================================
        // TEXTURE IMPORT SCAN (Batch)
        // ==========================================
        private static List<OptimizationTip> ScanTextureImports()
        {
            var tips = new List<OptimizationTip>();
            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });

            int uncompressedCount = 0;
            int oversizedCount = 0;
            int readWriteCount = 0;
            int noMipmapCount = 0;
            long totalWastedBytes = 0;

            foreach (var guid in texGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    uncompressedCount++;
                    var fi = new FileInfo(path);
                    if (fi.Exists) totalWastedBytes += fi.Length;
                }

                if (importer.maxTextureSize >= 4096)
                    oversizedCount++;

                if (importer.isReadable)
                    readWriteCount++;

                if (!importer.mipmapEnabled && importer.textureType != TextureImporterType.Sprite && importer.textureType != TextureImporterType.GUI)
                    noMipmapCount++;
            }

            if (uncompressedCount > 0)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Compress {uncompressedCount} Uncompressed Textures",
                    description = $"{uncompressedCount} textures are using 'Uncompressed' format, wasting ~{(totalWastedBytes / 1024f / 1024f):F0}MB. Compressed textures use 4-8x less VRAM and load faster.",
                    howToFix = "Select affected textures → Import Settings → Compression → Normal Quality or High Quality",
                    category = TipCategory.TextureImport,
                    priority = TipPriority.Critical,
                    estimatedImpact = "🔥 4-8x VRAM reduction per texture. Massive build size savings.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixUncompressedTextures();
                    }
                });
            }

            if (oversizedCount > 0)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Downscale {oversizedCount} Oversized (4K+) Textures",
                    description = $"{oversizedCount} textures are set to 4K resolution or higher. Most game objects only need 1024-2048. 4K textures eat VRAM exponentially.",
                    howToFix = "Select affected textures → Import Settings → Max Size → 2048",
                    category = TipCategory.TextureImport,
                    priority = TipPriority.High,
                    estimatedImpact = "📱 75% VRAM savings per downscaled texture.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixOversizedTextures();
                    }
                });
            }

            if (readWriteCount > 5)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Disable Read/Write on {readWriteCount} Textures",
                    description = $"{readWriteCount} textures have Read/Write enabled. This doubles their memory usage by keeping a CPU copy alongside the GPU copy.",
                    howToFix = "Select affected textures → Import Settings → Advanced → Read/Write Enabled ✗",
                    category = TipCategory.TextureImport,
                    priority = TipPriority.Medium,
                    estimatedImpact = "📱 50% RAM reduction on affected textures.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixReadWriteTextures();
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // AUDIO IMPORT SCAN (Batch)
        // ==========================================
        private static List<OptimizationTip> ScanAudioImports()
        {
            var tips = new List<OptimizationTip>();
            string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });

            int decompressOnLoadLong = 0;
            int notLoadInBackground = 0;
            int stereo3D = 0;

            foreach (var guid in audioGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                if (importer.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad && clip.length > 10f)
                    decompressOnLoadLong++;

                if (!importer.loadInBackground && clip.length > 15f)
                    notLoadInBackground++;

                if (!importer.forceToMono)
                {
                    var sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
                    foreach (var s in sources)
                    {
                        if (s.clip == clip && s.spatialBlend > 0f)
                        {
                            stereo3D++;
                            break;
                        }
                    }
                }
            }

            if (decompressOnLoadLong > 0)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Fix {decompressOnLoadLong} Long Audio Clips Using 'Decompress On Load'",
                    description = "Long audio files (>10s) set to 'Decompress On Load' are fully decompressed into RAM at load time, causing massive memory spikes and long load screens.",
                    howToFix = "Select affected clips → Import Settings → Load Type → Compressed In Memory or Streaming",
                    category = TipCategory.AudioImport,
                    priority = TipPriority.High,
                    estimatedImpact = "🔥 10-100x RAM reduction per long audio clip.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixAudioLoadTypes();
                    }
                });
            }

            if (notLoadInBackground > 0)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Enable 'Load in Background' for {notLoadInBackground} Long Clips",
                    description = "Large audio clips without 'Load in Background' freeze the main thread during loading, causing visible frame rate drops to 0 FPS.",
                    howToFix = "Select affected clips → Import Settings → Load in Background ✓",
                    category = TipCategory.AudioImport,
                    priority = TipPriority.Medium,
                    estimatedImpact = "⚡ Eliminates audio loading stutters.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixAudioBackground();
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // MESH IMPORT SCAN (Batch)
        // ==========================================
        private static List<OptimizationTip> ScanMeshImports()
        {
            var tips = new List<OptimizationTip>();
            string[] meshGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });

            int readWriteCount = 0;
            int noOptimizeMesh = 0;

            foreach (var guid in meshGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                if (importer.isReadable)
                    readWriteCount++;

                if (!importer.optimizeMeshVertices || !importer.optimizeMeshPolygons)
                    noOptimizeMesh++;
            }

            if (readWriteCount > 3)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Disable Read/Write on {readWriteCount} Mesh Imports",
                    description = $"{readWriteCount} models have Read/Write enabled. This doubles their vertex buffer memory by keeping a CPU-side copy that is rarely needed at runtime.",
                    howToFix = "Select affected models → Import Settings → Model → Read/Write Enabled ✗",
                    category = TipCategory.MeshImport,
                    priority = TipPriority.Medium,
                    estimatedImpact = "📱 50% RAM reduction on mesh vertex data.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixMeshReadWrite();
                    }
                });
            }

            if (noOptimizeMesh > 0)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Enable Mesh Optimization on {noOptimizeMesh} Models",
                    description = $"{noOptimizeMesh} models don't have vertex/polygon optimization. Unity can reorder vertices for GPU cache performance, significantly improving rendering speed.",
                    howToFix = "Select affected models → Import Settings → Model → Optimize Mesh ✓",
                    category = TipCategory.MeshImport,
                    priority = TipPriority.Low,
                    estimatedImpact = "⚡ Better GPU vertex cache utilization = faster rendering.",
                    canAutoFix = true,
                    autoFixAction = () => {
                        AutoFixMeshOptimization();
                    }
                });
            }

            return tips;
        }

        // ==========================================
        // SCENE HIERARCHY SCAN
        // ==========================================
        private static List<OptimizationTip> ScanSceneHierarchy()
        {
            var tips = new List<OptimizationTip>();

            // 1. Deep Hierarchy
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int deepObjects = 0;
            foreach (var t in allTransforms)
            {
                int depth = 0;
                Transform current = t;
                while (current.parent != null) { depth++; current = current.parent; }
                if (depth > 10) deepObjects++;
            }

            if (deepObjects > 5)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"{deepObjects} Objects Have Deep Hierarchy (>10 levels)",
                    description = "Deeply nested GameObjects force Unity to traverse the entire parent chain when calculating world transforms. This creates a hidden CPU tax on every moved object.",
                    howToFix = "Flatten your hierarchy. Move child objects to root level where possible. Use empty GameObjects as shallow organizers only.",
                    category = TipCategory.SceneHierarchy,
                    priority = TipPriority.Medium,
                    estimatedImpact = "⚡ Faster Transform.position calculations across the scene.",
                    canAutoFix = false
                });
            }

            // 2. Disabled GameObjects still in scene
            int disabledCount = 0;
            foreach (var t in allTransforms)
            {
                if (!t.gameObject.activeSelf && t.parent == null)
                    disabledCount++;
            }

            if (disabledCount > 10)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"{disabledCount} Root GameObjects Are Disabled",
                    description = "Disabled root objects still occupy memory and increase scene load times. If they're debug objects or old prefabs, remove them.",
                    howToFix = "Delete unused disabled objects or move them to a separate 'archive' scene.",
                    category = TipCategory.SceneHierarchy,
                    priority = TipPriority.Low,
                    estimatedImpact = "📦 Cleaner scene, faster load times.",
                    canAutoFix = false
                });
            }

            // 3. Total Object Count Warning
            if (allTransforms.Length > 5000)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"Scene Has {allTransforms.Length} GameObjects (Very Dense)",
                    description = "Scenes with >5000 active GameObjects can bottleneck the main thread. Consider using multi-scene loading, LOD culling, or object pooling to reduce active count.",
                    howToFix = "Use Additive Scene Loading to split your world. Implement distance-based object disabling.",
                    category = TipCategory.SceneHierarchy,
                    priority = TipPriority.High,
                    estimatedImpact = "🎮 Significant overall CPU and memory improvement.",
                    canAutoFix = false
                });
            }

            return tips;
        }

        // ==========================================
        // LIGHTING SETTINGS SCAN
        // ==========================================
        private static List<OptimizationTip> ScanLightingSettings()
        {
            var tips = new List<OptimizationTip>();

            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            int realtimeShadowLights = 0;
            int pointLights = 0;

            foreach (var l in lights)
            {
                if (l.shadows != LightShadows.None && l.lightmapBakeType == LightmapBakeType.Realtime)
                    realtimeShadowLights++;
                if (l.type == LightType.Point)
                    pointLights++;
            }

            if (realtimeShadowLights > 3)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"{realtimeShadowLights} Realtime Shadow-Casting Lights Active",
                    description = "Every realtime shadow light re-renders the entire scene geometry to create a shadow map. Having more than 2-3 simultaneously is extremely GPU-heavy.",
                    howToFix = "Switch most lights to 'Baked' mode. Only 1-2 key lights (sun, player torch) should cast realtime shadows.",
                    category = TipCategory.Lighting,
                    priority = TipPriority.Critical,
                    estimatedImpact = "🔥 Each removed shadow caster saves a full scene re-render.",
                    canAutoFix = false
                });
            }

            if (pointLights > 8)
            {
                tips.Add(new OptimizationTip
                {
                    title = $"{pointLights} Point Lights in Scene (Consider Baking)",
                    description = "Point lights are the most expensive light type because they illuminate in all directions. Consider baking static ones or using Light Probes.",
                    howToFix = "Mark static point lights as 'Baked'. Use Light Probes for dynamic objects.",
                    category = TipCategory.Lighting,
                    priority = TipPriority.Medium,
                    estimatedImpact = "🎮 Fewer additional passes in forward rendering.",
                    canAutoFix = false
                });
            }

            // Lightmap settings
            if (!Lightmapping.bakedGI && lights.Length > 3)
            {
                tips.Add(new OptimizationTip
                {
                    title = "Enable Baked Global Illumination",
                    description = "Your scene has multiple lights but Baked GI is disabled. Baking precomputes expensive lighting calculations, giving you free high-quality lighting at runtime.",
                    howToFix = "Window → Rendering → Lighting Settings → Mixed/Baked Global Illumination ✓. Then bake your lightmaps.",
                    category = TipCategory.Lighting,
                    priority = TipPriority.High,
                    estimatedImpact = "🔥 Replaces expensive realtime lighting with free baked data.",
                    canAutoFix = false
                });
            }

            return tips;
        }

        // ==================================================================
        // AUTO-FIX IMPLEMENTATIONS (The actual batch optimization actions)
        // ==================================================================

        public static void AutoFixUncompressedTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null && imp.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    imp.textureCompression = TextureImporterCompression.Compressed;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Compressed {fixed_count} uncompressed textures.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Compressed {fixed_count} textures. Re-scan to verify.", "OK");
        }

        public static void AutoFixOversizedTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null && imp.maxTextureSize >= 4096)
                {
                    imp.maxTextureSize = 2048;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Downscaled {fixed_count} oversized textures to 2048.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Downscaled {fixed_count} textures from 4K+ to 2048. Re-scan to verify.", "OK");
        }

        public static void AutoFixReadWriteTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null && imp.isReadable)
                {
                    imp.isReadable = false;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Disabled Read/Write on {fixed_count} textures.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Disabled Read/Write on {fixed_count} textures, halving their RAM usage. Re-scan to verify.", "OK");
        }

        public static void AutoFixAudioLoadTypes()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter imp = AssetImporter.GetAtPath(path) as AudioImporter;
                if (imp == null) continue;
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null && clip.length > 10f && imp.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad)
                {
                    var settings = imp.defaultSampleSettings;
                    settings.loadType = AudioClipLoadType.CompressedInMemory;
                    imp.defaultSampleSettings = settings;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Fixed load type on {fixed_count} long audio clips.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Switched {fixed_count} long audio clips to 'Compressed In Memory'. Re-scan to verify.", "OK");
        }

        public static void AutoFixAudioBackground()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter imp = AssetImporter.GetAtPath(path) as AudioImporter;
                if (imp == null) continue;
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null && clip.length > 15f && !imp.loadInBackground)
                {
                    imp.loadInBackground = true;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Enabled 'Load in Background' on {fixed_count} audio clips.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Enabled background loading on {fixed_count} long audio clips. Re-scan to verify.", "OK");
        }

        public static void AutoFixMeshReadWrite()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp != null && imp.isReadable)
                {
                    imp.isReadable = false;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Disabled Read/Write on {fixed_count} mesh imports.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Disabled Read/Write on {fixed_count} meshes, halving vertex buffer RAM. Re-scan to verify.", "OK");
        }

        public static void AutoFixMeshOptimization()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            int fixed_count = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter imp = AssetImporter.GetAtPath(path) as ModelImporter;
                if (imp != null && (!imp.optimizeMeshVertices || !imp.optimizeMeshPolygons))
                {
                    imp.optimizeMeshVertices = true;
                    imp.optimizeMeshPolygons = true;
                    imp.SaveAndReimport();
                    fixed_count++;
                }
            }
            Debug.Log($"[Advisor] ✅ Enabled mesh optimization on {fixed_count} models.");
            EditorUtility.DisplayDialog("Auto-Fix Complete", $"Enabled vertex/polygon optimization on {fixed_count} models. Re-scan to verify.", "OK");
        }

        // ==========================================
        // AUTO-FIX ALL: The Nuclear Option
        // ==========================================
        public static void AutoFixAllSafe(List<OptimizationTip> tips)
        {
            int fixedCount = 0;
            foreach (var tip in tips)
            {
                if (tip.canAutoFix && !tip.isFixed && tip.autoFixAction != null)
                {
                    try
                    {
                        tip.autoFixAction.Invoke();
                        tip.isFixed = true;
                        fixedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[Advisor] Could not auto-fix '{tip.title}': {e.Message}");
                    }
                }
            }
            Debug.Log($"[Advisor] ✅ Batch Auto-Fixed {fixedCount} optimization issues.");
        }
    }
}
