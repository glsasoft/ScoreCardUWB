#if UNITY_EDITOR
using System.Collections.Generic;
using GLSAQuadView;
using LetsPiu.Graphics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LetsPiu.Graphics.EditorTools
{
    /// <summary>
    /// 编辑器里的角色材质预览。
    ///
    /// 打开后，编辑模式也走 RuntimeMaterialInstancer 的副本，拖 Timeline 能实时看到
    /// isDead 切换、dissolve 溶解、hitFlash 闪白和描边/透视参数。
    ///
    /// 安全边界：
    ///   - 副本是 HideAndDontSave，不会进 Project、不进撤销栈；
    ///   - 保存场景前（sceneSaving）把所有 renderer 的 sharedMaterials 还原成资源引用；
    ///   - 保存后 / 场景打开后 / 退出 Play 后再挂回去；
    ///   - 进入 Play 前还原，Play 里由 Instancer 自己接管。
    ///
    /// 开关存在 EditorPrefs 里，不写进场景，不进构建。
    /// </summary>
    [InitializeOnLoad]
    public static class CharacterMaterialPreview
    {
        const string MenuPath = "Tools/GLSA/角色材质/编辑器预览";
        const string PrefKey = "GLSA.CharacterMaterialPreview";

        static CharacterMaterialPreview()
        {
            EditorApplication.delayCall += RestoreAndApply;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        // ---------------------------------------------------------------- 菜单

        [MenuItem(MenuPath, false, 20)]
        static void Toggle()
        {
            bool next = !EditorPrefs.GetBool(PrefKey, false);
            EditorPrefs.SetBool(PrefKey, next);
            MaterialPreviewState.EditorPreviewEnabled = next;

            if (next)
            {
                EnsureInstancers();
            }

            ApplyToAll();
            SceneView.RepaintAll();

            Debug.Log(next
                ? "[角色材质预览] 已开启。拖 Timeline 能看到死亡/溶解变化；副本是 HideAndDontSave，保存场景前会自动还原。"
                : "[角色材质预览] 已关闭。renderer 的材质引用已还原成 .mat 资源。");
        }

        [MenuItem(MenuPath, true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PrefKey, false));
            return true;
        }

        // ---------------------------------------------------------------- 生命周期

        static void RestoreAndApply()
        {
            MaterialPreviewState.EditorPreviewEnabled = EditorPrefs.GetBool(PrefKey, false);

            if (MaterialPreviewState.EditorPreviewEnabled)
            {
                EnsureInstancers();
            }

            ApplyToAll();
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            if (!MaterialPreviewState.EditorPreviewEnabled)
            {
                return;
            }

            foreach (RuntimeMaterialInstancer inst in AllInstancers())
            {
                if (inst.gameObject.scene != scene)
                {
                    continue;
                }
                inst.SuspendForSceneSave();
            }
        }

        static void OnSceneSaved(Scene scene)
        {
            if (!MaterialPreviewState.EditorPreviewEnabled)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (!MaterialPreviewState.EditorPreviewEnabled)
                {
                    return;
                }

                foreach (RuntimeMaterialInstancer inst in AllInstancers())
                {
                    if (inst != null && inst.gameObject.scene == scene)
                    {
                        inst.ResumePreview();
                    }
                }
                RefreshCharacters();
            };
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            EditorApplication.delayCall += RestoreAndApply;
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    // 进 Play 前先把编辑器副本收掉，Play 里由 Instancer 自己 Apply。
                    SuspendAll();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    // 退出 Play 后按开关状态恢复预览。
                    EditorApplication.delayCall += RestoreAndApply;
                    break;
            }
        }

        // ---------------------------------------------------------------- 执行

        static void SuspendAll()
        {
            foreach (RuntimeMaterialInstancer inst in AllInstancers())
            {
                inst.SuspendForSceneSave();
            }
        }

        static void ApplyToAll()
        {
            bool on = MaterialPreviewState.Active;

            foreach (RuntimeMaterialInstancer inst in AllInstancers())
            {
                if (on)
                {
                    inst.ResumePreview();
                }
                else
                {
                    inst.SuspendForSceneSave();
                }
            }

            if (on)
            {
                RefreshCharacters();
            }
        }

        /// <summary>
        /// DeadSkinSwitcher 是按字段变化才切的，开关刚打开时 isDead 没变过，
        /// 得强制刷一次才不会停在「资源材质」上。
        /// </summary>
        static void RefreshCharacters()
        {
            foreach (DeadSkinSwitcher d in AllOf<DeadSkinSwitcher>())
            {
                d.ForceRefresh();
            }

            foreach (RoleMaterialController c in AllOf<RoleMaterialController>())
            {
                c.RefreshMaterialList();
            }
        }

        /// <summary>预览开启时，给所有挂着角色材质脚本的对象补齐 Instancer，省得手动加。</summary>
        static void EnsureInstancers()
        {
            foreach (DeadSkinSwitcher d in AllOf<DeadSkinSwitcher>())
            {
                if (d.GetComponent<RuntimeMaterialInstancer>() == null)
                {
                    d.gameObject.AddComponent<RuntimeMaterialInstancer>();
                }
            }

            foreach (RoleMaterialController c in AllOf<RoleMaterialController>())
            {
                if (c.GetComponent<RuntimeMaterialInstancer>() == null)
                {
                    c.gameObject.AddComponent<RuntimeMaterialInstancer>();
                }
            }
        }

        static List<RuntimeMaterialInstancer> AllInstancers()
        {
            return AllOf<RuntimeMaterialInstancer>();
        }

        static List<T> AllOf<T>() where T : Component
        {
            List<T> result = new List<T>();
            T[] all = Resources.FindObjectsOfTypeAll<T>();

            for (int i = 0; i < all.Length; i++)
            {
                T item = all[i];
                if (item == null)
                {
                    continue;
                }

                GameObject go = item.gameObject;
                // 排除 Project 面板里的预制体/资产，只保留场景里的实例
                if (!go.scene.IsValid() || EditorUtility.IsPersistent(item))
                {
                    continue;
                }

                result.Add(item);
            }

            return result;
        }
    }
}
#endif
