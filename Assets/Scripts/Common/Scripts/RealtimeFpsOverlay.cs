using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// 全局左上角实时帧率显示，挂于 UIRoot，跨场景/面板持续生效。
    /// </summary>
    public class RealtimeFpsOverlay : MonoBehaviour
    {
        float m_SmoothedDelta;
        float m_Fps;
        GUIStyle m_Style;
        bool m_StyleReady;

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
            {
                return;
            }

            if (m_SmoothedDelta <= 0f)
            {
                m_SmoothedDelta = dt;
            }
            else
            {
                m_SmoothedDelta += (dt - m_SmoothedDelta) * 0.15f;
            }

            m_Fps = 1f / m_SmoothedDelta;
        }

        void OnGUI()
        {
            EnsureStyle();

            float dtMs = m_SmoothedDelta * 1000f;
            string text = $"FPS: {m_Fps:F0}  ({dtMs:F1}ms)";
            GUI.Label(new Rect(12f, 12f, Screen.width * 0.35f, Screen.height * 0.08f), text, m_Style);
        }

        void EnsureStyle()
        {
            if (m_StyleReady)
            {
                return;
            }

            int fontSize = Mathf.Max(18, Screen.height / 28);
            m_Style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Overflow
            };
            m_Style.normal.textColor = Color.green;
            m_StyleReady = true;
        }
    }
}
