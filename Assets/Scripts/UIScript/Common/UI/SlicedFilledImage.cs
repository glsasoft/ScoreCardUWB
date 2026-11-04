using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace LetsPiu.UI
{
    /// <summary>
    /// 支持九宫格(Sliced)的水平填充进度条。
    /// Unity 原生 Image：Filled 不走九宫格、Sliced 不认 fillAmount。本组件先由父类按 Sliced
    /// 生成九宫格网格（边角不拉伸），再按 fillAmount 沿水平方向裁剪该网格，兼得两者。
    /// slantAngle 让裁切边呈倾斜切口（默认 0 为竖直切口，与 SlantedFilledImageSplitBar 视觉一致）。
    /// fillOrigin：0=从左到右，1=从右到左。使用时 Image Type 请保持 Sliced 并给 Sprite 设置九宫格边框。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class SlicedFilledImage : Image, ISlantedFillImage
    {
        [SerializeField]
        [Range(-45f, 45f)]
        private float slantAngle = 0f;

        [SerializeField]
        [Tooltip("跟随进度末端的指示器节点，可整体替换为任意图片+文字子物体。建议作为本组件的子物体；只改其 x 贴合进度边缘，y 保持你摆放的位置")]
        private RectTransform handle;

        public float SlantAngle
        {
            get => slantAngle;
            set
            {
                if (Mathf.Approximately(slantAngle, value)) return;
                slantAngle = Mathf.Clamp(value, -45f, 45f);
                SetVerticesDirty();
            }
        }

        public RectTransform Handle
        {
            get => handle;
            set
            {
                handle = value;
                SetVerticesDirty();
            }
        }

        static readonly List<UIVertex> s_Stream = new List<UIVertex>();

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            float fa = Mathf.Clamp01(fillAmount);
            Rect r = GetPixelAdjustedRect();

            UpdateHandle(r, fa);

            if (fa >= 1f) return;

            if (fa <= 0f)
            {
                toFill.Clear();
                return;
            }

            if (r.width <= 0f)
            {
                toFill.Clear();
                return;
            }

            bool rightToLeft = fillOrigin == (int)OriginHorizontal.Right;
            float cutoffX = rightToLeft
                ? r.xMax - fa * r.width
                : r.xMin + fa * r.width;
            float yCenter = r.center.y;
            float tanSlant = Mathf.Tan(slantAngle * Mathf.Deg2Rad);

            s_Stream.Clear();
            toFill.GetUIVertexStream(s_Stream);
            toFill.Clear();

            var clipped = new List<UIVertex>(s_Stream.Count);
            for (int i = 0; i + 2 < s_Stream.Count; i += 3)
            {
                ClipTriangle(s_Stream[i], s_Stream[i + 1], s_Stream[i + 2],
                    cutoffX, yCenter, tanSlant, rightToLeft, clipped);
            }

            toFill.AddUIVertexTriangleStream(clipped);
        }

        // 把 handle 定位到当前进度边缘：只改 x（贴合可倾斜切口在其自身 y 处的位置），保留调用者摆放的 y。
        void UpdateHandle(Rect r, float fa)
        {
            if (handle == null || r.width <= 0f) return;

            bool rightToLeft = fillOrigin == (int)OriginHorizontal.Right;
            float cutoffX = rightToLeft
                ? r.xMax - fa * r.width
                : r.xMin + fa * r.width;
            float yCenter = r.center.y;
            float tanSlant = Mathf.Tan(slantAngle * Mathf.Deg2Rad);

            Vector3 lp = handle.localPosition;
            float boundaryX = cutoffX + tanSlant * (lp.y - yCenter);
            handle.localPosition = new Vector3(boundaryX, lp.y, lp.z);
        }

        // 用一条（可倾斜的）竖直边界裁剪三角形：保留的一侧由 keepGreater 决定。
        // 边界为线性函数，故沿三角形每条边符号距离也是线性，交点参数可直接由端点距离比求得。
        static void ClipTriangle(
            UIVertex a, UIVertex b, UIVertex c,
            float cutoffX, float yCenter, float tanSlant, bool keepGreater,
            List<UIVertex> outList)
        {
            var input = new UIVertex[] { a, b, c };
            var output = new List<UIVertex>(4);

            for (int i = 0; i < 3; i++)
            {
                UIVertex cur = input[i];
                UIVertex nxt = input[(i + 1) % 3];

                float dCur = InsideDistance(cur.position, cutoffX, yCenter, tanSlant, keepGreater);
                float dNxt = InsideDistance(nxt.position, cutoffX, yCenter, tanSlant, keepGreater);
                bool curIn = dCur >= 0f;
                bool nxtIn = dNxt >= 0f;

                if (curIn) output.Add(cur);
                if (curIn != nxtIn)
                {
                    float t = dCur / (dCur - dNxt);
                    output.Add(LerpVertex(cur, nxt, t));
                }
            }

            for (int i = 1; i + 1 < output.Count; i++)
            {
                outList.Add(output[0]);
                outList.Add(output[i]);
                outList.Add(output[i + 1]);
            }
        }

        static float InsideDistance(Vector3 p, float cutoffX, float yCenter, float tanSlant, bool keepGreater)
        {
            float boundaryX = cutoffX + tanSlant * (p.y - yCenter);
            float d = p.x - boundaryX;
            return keepGreater ? d : -d;
        }

        static UIVertex LerpVertex(UIVertex a, UIVertex b, float t)
        {
            return new UIVertex
            {
                position = Vector3.Lerp(a.position, b.position, t),
                normal = Vector3.Lerp(a.normal, b.normal, t),
                tangent = Vector4.Lerp(a.tangent, b.tangent, t),
                color = Color32.Lerp(a.color, b.color, t),
                uv0 = Vector4.Lerp(a.uv0, b.uv0, t),
                uv1 = Vector4.Lerp(a.uv1, b.uv1, t),
                uv2 = Vector4.Lerp(a.uv2, b.uv2, t),
                uv3 = Vector4.Lerp(a.uv3, b.uv3, t),
            };
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlicedFilledImage))]
    [CanEditMultipleObjects]
    public class SlicedFilledImageEditor : ImageEditor
    {
        SerializedProperty slantAngle;
        SerializedProperty handle;
        SerializedProperty m_FillOrigin;
        SerializedProperty m_FillAmount;
        SerializedProperty m_Type;

        protected override void OnEnable()
        {
            base.OnEnable();
            slantAngle = serializedObject.FindProperty("slantAngle");
            handle = serializedObject.FindProperty("handle");
            m_FillOrigin = serializedObject.FindProperty("m_FillOrigin");
            m_FillAmount = serializedObject.FindProperty("m_FillAmount");
            m_Type = serializedObject.FindProperty("m_Type");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            if (m_Type != null && m_Type.intValue != (int)Image.Type.Sliced)
            {
                EditorGUILayout.HelpBox(
                    "SlicedFilledImage 需要 Image Type = Sliced 才会走九宫格；\n" +
                    "并给 Sprite 设置九宫格边框(Border)。", MessageType.Warning);
            }

            EditorGUILayout.LabelField("Fill", EditorStyles.boldLabel);
            if (m_FillOrigin != null)
            {
                m_FillOrigin.intValue = EditorGUILayout.Popup(
                    "Fill Origin", m_FillOrigin.intValue, new[] { "Left", "Right" });
            }
            if (m_FillAmount != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(m_FillAmount, 0f, 1f, new GUIContent("Fill Amount"));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var t in targets)
                    {
                        if (t is SlicedFilledImage img)
                        {
                            img.SetVerticesDirty();
                            EditorUtility.SetDirty(img);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(slantAngle);
            EditorGUILayout.PropertyField(handle, new GUIContent("Handle (跟随指示器)"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
