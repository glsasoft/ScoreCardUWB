using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace LetsPiu.UI
{
    /// <summary>
    /// 左右分栏进度条专用倾斜填充图（红/蓝各半，向中间收缩）。
    /// 适用于歼灭等多模式顶部对比条，与全宽单条使用的 <see cref="SlantedFilledImage"/> 区分。
    /// fillOrigin=Right：右缘固定、左缘向中间收缩，贴图从左侧取样（整体向右滑出，仅右侧裁切）。
    /// fillOrigin=Left：左缘固定、右缘向中间收缩，贴图从右侧取样（整体向左滑出，仅左侧裁切）。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class SlantedFilledImageSplitBar : Image, ISlantedFillImage
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

            bool rightToLeft = (fillOrigin == (int)Image.OriginHorizontal.Right);

            float slantPixels = Mathf.Tan(slantAngle * Mathf.Deg2Rad) * r.height;
            float slantUV = slantPixels / r.width;

            float fa = fillAmount;

            if (rightToLeft)
            {
                float topCutX = Mathf.Clamp01(1f - fa - slantUV * 0.5f);
                float botCutX = Mathf.Clamp01(1f - fa + slantUV * 0.5f);

                float xMax = r.xMax;
                float xMinT = r.xMin + topCutX * r.width;
                float xMinB = r.xMin + botCutX * r.width;

                float uvRightT = Mathf.Lerp(uv.x, uv.z, Mathf.Clamp01(fa + slantUV * 0.5f));
                float uvRightB = Mathf.Lerp(uv.x, uv.z, Mathf.Clamp01(fa - slantUV * 0.5f));
                float uvXMinT = uv.x;
                float uvXMinB = uv.x;
                float uvYMin = uv.y;
                float uvYMax = uv.w;

                Color32 c = color;

                vh.AddVert(new Vector3(xMinB, r.yMin), c, new Vector2(uvXMinB, uvYMin));
                vh.AddVert(new Vector3(xMax, r.yMin), c, new Vector2(uvRightB, uvYMin));
                vh.AddVert(new Vector3(xMax, r.yMax), c, new Vector2(uvRightT, uvYMax));
                vh.AddVert(new Vector3(xMinT, r.yMax), c, new Vector2(uvXMinT, uvYMax));

                vh.AddTriangle(0, 2, 1);
                vh.AddTriangle(0, 3, 2);
            }
            else
            {
                float topFillX = Mathf.Clamp01(fa + slantUV * 0.5f);
                float botFillX = Mathf.Clamp01(fa - slantUV * 0.5f);

                float xMin = r.xMin;
                float xMaxT = r.xMin + topFillX * r.width;
                float xMaxB = r.xMin + botFillX * r.width;

                float uvLeftT = Mathf.Lerp(uv.x, uv.z, Mathf.Clamp01(1f - fa - slantUV * 0.5f));
                float uvLeftB = Mathf.Lerp(uv.x, uv.z, Mathf.Clamp01(1f - fa + slantUV * 0.5f));
                float uvXMaxT = uv.z;
                float uvXMaxB = uv.z;
                float uvYMin = uv.y;
                float uvYMax = uv.w;

                Color32 c = color;

                vh.AddVert(new Vector3(xMin, r.yMin), c, new Vector2(uvLeftB, uvYMin));
                vh.AddVert(new Vector3(xMaxB, r.yMin), c, new Vector2(uvXMaxB, uvYMin));
                vh.AddVert(new Vector3(xMaxT, r.yMax), c, new Vector2(uvXMaxT, uvYMax));
                vh.AddVert(new Vector3(xMin, r.yMax), c, new Vector2(uvLeftT, uvYMax));

                vh.AddTriangle(0, 2, 1);
                vh.AddTriangle(0, 3, 2);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlantedFilledImageSplitBar))]
    [CanEditMultipleObjects]
    public class SlantedFilledImageSplitBarEditor : ImageEditor
    {
        SerializedProperty slantAngle;

        protected override void OnEnable()
        {
            base.OnEnable();
            slantAngle = serializedObject.FindProperty("slantAngle");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUILayout.PropertyField(slantAngle);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
