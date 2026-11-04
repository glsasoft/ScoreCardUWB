using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 多功能材质PRO(Test) 的自定义 Inspector：把属性按功能区折叠
// 使用方法：shader 内 CustomEditor "MultiFuncShaderGUI"
public class MultiFuncShaderGUI : ShaderGUI
{
    private class SubSection
    {
        public string Title;
        public string EnableProp;   // 折叠头右侧的开关属性(可为 null)
        public string[] Props;

        public SubSection(string title, string enableProp, params string[] props)
        {
            Title = title;
            EnableProp = enableProp;
            Props = props;
        }
    }

    private class Section
    {
        public string Title;
        public string EnableProp;   // 折叠头右侧的开关属性(可为 null)
        public bool DefaultOpen;
        public string[] Props;
        public SubSection[] Subs;   // 子折叠区(可为 null)

        public Section(string title, string enableProp, bool defaultOpen, params string[] props)
        {
            Title = title;
            EnableProp = enableProp;
            DefaultOpen = defaultOpen;
            Props = props;
            Subs = null;
        }

        public Section(string title, string enableProp, bool defaultOpen, SubSection[] subs, params string[] props)
        {
            Title = title;
            EnableProp = enableProp;
            DefaultOpen = defaultOpen;
            Props = props;
            Subs = subs;
        }
    }

    private static readonly Section[] Sections =
    {
        new Section("基本设置", null, true,
            "_CullMode", "_dst", "_MainAlpha", "_Desaturate"),

        new Section("颜色调整", null, true,
            "_HueShift_Toggle", "_HueShift",
            "_ChangeSaturability_Toggle", "_Saturability",
            "_Contrast_Toggle", "_Contrast", "_ContrastMidColor",
            "_ColorAdjustmentOnlyAffectMainTex"),

        new Section("主贴图", null, true,
            new SubSection[]
            {
                new SubSection("主贴图02", "_UseMT02",
                    "_MT_Radial02", "_MT_Rotato02", "_MT_Color02", "_MT02", "_MT_Alpha02", "_MT_UV02",
                    "_MT02_WrapMode", "_MT02_Channel", "_MT02_RotSpeed", "_MT02_Distort"),
            },
            "_MT_Color", "_MT_Radial", "_MT_Rotato", "_MT_UseR", "_MT", "_MT_Alpha", "_MT_UV",
            "_MT_WrapMode", "_MT_Channel", "_MT_RotSpeed", "_MT_Distort"),

        new Section("遮罩", null, false,
            new SubSection[]
            {
                new SubSection("遮罩02", "_Use_Mask02",
                    "_Mask02_Radial2", "_Mask02_Rotato", "_Mask02", "_Mask02UV", "_Mask02_Power", "_Mask02_Value",
                    "_Mask02_WrapMode", "_Mask02_Channel", "_Mask02_RotSpeed", "_Mask02_Distort"),
            },
            "_MaskMutupyoradd",
            "_Mask_Radial1", "_Mask_Rotato", "_Mask", "_MaskUV", "_Mask_Power", "_Mask_Value",
            "_Mask_WrapMode", "_Mask_Channel", "_Mask_RotSpeed", "_Mask_Distort"),

        new Section("扰动", null, false,
            new SubSection[]
            {
                new SubSection("扰动遮罩", "_Use_TurbulenceMask",
                    "_TurbulenceMask_Rotato", "_Turbulence_Mask", "_Turbulence_Mask_Power1", "_Turbulence_Mask_Value", "_TurbulenceMask_UV",
                    "_TurbulenceMask_WrapMode", "_TurbulenceMask_Channel", "_TurbulenceMask_RotSpeed"),
            },
            "_UseTurbulence", "_Turbulence_Radial", "_Turbulence_Rotato1", "_Turbulence", "_Turbulence_UV", "_Turbulence_Value",
            "_Turbulence_WrapMode", "_Turbulence_Channel", "_Turbulence_RotSpeed"),

        new Section("溶解", null, false,
            new SubSection[]
            {
                new SubSection("溶解遮罩", "_Use_DissolveMask",
                    "_DissolveMask", "_DissolveMask_Power", "_DissolveMask_Value", "_DissolveMask_Rotato1", "_DissolveMask_UV",
                    "_DissolveMask_WrapMode", "_DissolveMask_Channel", "_DissolveMask_RotSpeed", "_DissolveMask_Distort"),
            },
            "_UseDissolve", "_Dissolve_Radial3", "_Dissolve_Rotato", "_Dissolve", "_Dissolve_UV", "_Dissolve_Value", "_Dissolve_Soft", "_DissolveWidth", "_DissolveWidth_Color", "_Dissolve_Raodong",
            "_Dissolve_WrapMode", "_Dissolve_Channel", "_Dissolve_RotSpeed", "_Dissolve_Distort"),

        new Section("渐变", null, false,
            "_UseRamp", "_Ramp", "_Ramp_Rotato", "_RampUV",
            "_Ramp_WrapMode", "_Ramp_RotSpeed"),
    };

    // 属性行里画成"左侧勾选框"的开关属性
    private static readonly HashSet<string> ToggleProps = new HashSet<string>
    {
        "_MT_Radial", "_MT_UseR", "_UseMT02", "_MT_Radial02",
        "_Mask_Radial1", "_Use_Mask02", "_MaskMutupyoradd", "_Mask02_Radial2",
        "_UseTurbulence", "_Turbulence_Radial", "_Use_TurbulenceMask",
        "_UseDissolve", "_Dissolve_Radial3", "_Use_DissolveMask",
        "_UseRamp",
        "_HueShift_Toggle", "_Contrast_Toggle", "_ChangeSaturability_Toggle",
        "_ColorAdjustmentOnlyAffectMainTex"
    };

    // 开关属性对应的材质关键字([Toggle(_KEYWORD)] 里的 keyword)
    private static readonly Dictionary<string, string> ToggleKeyword = new Dictionary<string, string>
    {
        { "_MT_Radial", "_MT_RADIAL_ON" },
        { "_MT_UseR", "_MT_USER_ON" },
        { "_UseMT02", "_USEMT02_ON" },
        { "_MT_Radial02", "_MT_RADIAL02_ON" },
        { "_Mask_Radial1", "_MASK_RADIAL1_ON" },
        { "_Use_Mask02", "_USE_MASK02_ON" },
        { "_MaskMutupyoradd", "_MASKMUTUPYORADD_ON" },
        { "_Mask02_Radial2", "_MASK02_RADIAL2_ON" },
        { "_UseTurbulence", "_USETURBULENCE_ON" },
        { "_Turbulence_Radial", "_TURBULENCE_RADIAL_ON" },
        { "_Use_TurbulenceMask", "_USE_TURBULENCEMASK_ON" },
        { "_UseDissolve", "_USEDISSOLVE_ON" },
        { "_Dissolve_Radial3", "_DISSOLVE_RADIAL3_ON" },
        { "_Use_DissolveMask", "_USE_DISSOLVEMASK_ON" },
        { "_UseRamp", "_USERAMP_ON" },
        { "_HueShift_Toggle", "_HUE_SHIFT_ON" },
        { "_Contrast_Toggle", "_CONTRAST_ON" },
        { "_ChangeSaturability_Toggle", "_SATURATION_ON" },
        { "_ColorAdjustmentOnlyAffectMainTex", "_COLOR_ADJUST_ONLY_MAINTEX" }
    };

    // 画成长条下拉的枚举属性(循环模式/通道选择)
    private static readonly Dictionary<string, string[]> EnumOptions = new Dictionary<string, string[]>
    {
        { "_MT_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_MT_Channel", new[] { "R", "G", "B", "A" } },
        { "_MT02_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_MT02_Channel", new[] { "ColorA", "R", "G", "B", "A" } },
        { "_Mask_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_Mask_Channel", new[] { "R", "G", "B", "A" } },
        { "_Mask02_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_Mask02_Channel", new[] { "R", "G", "B", "A" } },
        { "_Turbulence_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_Turbulence_Channel", new[] { "R", "G", "B", "A" } },
        { "_TurbulenceMask_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_TurbulenceMask_Channel", new[] { "R", "G", "B", "A" } },
        { "_Dissolve_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_Dissolve_Channel", new[] { "R", "G", "B", "A" } },
        { "_DissolveMask_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
        { "_DissolveMask_Channel", new[] { "R", "G", "B", "A" } },
        { "_Ramp_WrapMode", new[] { "Repeat", "Clamp", "RepeatU_ClampV", "ClampU_RepeatV" } },
    };

    private Dictionary<string, MaterialProperty> _propDict;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        materialEditor.SetDefaultGUIWidths();

        _propDict = new Dictionary<string, MaterialProperty>();
        for (int i = 0; i < props.Length; i++)
        {
            if (!_propDict.ContainsKey(props[i].name))
                _propDict.Add(props[i].name, props[i]);
        }

        HashSet<string> drawn = new HashSet<string>();
        foreach (Section sec in Sections)
        {
            foreach (string p in sec.Props) drawn.Add(p);
            if (!string.IsNullOrEmpty(sec.EnableProp)) drawn.Add(sec.EnableProp);
            if (sec.Subs != null)
            {
                foreach (SubSection sub in sec.Subs)
                {
                    foreach (string p in sub.Props) drawn.Add(p);
                    if (!string.IsNullOrEmpty(sub.EnableProp)) drawn.Add(sub.EnableProp);
                }
            }
        }

        EditorGUILayout.Space(4);
        for (int i = 0; i < Sections.Length; i++)
        {
            if (i > 0)
                DrawSeparator();
            DrawSection(materialEditor, Sections[i]);
        }

        // 兜底：未分区的可见属性(如以后 ASE 里新增的属性)
        EditorGUILayout.Space(4);
        foreach (MaterialProperty prop in props)
        {
            if (drawn.Contains(prop.name)) continue;
            if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0) continue;
            DrawProp(materialEditor, prop);
        }
        EditorGUILayout.Space(4);

        // 底部：层级调整折叠区
        DrawSeparator();
        DrawLayerSection(materialEditor);
        EditorGUILayout.Space(4);
    }

    // 底部"层级"折叠区：渲染队列/排序优先级/GPU实例化/双面GI
    private void DrawLayerSection(MaterialEditor materialEditor)
    {
        const string title = "层级";
        string key = "MultiFuncGUI_" + title;
        bool open = EditorPrefs.GetBool(key, false);

        Rect headerRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldoutHeader);
        if (Event.current.type == EventType.Repaint)
            EditorStyles.foldoutHeader.Draw(headerRect, GUIContent.none, false, false, false, false);
        open = EditorGUI.Foldout(headerRect, open, title, true, EditorStyles.foldoutHeader);
        EditorPrefs.SetBool(key, open);

        if (open)
        {
            EditorGUI.indentLevel++;
#if UNITY_2019_2_OR_NEWER
            materialEditor.RenderQueueField();
#endif
            DrawSortingPriorityField(materialEditor);
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }
    }

    // 手动绘制 Sorting Priority：部分 Unity 版本(如中国版)裁剪了 MaterialEditor.RenderingPriorityField API
    private void DrawSortingPriorityField(MaterialEditor materialEditor)
    {
        try
        {
            SerializedObject so = new SerializedObject(materialEditor.targets);
            SerializedProperty sp = so.FindProperty("m_SortingPriority");
            if (sp == null) return;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(sp, -100f, 100f, new GUIContent("Sorting Priority"));
            if (EditorGUI.EndChangeCheck())
                so.ApplyModifiedProperties();
        }
        catch { /* 该版本无此字段时静默跳过 */ }
    }

    private void DrawSeparator()
    {
        EditorGUILayout.Space(4);
        Rect r = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(r, new Color(0.24f, 0.24f, 0.24f, 1f));
        EditorGUILayout.Space(4);
    }

    private void DrawSection(MaterialEditor materialEditor, Section sec)
    {
        string key = "MultiFuncGUI_" + sec.Title;
        bool open = EditorPrefs.GetBool(key, sec.DefaultOpen);

        MaterialProperty enableProp = null;
        if (!string.IsNullOrEmpty(sec.EnableProp))
            _propDict.TryGetValue(sec.EnableProp, out enableProp);

        Rect headerRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldoutHeader);

        // 先铺满整行折叠头背景，避免右侧开关处出现缺口
        if (Event.current.type == EventType.Repaint)
            EditorStyles.foldoutHeader.Draw(headerRect, GUIContent.none, false, false, false, false);

        if (enableProp != null)
        {
            // 开关贴到折叠头最左侧
            const float toggleSlot = 24f;
            Rect toggleRect = new Rect(headerRect.x, headerRect.y, toggleSlot, headerRect.height);
            Rect foldRect = new Rect(headerRect.x + toggleSlot, headerRect.y, headerRect.width - toggleSlot, headerRect.height);

            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(toggleRect, enableProp, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && enableProp.floatValue > 0.5f)
                open = true;   // 打开开关时自动展开该区域

            open = EditorGUI.Foldout(foldRect, open, sec.Title, true, EditorStyles.foldoutHeader);
        }
        else
        {
            open = EditorGUI.Foldout(headerRect, open, sec.Title, true, EditorStyles.foldoutHeader);
        }
        EditorPrefs.SetBool(key, open);

        if (open)
        {
            EditorGUI.indentLevel++;
            foreach (string pname in sec.Props)
            {
                MaterialProperty prop;
                if (_propDict.TryGetValue(pname, out prop))
                    DrawProp(materialEditor, prop);
            }
            if (sec.Subs != null)
            {
                foreach (SubSection sub in sec.Subs)
                    DrawSubSection(materialEditor, sec, sub);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }
    }

    private void DrawSubSection(MaterialEditor materialEditor, Section parent, SubSection sub)
    {
        string key = "MultiFuncGUI_" + parent.Title + "_" + sub.Title;
        bool open = EditorPrefs.GetBool(key, false);

        MaterialProperty enableProp = null;
        if (!string.IsNullOrEmpty(sub.EnableProp))
            _propDict.TryGetValue(sub.EnableProp, out enableProp);

        EditorGUILayout.Space(2);
        Rect headerRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldoutHeader);

        if (Event.current.type == EventType.Repaint)
            EditorStyles.foldoutHeader.Draw(headerRect, GUIContent.none, false, false, false, false);

        if (enableProp != null)
        {
            const float toggleSlot = 28f;
            Rect toggleRect = new Rect(headerRect.x + 4f, headerRect.y, toggleSlot, headerRect.height);
            Rect foldRect = new Rect(headerRect.x + toggleSlot + 4f, headerRect.y, headerRect.width - toggleSlot - 4f, headerRect.height);

            // 自定义开关绘制：不依赖 materialEditor.ShaderProperty 的窄区域 Toggle，
            // 保证点击区域稳定，并手动同步 [Toggle(_KEYWORD)] 的材质关键字
            EditorGUI.BeginChangeCheck();
            bool newVal = EditorGUI.Toggle(toggleRect, enableProp.floatValue > 0.5f);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(enableProp.displayName);
                enableProp.floatValue = newVal ? 1f : 0f;
                string kw;
                if (ToggleKeyword.TryGetValue(enableProp.name, out kw))
                {
                    foreach (UnityEngine.Object o in enableProp.targets)
                    {
                        Material m = o as Material;
                        if (m == null) continue;
                        if (newVal) m.EnableKeyword(kw);
                        else m.DisableKeyword(kw);
                    }
                }
                if (newVal) open = true;
            }
            open = EditorGUI.Foldout(foldRect, open, sub.Title, true, EditorStyles.foldoutHeader);
        }
        else
        {
            open = EditorGUI.Foldout(headerRect, open, sub.Title, true, EditorStyles.foldoutHeader);
        }
        EditorPrefs.SetBool(key, open);

        if (open)
        {
            EditorGUI.indentLevel++;
            foreach (string pname in sub.Props)
            {
                MaterialProperty prop;
                if (_propDict.TryGetValue(pname, out prop))
                    DrawProp(materialEditor, prop);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }
    }

    // 属性行统一入口：开关画左侧勾选框，其余走默认布局；行尾加"重置"小按钮
    // 注意：全程不用 GUILayout 布局，只用 Rect 绘制，避免污染 EditorGUIUtility.labelWidth
    private void DrawProp(MaterialEditor materialEditor, MaterialProperty prop)
    {
        const float btnWidth = 20f;
        Rect btnRect;

        string[] enumOpts;
        if (EnumOptions.TryGetValue(prop.name, out enumOpts))
        {
            Rect r = EditorGUILayout.GetControlRect();
            const float labelW = 120f;
            Rect labelRect = new Rect(r.x, r.y, labelW, r.height);
            Rect fieldRect = new Rect(r.x + labelW, r.y, r.width - labelW - btnWidth, r.height);
            btnRect = new Rect(r.x + r.width - btnWidth, r.y, btnWidth, r.height);

            EditorGUI.LabelField(labelRect, prop.displayName);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            int idx = Mathf.Clamp((int)prop.floatValue, 0, enumOpts.Length - 1);
            int newIdx = EditorGUI.Popup(fieldRect, idx, enumOpts);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(prop.displayName);
                prop.floatValue = newIdx;
            }
            EditorGUI.showMixedValue = false;

            if (GUI.Button(btnRect, new GUIContent("R", "重置"), EditorStyles.miniButton))
                ResetProp(materialEditor, prop);
            return;
        }

        if (ToggleProps.Contains(prop.name))
        {
            Rect r = EditorGUILayout.GetControlRect();
            Rect toggleRect = new Rect(r.x, r.y, r.width - btnWidth, r.height);

            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            bool newVal = EditorGUI.ToggleLeft(toggleRect, prop.displayName, prop.floatValue > 0.5f);
            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(prop.displayName);
                prop.floatValue = newVal ? 1f : 0f;
                // 同步 [Toggle(_KEYWORD)] 的材质关键字，否则开关不生效
                string kw;
                if (ToggleKeyword.TryGetValue(prop.name, out kw))
                {
                    foreach (UnityEngine.Object o in prop.targets)
                    {
                        Material m = o as Material;
                        if (m == null) continue;
                        if (newVal) m.EnableKeyword(kw);
                        else m.DisableKeyword(kw);
                    }
                }
            }
            EditorGUI.showMixedValue = false;

            btnRect = new Rect(r.x + r.width - btnWidth, r.y, btnWidth, r.height);
            if (GUI.Button(btnRect, new GUIContent("R", "重置"), EditorStyles.miniButton))
                ResetProp(materialEditor, prop);
        }
        else
        {
            // 用 GetPropertyHeight 取真实高度（Texture 属性含 Tiling/Offset 多行），避免溢出重叠
            float propH = materialEditor.GetPropertyHeight(prop, prop.displayName);
            Rect r = EditorGUILayout.GetControlRect(true, propH);
            Rect fieldRect = new Rect(r.x, r.y, r.width - btnWidth, r.height);
            float btnH = EditorGUIUtility.singleLineHeight;
            btnRect = new Rect(r.x + r.width - btnWidth, r.y + (r.height - btnH) * 0.5f, btnWidth, btnH);

            materialEditor.ShaderProperty(fieldRect, prop, prop.displayName);
            if (GUI.Button(btnRect, new GUIContent("R", "重置"), EditorStyles.miniButton))
                ResetProp(materialEditor, prop);
        }
    }

    // 把单个属性恢复为 shader 里声明的默认值（默认值解析自 shader 文件 Properties 段）
    private void ResetProp(MaterialEditor materialEditor, MaterialProperty prop)
    {
        Shader shader = null;
        foreach (UnityEngine.Object o in prop.targets)
        {
            Material m = o as Material;
            if (m != null) { shader = m.shader; break; }
        }
        if (shader == null) return;

        Dictionary<string, object> defaults = GetShaderDefaults(shader);

        materialEditor.RegisterPropertyChangeUndo(prop.displayName);
        foreach (UnityEngine.Object o in prop.targets)
        {
            Material m = o as Material;
            if (m == null) continue;

            object def;
            switch (prop.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    if (defaults.TryGetValue(prop.name, out def) && def is float)
                    {
                        float f = (float)def;
                        m.SetFloat(prop.name, f);
                        // 开关属性同步材质关键字
                        string kw;
                        if (ToggleKeyword.TryGetValue(prop.name, out kw))
                        {
                            if (f > 0.5f) m.EnableKeyword(kw);
                            else m.DisableKeyword(kw);
                        }
                    }
                    break;
                case MaterialProperty.PropType.Color:
                    // GetShaderDefaults 里 Color 也解析为 Vector4，这里兼容两种存储
                    if (defaults.TryGetValue(prop.name, out def))
                    {
                        if (def is Color) m.SetColor(prop.name, (Color)def);
                        else if (def is Vector4) m.SetColor(prop.name, (Vector4)def);
                    }
                    break;
                case MaterialProperty.PropType.Vector:
                    if (defaults.TryGetValue(prop.name, out def) && def is Vector4)
                        m.SetVector(prop.name, (Vector4)def);
                    break;
                case MaterialProperty.PropType.Texture:
                    // 清空贴图引用，运行时会回退到 shader 声明的默认纹理
                    m.SetTexture(prop.name, null);
                    // 同时重置贴图的 Tiling/Offset(对应 _XXX_ST 属性)；无对应属性时静默跳过
                    string stName = prop.name + "_ST";
                    object stDef;
                    if (defaults.TryGetValue(stName, out stDef) && stDef is Vector4)
                        m.SetVector(stName, (Vector4)stDef);
                    break;
            }
        }
    }

    // 解析 shader 文件 Properties 段的属性默认值（Float/Range/Int -> float，Color/Vector -> Vector4）
    private Dictionary<string, object> GetShaderDefaults(Shader shader)
    {
        Dictionary<string, object> defaults = new Dictionary<string, object>();
        string path = AssetDatabase.GetAssetPath(shader);
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            return defaults;

        string[] lines;
        try { lines = System.IO.File.ReadAllLines(path); }
        catch { return defaults; }

        System.Globalization.CultureInfo inv = System.Globalization.CultureInfo.InvariantCulture;
        System.Text.RegularExpressions.Regex floatRegex = new System.Text.RegularExpressions.Regex(
            @"(_\w+)\s*\(\s*""[^""]*""\s*,\s*(?:Float|Int|Integer|Range\s*\(\s*[^)]*\))\s*\)\s*=\s*(-?[\d.]+(?:[eE][-+]?\d+)?)");
        System.Text.RegularExpressions.Regex vecRegex = new System.Text.RegularExpressions.Regex(
            @"(_\w+)\s*\(\s*""[^""]*""\s*,\s*(?:Color|Vector)\s*\)\s*=\s*\(\s*(-?[\d.]+(?:[eE][-+]?\d+)?)\s*,\s*(-?[\d.]+(?:[eE][-+]?\d+)?)\s*,\s*(-?[\d.]+(?:[eE][-+]?\d+)?)\s*,\s*(-?[\d.]+(?:[eE][-+]?\d+)?)\s*\)");

        bool inProps = false;
        foreach (string line in lines)
        {
            string t = line.Trim();
            if (t.StartsWith("Properties")) { inProps = true; continue; }
            if (inProps && t == "}") break;
            if (!inProps) continue;

            System.Text.RegularExpressions.Match mf = floatRegex.Match(line);
            if (mf.Success)
            {
                float v;
                if (float.TryParse(mf.Groups[2].Value, System.Globalization.NumberStyles.Float, inv, out v))
                    defaults[mf.Groups[1].Value] = v;
                continue;
            }
            System.Text.RegularExpressions.Match mv = vecRegex.Match(line);
            if (mv.Success)
            {
                Vector4 v = new Vector4();
                v.x = ParseInvFloat(mv.Groups[2].Value, inv);
                v.y = ParseInvFloat(mv.Groups[3].Value, inv);
                v.z = ParseInvFloat(mv.Groups[4].Value, inv);
                v.w = ParseInvFloat(mv.Groups[5].Value, inv);
                defaults[mv.Groups[1].Value] = v;
            }
        }
        return defaults;
    }

    private static float ParseInvFloat(string s, System.Globalization.CultureInfo inv)
    {
        float v;
        return float.TryParse(s, System.Globalization.NumberStyles.Float, inv, out v) ? v : 0f;
    }
}
