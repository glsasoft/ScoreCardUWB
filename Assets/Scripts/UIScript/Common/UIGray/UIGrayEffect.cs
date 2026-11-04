using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using YooAsset;

namespace LetsPiu.UI
{
    public class UIGrayEffect : MonoBehaviour
    {
        const string CircleRawImageShaderName = "UI/CircleRawImage";
        static readonly int s_DesaturateId = Shader.PropertyToID("_Desaturate");
        static readonly int s_GrayAmountId = Shader.PropertyToID("_GrayAmount");

        [Tooltip("包含的外部Object")]
        public GameObject[] m_ExternalObjects;

        ResourcePackage m_DefaultPackage;
        Material s_ImgGrayMat;
        Material m_CircleGrayMat;

        Dictionary<int, Material> m_ImgMatMap = new();
        Dictionary<int, Color> m_TxtColorMap = new();
        Dictionary<int, Dictionary<string, Color>> m_RendererColorMap = new();
        Dictionary<int, Dictionary<string, float>> m_RendererFloatMap = new();

        readonly List<Transform> m_ExcludeRoots = new();
        readonly List<Transform> m_KeepColorRoots = new();

        List<TMP_Text> m_TmpList;
        List<Graphic> m_GraphicList;
        List<Renderer> m_RendererList;

        MaterialPropertyBlock m_MPB;

        private void Awake()
        {
            if (m_MPB == null)
                m_MPB = new MaterialPropertyBlock();

            TryGetDefaultColorAndImage();
            TryGetRenderers();
        }

        public void SetExclude(params Transform[] transforms)
        {
            m_ExcludeRoots.Clear();
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                    m_ExcludeRoots.Add(transforms[i]);
            }
        }

        // 指定节点（含子节点）始终保持彩色。组件的 Awake 会提前缓存列表，所以这里同时把已缓存的条目剔除并还原。
        public void SetKeepColor(params Transform[] transforms)
        {
            m_KeepColorRoots.Clear();
            if (transforms == null) return;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null)
                    m_KeepColorRoots.Add(transforms[i]);
            }

            if (m_KeepColorRoots.Count == 0) return;

            if (m_TmpList != null)
            {
                for (int i = m_TmpList.Count - 1; i >= 0; i--)
                {
                    var tmpText = m_TmpList[i];
                    if (!IsUnderRoots(tmpText, m_KeepColorRoots)) continue;
                    if (tmpText != null && m_TxtColorMap.TryGetValue(tmpText.GetInstanceID(), out var color))
                        tmpText.color = color;
                    m_TmpList.RemoveAt(i);
                }
            }

            if (m_GraphicList != null)
            {
                for (int i = m_GraphicList.Count - 1; i >= 0; i--)
                {
                    var graphic = m_GraphicList[i];
                    if (!IsUnderRoots(graphic, m_KeepColorRoots)) continue;
                    if (graphic != null && m_ImgMatMap.TryGetValue(graphic.GetInstanceID(), out var mat))
                        graphic.material = mat;
                    m_GraphicList.RemoveAt(i);
                }
            }

            m_RendererList?.RemoveAll(r => IsUnderRoots(r, m_KeepColorRoots));
        }

        bool IsExcluded(UnityEngine.Component component)
        {
            return IsUnderRoots(component, m_ExcludeRoots) || IsUnderRoots(component, m_KeepColorRoots);
        }

        static bool IsUnderRoots(UnityEngine.Component component, List<Transform> roots)
        {
            if (component == null || roots.Count == 0) return false;

            Transform t = component.transform;
            for (int i = 0; i < roots.Count; i++)
            {
                Transform ex = roots[i];
                if (ex == null) continue;
                if (t == ex || t.IsChildOf(ex)) return true;
            }

            return false;
        }

        /// <summary>
        /// 注意：此脚本的使用场景，主要是用于UI的灰色效果处理。
        /// 注意：此脚本的根节点下不可有资源的生成与销毁，否则效果不做保证。
        /// </summary>
        /// <param name="isGray"></param>
        /// <returns></returns>
        public async UniTask<bool> SetUIGray(bool isGray)
        {
            if (m_DefaultPackage == null)
            {
                m_DefaultPackage = YooAssets.GetPackage("DefaultPackage");
            }

            if (s_ImgGrayMat == null)
            {
                await LoadImgGrayMat();
                if (s_ImgGrayMat == null)
                {
                    return false;
                }
            }

            TryGetDefaultColorAndImage();

            if (m_TmpList is not null)
            {
                for (int i = 0; i < m_TmpList.Count; i++)
                {
                    var tmpText = m_TmpList[i];
                    tmpText.color = isGray ? Color.gray : m_TxtColorMap[tmpText.GetInstanceID()];
                }
            }

            if (m_GraphicList is not null)
            {
                for (int i = 0; i < m_GraphicList.Count; i++)
                {
                    var graphic = m_GraphicList[i];
                    int id = graphic.GetInstanceID();
                    graphic.material = isGray
                        ? ResolveGrayMaterial(id)
                        : m_ImgMatMap[id];
                }
            }

            ApplyRendererGray(isGray);

            return true;
        }

        async UniTask LoadImgGrayMat()
        {
            var handle = m_DefaultPackage.LoadAssetAsync<Material>("MatImageGray");
            await handle.ToUniTask();
            if (handle.Status == EOperationStatus.Succeed)
            {
                s_ImgGrayMat = handle.AssetObject as Material;
            }
            handle.AttachToGameObject(this.gameObject);

            if (m_CircleGrayMat == null)
            {
                var circleHandle = m_DefaultPackage.LoadAssetAsync<Material>("CircleRawImage_Gray");
                await circleHandle.ToUniTask();
                if (circleHandle.Status == EOperationStatus.Succeed)
                    m_CircleGrayMat = circleHandle.AssetObject as Material;
                circleHandle.AttachToGameObject(this.gameObject);
            }
        }

        Material ResolveGrayMaterial(int graphicId)
        {
            if (m_ImgMatMap.TryGetValue(graphicId, out var original)
                && original != null
                && original.shader != null
                && original.shader.name == CircleRawImageShaderName)
            {
                if (m_CircleGrayMat == null)
                {
                    m_CircleGrayMat = new Material(original);
                    m_CircleGrayMat.name = "CircleRawImage_Gray";
                    if (m_CircleGrayMat.HasProperty(s_GrayAmountId))
                        m_CircleGrayMat.SetFloat(s_GrayAmountId, 1f);
                }
                return m_CircleGrayMat;
            }

            return s_ImgGrayMat;
        }

        void TryGetDefaultColorAndImage()
        {
            if (m_TxtColorMap.Count == 0)
            {
                List<TMP_Text> tmpList = new List<TMP_Text>();
                GetComponentsInChildren<TMP_Text>(true, tmpList);
                var filteredTmp = new List<TMP_Text>();
                for (int i = 0; i < tmpList.Count; i++)
                {
                    if (IsExcluded(tmpList[i])) continue;
                    filteredTmp.Add(tmpList[i]);
                    m_TxtColorMap.Add(tmpList[i].GetInstanceID(), tmpList[i].color);
                }

                m_TmpList = filteredTmp;

                if (m_ExternalObjects != null && m_ExternalObjects.Length > 0)
                {
                    HashSet<TMP_Text> tmpHashSet = new HashSet<TMP_Text>();
                    foreach(var obj in m_ExternalObjects)
                    {
                        if (obj == null || obj.transform.IsChildOf(transform)) continue;
                        obj.GetComponentsInChildren<TMP_Text>(true, tmpList);
                        for (int i = 0; i < tmpList.Count; i++)
                        {
                            if (IsExcluded(tmpList[i])) continue;
                            tmpHashSet.Add(tmpList[i]);
                            if (!m_TxtColorMap.ContainsKey(tmpList[i].GetInstanceID()))
                                m_TxtColorMap.Add(tmpList[i].GetInstanceID(), tmpList[i].color);
                        }
                    }
                    m_TmpList.AddRange(tmpHashSet);
                }
            }

            if (m_ImgMatMap.Count == 0)
            {
                List<Graphic> graphicList = new List<Graphic>();
                GetComponentsInChildren<Graphic>(true, graphicList);
                var filteredGraphic = new List<Graphic>();
                for (int i = 0; i < graphicList.Count; i++)
                {
                    if (IsExcluded(graphicList[i])) continue;
                    filteredGraphic.Add(graphicList[i]);
                    m_ImgMatMap.Add(graphicList[i].GetInstanceID(), graphicList[i].material);
                }

                m_GraphicList = filteredGraphic;

                if (m_ExternalObjects != null && m_ExternalObjects.Length > 0)
                {
                    HashSet<Graphic> graphicHashSet = new HashSet<Graphic>();
                    foreach (var obj in m_ExternalObjects)
                    {
                        if (obj == null || obj.transform.IsChildOf(transform)) continue;
                        obj.GetComponentsInChildren<Graphic>(true, graphicList);
                        for (int i = 0; i < graphicList.Count; i++)
                        {
                            if (IsExcluded(graphicList[i])) continue;
                            graphicHashSet.Add(graphicList[i]);
                            if (!m_ImgMatMap.ContainsKey(graphicList[i].GetInstanceID()))
                                m_ImgMatMap.Add(graphicList[i].GetInstanceID(), graphicList[i].material);
                        }
                    }
                    m_GraphicList.AddRange(graphicHashSet);
                }
            }
        }

        void TryGetRenderers()
        {
            if (m_RendererList != null) return;

            List<Renderer> rendererList = new List<Renderer>();
            GetComponentsInChildren<Renderer>(true, rendererList);
            var filtered = new List<Renderer>();
            for (int i = 0; i < rendererList.Count; i++)
            {
                if (!IsExcluded(rendererList[i]))
                    filtered.Add(rendererList[i]);
            }

            m_RendererList = filtered;

            if (m_ExternalObjects != null && m_ExternalObjects.Length > 0)
            {
                HashSet<Renderer> rendererHashSet = new HashSet<Renderer>();
                foreach (var obj in m_ExternalObjects)
                {
                    if (obj == null || obj.transform.IsChildOf(transform)) continue;
                    obj.GetComponentsInChildren<Renderer>(true, rendererList);
                    for (int i = 0; i < rendererList.Count; i++)
                    {
                        if (!IsExcluded(rendererList[i]))
                            rendererHashSet.Add(rendererList[i]);
                    }
                }
                m_RendererList.AddRange(rendererHashSet);
            }
        }

        void ApplyRendererGray(bool isGray)
        {
            TryGetRenderers();
            if (m_RendererList == null) return;

            if (m_MPB == null)
                m_MPB = new MaterialPropertyBlock();

            for (int i = 0; i < m_RendererList.Count; i++)
            {
                var renderer = m_RendererList[i];
                if (renderer == null) continue;

                var mat = renderer.sharedMaterial;
                if (mat == null) continue;

                int id = renderer.GetInstanceID();
                if (mat.HasProperty(s_DesaturateId))
                {
                    renderer.GetPropertyBlock(m_MPB);

                    if (!m_RendererFloatMap.TryGetValue(id, out var floatMap))
                    {
                        floatMap = new Dictionary<string, float>();
                        m_RendererFloatMap[id] = floatMap;
                    }

                    const string propName = "_Desaturate";
                    if (isGray)
                    {
                        if (!floatMap.ContainsKey(propName))
                            floatMap[propName] = mat.GetFloat(s_DesaturateId);
                        //mat.SetFloat(s_DesaturateId, 1f);
                        m_MPB.SetFloat(s_DesaturateId, 1f);
                    }
                    else if (floatMap.TryGetValue(propName, out float original))
                    {
                        //mat.SetFloat(s_DesaturateId, original);
                        m_MPB.SetFloat(s_DesaturateId, original);
                    }

                    renderer.SetPropertyBlock(m_MPB);

                    continue;
                }

                var shader = mat.shader;
                if (shader == null) continue;

                renderer.GetPropertyBlock(m_MPB);

                if (!m_RendererColorMap.TryGetValue(id, out var colorMap))
                {
                    colorMap = new Dictionary<string, Color>();
                    m_RendererColorMap[id] = colorMap;
                }

                for (int p = 0; p < shader.GetPropertyCount(); p++)
                {
                    if (shader.GetPropertyType(p) != ShaderPropertyType.Color) continue;

                    string propName = shader.GetPropertyName(p);
                    if (isGray)
                    {
                        Color original = mat.GetColor(propName);
                        if (!colorMap.ContainsKey(propName))
                            colorMap[propName] = original;
                        //mat.SetColor(propName, DesaturateColor(original));
                        m_MPB.SetColor(propName, DesaturateColor(original));
                    }
                    else if (colorMap.TryGetValue(propName, out Color original))
                    {
                        //mat.SetColor(propName, original);
                        m_MPB.SetColor(propName, original);
                    }
                }

                renderer.SetPropertyBlock(m_MPB);
            }
        }

        static Color DesaturateColor(Color color)
        {
            float gray = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            return new Color(gray, gray, gray, color.a);
        }
    }
}
