#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器窗口，切换环境
/// </summary>
public class ScriptingDefineSymbolsWindow : EditorWindow
{
    [MenuItem("Tools/宏定义管理 &3", false, 3)]
    public static void ShowWindow()
    {
        BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        Debug.Log("获取当前宏: "+defines);
        ScriptingDefineSymbolsManager.CurrentSymbols = defines;

        GetWindow<ScriptingDefineSymbolsWindow>("ScriptingDefineSymbolsManager");
    }

    private void OnGUI()

    {
        GUILayout.Label("选择环境", EditorStyles.boldLabel);

        if (GUILayout.Button("TEST_SERVER"))
            ScriptingDefineSymbolsManager.SetEnvironment("TEST_SERVER");

        if (GUILayout.Button("TEST_SERVER  ( Enable_Bluetooth_PC )"))
            ScriptingDefineSymbolsManager.SetEnvironment("TEST_SERVER1");

        GUILayout.Space(10);

        if (GUILayout.Button("RELEASE"))
            ScriptingDefineSymbolsManager.SetEnvironment("RELEASE");

        GUILayout.Space(10);
        GUILayout.Label("当前宏字符串", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(ScriptingDefineSymbolsManager.CurrentSymbols);
    }
}
#endif
