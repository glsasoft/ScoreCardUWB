using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 命令行 / 菜单：打 Windows x86_64 Release 文件夹包（ScoreCardUWB）。
/// 流程：切 StandaloneWindows64 → CreatBundle 离线全包（强制变体收集）→ Release。
/// Mac 编辑器自动 Mono，Windows 编辑器自动 IL2CPP。
/// 输出：Builds/Windows/&lt;product&gt;_v&lt;ver&gt;_&lt;build&gt;_&lt;time&gt;/ScoreCardUWB.exe + _Data。
/// </summary>
public static class WindowsReleaseBuilder
{
    const string DefaultOutputDir = "Builds/Windows";
    const string DefaultVersionName = "1.0.0";
    const string BuildNumberPrefKey = "ScoreCardUWB.WindowsRelease.BuildNumber";

    [MenuItem("Tools/Build/Windows Release")]
    public static void BuildFromMenu()
    {
        if (!WindowsReleaseVersionWindow.Prompt(out string versionName, out int buildNumber))
        {
            return;
        }

        ApplyVersion(versionName, buildNumber);
        AutoOfflineReleasePipeline.StartWindows(openExplorerOnSuccess: true);
    }

    /// <summary>供 bat：Unity -batchmode -executeMethod WindowsReleaseBuilder.Build。无界面，版本名沿用上次，Build 号自动 +1。</summary>
    public static void Build()
    {
        string versionName = PlayerSettings.bundleVersion;
        if (string.IsNullOrWhiteSpace(versionName))
        {
            versionName = DefaultVersionName;
        }

        int buildNumber = EditorPrefs.GetInt(BuildNumberPrefKey, 0) + 1;
        if (buildNumber < 1)
        {
            buildNumber = 1;
        }

        ApplyVersion(versionName.Trim(), buildNumber);
        AutoOfflineReleasePipeline.StartWindows(openExplorerOnSuccess: false);
    }

    static void ApplyVersion(string versionName, int buildNumber)
    {
        PlayerSettings.bundleVersion = versionName;
        EditorPrefs.SetInt(BuildNumberPrefKey, buildNumber);
    }

    static int GetLastBuildNumber()
    {
        return EditorPrefs.GetInt(BuildNumberPrefKey, 0);
    }

    /// <summary>Mac 编辑器只能打 Windows Mono；Windows 编辑器打 IL2CPP。</summary>
    static ScriptingImplementation ResolveScriptingBackend()
    {
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            return ScriptingImplementation.Mono2x;
        }

        return ScriptingImplementation.IL2CPP;
    }

    static string BackendDisplayName(ScriptingImplementation backend)
    {
        return backend == ScriptingImplementation.IL2CPP ? "IL2CPP" : "Mono";
    }

    /// <summary>离线全包成功后由 <see cref="AutoOfflineReleasePipeline"/> 调用。</summary>
    public static void BuildPlayerAfterOffline(bool openExplorerOnSuccess)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length == 0)
        {
            Fail("EditorBuildSettings 中没有启用的场景。");
            return;
        }

        var backend = ResolveScriptingBackend();
        string backendName = BackendDisplayName(backend);

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, backend);
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.allowDebugging = false;
        EditorUserBuildSettings.connectProfiler = false;

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string folderName =
            $"{PlayerSettings.productName}_v{PlayerSettings.bundleVersion}_{GetLastBuildNumber()}_{DateTime.Now:yyyyMMdd_HHmmss}";
        string outDir = Path.Combine(projectRoot, DefaultOutputDir, folderName);
        Directory.CreateDirectory(outDir);

        string exePath = Path.Combine(outDir, PlayerSettings.productName + ".exe");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            targetGroup = BuildTargetGroup.Standalone,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = BuildOptions.CompressWithLz4HC,
        };

        Debug.Log(
            $"[WindowsReleaseBuilder] 开始打 Windows Release\n" +
            $"  product={PlayerSettings.productName}\n" +
            $"  version={PlayerSettings.bundleVersion} (build {GetLastBuildNumber()})\n" +
            $"  backend={backendName} (editor={Application.platform})\n" +
            $"  target=StandaloneWindows64\n" +
            $"  scenes={string.Join(", ", scenes)}\n" +
            $"  output={outDir}");

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            Fail($"打包失败：{summary.result}，errors={summary.totalErrors}，见 Editor.log。");
            return;
        }

        Debug.Log($"[WindowsReleaseBuilder] 成功：{outDir}  size={summary.totalSize} bytes");

        if (openExplorerOnSuccess && Directory.Exists(outDir))
        {
            EditorUtility.RevealInFinder(exePath);
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    static void Fail(string message)
    {
        Debug.LogError("[WindowsReleaseBuilder] " + message);
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(1);
        }
        else
        {
            throw new InvalidOperationException(message);
        }
    }
}

/// <summary>菜单打包前确认版本名和 Build 号。版本名默认沿用 PlayerSettings，Build 号为上次 Windows 包 +1。</summary>
class WindowsReleaseVersionWindow : EditorWindow
{
    const string DefaultVersionName = "1.0.0";
    const string BuildNumberPrefKey = "ScoreCardUWB.WindowsRelease.BuildNumber";

    string m_VersionName;
    int m_BuildNumber;
    bool m_Confirmed;

    public static bool Prompt(out string versionName, out int buildNumber)
    {
        var window = CreateInstance<WindowsReleaseVersionWindow>();
        string current = PlayerSettings.bundleVersion;
        window.m_VersionName = string.IsNullOrWhiteSpace(current) ? DefaultVersionName : current.Trim();
        int next = EditorPrefs.GetInt(BuildNumberPrefKey, 0) + 1;
        window.m_BuildNumber = next < 1 ? 1 : next;
        window.titleContent = new GUIContent("设置 Windows 版本");
        window.minSize = new Vector2(460f, 200f);
        window.ShowModalUtility();

        versionName = window.m_VersionName;
        buildNumber = window.m_BuildNumber;
        bool confirmed = window.m_Confirmed;
        DestroyImmediate(window);
        return confirmed;
    }

    void OnGUI()
    {
        EditorGUILayout.Space(8f);
        string backendHint = Application.platform == RuntimePlatform.OSXEditor
            ? "当前为 Mac 编辑器，将使用 Mono。"
            : "当前为 Windows 编辑器，将使用 IL2CPP。";
        EditorGUILayout.HelpBox(
            "将自动：切到 Windows → CreatBundle 离线全包（收集全部 Shader 变体）→ 打 Release。\n" +
            "版本号写入 PlayerSettings.bundleVersion。Build 号仅用于 Windows 输出目录命名。\n" +
            backendHint,
            MessageType.Info);
        EditorGUILayout.Space(6f);
        m_VersionName = EditorGUILayout.TextField("版本号", m_VersionName);
        m_BuildNumber = EditorGUILayout.IntField("Build 号", m_BuildNumber);
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            "输出目录",
            $"Builds/Windows/{PlayerSettings.productName}_v{m_VersionName}_{m_BuildNumber}_时间/");
        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("取消", GUILayout.Width(90f)))
        {
            m_Confirmed = false;
            Close();
        }

        if (GUILayout.Button("开始打包", GUILayout.Width(90f)))
        {
            if (string.IsNullOrWhiteSpace(m_VersionName))
            {
                EditorUtility.DisplayDialog("版本号", "版本号不能为空。", "确定");
                return;
            }

            if (m_BuildNumber < 1)
            {
                EditorUtility.DisplayDialog("Build 号", "Build 号必须大于 0。", "确定");
                return;
            }

            m_VersionName = m_VersionName.Trim();
            m_Confirmed = true;
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }
}
