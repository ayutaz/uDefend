using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace uDefend.Editor
{
    public static class PackageBuilder
    {
        private const string RootFolder = "Assets/uDefend";
        private const string OutputPath = "uDefend.unitypackage";

        private static readonly string[] ExcludePrefixes = new[]
        {
            "Assets/uDefend/Tests"
        };

        public static void Export()
        {
            try
            {
                var guids = AssetDatabase.FindAssets("", new[] { RootFolder });

                var paths = guids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => !ExcludePrefixes.Any(ex => p.StartsWith(ex, StringComparison.Ordinal)))
                    .Distinct()
                    .ToArray();

                if (paths.Length == 0)
                {
                    Debug.LogError("[PackageBuilder] No assets found to export.");
                    EditorApplication.Exit(1);
                    return;
                }

                AssetDatabase.ExportPackage(
                    paths,
                    OutputPath,
                    ExportPackageOptions.Recurse);

                Debug.Log($"[PackageBuilder] Exported {paths.Length} assets to {OutputPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PackageBuilder] Export failed: {ex}");
                EditorApplication.Exit(1);
            }
        }
    }
}
