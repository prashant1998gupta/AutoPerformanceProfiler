using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// Right-click context menu optimization tips for any selected GameObject.
    /// Provides instant per-object optimization analysis without opening the main profiler.
    /// </summary>
    public static class QuickOptimizeMenu
    {
        [MenuItem("GameObject/Auto Profiler/🔍 Quick Performance Check", false, 20)]
        public static void QuickPerformanceCheck()
        {
            if (Selection.activeGameObject == null) return;
            
            GameObject go = Selection.activeGameObject;
            List<string> issues = new List<string>();
            List<string> passed = new List<string>();

            // ── MeshFilter / MeshRenderer Checks ──
            var mf = go.GetComponent<MeshFilter>();
            var mr = go.GetComponent<MeshRenderer>();

            if (mr != null)
            {
                if (!go.isStatic && go.GetComponent<Rigidbody>() == null && go.GetComponent<Animator>() == null)
                    issues.Add("⚠️ Not marked Static — missing free batching optimization.");
                else
                    passed.Add("✅ Static batching configured correctly.");

                if (mr.sharedMaterials.Length > 2)
                    issues.Add($"⚠️ Uses {mr.sharedMaterials.Length} materials — each one = 1 extra Draw Call.");
                else
                    passed.Add($"✅ Material count OK ({mr.sharedMaterials.Length}).");

                if (mf != null && mf.sharedMesh != null)
                {
                    if (mf.sharedMesh.vertexCount > 10000 && !go.GetComponentInParent<LODGroup>())
                        issues.Add($"⚠️ High-poly mesh ({mf.sharedMesh.vertexCount} verts) with no LODGroup.");
                    else if (mf.sharedMesh.vertexCount > 10000)
                        passed.Add($"✅ High-poly mesh has LODGroup.");
                    
                    if (mf.sharedMesh.isReadable)
                        issues.Add("⚠️ Mesh has Read/Write enabled — doubles RAM usage.");
                    else
                        passed.Add("✅ Mesh Read/Write is disabled.");
                }
            }

            // ── Light Checks ──
            var light = go.GetComponent<Light>();
            if (light != null)
            {
                if (light.shadows != LightShadows.None && light.lightmapBakeType == LightmapBakeType.Realtime)
                    issues.Add("🔴 Realtime shadow-casting light — re-renders entire scene per frame.");
                else
                    passed.Add("✅ Light shadow configuration OK.");
                
                if (light.type == LightType.Point && light.range > 50)
                    issues.Add($"⚠️ Point light with large range ({light.range}) — affects many objects.");
            }

            // ── Particle System Checks ──
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (ps.main.maxParticles > 3000)
                    issues.Add($"⚠️ Particle max count is {ps.main.maxParticles} — extreme GPU overdraw.");
                else
                    passed.Add($"✅ Particle count OK ({ps.main.maxParticles}).");
            }

            // ── Animator Checks ──
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                if (animator.cullingMode == AnimatorCullingMode.AlwaysAnimate)
                    issues.Add("⚠️ Animator set to 'Always Animate' — wastes CPU even when off-screen.");
                else
                    passed.Add("✅ Animator culling mode configured.");
            }

            // ── UI Checks ──
            var graphic = go.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic != null)
            {
                if (graphic.raycastTarget && go.GetComponent<UnityEngine.UI.Selectable>() == null)
                    issues.Add("⚠️ Raycast Target enabled on non-interactive UI — CPU overhead every frame.");
                
                if (graphic is UnityEngine.UI.Text)
                    issues.Add("⚠️ Using legacy UI.Text — switch to TextMeshPro for better performance.");
            }

            // ── Rigidbody Checks ──
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (rb.interpolation != RigidbodyInterpolation.None)
                    issues.Add("⚠️ Rigidbody Interpolation enabled — expensive CPU sync unless this is the player.");
                
                var mc = go.GetComponent<MeshCollider>();
                if (mc != null && !mc.convex && !rb.isKinematic)
                    issues.Add("🔴 Dynamic non-convex MeshCollider — one of the most expensive physics setups.");
            }

            // ── AudioSource Checks ──
            var audioSource = go.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                if (audioSource.clip.length > 10f)
                {
                    string path = AssetDatabase.GetAssetPath(audioSource.clip);
                    var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                    if (importer != null && importer.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad)
                        issues.Add($"⚠️ Long audio ({audioSource.clip.length:F0}s) using 'Decompress On Load' — RAM spike.");
                }
            }

            // ── Reflection Probe ──
            var rp = go.GetComponent<ReflectionProbe>();
            if (rp != null && rp.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame)
                issues.Add("🔴 Reflection Probe updating every frame — renders scene 6x per frame.");

            // ── Hierarchy Depth ──
            int depth = 0;
            Transform t = go.transform;
            while (t.parent != null) { depth++; t = t.parent; }
            if (depth > 10)
                issues.Add($"⚠️ Very deep hierarchy ({depth} levels) — slow Transform calculations.");

            // ── Component Count ──
            var allComponents = go.GetComponents<Component>();
            if (allComponents.Length > 15)
                issues.Add($"⚠️ 'God Object' with {allComponents.Length} components — consider splitting.");

            // ── Build Result ──
            string result = $"━━━ QUICK PERFORMANCE CHECK ━━━\n" +
                            $"Object: {go.name}\n" +
                            $"Components: {allComponents.Length}\n" +
                            $"Hierarchy Depth: {depth}\n\n";

            if (issues.Count == 0)
            {
                result += "🏆 PERFECT! No performance issues detected.\n\n";
            }
            else
            {
                result += $"Found {issues.Count} issue(s):\n\n";
                foreach (var issue in issues)
                    result += issue + "\n";
                result += "\n";
            }

            if (passed.Count > 0)
            {
                result += $"Passed {passed.Count} check(s):\n";
                foreach (var p in passed)
                    result += p + "\n";
            }

            EditorUtility.DisplayDialog($"⚡ Performance: {go.name}", result, "OK");
        }

        [MenuItem("GameObject/Auto Profiler/✨ Auto-Optimize Selected", false, 21)]
        public static void AutoOptimizeSelected()
        {
            if (Selection.activeGameObject == null) return;
            
            GameObject go = Selection.activeGameObject;
            int fixes = 0;

            // Mark Static
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null && !go.isStatic && go.GetComponent<Rigidbody>() == null && go.GetComponent<Animator>() == null)
            {
                go.isStatic = true;
                fixes++;
            }

            // Fix Animator Culling
            var animator = go.GetComponent<Animator>();
            if (animator != null && animator.cullingMode == AnimatorCullingMode.AlwaysAnimate)
            {
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                fixes++;
            }

            // Fix UI Raycast
            var graphic = go.GetComponent<UnityEngine.UI.Graphic>();
            if (graphic != null && graphic.raycastTarget && go.GetComponent<UnityEngine.UI.Selectable>() == null)
            {
                graphic.raycastTarget = false;
                fixes++;
            }

            // Fix Rigidbody Interpolation
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null && rb.interpolation != RigidbodyInterpolation.None)
            {
                rb.interpolation = RigidbodyInterpolation.None;
                fixes++;
            }

            // Fix Reflection Probe
            var rp = go.GetComponent<ReflectionProbe>();
            if (rp != null && rp.refreshMode == UnityEngine.Rendering.ReflectionProbeRefreshMode.EveryFrame)
            {
                rp.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
                fixes++;
            }

            // Fix Light Shadows
            var light = go.GetComponent<Light>();
            if (light != null && (light.type == LightType.Point || light.type == LightType.Spot) && 
                light.shadows != LightShadows.None && light.lightmapBakeType == LightmapBakeType.Realtime)
            {
                light.shadows = LightShadows.None;
                fixes++;
            }

            if (fixes > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                EditorUtility.DisplayDialog("✨ Auto-Optimize Complete", 
                    $"Applied {fixes} optimization(s) to '{go.name}'.\n\nRight-click → Quick Performance Check to verify.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("✨ Already Optimized", 
                    $"'{go.name}' has no auto-fixable issues. It's already clean!", "OK");
            }
        }

        [MenuItem("GameObject/Auto Profiler/📊 Batch Optimize All Selected", false, 22)]
        public static void BatchOptimizeAllSelected()
        {
            if (Selection.gameObjects.Length == 0) return;

            int totalFixes = 0;
            int objectsFixed = 0;

            foreach (var go in Selection.gameObjects)
            {
                int fixes = 0;

                var mr = go.GetComponent<MeshRenderer>();
                if (mr != null && !go.isStatic && go.GetComponent<Rigidbody>() == null && go.GetComponent<Animator>() == null)
                { go.isStatic = true; fixes++; }

                var anim = go.GetComponent<Animator>();
                if (anim != null && anim.cullingMode == AnimatorCullingMode.AlwaysAnimate)
                { anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms; fixes++; }

                var gfx = go.GetComponent<UnityEngine.UI.Graphic>();
                if (gfx != null && gfx.raycastTarget && go.GetComponent<UnityEngine.UI.Selectable>() == null)
                { gfx.raycastTarget = false; fixes++; }

                if (fixes > 0)
                {
                    objectsFixed++;
                    totalFixes += fixes;
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }

            EditorUtility.DisplayDialog("📊 Batch Optimize Complete",
                $"Optimized {objectsFixed}/{Selection.gameObjects.Length} selected objects.\nApplied {totalFixes} total fixes.", "OK");
        }

        [MenuItem("GameObject/Auto Profiler/🔍 Quick Performance Check", true)]
        [MenuItem("GameObject/Auto Profiler/✨ Auto-Optimize Selected", true)]
        public static bool ValidateSelection()
        {
            return Selection.activeGameObject != null;
        }

        [MenuItem("GameObject/Auto Profiler/📊 Batch Optimize All Selected", true)]
        public static bool ValidateBatchSelection()
        {
            return Selection.gameObjects.Length > 0;
        }
    }
}
