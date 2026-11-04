using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 简单宏管理，每个环境对应一个字符串，切环境时自动根据当前平台更新宏
/// </summary>
public static class ScriptingDefineSymbolsManager
{
    // 当前宏字符串
    public static string CurrentSymbols;

    // 每个环境对应宏字符串
    public const string TEST_SERVER = "DOTWEEN;BAKERY_INCLUDED;BAKERY_NOREIMPORT;UIDEV";
    public const string TEST_SERVER1 = "DOTWEEN;BAKERY_INCLUDED;BAKERY_NOREIMPORT;ENABLE_BLUETOOTH_PC;UIDEV";
    public const string RELEASE = "DOTWEEN;BAKERY_INCLUDED;BAKERY_NOREIMPORT";

    /// <summary>
    /// 切换环境
    /// </summary>
    public static void SetEnvironment(string env)
    {
        if (string.IsNullOrEmpty(CurrentSymbols)) return;
        string[] currentSymbolsArray = CurrentSymbols.Split(';');
        currentSymbolsArray = Array.FindAll(currentSymbolsArray, s => s != "ENABLE_BLUETOOTH_PC" && s != "UIDEV");
        switch (env)
        {
            case "TEST_SERVER":
                //CurrentSymbols = TEST_SERVER;
                CurrentSymbols = string.Join(';', currentSymbolsArray) + ";UIDEV";
                break;
            case "TEST_SERVER1":
                //CurrentSymbols = TEST_SERVER1;
                CurrentSymbols = string.Join(';', currentSymbolsArray) + ";ENABLE_BLUETOOTH_PC;UIDEV";
                break;
            case "RELEASE":
                //CurrentSymbols = RELEASE;
                CurrentSymbols = string.Join(';', currentSymbolsArray);
                break;
            default:
                Debug.LogWarning("未知环境: " + env);
                return;
        }

        // 编辑器中同步当前平台宏
        UpdateCurrentPlatformDefines();

        Debug.Log("[设置完成] 当前宏: " + CurrentSymbols);
    }

    /// <summary>
    /// 根据当前平台更新宏
    /// </summary>
    private static void UpdateCurrentPlatformDefines()
    {
#if UNITY_EDITOR
        BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, CurrentSymbols);
#endif
    }
}
