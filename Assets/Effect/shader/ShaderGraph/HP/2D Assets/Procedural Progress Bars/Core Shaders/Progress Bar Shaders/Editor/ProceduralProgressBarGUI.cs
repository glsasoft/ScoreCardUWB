using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProceduralProgressBarGUI : ShaderGUI
{
    static readonly Dictionary<string, string> headers = new Dictionary<string, string>
    {
        { "_MainAlpha", "总控" },
        { "_Fill_Amount", "通用 / 核心" },
        { "_General_Shape_Height", "基础形状" },
        { "_CUSTOM_SHAPE_ENABLE", "自定义形状" },
        { "_Simple_Outline_Thickness", "简单描边" },
        { "_CUSTOM_OUTLINE_ENABLE", "自定义描边" },
        { "_Shadow_Enable", "阴影" },
        { "_Fill_Skew", "填充 / 核心" },
        { "_Fill_Use_Horizontal_Gradient", "填充 / 渐变" },
        { "_Fill_Second_Bar_Enable", "填充 / 第二进度" },
        { "_Fill_Vertical_Darken_Enable", "填充 / 纵向压暗" },
        { "_FILL_OVERLAY_ENABLE", "填充 / 叠加" },
        { "_FILL_SEGMENTS_ENABLE", "填充 / 分段" },
        { "_Fill_Enable_Smooth_Edge", "填充 / 边缘" },
        { "_Fill_Shadow_Enable", "填充 / 阴影" },
        { "_END_LINE_ENABLE", "末端线" },
    };

    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        var first = true;
        foreach (var property in properties)
        {
            if ((property.flags & MaterialProperty.PropFlags.HideInInspector) != 0)
                continue;

            if (headers.TryGetValue(property.name, out var title))
            {
                if (!first)
                    EditorGUILayout.Space(8);
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            }

            first = false;
            editor.ShaderProperty(property, property.displayName);
        }
    }
}
