using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 正式包流水线：切平台 → CreatBundle 离线全包（强制变体收集）→ 再打 Player。
/// 用 SessionState 扛 Domain Reload；用 EditorApplication.update 驱动，避免 delayCall 丢失。
/// </summary>
public static class AutoOfflineReleasePipeline
{
    const string KeyJob = "ScoreCardUWB.AutoOfflineRelease.Job";
    const string KeyPhase = "ScoreCardUWB.AutoOfflineRelease.Phase";
    const string KeyOpenExplorer = "ScoreCardUWB.AutoOfflineRelease.OpenExplorer";

    const string PhaseOffline = "Offline";
    const string PhaseOfflineRunning = "OfflineRunning";
    const string PhasePlayer = "Player";

    const string JobAndroid = "Android";
    const string JobWindows = "Windows";

    static bool s_UpdateHooked;
    static bool s_TickEntered;

    public static void StartAndroid(bool openExplorerOnSuccess)
    {
        Start(JobAndroid, BuildTargetGroup.Android, BuildTarget.Android, openExplorerOnSuccess);
    }

    public static void StartWindows(bool openExplorerOnSuccess)
    {
        Start(
            JobWindows,
            BuildTargetGroup.Standalone,
            BuildTarget.StandaloneWindows64,
            openExplorerOnSuccess);
    }

    static void Start(
        string job,
        BuildTargetGroup group,
        BuildTarget target,
        bool openExplorerOnSuccess)
    {
        if (!string.IsNullOrEmpty(SessionState.GetString(KeyJob, string.Empty)))
        {
            Fail(job, "已有正式包流水线在进行，请等待完成后再试。");
            return;
        }

        SessionState.SetString(KeyJob, job);
        SessionState.SetString(KeyPhase, PhaseOffline);
        SessionState.SetBool(KeyOpenExplorer, openExplorerOnSuccess);

        Debug.Log($"[AutoOfflineReleasePipeline] 开始 {job}：切到 {target} → 离线全包（含变体）→ Release");

        if (job == JobWindows)
        {
            ApplyWindowsScriptingBackendForCurrentEditor();
        }

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
        {
            Clear();
            Fail(
                job,
                job == JobWindows
                    ? "无法切换到 StandaloneWindows64。请确认已安装对应 Windows Build Support。"
                    : "无法切换到 Android。请确认已安装 Android Build Support。");
            return;
        }

        EnsureUpdateHook();
    }

    static void ApplyWindowsScriptingBackendForCurrentEditor()
    {
        var backend = Application.platform == RuntimePlatform.OSXEditor
            ? ScriptingImplementation.Mono2x
            : ScriptingImplementation.IL2CPP;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, backend);
        Debug.Log(
            $"[AutoOfflineReleasePipeline] Standalone scripting backend → {backend} (editor={Application.platform})");
    }

    [InitializeOnLoadMethod]
    static void ResumeAfterDomainReload()
    {
        if (string.IsNullOrEmpty(SessionState.GetString(KeyJob, string.Empty)))
        {
            return;
        }

        if (SessionState.GetString(KeyPhase, string.Empty) == PhaseOfflineRunning)
        {
            Debug.LogWarning(
                "[AutoOfflineReleasePipeline] Domain Reload 中断了离线全包，将重新执行变体收集 + 离线全包");
            SessionState.SetString(KeyPhase, PhaseOffline);
        }

        EnsureUpdateHook();
    }

    static void EnsureUpdateHook()
    {
        if (s_UpdateHooked)
        {
            return;
        }

        EditorApplication.update += OnEditorUpdate;
        s_UpdateHooked = true;
    }

    static void StopUpdateHook()
    {
        if (!s_UpdateHooked)
        {
            return;
        }

        EditorApplication.update -= OnEditorUpdate;
        s_UpdateHooked = false;
    }

    static void OnEditorUpdate()
    {
        if (s_TickEntered)
        {
            return;
        }

        string job = SessionState.GetString(KeyJob, string.Empty);
        if (string.IsNullOrEmpty(job))
        {
            StopUpdateHook();
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        string phase = SessionState.GetString(KeyPhase, PhaseOffline);
        if (phase == PhaseOfflineRunning)
        {
            return;
        }

        s_TickEntered = true;
        try
        {
            Tick(job, phase);
        }
        finally
        {
            s_TickEntered = false;
        }
    }

    static void Tick(string job, string phase)
    {
        if (phase == PhaseOffline)
        {
            if (job == JobWindows)
            {
                ApplyWindowsScriptingBackendForCurrentEditor();
            }

            SessionState.SetString(KeyPhase, PhaseOfflineRunning);
            Debug.Log($"[AutoOfflineReleasePipeline] {job}：开始 CreatBundle 离线全包（自动收集全部变体）");

            LetsPiu.CreatBundle.CreateOfflineBundleAuto(
                ok =>
                {
                    if (!ok)
                    {
                        Clear();
                        StopUpdateHook();
                        Fail(job, "CreatBundle 离线全包失败，已中止 Release 打包。见 Console。");
                        return;
                    }

                    Debug.Log($"[AutoOfflineReleasePipeline] {job}：离线全包完成，开始打 Release");
                    SessionState.SetString(KeyPhase, PhasePlayer);
                    EnsureUpdateHook();
                },
                revealPackageFolder: false);
            return;
        }

        if (phase == PhasePlayer)
        {
            bool openExplorer = SessionState.GetBool(KeyOpenExplorer, false);
            Clear();
            StopUpdateHook();
            DispatchPlayerBuild(job, openExplorer);
        }
    }

    static void DispatchPlayerBuild(string job, bool openExplorer)
    {
        try
        {
            if (job == JobAndroid)
            {
                AndroidReleaseApkBuilder.BuildPlayerAfterOffline(openExplorer);
            }
            else if (job == JobWindows)
            {
                WindowsReleaseBuilder.BuildPlayerAfterOffline(openExplorer);
            }
            else
            {
                Fail(job, $"未知任务：{job}");
            }
        }
        catch (Exception e)
        {
            Fail(job, e.Message);
        }
    }

    static void Clear()
    {
        SessionState.EraseString(KeyJob);
        SessionState.EraseString(KeyPhase);
        SessionState.EraseBool(KeyOpenExplorer);
    }

    static void Fail(string job, string message)
    {
        string tag = job == JobWindows ? "[WindowsReleaseBuilder]" : "[AndroidReleaseApkBuilder]";
        Debug.LogError($"{tag} {message}");
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(1);
        }
    }
}
