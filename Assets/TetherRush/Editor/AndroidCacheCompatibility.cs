using System;
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace TetherRush.Editor
{
    /// <summary>OneDrive can mark generated directories read-only, blocking Gradle's next incremental cleanup.</summary>
    public sealed class AndroidCacheCompatibility : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string generated = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle"));
            string actual = Path.GetFullPath(Path.Combine(path, ".."));
            // Never change exported projects, sources, SDKs, ACLs, or arbitrary caller-supplied paths.
            if (!string.Equals(actual.TrimEnd(Path.DirectorySeparatorChar), generated.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("ANDROID_CACHE_COMPAT skipped non-cache project");
                return;
            }
            int changed = 0;
            foreach (string module in new[] { "unityLibrary", "launcher" })
            {
                string root = Path.Combine(generated, module, "build");
                if (!Directory.Exists(root)) continue;
                changed += ClearReadOnly(root);
                foreach (string directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories))
                    changed += ClearReadOnly(directory);
            }
            Debug.Log("ANDROID_CACHE_COMPAT cleared read-only flag on " + changed + " generated build directories");
        }

        private static int ClearReadOnly(string directory)
        {
            var attributes = File.GetAttributes(directory);
            if ((attributes & FileAttributes.ReadOnly) == 0) return 0;
            File.SetAttributes(directory, attributes & ~FileAttributes.ReadOnly);
            return 1;
        }
    }
}
