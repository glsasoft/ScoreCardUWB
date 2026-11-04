using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LetsPiu.Graphics
{
    public class AnimationSetter : EditorWindowWithAssetSelector
    {
        private Vector2 scrollPosition;
        private bool checkHuman = false;
        private bool checkLoop = false;
        
        private GameObject avatarSourcePrefab;
        
        private bool specialCheck;
        
        [MenuItem("Tools/Graphics/资产选择器_动画设置器")]
        private static void OpenWindow()
        {
            GetWindow<AnimationSetter>("动画设置器").Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height));
            
            DrawAssetsGUI();
            if (GUILayout.Button("打开资产选择器")) AssetSelector.OpenWindow();
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            
            
            EditorGUILayout.LabelField("1. 资产设置",EditorStyles.boldLabel);
            
            if (GUILayout.Button("更新资产名称（anim_ -> AC_）"))
            {
                if (EditorUtility.DisplayDialog("更新资产名称", "你确定要更新列表中所有资产名名吗？该操作无法被撤销", "确认", "取消"))
                {
                    string Capitalize(string s) => s[..1].ToUpper() + s[1..];
                    
                    for (int i = 0; i < assets.Count; i++)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(assets[i]);
                        EditorUtility.DisplayProgressBar("更新资产名称中...",
                            $"{assetPath} ({i}/{assets.Count})", (float)i / assets.Count);
                        
                        Match match = Regex.Match(assetPath, "anim_(.+)_tps_(.+)$");
                        string characterName = Capitalize(match.Groups[1].Value);
                        string[] animationName = match.Groups[2].Value.Split('_');
                        for (int j = 0; j < animationName.Length; j++) animationName[j] = Capitalize(animationName[j]);
                        AssetDatabase.RenameAsset(assetPath, $"AC_{characterName}_TPS_{string.Concat(animationName)}");
                    }
                    
                    AssetDatabase.SaveAssets();
                    EditorUtility.ClearProgressBar();
                }
            }
            
            if (GUILayout.Button("设置 动画名称"))
            {
                if (EditorUtility.DisplayDialog("设置动画名称", "你确定要将列表中所有FBX中的动画名称设置为FBX文件名吗？该操作无法被撤销", "确认", "取消"))
                {
                    for (int i = 0; i < assets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("设置动画名称中...",
                            $"{AssetDatabase.GetAssetPath(assets[i])} ({i}/{assets.Count})", (float)i / assets.Count);
                        SetAnimationName(assets[i]);
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
            
            EditorGUILayout.Space();
            avatarSourcePrefab = (GameObject)EditorGUILayout.ObjectField("Avatar Prefab", avatarSourcePrefab, typeof(GameObject), false);
            if (GUILayout.Button("设置 动画Rig属性"))
            {
                if (EditorUtility.DisplayDialog("设置 动画Rig属性", "Animation Type：Humanoid；\n" +
                                                              "Anim Definition：Copy From Other Avatar；\n" +
                                                              $"Source：{avatarSourcePrefab.name}。\n" +
                                                              "该操作无法被撤销,是否确定？", "确认", "取消"))
                {
                    for (int i = 0; i < assets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("设置动画名称中...",
                            $"{AssetDatabase.GetAssetPath(assets[i])} ({i}/{assets.Count})", (float)i / assets.Count);
                        SetAnimationRigToHumanoid(assets[i]);
                    }
                    EditorUtility.ClearProgressBar();
                }
            }
            EditorGUILayout.Space();
            
            if (GUILayout.Button("设置 动画Root为Original"))
            {
                if (EditorUtility.DisplayDialog("设置动画原点为 Original", "你确定要将列表中所有FBX中的动画原点属性均设置为 Original 吗？该操作无法被撤销", "确认", "取消"))
                {
                    for (int i = 0; i < assets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("设置动画名称中...",
                            $"{AssetDatabase.GetAssetPath(assets[i])} ({i}/{assets.Count})", (float)i / assets.Count);
                        SetAnimationOrigin(assets[i]);
                    }
                    EditorUtility.ClearProgressBar();
                }
            }

            if (GUILayout.Button("设置 动画LoopTime为True"))
            {
                if (EditorUtility.DisplayDialog("设置动画 LoopTime 为 True", "你确定要将列表中所有动画的LoopTime属性设置为True吗？（该操作无法被撤销）",
                        "确认", "取消"))
                {
                    for (int i = 0; i < assets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("设置动画LoopTime中...",
                            $"{AssetDatabase.GetAssetPath(assets[i])} ({i}/{assets.Count})", (float)i / assets.Count);
                        SetAnimationLoopTime(assets[i]);
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            specialCheck = EditorGUILayout.BeginFoldoutHeaderGroup(specialCheck, "2. 特殊检查");
            if (specialCheck)
            {
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 300f; // 调整行宽
                checkHuman = EditorGUILayout.Toggle($"检查人形骨骼({checkHuman})", checkHuman);
                checkLoop = EditorGUILayout.Toggle($"尝试检查Loop Match({checkLoop},比较算法待优化)", checkLoop);
                EditorGUIUtility.labelWidth = originalLabelWidth; // 恢复默认
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }
        
        private void SetAnimationName(Object asset)
        {
            ModelImporter importer =
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as ModelImporter;
            if (!importer) return;

            ModelImporterClipAnimation[] animations = importer.clipAnimations;
            if (animations.Length == 0) animations = importer.defaultClipAnimations;

            switch (animations.Length)
            {
                case 0: return;
                case 1:
                {
                    animations[0].name = asset.name;
                    break;
                }
                default:
                {
                    for (int i = 0; i < animations.Length; i++)
                        animations[i].name = $"{asset.name}_{i + 1}";
                    break;
                }
            }

            importer.clipAnimations = animations;
            importer.SaveAndReimport();
        }
        
        private void SetAnimationRigToHumanoid(Object asset)
        {
            if (avatarSourcePrefab == null)
            {
                Debug.LogWarning("未指定 Avatar Prefab。跳过该资源处理。");
                return;
            }

            Animator sourceAnimator = avatarSourcePrefab.GetComponent<Animator>();
            if (sourceAnimator == null || sourceAnimator.avatar == null)
            {
                Debug.LogWarning($"Prefab {avatarSourcePrefab.name} 上未找到 Avatar。");
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path)) return;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
            importer.sourceAvatar = sourceAnimator.avatar;

            importer.SaveAndReimport();
        }

        private void SetAnimationOrigin(Object asset)
        {
            ModelImporter importer =
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as ModelImporter;
            if (!importer) return;

            ModelImporterClipAnimation[] animations = importer.clipAnimations;
            if (animations.Length == 0) animations = importer.defaultClipAnimations;

            foreach (ModelImporterClipAnimation animation in animations)
            {
                animation.keepOriginalOrientation = true;
                animation.keepOriginalPositionY = true;
                animation.keepOriginalPositionXZ = true;
            }

            importer.clipAnimations = animations;
            importer.SaveAndReimport();
        }
        
        private void SetAnimationLoopTime(Object asset)
        {
            ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as ModelImporter;
            if (!importer) return;
            
            ModelImporterClipAnimation[] animations = importer.clipAnimations;
            if(animations.Length == 0) animations = importer.defaultClipAnimations;

            foreach (ModelImporterClipAnimation animation in animations)
            {
                animation.loopTime = true;
            }
            
            importer.clipAnimations = animations;
            importer.SaveAndReimport();
        }
        
        
        
        protected override bool ValidateAsset(Object asset, out string error)
        {
            if (!base.ValidateAsset(asset, out error)) return false;
            
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) is not ModelImporter importer)
            {
                error = "资产类型需要为模型资产";
                return false;
            }
            
            ModelImporterClipAnimation[] animations = importer.clipAnimations;
            if (animations.Length == 0) animations = importer.defaultClipAnimations;
            if (animations.Length != 1)
            {
                error = "模型资产包含的动画数量为零或包含多个动画";
                return false;
            }
            
            if (checkHuman)
            {
                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    error = "模型资产不是人";
                    return false;
                }
            }

            if (checkLoop)
            {
                if (!IsAnimationLoopable(importer))
                {
                    error = "模型不可循环";
                    return false;
                }
            }
            
            error = null;
            return true;
        }
        
        private bool IsAnimationLoopable(ModelImporter importer)
        {
            string path = importer.assetPath;
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            // 找出动画剪辑
            AnimationClip clip = null;
            foreach (var asset in subAssets)
            {
                // 避免Unity自己生成的临时文件
                if (asset is AnimationClip ac && !ac.name.StartsWith("__preview__"))
                {
                    clip = ac;
                    break;
                }
            }
            
            // 通过 importer.sourceAvatar 获取 Avatar 引用
            Avatar avatar = importer.sourceAvatar;
            if (avatar == null || !avatar.isHuman) return false;

            // 使用 Avatar 所在的 Prefab
            string avatarPath = AssetDatabase.GetAssetPath(avatar);
            GameObject avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(avatarPath);
            if (avatarPrefab == null) return false;

            GameObject instance = GameObject.Instantiate(avatarPrefab);
            Animator animator = instance.GetComponent<Animator>();
            if (animator == null || !animator.isHuman)
            {
                GameObject.DestroyImmediate(instance);
                return false;
            }

            // 强制使用提供的 Avatar
            animator.avatar = avatar;

            clip.SampleAnimation(instance, 0f); // 第一帧
            Vector3 handStart = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 footStart = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;

            clip.SampleAnimation(instance, clip.length); // 最后一帧
            Vector3 handEnd = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 footEnd = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;

            GameObject.DestroyImmediate(instance);

            float handDiff = Vector3.Distance(handStart, handEnd);
            float footDiff = Vector3.Distance(footStart, footEnd);

            return handDiff < 0.01f && footDiff < 0.01f;
        }

    }
}