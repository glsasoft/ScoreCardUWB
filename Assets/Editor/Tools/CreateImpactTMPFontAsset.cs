using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class CreateImpactTMPFontAsset
{
    const string TtfPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Fonts/Impact.ttf";
    const string OutPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/Fonts/Impact SDF.asset";

    [MenuItem("Tools/字体相关/(TMP)Create Impact SDF Font", false, 2)]
    public static void CreateFromMenu()
    {
        if (!CreateInternal(true))
        {
            EditorUtility.DisplayDialog("Impact SDF", "创建失败，请查看 Console。", "OK");
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

        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (sourceFont == null)
        {
            Debug.LogError($"[CreateImpactTMPFontAsset] 找不到源字体: {TtfPath}");
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
            1024,
            1024,
            AtlasPopulationMode.Dynamic,
            true);

        if (fontAsset == null)
        {
            Debug.LogError("[CreateImpactTMPFontAsset] TMP_FontAsset.CreateFontAsset 返回 null");
            return false;
        }

        string ascii = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        fontAsset.TryAddCharacters(ascii);

        fontAsset.name = "Impact SDF";
        if (fontAsset.material != null)
        {
            fontAsset.material.name = "Impact Atlas Material";
        }

        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = "Impact Atlas";
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

        Debug.Log($"[CreateImpactTMPFontAsset] 已创建: {OutPath}");
        Selection.activeObject = fontAsset;
        return true;
    }
}
