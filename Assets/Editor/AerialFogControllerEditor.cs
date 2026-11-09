#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// AerialFogController 的 Inspector：默认字段照旧，下面加一排预设按钮和当前生效参数。
/// Play 模式下按 TransitionTo 走过渡，非 Play 模式下走 SnapTo 立刻看效果。
/// </summary>
[CustomEditor(typeof(AerialFogController))]
public class AerialFogControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var ctrl = (AerialFogController)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("天气切换", EditorStyles.boldLabel);

        if (ctrl.presets == null || ctrl.presets.Length == 0)
        {
            EditorGUILayout.HelpBox("还没有配置预设。把 Presets 目录下的 .asset 拖进上面的数组。", MessageType.Warning);
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            for (int i = 0; i < ctrl.presets.Length; i++)
            {
                var p = ctrl.presets[i];
                string label = p != null && !string.IsNullOrEmpty(p.displayName)
                    ? p.displayName
                    : "空 (" + i + ")";

                if (GUILayout.Button(label, GUILayout.Height(24)))
                {
                    if (Application.isPlaying)
                        ctrl.TransitionTo(i);
                    else
                        ctrl.SnapTo(i);

                    EditorUtility.SetDirty(ctrl);
                    RepaintAllViews();
                }
            }
        }

        if (ctrl.manualOverride)
        {
            EditorGUILayout.HelpBox(
                "手动覆盖已开启：上面的 fogStart / fogEnd / 两个颜色会盖掉预设里的对应字段，切预设不会重置它们。",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("从当前预设同步覆盖值"))
                {
                    var p = ctrl.CurrentParams;
                    var so = new SerializedObject(ctrl);
                    so.FindProperty("overrideFogStart").floatValue = p.fogStart;
                    so.FindProperty("overrideFogEnd").floatValue = p.fogEnd;
                    so.FindProperty("overrideColorNear").colorValue = p.colorNear;
                    so.FindProperty("overrideColorFar").colorValue = p.colorFar;
                    so.ApplyModifiedProperties();
                    RepaintAllViews();
                }
            }
        }

        if (Application.isPlaying && !Application.runInBackground)
        {
            EditorGUILayout.HelpBox(
                "Play 模式下 Unity 失焦会暂停 Update，过渡会卡住不动。测试时把 Game 视图置前，或者开 Edit > Project Settings > Player > Run In Background。",
                MessageType.Info);
        }

        EditorGUILayout.Space();
        var cur = ctrl.CurrentParams;
        EditorGUILayout.LabelField("当前生效参数", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("fogStart", cur.fogStart.ToString("F2"));
        EditorGUILayout.LabelField("fogEnd", cur.fogEnd.ToString("F2"));
        EditorGUILayout.LabelField("density", cur.density.ToString("F4"));
        EditorGUILayout.LabelField("heightFalloff", cur.heightFalloff.ToString("F4"));
        EditorGUILayout.LabelField("maxOpacity", cur.maxOpacity.ToString("F3"));
        EditorGUILayout.LabelField("colorNear", cur.colorNear.ToString());
        EditorGUILayout.LabelField("colorFar", cur.colorFar.ToString());
        EditorGUI.indentLevel--;

        // 改了任何字段都让 Scene / Game 视图重画，拖数值时能实时看到雾变化。
        if (GUI.changed)
            RepaintAllViews();
    }

    static void RepaintAllViews()
    {
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
}
#endif
