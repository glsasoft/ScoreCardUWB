using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class CreateAlibabaPuHuiTiThinTMPFontAsset
{
    const string SourcePath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Fonts/AlibabaPuHuiTi-3-35-Thin.otf";
    const string OutPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Fonts/AlibabaPuHuiTi-3-35-Thin SDF.asset";

    [MenuItem("Tools/字体相关/(TMP)Create AlibabaPuHuiTi Thin SDF Font", false, 3)]
    public static void CreateFromMenu()
    {
        if (!CreateInternal(true))
        {
            EditorUtility.DisplayDialog("AlibabaPuHuiTi Thin SDF", "创建失败，请查看 Console。", "OK");
        }
    }

    public static void CreateAndExit()
    {
        bool ok = CreateInternal(false);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    static bool CreateInternal(bool refreshAfter)
    {
        AssetDatabase.Refresh();

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
        if (sourceFont == null)
        {
            Debug.LogError($"[CreateAlibabaPuHuiTiThinTMPFontAsset] 找不到源字体: {SourcePath}");
            return false;
        }

        if (File.Exists(OutPath))
        {
            AssetDatabase.DeleteAsset(OutPath);
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            Debug.LogError("[CreateAlibabaPuHuiTiThinTMPFontAsset] TMP_FontAsset.CreateFontAsset 返回 null");
            return false;
        }

        string ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        fontAsset.TryAddCharacters(ascii);

        fontAsset.name = "AlibabaPuHuiTi-3-35-Thin SDF";
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "AlibabaPuHuiTi-3-35-Thin Atlas Material";
        }

        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = "AlibabaPuHuiTi-3-35-Thin Atlas";
        }

        AssetDatabase.CreateAsset(fontAsset, OutPath);

        if (fontAsset.material != null)
        {
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTextures != null)
        {
            foreach (var tex in fontAsset.atlasTextures)
            {
                if (tex != null)
                {
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        if (refreshAfter)
        {
            AssetDatabase.Refresh();
        }

        Debug.Log($"[CreateAlibabaPuHuiTiThinTMPFontAsset] 已创建: {OutPath}");
        Selection.activeObject = fontAsset;
        return true;
    }
}
