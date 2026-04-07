using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace AutoPerformanceProfiler.Editor
{
    /// <summary>
    /// Finds duplicate assets (textures, meshes, audio, materials) in the project
    /// that are wasting build size and VRAM. Premium feature for $100 asset.
    /// </summary>
    public static class DuplicateAssetFinder
    {
        [System.Serializable]
        public class DuplicateGroup
        {
            public string hash;
            public string assetType;
            public List<string> paths = new List<string>();
            public long individualSizeBytes;
            public long wastedBytes; // (count - 1) * size
        }

        public static List<DuplicateGroup> FindAllDuplicates(System.Action<float, string> onProgress = null)
        {
            var hashGroups = new Dictionary<string, DuplicateGroup>();
            
            string[] extensions = { ".png", ".jpg", ".tga", ".psd", ".bmp", ".tif", ".exr",
                                     ".wav", ".mp3", ".ogg", ".aif", ".flac",
                                     ".fbx", ".obj", ".blend", ".dae",
                                     ".mat", ".shader", ".anim", ".controller" };

            string[] allFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToArray();

            for (int i = 0; i < allFiles.Length; i++)
            {
                string file = allFiles[i];
                onProgress?.Invoke((float)i / allFiles.Length, Path.GetFileName(file));

                try
                {
                    var fi = new FileInfo(file);
                    if (!fi.Exists || fi.Length == 0) continue;

                    // Use file size + partial content hash for speed (full hash is too slow for large projects)
                    string hash = ComputeFastHash(file, fi.Length);
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    string assetType = GetAssetType(ext);

                    if (!hashGroups.ContainsKey(hash))
                    {
                        hashGroups[hash] = new DuplicateGroup
                        {
                            hash = hash,
                            assetType = assetType,
                            individualSizeBytes = fi.Length
                        };
                    }
                    
                    // Convert to relative path for Unity
                    string relativePath = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                    hashGroups[hash].paths.Add(relativePath);
                }
                catch { }
            }

            // Filter to only groups with 2+ files (actual duplicates)
            var duplicates = hashGroups.Values
                .Where(g => g.paths.Count > 1)
                .OrderByDescending(g => g.individualSizeBytes * (g.paths.Count - 1))
                .ToList();

            foreach (var group in duplicates)
            {
                group.wastedBytes = group.individualSizeBytes * (group.paths.Count - 1);
            }

            return duplicates;
        }

        private static string ComputeFastHash(string filePath, long fileSize)
        {
            // Fast hash: file size + first 8KB + last 8KB
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] sizeBytes = System.BitConverter.GetBytes(fileSize);
                    md5.TransformBlock(sizeBytes, 0, sizeBytes.Length, sizeBytes, 0);

                    byte[] buffer = new byte[8192];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);

                    if (fileSize > 16384)
                    {
                        stream.Seek(-8192, SeekOrigin.End);
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        md5.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    }

                    md5.TransformFinalBlock(new byte[0], 0, 0);
                    return System.BitConverter.ToString(md5.Hash).Replace("-", "");
                }
            }
        }

        private static string GetAssetType(string ext)
        {
            if (new[] { ".png", ".jpg", ".tga", ".psd", ".bmp", ".tif", ".exr" }.Contains(ext)) return "Texture";
            if (new[] { ".wav", ".mp3", ".ogg", ".aif", ".flac" }.Contains(ext)) return "Audio";
            if (new[] { ".fbx", ".obj", ".blend", ".dae" }.Contains(ext)) return "Mesh";
            if (ext == ".mat") return "Material";
            if (ext == ".shader") return "Shader";
            if (ext == ".anim") return "Animation";
            if (ext == ".controller") return "Animator";
            return "Other";
        }

        public static long GetTotalWastedBytes(List<DuplicateGroup> groups)
        {
            return groups.Sum(g => g.wastedBytes);
        }
    }
}
