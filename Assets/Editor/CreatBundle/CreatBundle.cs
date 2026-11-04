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
    /// 已移除 HybridCLR 相关步骤（CopyPreLoadDll / CopyUpdateDll / PrebuildCommand.GenerateAll）。
    /// 流程：变体收集（可选） → 打开 LoginScene → 强制 OfflinePlayMode → YooAsset 全量打包到 StreamingAssets。
    /// </summary>
    public static class CreatBundle
    {
        const string PACKAGE_NAME = "DefaultPackage";
        const string LOGIN_SCENE_PATH = "Assets/Package_Initial/Scene/LoginScene.unity";

        static Boot s_Boot;

        [MenuItem("CreatBundle/离线全包（OfflinePlayMode）", priority = 100)]
        public static void CreateOfflineBundle_Menu()
        {
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

        static void CollectShaderVariants()
        {
            const string savePath = "Assets/Graphics/ShaderVariantsCollect/AllShaderVariants.shadervariants";
            const int processCapacity = 1000;
            ShaderVariantCollector.Run(savePath, PACKAGE_NAME, processCapacity, OnCollectCompleted);
        }

        static void OnCollectCompleted()
        {
            Debug.Log("[CreatBundle] 变体收集完成");
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.OpenScene(LOGIN_SCENE_PATH, OpenSceneMode.Single);
            AssetDatabase.Refresh();
        }

        static void OpenLoginSceneAndBuild()
        {
            var current = EditorSceneManager.GetActiveScene();
            if (current.path == LOGIN_SCENE_PATH)
            {
                OnSceneOpened(current, OpenSceneMode.Single);
            }
            else
            {
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorSceneManager.OpenScene(LOGIN_SCENE_PATH, OpenSceneMode.Single);
            }
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;

            if (!scene.path.EndsWith("LoginScene.unity"))
            {
                Debug.LogError($"[CreatBundle] 打开的场景不是 LoginScene: {scene.path}");
                return;
            }

            EditorApplication.delayCall += BuildOfflinePackage;
        }

        static void BuildOfflinePackage()
        {
            s_Boot = GameObject.Find("Boot")?.GetComponent<Boot>();
            if (s_Boot == null)
            {
                Debug.LogError("[CreatBundle] LoginScene 中找不到 Boot 对象");
                return;
            }

            // 永久离线，强制 OfflinePlayMode
            s_Boot.m_InitModel = EPlayMode.OfflinePlayMode;
            SaveCurrentScene();

            if (!EditorUtility.DisplayDialog("提示", $"开始构建离线全部资源包 [{PACKAGE_NAME}]！", "Yes", "No"))
            {
                Debug.LogWarning("[CreatBundle] 打包已取消");
                return;
            }

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.Refresh();
                new SelfCreatBundle().ExecuteBuild(PACKAGE_NAME, EBuildinFileCopyOption.ClearAndCopyAll, null);
                Debug.Log("[CreatBundle] 离线全包构建完成");
            };
        }

        static void SaveCurrentScene()
        {
            var currentScene = EditorSceneManager.GetActiveScene();
            if (EditorSceneManager.SaveScene(currentScene))
            {
                Debug.Log("[CreatBundle] 当前场景已保存到: " + currentScene.path);
            }
            else
            {
                Debug.LogError("[CreatBundle] 无法保存当前场景");
            }
        }
    }

    public class SelfCreatBundle
    {
        string packageName;

        public void ExecuteBuild(string packageName, EBuildinFileCopyOption copyType, string tag)
        {
            this.packageName = packageName;
            var packageList = GetBuildPackageNames();
            if (packageList.Count == 0 || !packageList.Contains(packageName))
            {
                Debug.LogError($"[CreatBundle] 包名检测异常，PackageName:{packageName} 不存在");
                return;
            }

            Debug.Log($"[CreatBundle] BuildBundleStart! PackageName:{packageName}, copyType:{copyType}, tag:{tag}");

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
                EditorUtility.RevealInFinder(buildResult.OutputPackageDirectory);
            }
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
            var encyptionClassName = AssetBundleBuilderSetting.GetPackageEncyptionClassName(packageName, EBuildPipeline.BuiltinBuildPipeline);
            var encryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = encryptionClassTypes.Find(x => x.FullName.Equals(encyptionClassName));
            return classType != null ? (IEncryptionServices)Activator.CreateInstance(classType) : null;
        }

        string GetPackageVersion()
        {
            var date = DateTime.Now;
            return $"{date.Year}_{date.Month:D2}_{date.Day:D2}_{date.Hour:D2}{date.Minute:D2}_{date.Second:D2}";
        }
    }
}
