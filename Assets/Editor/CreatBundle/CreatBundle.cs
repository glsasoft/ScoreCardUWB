using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using YooAsset.Editor;

namespace LetsPiu
{
    /// <summary>
    /// 永久离线包模式打包入口。
    /// 流程：变体收集（菜单可选 / 自动强制）→ 打开 LoginScene → 强制 OfflinePlayMode → YooAsset 全量打包到 StreamingAssets。
    /// </summary>
    public static class CreatBundle
    {
        const string PACKAGE_NAME = "DefaultPackage";
        const string LOGIN_SCENE_PATH = "Assets/Package_Initial/Scene/LoginScene.unity";
        const string ShaderVariantSavePath =
            "Assets/Graphics/ShaderVariantsCollect/AllShaderVariants.shadervariants";
        const int ShaderVariantProcessCapacity = 1000;

        static Boot s_Boot;
        static Action<bool> s_AutoCompleted;
        static bool s_AutoMode;
        static bool s_RevealPackageFolder = true;

        [MenuItem("CreatBundle/离线全包（OfflinePlayMode）", priority = 100)]
        public static void CreateOfflineBundle_Menu()
        {
            if (s_AutoMode)
            {
                Debug.LogError("[CreatBundle] 自动离线打包进行中，请等待完成。");
                return;
            }

            EditorTools.ClearUnityConsole();

            bool collectVariants = EditorUtility.DisplayDialog(
                "Shader 变体收集",
                "是否在打包前先做 Shader 变体收集？\n\n建议第一次打包或新增了特效后选 Yes。",
                "Yes (推荐)",
                "No (跳过)");

            if (collectVariants)
            {
                CollectShaderVariants();
            }
            else
            {
                OpenLoginSceneAndBuild();
            }
        }

        /// <summary>
        /// 无弹窗：强制收集全部 Shader 变体 → OfflinePlayMode 离线全包。
        /// 供 Android / Windows Release 流水线调用。
        /// </summary>
        public static void CreateOfflineBundleAuto(
            Action<bool> onCompleted,
            bool revealPackageFolder = false)
        {
            if (s_AutoMode)
            {
                Debug.LogError("[CreatBundle] 已有自动离线打包在进行。");
                onCompleted?.Invoke(false);
                return;
            }

            s_AutoMode = true;
            s_AutoCompleted = onCompleted;
            s_RevealPackageFolder = revealPackageFolder;

            Debug.Log("[CreatBundle] 自动模式：收集全部 Shader 变体 → 离线全包");
            CollectShaderVariants();
        }

        static void CollectShaderVariants()
        {
            ShaderVariantCollector.Run(
                ShaderVariantSavePath,
                PACKAGE_NAME,
                ShaderVariantProcessCapacity,
                OnCollectCompleted);
        }

        static void OnCollectCompleted()
        {
            Debug.Log("[CreatBundle] 变体收集完成");
            // 变体收集后不要立刻 Refresh：损坏的 .meta / 域重载会清掉 delayCall，导致流水线卡死在 OfflineRunning。
            EditorApplication.delayCall += OpenLoginSceneAndBuild;
        }

        static void OpenLoginSceneAndBuild()
        {
            var current = EditorSceneManager.GetActiveScene();
            if (current.IsValid() && current.path == LOGIN_SCENE_PATH)
            {
                EditorApplication.delayCall += BuildOfflinePackage;
                return;
            }

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.OpenScene(LOGIN_SCENE_PATH, OpenSceneMode.Single);
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;

            if (!scene.path.EndsWith("LoginScene.unity"))
            {
                FailOrLog($"打开的场景不是 LoginScene: {scene.path}");
                return;
            }

            EditorApplication.delayCall += BuildOfflinePackage;
        }

        static void BuildOfflinePackage()
        {
            s_Boot = GameObject.Find("Boot")?.GetComponent<Boot>();
            if (s_Boot == null)
            {
                FailOrLog("LoginScene 中找不到 Boot 对象");
                return;
            }

            s_Boot.m_InitModel = EPlayMode.OfflinePlayMode;
            if (!SaveCurrentScene())
            {
                FailOrLog("无法保存 LoginScene");
                return;
            }

            if (!s_AutoMode)
            {
                if (!EditorUtility.DisplayDialog(
                        "提示",
                        $"开始构建离线全部资源包 [{PACKAGE_NAME}]！",
                        "Yes",
                        "No"))
                {
                    Debug.LogWarning("[CreatBundle] 打包已取消");
                    return;
                }
            }

            bool reveal = s_RevealPackageFolder;
            EditorApplication.delayCall += () =>
            {
                bool ok = new SelfCreatBundle().ExecuteBuild(
                    PACKAGE_NAME,
                    EBuildinFileCopyOption.ClearAndCopyAll,
                    null,
                    reveal);

                if (ok)
                {
                    Debug.Log("[CreatBundle] 离线全包构建完成");
                    FinishAuto(true);
                }
                else
                {
                    FailOrLog("离线全包构建失败，请查看 Console 中的 ErrorCode / 编译错误");
                }
            };
        }

        static bool SaveCurrentScene()
        {
            var currentScene = EditorSceneManager.GetActiveScene();
            if (EditorSceneManager.SaveScene(currentScene))
            {
                Debug.Log("[CreatBundle] 当前场景已保存到: " + currentScene.path);
                return true;
            }

            Debug.LogError("[CreatBundle] 无法保存当前场景");
            return false;
        }

        static void FailOrLog(string message)
        {
            Debug.LogError("[CreatBundle] " + message);
            if (s_AutoMode)
            {
                FinishAuto(false);
            }
        }

        static void FinishAuto(bool ok)
        {
            if (!s_AutoMode)
            {
                return;
            }

            var cb = s_AutoCompleted;
            s_AutoCompleted = null;
            s_AutoMode = false;
            s_RevealPackageFolder = true;
            cb?.Invoke(ok);
        }
    }

    public class SelfCreatBundle
    {
        string packageName;

        public bool ExecuteBuild(
            string packageName,
            EBuildinFileCopyOption copyType,
            string tag,
            bool revealInFinder = true)
        {
            this.packageName = packageName;
            var packageList = GetBuildPackageNames();
            if (packageList.Count == 0 || !packageList.Contains(packageName))
            {
                Debug.LogError($"[CreatBundle] 包名检测异常，PackageName:{packageName} 不存在");
                return false;
            }

            Debug.Log(
                $"[CreatBundle] BuildBundleStart! PackageName:{packageName}, copyType:{copyType}, tag:{tag}, target={EditorUserBuildSettings.activeBuildTarget}");

            var buildParameters = new BuiltinBuildParameters
            {
                BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),
                BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                BuildPipeline = EBuildPipeline.BuiltinBuildPipeline.ToString(),
                BuildBundleType = (int)EBuildBundleType.AssetBundle,
                BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                PackageName = packageName,
                PackageVersion = GetPackageVersion(),
                EnableSharePackRule = true,
                VerifyBuildingResult = true,
                FileNameStyle = EFileNameStyle.HashName,
                BuildinFileCopyOption = copyType,
                BuildinFileCopyParams = tag,
                CompressOption = ECompressOption.LZ4,
                ClearBuildCacheFiles = true,
                UseAssetDependencyDB = true,
                EncryptionServices = CreateEncryptionInstance(),
            };

            var pipeline = new BuiltinBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                if (revealInFinder)
                {
                    EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
                }

                return true;
            }

            return false;
        }

        System.Collections.Generic.List<string> GetBuildPackageNames()
        {
            var result = new System.Collections.Generic.List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }

            return result;
        }

        IEncryptionServices CreateEncryptionInstance()
        {
            var encyptionClassName = AssetBundleBuilderSetting.GetPackageEncyptionClassName(
                packageName,
                EBuildPipeline.BuiltinBuildPipeline);
            var encryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = encryptionClassTypes.Find(x => x.FullName.Equals(encyptionClassName));
            return classType != null
                ? (IEncryptionServices)Activator.CreateInstance(classType)
                : null;
        }

        string GetPackageVersion()
        {
            var date = DateTime.Now;
            return $"{date.Year}_{date.Month:D2}_{date.Day:D2}_{date.Hour:D2}{date.Minute:D2}_{date.Second:D2}";
        }
    }
}
