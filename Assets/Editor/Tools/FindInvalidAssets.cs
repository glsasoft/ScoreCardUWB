using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FindInvalidAssets
{
    private const string ReportFile = "InvalidAssetsReport.txt";

    [MenuItem("Tools/YooAsset/Report Invalid Assets")]
    public static void ReportInvalidAssets()
    {
        ScanAssets(false);
    }

    [MenuItem("Tools/YooAsset/Report And Reimport Invalid Assets")]
    public static void ReportAndReimportInvalidAssets()
    {
        ScanAssets(true);
    }

    private static void ScanAssets(bool tryReimport)
    {
        var allPaths = AssetDatabase.GetAllAssetPaths();
        var invalids = new List<string>();

        foreach (var path in allPaths)
        {
            if (!path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                continue;

            // Skip folders
            if (AssetDatabase.IsValidFolder(path))
                continue;

            try
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == null)
                {
                    invalids.Add(path);
                    Debug.LogError($"Invalid asset detected: {path}");
                    if (tryReimport)
                    {
                        Debug.Log($"Trying to reimport: {path}");
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                        var newType = AssetDatabase.GetMainAssetTypeAtPath(path);
                        if (newType != null)
                        {
                            Debug.Log($"Reimport succeeded: {path} -> {newType}");
                        }
                        else
                        {
                            Debug.LogWarning($"Reimport did NOT fix: {path}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while checking asset '{path}': {ex.Message}");
            }
        }

        // Write report file at project root
        try
        {
            var reportPath = Path.Combine(Application.dataPath, "..", ReportFile);
            File.WriteAllText(reportPath, string.Join(Environment.NewLine, invalids));
            Debug.Log($"Invalid assets scan completed. Found {invalids.Count} items. Report: {reportPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write report file: {ex.Message}");
        }
    }
}
