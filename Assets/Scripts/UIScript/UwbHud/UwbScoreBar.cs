using System.Collections.Generic;
using Coffee.UIExtensions;
using LetsPiu.Base;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.UI
{
    /// <summary>
    /// 顶部红蓝对比进度条（含升降级、分割线特效），由 ScoreCardUnity 的 ScoreModeNvNFightTopbar 预制体裁出 FillBg 部分，
    /// 比分到条宽的换算与特效切换沿用 ScoreModeFightTopbarBase.ApplyTopScoreValues。
    /// 预制体内的子节点和特效都靠名字查找，见 Awake。
    /// </summary>
    public class UwbScoreBar : MonoBehaviour
    {
        const float FullWidthHalf = 2195f;

        /// <summary>ScoreCardUnity 根画布缩放：10240x1440 参考分辨率 + 透视相机，1080 高屏幕上为 72.794 / 1440。</summary>
        const float OriginalCanvasScale = 0.05055f;

        /// <summary>分割线特效相对原版尺寸的比例。</summary>
        const float SplitEffectRatio = 0.375f;

        [Tooltip("盖在比分条上方的 Canvas（中央计时牌、旗帜、文字所在层），由 HUD 预制体连好")]
        [SerializeField] Canvas topCanvas;

        Canvas m_FillCanvas;
        Image m_RedFill;
        Image m_BlueFill;
        GameObject m_Split;
        GameObject m_RedLvUp;
        GameObject m_BlueLvUp;
        GameObject m_RedNormal;
        GameObject m_RedHigh;
        GameObject m_BlueNormal;
        GameObject m_BlueHigh;
        GameObject m_FillEffectRoot;
        GameObject m_RedFillEffect;
        GameObject m_BlueFillEffect;

        int m_LastRed = -1;
        int m_LastBlue = -1;
        int m_AppliedPanelOrder = int.MinValue;
        Canvas m_PanelCanvas;

        void Awake()
        {
            m_FillCanvas = Find<Canvas>("FillBg");
            m_RedFill = Find<Image>("FillBg/RedFill");
            m_BlueFill = Find<Image>("FillBg/BlueFill");
            m_Split = Find("FillBg/VFX_FightingScreen_Line");
            m_RedLvUp = Find("FillBg/VFX_FightingScreen_Lvlup_Red");
            m_BlueLvUp = Find("FillBg/VFX_FightingScreen_Lvlup");
            m_FillEffectRoot = Find("FillBg/Mask/FillEffect");
            m_RedFillEffect = Find("FillBg/Mask/FillEffect/VFX_FightingScreen_Plater_Red");
            m_BlueFillEffect = Find("FillBg/Mask/FillEffect/VFX_FightingScreen_Plater_Blue");
            m_RedNormal = Find("VFX_FightingScreen_Plater_Red_Normal02");
            m_RedHigh = Find("VFX_FightingScreen_Plater_Red_High02");
            m_BlueNormal = Find("VFX_FightingScreen_Plater_Blue_Normal02");
            m_BlueHigh = Find("VFX_FightingScreen_Plater_Blue_High02");

            m_PanelCanvas = GetComponentInParent<Canvas>();

            // 比分条整体缩放后，UIParticle 的粒子尺寸会被父节点缩放重复乘一次（实测变成缩放比例的平方），
            // 这里除回去，特效才能和缩放后的进度条等宽。
            float scale = transform.localScale.x;
            foreach (UIParticle particle in GetComponentsInChildren<UIParticle>(true))
                particle.scale3D /= scale;

            // 分割线特效的尺寸是按 ScoreCardUnity 的画布比例做的（光束长度、宽度、光晕都是绝对大小），
            // 比分条缩小后它会跟着变小，这里先放回原版的世界尺寸，再按 SplitEffectRatio 等比缩小。
            if (m_Split != null)
                m_Split.transform.localScale *= OriginalCanvasScale * SplitEffectRatio / m_Split.transform.lossyScale.x;

            // 预制体里 Wave、Lizi 这些粒子在 Default 层，UICamera 只渲染 UI 层，所以看不到，统一放到比分条所在的 UI 层。
            int uiLayer = LayerMask.NameToLayer("UI");
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = uiLayer;
        }

        GameObject Find(string path)
        {
            Transform t = transform.Find(path);
            return t != null ? t.gameObject : null;
        }

        T Find<T>(string path) where T : UnityEngine.Component
        {
            Transform t = transform.Find(path);
            return t != null ? t.GetComponent<T>() : null;
        }

        void LateUpdate()
        {
            ApplyCanvasOrder();
        }

        // 层级自下而上：红蓝常态/高亮特效 → 进度条 Canvas → 顶部 Canvas → 分割线 → 升级特效。
        // 以面板 Canvas 的 sortingOrder 为基准，面板层级变化后重排一次。
        void ApplyCanvasOrder()
        {
            if (m_PanelCanvas == null || m_PanelCanvas.sortingOrder == m_AppliedPanelOrder)
                return;

            m_AppliedPanelOrder = m_PanelCanvas.sortingOrder;
            int baseOrder = m_AppliedPanelOrder + 10;
            var renderers = new List<Renderer>();
            int o1 = EffectUtility.SetEffectOrder(m_RedNormal, baseOrder, renderers);
            int o2 = EffectUtility.SetEffectOrder(m_RedHigh, baseOrder, renderers);
            int o3 = EffectUtility.SetEffectOrder(m_BlueNormal, baseOrder, renderers);
            int o4 = EffectUtility.SetEffectOrder(m_BlueHigh, baseOrder, renderers);
            baseOrder = Mathf.Max(Mathf.Max(o1, o2), Mathf.Max(o3, o4));
            if (m_FillCanvas != null) m_FillCanvas.sortingOrder = ++baseOrder;
            if (topCanvas != null) topCanvas.sortingOrder = ++baseOrder;
            baseOrder = EffectUtility.SetEffectOrder(m_Split, baseOrder, renderers);
            EffectUtility.SetEffectOrder(m_RedLvUp, baseOrder, renderers);
            EffectUtility.SetEffectOrder(m_BlueLvUp, baseOrder, renderers);
        }

        public void Apply(int redScore, int blueScore)
        {
            if (redScore == m_LastRed && blueScore == m_LastBlue)
                return;

            if (redScore == 0 && blueScore == 0)
            {
                SetActive(m_Split, false);
                SetActive(m_RedLvUp, false);
                SetActive(m_BlueLvUp, false);
                SetActive(m_FillEffectRoot, false);
                SetActive(m_RedNormal, true);
                SetActive(m_RedHigh, false);
                SetActive(m_BlueNormal, true);
                SetActive(m_BlueHigh, false);
                SetFill(m_RedFill, -45f, 0f);
                SetFill(m_BlueFill, -45f, 0f);
                m_LastRed = redScore;
                m_LastBlue = blueScore;
                return;
            }

            SetActive(m_Split, true);
            SetActive(m_FillEffectRoot, true);

            float redPercent = (float)redScore / (redScore + blueScore);
            float bluePercent = (float)blueScore / (redScore + blueScore);
            float totalWidth = m_RedFill != null ? m_RedFill.rectTransform.sizeDelta.x : 0f;
            float splitX = (redPercent - 0.5f) * totalWidth;

            if (redScore == blueScore)
            {
                SetActive(m_RedNormal, true);
                SetActive(m_RedHigh, false);
                SetActive(m_BlueNormal, true);
                SetActive(m_BlueHigh, false);
                SetLocalX(m_Split, 0f);
                SetLocalX(m_FillEffectRoot, 0f);
                SetFlip(false);
                SetFill(m_RedFill, 45f, 0.5f);
                SetFill(m_BlueFill, -45f, 0.5f);
            }
            else if (redScore > blueScore)
            {
                if (m_LastRed <= m_LastBlue)
                    Restart(m_RedLvUp);

                SetActive(m_RedNormal, false);
                SetActive(m_RedHigh, true);
                SetActive(m_BlueNormal, false);
                SetActive(m_BlueHigh, false);
                if (bluePercent < 0.01f)
                    SetActive(m_Split, false);
                SetLocalX(m_Split, splitX);
                SetLocalX(m_FillEffectRoot, bluePercent < 0.01f ? FullWidthHalf : splitX);
                SetFlip(false);
                SetFill(m_RedFill, 45f, redPercent);
                SetFill(m_BlueFill, -45f, bluePercent);
            }
            else
            {
                if (m_LastRed >= m_LastBlue)
                    Restart(m_BlueLvUp);

                SetActive(m_RedNormal, false);
                SetActive(m_RedHigh, false);
                SetActive(m_BlueNormal, false);
                SetActive(m_BlueHigh, true);
                if (redPercent < 0.01f)
                    SetActive(m_Split, false);
                SetLocalX(m_Split, splitX);
                SetLocalX(m_FillEffectRoot, redPercent < 0.01f ? -FullWidthHalf : splitX);
                SetFlip(true);
                SetFill(m_RedFill, -45f, redPercent);
                SetFill(m_BlueFill, 45f, bluePercent);
            }

            m_LastRed = redScore;
            m_LastBlue = blueScore;
        }

        void SetFlip(bool flip)
        {
            Vector3 angle = flip ? new Vector3(180f, 0f, 0f) : Vector3.zero;
            if (m_RedFillEffect != null) m_RedFillEffect.transform.localEulerAngles = angle;
            if (m_BlueFillEffect != null) m_BlueFillEffect.transform.localEulerAngles = angle;
        }

        static void SetActive(GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }

        static void Restart(GameObject go)
        {
            if (go == null)
                return;
            go.SetActive(false);
            go.SetActive(true);
        }

        static void SetLocalX(GameObject go, float x)
        {
            if (go == null)
                return;
            go.transform.localPosition = new Vector3(x, 0f, 0f);
        }

        static void SetFill(Image image, float slantAngle, float fillAmount)
        {
            if (image == null)
                return;
            if (image is ISlantedFillImage slanted)
                slanted.SlantAngle = slantAngle;
            image.fillAmount = fillAmount;
        }
    }
}
