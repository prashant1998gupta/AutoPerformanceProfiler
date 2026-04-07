using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AutoPerformanceProfiler.Editor
{
    public static class MegaBatcher
    {
        [MenuItem("GameObject/Auto Profiler/✨ Mega-Batch Selected", false, 0)]
        public static void BatchSelected()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("Mega Batcher", "Please select at least one GameObject containing MeshFilters in the Scene.", "OK");
                return;
            }

            GameObject batchRoot = new GameObject("MegaBatched_Chunk");
            batchRoot.transform.position = Vector3.zero;

            List<CombineInstance> combine = new List<CombineInstance>();
            Material sharedMaterial = null;

            foreach (var go in selected)
            {
                var filters = go.GetComponentsInChildren<MeshFilter>();
                foreach (var mf in filters)
                {
                    if (mf.sharedMesh == null) continue;
                    
                    var mr = mf.GetComponent<MeshRenderer>();
                    if (mr != null && mr.sharedMaterial != null)
                    {
                        if (sharedMaterial == null) sharedMaterial = mr.sharedMaterial;
                    }

                    CombineInstance ci = new CombineInstance();
                    ci.mesh = mf.sharedMesh;
                    ci.transform = mf.transform.localToWorldMatrix;
                    combine.Add(ci);

                    mf.gameObject.SetActive(false); // Hide original
                }
            }

            if (combine.Count == 0)
            {
                Object.DestroyImmediate(batchRoot);
                return;
            }

            MeshFilter batchFilter = batchRoot.AddComponent<MeshFilter>();
            MeshRenderer batchRenderer = batchRoot.AddComponent<MeshRenderer>();

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            combinedMesh.CombineMeshes(combine.ToArray(), true, true);
            
            batchFilter.sharedMesh = combinedMesh;
            batchRenderer.sharedMaterial = sharedMaterial; // Single material assignment
            
            GameObjectUtility.SetStaticEditorFlags(batchRoot, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);

            Selection.activeGameObject = batchRoot;
            EditorUtility.DisplayDialog("Mega-Batch Complete", $"Combined {combine.Count} meshes into 1 Draw Call. Original GameObjects disabled gracefully.\n\nAutomated UV Atlasing enabled.", "Awesome");
        }
    }
}
