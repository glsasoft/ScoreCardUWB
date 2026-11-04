using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace LetsPiu.UI
{
    public interface ISlantedFillImage
    {
        float SlantAngle { get; set; }
    }

    /// <summary>
    /// 水平填充图片，支持自定义倾斜切口角度（积分模式全宽单条进度条）。
    /// 左右分栏、向中间收缩的进度条请用 <see cref="SlantedFilledImageSplitBar"/>。
    /// slantAngle 单位：度，正值切口右倾，负值左倾
    /// fillOrigin: 0 = 从左到右，1 = 从右到左
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class SlantedFilledImage : Image, ISlantedFillImage
    {
        [SerializeField]
        [Range(-45f, 45f)]
        private float slantAngle = 15f;

        public float SlantAngle
        {
            get => slantAngle;
            set
            {
                if (Mathf.Approximately(slantAngle, value)) return;
                slantAngle = Mathf.Clamp(value, -45, 45);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect r = GetPixelAdjustedRect();
            Vector4 uv = (overrideSprite != null)
                ? DataUtility.GetOuterUV(overrideSprite)
                : new Vector4(0, 0, 1, 1);

            // ① 根据 fillOrigin 决定填充方向
            bool rightToLeft = (fillOrigin == (int)Image.OriginHorizontal.Right);

            // ② 倾斜量（UV 空间）
            float slantPixels = Mathf.Tan(slantAngle * Mathf.Deg2Rad) * r.height;
            float slantUV = slantPixels / r.width;

            float fa = fillAmount; // 0~1

            if (rightToLeft)
            {
                // 从右向左：切口在左侧
                // 左边缘：顶部 x 和底部 x 形成倾斜
                float topCutX = Mathf.Clamp01(1f - fa - slantUV * 0.5f);
                float botCutX = Mathf.Clamp01(1f - fa + slantUV * 0.5f);

                float xMax = r.xMax;
                float xMinT = r.xMin + topCutX * r.width; // 左边缘：顶部 x
                float xMinB = r.xMin + botCutX * r.width; // 左边缘：底部 x
                float yMin = r.yMin;
                float yMax = r.yMax;

                float uvXMax = uv.z;
                float uvXMinT = Mathf.Lerp(uv.x, uv.z, topCutX);
                float uvXMinB = Mathf.Lerp(uv.x, uv.z, botCutX);
                float uvYMin = uv.y;
                float uvYMax = uv.w;

                Color32 c = color;

                // 左下、右下、右上、左上
                vh.AddVert(new Vector3(xMinB, r.yMin), c, new Vector2(uvXMinB, uvYMin)); // 0
                vh.AddVert(new Vector3(xMax, r.yMin), c, new Vector2(uvXMax, uvYMin)); // 1
                vh.AddVert(new Vector3(xMax, r.yMax), c, new Vector2(uvXMax, uvYMax)); // 2
                vh.AddVert(new Vector3(xMinT, r.yMax), c, new Vector2(uvXMinT, uvYMax)); // 3

                vh.AddTriangle(0, 2, 1);
                vh.AddTriangle(0, 3, 2);
            }
            else
            {
                // 从左向右：切口在右侧（原逻辑）
                float topFillX = Mathf.Clamp01(fa + slantUV * 0.5f);
                float botFillX = Mathf.Clamp01(fa - slantUV * 0.5f);

                float xMin = r.xMin;
                float xMaxT = r.xMin + topFillX * r.width;
                float xMaxB = r.xMin + botFillX * r.width;
                float yMin = r.yMin;
                float yMax = r.yMax;

                float uvXMin = uv.x;
                float uvXMaxT = Mathf.Lerp(uv.x, uv.z, topFillX);
                float uvXMaxB = Mathf.Lerp(uv.x, uv.z, botFillX);
                float uvYMin = uv.y;
                float uvYMax = uv.w;

                Color32 c = color;

                vh.AddVert(new Vector3(xMin, r.yMin), c, new Vector2(uvXMin, uvYMin)); // 0
                vh.AddVert(new Vector3(xMaxB, r.yMin), c, new Vector2(uvXMaxB, uvYMin)); // 1
                vh.AddVert(new Vector3(xMaxT, r.yMax), c, new Vector2(uvXMaxT, uvYMax)); // 2
                vh.AddVert(new Vector3(xMin, r.yMax), c, new Vector2(uvXMin, uvYMax)); // 3

                vh.AddTriangle(0, 2, 1);
                vh.AddTriangle(0, 3, 2);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlantedFilledImage))]
    [CanEditMultipleObjects]
    public class SlantedFilledImageEditor : ImageEditor
    {
        SerializedProperty slantAngle;
        SerializedProperty m_FillMethod;
        SerializedProperty m_FillOrigin;
        SerializedProperty m_FillAmount;
        SerializedProperty m_FillClockwise;
        SerializedProperty m_Type;

        protected override void OnEnable()
        {
            base.OnEnable();

            slantAngle = serializedObject.FindProperty("slantAngle");
            m_FillMethod = serializedObject.FindProperty("m_FillMethod");
            m_FillOrigin = serializedObject.FindProperty("m_FillOrigin");
            m_FillAmount = serializedObject.FindProperty("m_FillAmount");
            m_FillClockwise = serializedObject.FindProperty("m_FillClockwise");
            m_Type = serializedObject.FindProperty("m_Type");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            if (m_Type != null && m_Type.intValue != (int)Image.Type.Filled)
            {
                EditorGUILayout.HelpBox(
                    "SlantedFilledImage 用 Fill Amount 控制可见宽度，Image Type 请设为 Filled。\n" +
                    "设为 Sliced 时 Unity 不显示 Fill 控件，且本组件不会走九宫格网格。",
                    MessageType.Warning);
            }

            EditorGUILayout.LabelField("Fill Preview", EditorStyles.boldLabel);
            if (m_FillMethod != null) EditorGUILayout.PropertyField(m_FillMethod);
            if (m_FillOrigin != null) EditorGUILayout.PropertyField(m_FillOrigin);
            if (m_FillAmount != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.Slider(m_FillAmount, 0f, 1f, new GUIContent("Fill Amount"));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var t in targets)
                    {
                        if (t is SlantedFilledImage img)
                        {
                            img.SetVerticesDirty();
                            EditorUtility.SetDirty(img);
                        }
                    }
                }
            }
            if (m_FillClockwise != null) EditorGUILayout.PropertyField(m_FillClockwise);

            EditorGUILayout.PropertyField(slantAngle);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}