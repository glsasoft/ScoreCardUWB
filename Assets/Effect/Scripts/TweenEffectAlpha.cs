using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Base
{
    public class TweenEffectAlpha : MonoBehaviour
    {
        public GameObject target;
        public bool autoPlay = true;
        public bool restartOnEnable = false;
        public float duration = 1;
        public float delay = 0;
        public Ease easeType = Ease.OutQuad;
        public LoopType loopType = LoopType.Restart;
        [Tooltip("-1为无限循环")]
        public int loops = 1;
        public float startAlpha = 0;
        public float endAlpha = 1;
        /// <summary>
        /// 为了使 SRP Batcher 代码路径能够渲染对象：
        /// 1.渲染的对象必须是网格或蒙皮网格。该对象不能是粒子。
        /// 2.着色器必须与 SRP Batcher 兼容。HDRP 和 URP 中的所有光照和无光照着色器均符合此要求（这些着色器的“粒子”版本除外）。
        /// 3.渲染对象不得使用 MaterialPropertyBlocks。
        /// </summary>
        [Tooltip("使用 MaterialPropertyBlock，与 SRP Batcher 不兼容")]
        public bool useMPB = false;
        [Tooltip("使用 SharedMaterials")]
        public bool useSM = false;
        [NonSerialized]
        public Tween tween;

        int mainAlphaID = 0;
        Renderer[] renderers;
        List<float> originalAlphas = new List<float>();

        private void OnEnable()
        {
            if (tween != null)
            {
                if (restartOnEnable && tween.active) tween.Restart();
                return;
            }

            mainAlphaID = Shader.PropertyToID("_MainAlpha");
            if (renderers == null) renderers = GetTweenTarget().GetComponentsInChildren<Renderer>(true);

            MaterialPropertyBlock materialPropertyBlock = null;
            if (useMPB)
            {
                materialPropertyBlock = new MaterialPropertyBlock();
                foreach (Renderer r in renderers)
                {
                    r.GetPropertyBlock(materialPropertyBlock);
                    materialPropertyBlock.SetFloat(mainAlphaID, startAlpha);
                    r.SetPropertyBlock(materialPropertyBlock);
                }
            }
            else
            {
                Material[] materials = null;
                foreach (Renderer r in renderers)
                {
                    materials = useSM ? r.sharedMaterials : r.materials;
                    if (materials != null)
                    {
                        foreach (Material mat in materials)
                        {
                            if (mat.HasProperty(mainAlphaID))
                            {
                                originalAlphas.Add(mat.GetFloat(mainAlphaID));
                                mat.SetFloat(mainAlphaID, startAlpha);
                            }
                        }
                    }
                }
            }

            tween = DOTween.To(() => startAlpha, x =>
            {
                if (useMPB)
                {
                    foreach (Renderer r in renderers)
                    {
                        r.GetPropertyBlock(materialPropertyBlock);
                        materialPropertyBlock.SetFloat(mainAlphaID, x);
                        r.SetPropertyBlock(materialPropertyBlock);
                    }
                }
                else
                {
                    Material[] materials = null;
                    foreach (Renderer r in renderers)
                    {
                        materials = useSM ? r.sharedMaterials : r.materials;
                        if (materials != null)
                        {
                            foreach (Material mat in materials)
                            {
                                if (mat.HasProperty(mainAlphaID))
                                    mat.SetFloat(mainAlphaID, x);
                            }
                        }
                    }
                }
            }, endAlpha, duration);

            GameObject setTarget = GetTweenTarget();
            tween.SetTarget(setTarget).SetDelay(delay).SetLoops(loops, loopType).SetAutoKill(false).OnKill(() => tween = null);
            tween.SetEase(easeType);

            if (autoPlay || restartOnEnable)
                tween.Play();
            else
                tween.Pause();
        }

        public GameObject GetTweenTarget()
        {
            if (target)
                return target;
            else
                return gameObject;
        }

        private void OnDisable()
        {
            if (tween != null && tween.active) tween.Pause();
        }

        private void OnDestroy()
        {
            if (tween != null && tween.active) tween.Kill();
            tween = null;

            if (renderers == null) return;

            if (useMPB)
            {
                foreach (Renderer r in renderers)
                {
                    r.SetPropertyBlock(null);
                }
            }
            else if (useSM)
            {
                int index = 0;
                foreach (Renderer r in renderers)
                {
                    if (r.sharedMaterials != null)
                    {
                        foreach (Material mat in r.sharedMaterials)
                        {
                            if (mat.HasProperty(mainAlphaID))
                                mat.SetFloat(mainAlphaID, originalAlphas[index++]);
                        }
                    }
                }
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
#endif
            }
        }

        [ContextMenu("DORewind")]
        public void DORewind()
        {
            // DOTween.Rewind(GetTweenTarget());
            if (tween != null && tween.IsInitialized()) tween.Rewind();
        }

        [ContextMenu("DORestart")]
        public void DORestart()
        {
            DOTween.Restart(GetTweenTarget());
        }

        [ContextMenu("DOComplete")]
        public void DOComplete()
        {
            DOTween.Complete(GetTweenTarget());
        }
    }
}
