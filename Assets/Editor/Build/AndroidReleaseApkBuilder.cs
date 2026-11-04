using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 命令行 / 菜单：打 Android Release APK。
/// 签名读取项目 PlayerSettings（keystore/alias）+ <see cref="GlobalConfig"/> 注入的密码。
/// 打包前自动校正 JDK/SDK/NDK（本机 2022 编辑器未附带 OpenJDK）。
/// </summary>
public static class AndroidReleaseApkBuilder
{
    const string DefaultOutputDir = "Builds/Android";
    // 备份必须放在 cmdline-tools 目录外。若仍写到 cmdline-tools/11.0.__bak_jdk17，
    // Unity 的 SDKManager 会把它当成更高版本工具链扫描到，最终仍用 JDK17 的 sdkmanager
    // 去跑 JDK11，触发 UnsupportedClassVersionError(class 61 vs 55)。
    const string CmdlineToolsBakRootName = "cmdline-tools.__bak_jdk17";
    const string UnityCmdlineTools21 =
        @"C:\Program Files\Unity\Hub\Editor\2021.3.16f1c1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\cmdline-tools\2.1";

    // Unity 的 Android 构建管线会去探测 cmdline-tools 下固定的几个目录名（实测本机
    // 2022.3.53f1 版本检测走的是 "11.0"，另外 "latest" 对应 SDK Manager 包
    // "cmdline-tools;latest" 也可能被用到），且不会跨目录名找兼容版本。
    // 这些目录里的工具需要 Java 17，与 Unity 2022 要求的 JDK11 冲突，
    // 所以构建期把这几个目录名的内容都临时替换成兼容 JDK11 的 2.1（来自本机 Unity 2021
    // 附带 SDK），构建结束/失败都会在 RestoreStashedCmdlineTools 还原成原样。
    static readonly string[] CmdlineToolsTargetNames = { "latest", "11.0" };

    class StashedCmdlineTools
    {
        public string DirPath;
        public string BackupPath;
        public bool WasStashed;
    }

    static readonly List<StashedCmdlineTools> s_StashedCmdlineTools = new List<StashedCmdlineTools>();

    const string DefaultVersionName = "1.0.0";

    [MenuItem("Tools/Build/Android Release APK")]
    public static void BuildFromMenu()
    {
        if (!AndroidReleaseVersionWindow.Prompt(out string versionName, out int buildNumber))
        {
            return;
        }

        ApplyVersion(versionName, buildNumber);
        Build(openExplorerOnSuccess: true);
    }

    /// <summary>供 bat：Unity -batchmode -executeMethod AndroidReleaseApkBuilder.Build。无界面，版本名沿用上次，Build 号自动 +1。</summary>
    public static void Build()
    {
        string versionName = PlayerSettings.bundleVersion;
        if (string.IsNullOrWhiteSpace(versionName))
        {
            versionName = DefaultVersionName;
        }

        int buildNumber = PlayerSettings.Android.bundleVersionCode + 1;
        if (buildNumber < 1)
        {
            buildNumber = 1;
        }

        ApplyVersion(versionName.Trim(), buildNumber);
        Build(openExplorerOnSuccess: false);
    }

    static void ApplyVersion(string versionName, int buildNumber)
    {
        PlayerSettings.bundleVersion = versionName;
        PlayerSettings.Android.bundleVersionCode = buildNumber;
    }

    static void Build(bool openExplorerOnSuccess)
    {
        if (!EnsureAndroidExternalTools())
        {
            return;
        }

        string sdkRoot = AndroidExternalToolsSettings.sdkRootPath;
        try
        {
            if (!PrepareJdk11CompatibleCmdlineTools(sdkRoot))
            {
                return;
            }

            ApplyReleaseSigningFromProject();

            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                Fail("EditorBuildSettings 中没有启用的场景。");
                return;
            }

            string keystore = PlayerSettings.Android.keystoreName;
            string alias = PlayerSettings.Android.keyaliasName;
            if (!PlayerSettings.Android.useCustomKeystore)
            {
                Fail("未启用自定义 Keystore（androidUseCustomKeystore=0）。");
                return;
            }

            if (string.IsNullOrEmpty(keystore) || string.IsNullOrEmpty(alias))
            {
                Fail($"签名信息不完整。keystoreName={keystore}, keyaliasName={alias}");
                return;
            }

            if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass)
                || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
            {
                Fail("keystore/keyalias 密码为空。请确认 Assets/Editor/GlobalConfig.cs 已注入密码。");
                return;
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string outDir = Path.Combine(projectRoot, DefaultOutputDir);
            Directory.CreateDirectory(outDir);

            string fileName =
                $"{PlayerSettings.productName}_v{PlayerSettings.bundleVersion}_{PlayerSettings.Android.bundleVersionCode}_{DateTime.Now:yyyyMMdd_HHmmss}.apk";
            string apkPath = Path.Combine(outDir, fileName);

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.CompressWithLz4HC,
            };

            Debug.Log(
                $"[AndroidReleaseApkBuilder] 开始打 Release APK\n" +
                $"  product={PlayerSettings.productName}\n" +
                $"  package={PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}\n" +
                $"  version={PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode})\n" +
                $"  keystore={keystore}\n" +
                $"  alias={alias}\n" +
                $"  jdk={AndroidExternalToolsSettings.jdkRootPath}\n" +
                $"  sdk={AndroidExternalToolsSettings.sdkRootPath}\n" +
                $"  ndk={AndroidExternalToolsSettings.ndkRootPath}\n" +
                $"  scenes={string.Join(", ", scenes)}\n" +
                $"  output={apkPath}");

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"打包失败：{summary.result}，errors={summary.totalErrors}，见 Editor.log / Builds 日志。");
                return;
            }

            Debug.Log($"[AndroidReleaseApkBuilder] 成功：{apkPath}  size={summary.totalSize} bytes");

            if (openExplorerOnSuccess && File.Exists(apkPath))
            {
                EditorUtility.RevealInFinder(apkPath);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        finally
        {
            RestoreStashedCmdlineTools();
        }
    }

    /// <summary>
    /// 2022.3.53f1c1 未安装自带 OpenJDK 时，Preferences 会指向空目录导致 JDK not found。
    /// 打包前自动落到本机可用的 JDK11 / SDK / NDK。
    /// </summary>
    static bool EnsureAndroidExternalTools()
    {
        string jdk = ResolveJdkRoot();
        if (string.IsNullOrEmpty(jdk))
        {
            Fail(
                "未找到可用 JDK11。请安装 Amazon Corretto 11 / Temurin 11，" +
                "或设置环境变量 JAVA_HOME / ANDROID_JDK 指向带 bin/javac.exe 的 JDK 根目录。");
            return false;
        }

        string sdk = ResolveSdkRoot();
        if (string.IsNullOrEmpty(sdk))
        {
            Fail(
                "未找到 Android SDK。请确认存在 %LOCALAPPDATA%\\Android\\Sdk，" +
                "或设置环境变量 ANDROID_HOME / ANDROID_SDK_ROOT。");
            return false;
        }

        string ndk = ResolveNdkRoot(sdk);

        try
        {
            AndroidExternalToolsSettings.jdkRootPath = jdk;
            AndroidExternalToolsSettings.sdkRootPath = sdk;
            if (!string.IsNullOrEmpty(ndk))
            {
                AndroidExternalToolsSettings.ndkRootPath = ndk;
            }
        }
        catch (Exception e)
        {
            Fail($"写入 AndroidExternalToolsSettings 失败：{e.Message}");
            return false;
        }

        // 同步 EditorPrefs，避免 Preferences UI / 后续进程仍读到旧的空 OpenJDK 路径
        EditorPrefs.SetBool("JdkUseEmbedded", false);
        EditorPrefs.SetString("JdkPath", jdk);
        EditorPrefs.SetString("Jdk11Path", jdk);
        EditorPrefs.SetBool("SdkUseEmbedded", false);
        EditorPrefs.SetString("AndroidSdkRoot", sdk);
        if (!string.IsNullOrEmpty(ndk))
        {
            EditorPrefs.SetBool("NdkUseEmbedded", false);
            EditorPrefs.SetString("AndroidNdkRootR23B", ndk.Replace('\\', '/'));
        }

        if (!IsValidUnityAndroidJdkRoot(jdk))
        {
            Fail($"JDK 无效（需要带 javac 的 JDK 11）：{jdk}");
            return false;
        }

        Debug.Log(
            $"[AndroidReleaseApkBuilder] External Tools 已校正\n" +
            $"  JDK={AndroidExternalToolsSettings.jdkRootPath}\n" +
            $"  SDK={AndroidExternalToolsSettings.sdkRootPath}\n" +
            $"  NDK={AndroidExternalToolsSettings.ndkRootPath}");
        return true;
    }

    static string ResolveJdkRoot()
    {
        // Unity 2022 Android 构建要求 JDK 11；优先已知 11，再扫环境变量（可能是 Android Studio 的 21）。
        string[] candidates =
        {
            @"C:\Program Files\Amazon Corretto\jdk11.0.29_7",
            @"C:\Program Files\Eclipse Adoptium\jdk-11.0.29.7-hotspot",
            @"C:\Program Files\Unity\Hub\Editor\2021.3.16f1c1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK",
            @"C:\Program Files\Unity\Hub\Editor\2022.3.53f1c1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK",
            Environment.GetEnvironmentVariable("ANDROID_JDK"),
            Environment.GetEnvironmentVariable("JAVA_HOME"),
            AndroidExternalToolsSettings.jdkRootPath,
        };

        foreach (string raw in candidates)
        {
            string path = NormalizeExistingDir(raw);
            if (IsValidUnityAndroidJdkRoot(path))
            {
                return path;
            }
        }

        string[] roots =
        {
            @"C:\Program Files\Amazon Corretto",
            @"C:\Program Files\Eclipse Adoptium",
            @"C:\Program Files\Java",
            @"C:\Program Files\Microsoft",
        };
        foreach (string root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (string dir in Directory.GetDirectories(root))
            {
                if (IsValidUnityAndroidJdkRoot(dir))
                {
                    return dir;
                }
            }
        }

        return null;
    }

    static string ResolveSdkRoot()
    {
        string[] candidates =
        {
            Environment.GetEnvironmentVariable("ANDROID_HOME"),
            Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
            AndroidExternalToolsSettings.sdkRootPath,
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android",
                "Sdk"),
        };

        foreach (string raw in candidates)
        {
            string path = NormalizeExistingDir(raw);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (Directory.Exists(Path.Combine(path, "platform-tools"))
                || Directory.Exists(Path.Combine(path, "platforms")))
            {
                return path;
            }
        }

        return null;
    }

    static string ResolveNdkRoot(string sdkRoot)
    {
        string current = NormalizeExistingDir(AndroidExternalToolsSettings.ndkRootPath);
        if (!string.IsNullOrEmpty(current) && Directory.Exists(current))
        {
            return current;
        }

        string preferred = Path.Combine(sdkRoot, "ndk", "23.1.7779620");
        if (Directory.Exists(preferred))
        {
            return preferred;
        }

        string ndkRoot = Path.Combine(sdkRoot, "ndk");
        if (!Directory.Exists(ndkRoot))
        {
            return null;
        }

        string[] versions = Directory.GetDirectories(ndkRoot);
        return versions.Length > 0 ? versions.OrderByDescending(v => v).First() : null;
    }

    /// <summary>Unity 2022 Android 需要 JDK 11（不能用 Android Studio 自带的 17/21）。</summary>
    static bool IsValidUnityAndroidJdkRoot(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return false;
        }

        bool hasJavac = File.Exists(Path.Combine(path, "bin", "javac.exe"))
                        || File.Exists(Path.Combine(path, "bin", "javac"));
        if (!hasJavac)
        {
            return false;
        }

        int major = TryReadJavaMajorVersion(path);
        return major == 11;
    }

    static int TryReadJavaMajorVersion(string jdkRoot)
    {
        string releaseFile = Path.Combine(jdkRoot, "release");
        if (File.Exists(releaseFile))
        {
            try
            {
                foreach (string line in File.ReadAllLines(releaseFile))
                {
                    // JAVA_VERSION="11.0.29" 或 "1.8.0_xxx"
                    if (!line.StartsWith("JAVA_VERSION=", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string ver = line.Substring("JAVA_VERSION=".Length).Trim().Trim('"');
                    return ParseJavaMajor(ver);
                }
            }
            catch
            {
                // ignore and fall through
            }
        }

        return -1;
    }

    static int ParseJavaMajor(string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            return -1;
        }

        // 1.8.0_xxx -> 8
        if (version.StartsWith("1.", StringComparison.Ordinal))
        {
            string[] legacy = version.Split('.');
            if (legacy.Length >= 2 && int.TryParse(legacy[1], out int legacyMajor))
            {
                return legacyMajor;
            }

            return -1;
        }

        int dot = version.IndexOf('.');
        string majorText = dot > 0 ? version.Substring(0, dot) : version;
        return int.TryParse(majorText, out int major) ? major : -1;
    }

    static string NormalizeExistingDir(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        path = path.Trim().Trim('"');
        try
        {
            path = Path.GetFullPath(path);
        }
        catch
        {
            return null;
        }

        return Directory.Exists(path) ? path : null;
    }

    /// <summary>
    /// 把 <see cref="CmdlineToolsTargetNames"/> 里的每个目录都临时换成兼容 JDK11 的
    /// cmdline-tools 2.1（取自本机 Unity 2021 附带 SDK）；原内容备份后，构建结束/失败都会
    /// 在 <see cref="RestoreStashedCmdlineTools"/> 还原成原样。
    /// </summary>
    static bool PrepareJdk11CompatibleCmdlineTools(string sdkRoot)
    {
        s_StashedCmdlineTools.Clear();

        if (string.IsNullOrEmpty(sdkRoot) || !Directory.Exists(sdkRoot))
        {
            Fail($"SDK 路径无效，无法准备 cmdline-tools：{sdkRoot}");
            return false;
        }

        if (!Directory.Exists(UnityCmdlineTools21)
            || !File.Exists(Path.Combine(UnityCmdlineTools21, "bin", "sdkmanager.bat")))
        {
            Fail(
                "缺少兼容 JDK11 的 cmdline-tools 2.1。\n" +
                $"期望来源：{UnityCmdlineTools21}\n" +
                "请保留 Unity 2021.3 的 Android SDK 模块，或手动安装旧版 cmdline-tools。");
            return false;
        }

        string cmdlineRoot = Path.Combine(sdkRoot, "cmdline-tools");
        Directory.CreateDirectory(cmdlineRoot);

        // 清理历史错误备份：曾经把备份留在 cmdline-tools 内，Unity 会误扫到。
        CleanupLegacyInlineCmdlineToolsBackups(cmdlineRoot);

        string bakRoot = Path.Combine(sdkRoot, CmdlineToolsBakRootName);
        Directory.CreateDirectory(bakRoot);

        foreach (string name in CmdlineToolsTargetNames)
        {
            string targetDir = Path.Combine(cmdlineRoot, name);
            string backupDir = Path.Combine(bakRoot, name);
            var stashed = new StashedCmdlineTools { DirPath = targetDir, BackupPath = backupDir };
            s_StashedCmdlineTools.Add(stashed);

            try
            {
                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, true);
                }

                if (Directory.Exists(targetDir))
                {
                    Directory.Move(targetDir, backupDir);
                    stashed.WasStashed = true;
                    Debug.Log($"[AndroidReleaseApkBuilder] 已备份原 cmdline-tools/{name} -> {backupDir}");
                }

                FileUtil.CopyFileOrDirectory(UnityCmdlineTools21, targetDir);
                Debug.Log($"[AndroidReleaseApkBuilder] 已用 JDK11 兼容工具覆盖 cmdline-tools/{name}（来自 {UnityCmdlineTools21}）");
            }
            catch (Exception e)
            {
                Fail($"准备 cmdline-tools/{name} 失败（请关闭 Android Studio / 资源管理器中打开该目录的窗口后重试）：{e.Message}");
                RestoreStashedCmdlineTools();
                return false;
            }
        }

        return true;
    }

    static void CleanupLegacyInlineCmdlineToolsBackups(string cmdlineRoot)
    {
        if (!Directory.Exists(cmdlineRoot))
        {
            return;
        }

        foreach (string dir in Directory.GetDirectories(cmdlineRoot, "*.__bak_jdk17"))
        {
            try
            {
                Directory.Delete(dir, true);
                Debug.Log($"[AndroidReleaseApkBuilder] 已删除遗留内联备份：{dir}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AndroidReleaseApkBuilder] 删除遗留备份失败：{dir}\n{e.Message}");
            }
        }
    }

    static void RestoreStashedCmdlineTools()
    {
        foreach (StashedCmdlineTools stashed in s_StashedCmdlineTools)
        {
            try
            {
                if (Directory.Exists(stashed.DirPath))
                {
                    Directory.Delete(stashed.DirPath, true);
                }

                if (stashed.WasStashed && Directory.Exists(stashed.BackupPath))
                {
                    Directory.Move(stashed.BackupPath, stashed.DirPath);
                    Debug.Log($"[AndroidReleaseApkBuilder] 已还原 cmdline-tools/{Path.GetFileName(stashed.DirPath)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[AndroidReleaseApkBuilder] 还原 cmdline-tools/{Path.GetFileName(stashed.DirPath)} 失败：{e.Message}\n" +
                    $"可手动删除 {stashed.DirPath}，再把 {stashed.BackupPath} 移回 {stashed.DirPath}。");
            }
        }

        s_StashedCmdlineTools.Clear();
    }

    static void ApplyReleaseSigningFromProject()
    {
        PlayerSettings.Android.useCustomKeystore = true;

        if (string.IsNullOrEmpty(PlayerSettings.Android.keyaliasName))
        {
            PlayerSettings.Android.keyaliasName = "wogemu";
        }

        PlayerSettings.Android.keystorePass = "Wogemu2022";
        PlayerSettings.Android.keyaliasPass = "Wogemu2022";
    }

    static void Fail(string message)
    {
        // 失败路径也尽量还原，避免 SDK 目录一直处于暂隐状态
        RestoreStashedCmdlineTools();

        Debug.LogError("[AndroidReleaseApkBuilder] " + message);
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

/// <summary>菜单打包前确认版本名和 Build 号。版本名默认 1.0.0，Build 号为上次 +1，都可以手改。</summary>
class AndroidReleaseVersionWindow : EditorWindow
{
    const string DefaultVersionName = "1.0.0";

    string m_VersionName;
    int m_BuildNumber;
    bool m_Confirmed;

    public static bool Prompt(out string versionName, out int buildNumber)
    {
        var window = CreateInstance<AndroidReleaseVersionWindow>();
        string current = PlayerSettings.bundleVersion;
        window.m_VersionName = string.IsNullOrWhiteSpace(current) ? DefaultVersionName : current.Trim();
        int next = PlayerSettings.Android.bundleVersionCode + 1;
        window.m_BuildNumber = next < 1 ? 1 : next;
        window.titleContent = new GUIContent("设置 APK 版本");
        window.minSize = new Vector2(420f, 180f);
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
        EditorGUILayout.HelpBox("版本号写入 APK 的 versionName，也可以手改。Build 号已按上次打包自动 +1。", MessageType.None);
        EditorGUILayout.Space(6f);
        m_VersionName = EditorGUILayout.TextField("版本号", m_VersionName);
        m_BuildNumber = EditorGUILayout.IntField("Build 号", m_BuildNumber);
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(
            "文件名",
            $"{PlayerSettings.productName}_v{m_VersionName}_{m_BuildNumber}_时间.apk");
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
