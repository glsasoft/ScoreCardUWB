using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// GLSA/Scene/PBR_EnvStandard 的中文材质面板。
///
/// ShaderLab 的 drawer / decorator 参数（Header / Title / Helpbox / Enum 括号里那段）
/// 不接受非 ASCII 字符，写中文直接 Parse error: unexpected $undefined；
/// 属性显示名（引号里那个）反而可以。所以分节和枚举都放在这一层。
/// </summary>
public class GLSAPBRGUI : ShaderGUI
{
    static readonly string[] kNormalModeNames = { "RGB 法线（BC7）", "原始 DXT5nm 通道", "Unity 法线图（已编码）" };
    static readonly string[] kCullNames = { "双面", "剔除正面", "剔除背面" };

    // ---------------------------------------------------------------- 折叠状态
    // key = 材质 instanceID，切材质不会把上一个的展开状态带过去
    static readonly Dictionary<int, bool> s_foldBase = new Dictionary<int, bool>();
    static readonly Dictionary<int, bool> s_foldNormal = new Dictionary<int, bool>();
    static readonly Dictionary<int, bool> s_foldSmo = new Dictionary<int, bool>();
    static readonly Dictionary<int, bool> s_foldEmission = new Dictionary<int, bool>();
    static readonly Dictionary<int, bool> s_foldExtras = new Dictionary<int, bool>();
    static readonly Dictionary<int, bool> s_foldAdvanced = new Dictionary<int, bool>();

    static bool Fold(Dictionary<int, bool> store, int id, string title, bool defaultOpen)
    {
        bool open;
        if (!store.TryGetValue(id, out open))
        {
            open = defaultOpen;
            store[id] = open;
        }
        open = ShaderGUI_Helper.Foldout(open, title);
        store[id] = open;
        return open;
    }

    // ---------------------------------------------------------------- 属性

    MaterialEditor _editor;
    Material _mat;
    int _matId;

    MaterialProperty _baseMap, _baseColor, _alphaClip, _cutoff;
    MaterialProperty _nrmTex, _bumpScale, _useNormalMap, _normalMode, _normalFlipY, _dxt5nmSwap;
    MaterialProperty _smo, _metallic, _smoothness, _occlusion;
    MaterialProperty _emissionSize;
    MaterialProperty _lerpColor, _lerpWeight, _tutorialColor, _tutorial, _aoeColor, _aoe;
    MaterialProperty _cull;

    static MaterialProperty Find(MaterialProperty[] props, string name)
    {
        if (props == null) return null;
        for (int i = 0; i < props.Length; i++)
        {
            if (props[i] != null && props[i].name == name) return props[i];
        }
        return null;
    }

    void Cache(MaterialProperty[] props)
    {
        _baseMap = Find(props, "_BaseMap");
        _baseColor = Find(props, "_BaseColor");
        _alphaClip = Find(props, "_AlphaClip");
        _cutoff = Find(props, "_Cutoff");

        _nrmTex = Find(props, "_NRMTex");
        _bumpScale = Find(props, "_BumpScale");
        _useNormalMap = Find(props, "_UseNormalMap");
        _normalMode = Find(props, "_NormalMode");
        _normalFlipY = Find(props, "_NormalFlipY");
        _dxt5nmSwap = Find(props, "_DXT5nmSwap");

        _smo = Find(props, "_SMO");
        _metallic = Find(props, "_Metallic");
        _smoothness = Find(props, "_Smoothness");
        _occlusion = Find(props, "_OcclusionStrength");

        _emissionSize = Find(props, "_EmissionSize");

        _lerpColor = Find(props, "_LerpColor");
        _lerpWeight = Find(props, "_LerpWeight");
        _tutorialColor = Find(props, "_TutorialColor");
        _tutorial = Find(props, "_Tutorial");
        _aoeColor = Find(props, "_AoeSelectColor");
        _aoe = Find(props, "_AoeSelect");

        _cull = Find(props, "_Cull");
    }

    // ---------------------------------------------------------------- 入口

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        _editor = materialEditor;
        _mat = materialEditor.target as Material;
        _matId = _mat != null ? _mat.GetInstanceID() : 0;

        Cache(properties);

        if (_mat == null)
        {
            base.OnGUI(materialEditor, properties);
            return;
        }

        DrawBase();
        DrawNormal();
        DrawSmo();
        DrawEmission();
        DrawExtras();
        DrawAdvanced();
    }

    // ---------------------------------------------------------------- 工具

    /// 0/1 开关：写值 + 同步 keyword（多选时逐个 target 处理）
    void Toggle(MaterialProperty prop, string label, string keyword)
    {
        if (prop == null) return;

        bool cur = prop.floatValue > 0.5f;
        EditorGUI.showMixedValue = prop.hasMixedValue;
        bool next = EditorGUILayout.Toggle(label, cur);
        EditorGUI.showMixedValue = false;

        if (next == cur && !prop.hasMixedValue) return;

        _editor.RegisterPropertyChangeUndo(label);
        prop.floatValue = next ? 1f : 0f;
        if (string.IsNullOrEmpty(keyword)) return;

        foreach (var t in _editor.targets)
        {
            var m = t as Material;
            if (m == null) continue;
            if (next) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }
    }

    /// 整数枚举下拉
    void Popup(MaterialProperty prop, string label, string[] names)
    {
        if (prop == null) return;

        int cur = Mathf.Clamp(Mathf.RoundToInt(prop.floatValue), 0, names.Length - 1);
        EditorGUI.showMixedValue = prop.hasMixedValue;
        int next = EditorGUILayout.Popup(label, cur, names);
        EditorGUI.showMixedValue = false;

        if (next == cur) return;
        _editor.RegisterPropertyChangeUndo(label);
        prop.floatValue = next;
    }

    // ---------------------------------------------------------------- 分区

    void DrawBase()
    {
        if (!Fold(s_foldBase, _matId, "基础色", true)) return;

        EditorGUI.indentLevel++;
        _editor.TexturePropertySingleLine(new GUIContent("基础贴图"), _baseMap, _baseColor);
        _editor.TextureScaleOffsetProperty(_baseMap);

        Toggle(_alphaClip, "透明裁剪", "_ALPHATEST_ON");
        using (new EditorGUI.DisabledScope(_alphaClip == null || _alphaClip.floatValue < 0.5f))
        {
            _editor.ShaderProperty(_cutoff, new GUIContent("裁剪阈值"));
        }
        EditorGUI.indentLevel--;
    }

    void DrawNormal()
    {
        if (!Fold(s_foldNormal, _matId, "法线", true)) return;

        EditorGUI.indentLevel++;
        _editor.TexturePropertySingleLine(new GUIContent("法线贴图"), _nrmTex);
        _editor.TextureScaleOffsetProperty(_nrmTex);
        _editor.ShaderProperty(_bumpScale, new GUIContent("法线强度"));

        Toggle(_useNormalMap, "启用法线贴图", null);
        Popup(_normalMode, "法线模式", kNormalModeNames);
        Toggle(_normalFlipY, "翻转法线 Y", null);
        Toggle(_dxt5nmSwap, "DXT5nm 交换 XY（G↔A）", null);
        EditorGUI.indentLevel--;
    }

    void DrawSmo()
    {
        if (!Fold(s_foldSmo, _matId, "光滑度 / 金属度 / AO", true)) return;

        EditorGUI.indentLevel++;
        _editor.TexturePropertySingleLine(new GUIContent("SMO 贴图"), _smo);
        _editor.TextureScaleOffsetProperty(_smo);
        _editor.ShaderProperty(_metallic, new GUIContent("金属度"));
        _editor.ShaderProperty(_smoothness, new GUIContent("光滑度"));
        _editor.ShaderProperty(_occlusion, new GUIContent("AO 强度"));
        EditorGUI.indentLevel--;
    }

    void DrawEmission()
    {
        if (!Fold(s_foldEmission, _matId, "自发光", true)) return;

        EditorGUI.indentLevel++;
        _editor.ShaderProperty(_emissionSize, new GUIContent("自发光强度倍率"));
        EditorGUI.indentLevel--;
    }

    void DrawExtras()
    {
        if (!Fold(s_foldExtras, _matId, "场景附加", false)) return;

        EditorGUI.indentLevel++;
        _editor.ShaderProperty(_lerpColor, new GUIContent("覆盖颜色"));
        _editor.ShaderProperty(_lerpWeight, new GUIContent("覆盖权重"));

        _editor.ShaderProperty(_tutorialColor, new GUIContent("指引颜色"));
        _editor.ShaderProperty(_tutorial, new GUIContent("指引强度"));

        _editor.ShaderProperty(_aoeColor, new GUIContent("范围指示颜色"));
        _editor.ShaderProperty(_aoe, new GUIContent("范围指示强度"));
        EditorGUI.indentLevel--;
    }

    void DrawAdvanced()
    {
        if (!Fold(s_foldAdvanced, _matId, "高级设置", false)) return;

        EditorGUI.indentLevel++;
        Popup(_cull, "剔除模式", kCullNames);
        EditorGUILayout.Space(2);
        _editor.RenderQueueField();
        _editor.EnableInstancingField();
        EditorGUI.indentLevel--;
    }
}
