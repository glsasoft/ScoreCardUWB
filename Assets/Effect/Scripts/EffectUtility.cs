using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu.Base
{
    public static class EffectUtility
    {
        /// <summary>
        /// 设置特效 Renderer 的 SortingOrder。
        /// <paramref name="increase"/> 为 true:递增，false:递减，null:相等
        /// </summary>
        /// <param name="effectObj"></param>
        /// <param name="baseOrder"></param>
        /// <param name="renderers"></param>
        /// <param name="increase">true:递增，false:递减，null:相等</param>
        /// <returns></returns>
        public static int SetEffectOrder(GameObject effectObj, int baseOrder, List<Renderer> renderers = null, bool? increase = true)
        {
            if (effectObj == null) return baseOrder;

            renderers = renderers ?? new List<Renderer>();
            effectObj.GetComponentsInChildren<Renderer>(true, renderers);

            return SetEffectOrderByRendererList(baseOrder, renderers, increase);
        }

        /// <summary>
        /// 设置特效 Renderer 的 SortingOrder。
        /// <paramref name="increase"/> 为 true:递增，false:递减，null:相等
        /// </summary>
        /// <param name="baseOrder"></param>
        /// <param name="renderers"></param>
        /// <param name="increase">true:递增，false:递减，null:相等</param>
        /// <returns></returns>
        public static int SetEffectOrderByRendererList(int baseOrder, List<Renderer> renderers, bool? increase = true)
        {
            if (renderers == null || renderers.Count == 0) return baseOrder;

            if (increase != null)
            {
                if (increase.Value)
                    renderers.Sort((a, b) => a.sortingOrder.CompareTo(b.sortingOrder));
                else
                    renderers.Sort((a, b) => b.sortingOrder.CompareTo(a.sortingOrder));
            }

            int? lastOrder = null;
            foreach (var r in renderers)
            {
                if (/*r.gameObject.activeSelf && r.enabled*/ true)
                {
                    if (increase != null && r.sortingOrder != lastOrder)
                    {
                        lastOrder = r.sortingOrder;
                        if (increase.Value)
                            baseOrder++;
                        else
                            baseOrder--;
                    }
                    r.sortingOrder = baseOrder;
                }
            }
            return baseOrder;
        }


        static readonly int m_ClipRectId = Shader.PropertyToID("_ClipRect");
        static readonly Vector3[] m_Corners = new Vector3[4];
        static MaterialPropertyBlock m_MPB;
        public static void SetEffectClipRect(GameObject effectObj, RectTransform clipRectTransform, List<Renderer> renderers = null)
        {
            if (effectObj == null) return;

            renderers = renderers ?? new List<Renderer>();
            effectObj.GetComponentsInChildren<Renderer>(true, renderers);

            if (clipRectTransform == null)
            {
                foreach (var r in renderers)
                {
                    r.material.DisableKeyword("_CLIPRECT_WORLDSPACE");
                }
                return;
            }

            clipRectTransform.GetWorldCorners(m_Corners);
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < 4; i++)
            {
                min = Vector2.Min(min, m_Corners[i]);
                max = Vector2.Max(max, m_Corners[i]);
            }

            Vector4 clipRect = new Vector4(min.x, min.y, max.x, max.y);
            if (m_MPB == null) m_MPB = new MaterialPropertyBlock();

            foreach (var r in renderers)
            {
                r.material.EnableKeyword("_CLIPRECT_WORLDSPACE");

                r.GetPropertyBlock(m_MPB);
                m_MPB.SetVector(m_ClipRectId, clipRect);
                r.SetPropertyBlock(m_MPB);
            }
        }
    }
}
