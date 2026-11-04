using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu
{
    public class UIRoot : MonoBehaviour
    {
        Camera m_UICamera;
        Transform m_PanelRoot;
        Canvas m_RootCanvas;

        Canvas m_TopCanvas;
        Transform m_TopRoot;

        Transform m_BottomEffectRoot;
        Transform m_TopEffectRoot;

        public const int DEFAULT_UI_WIDTH = 1920;
        public const int DEFAULT_UI_HEIGHT = 1080;
        public const float DEFAULT_UI_ASPECT = 1920 / 1080f;

        static UIRoot s_Root;

        public static UIRoot S => s_Root;

        public Camera uiCamera => m_UICamera;

        public Canvas rootCanvas => m_RootCanvas;

        public Transform panelRoot => m_PanelRoot;

        public Canvas topCanvas => m_TopCanvas;

        public Transform topPanelRoot => m_TopRoot;

        public Transform bottomEffectRoot => m_BottomEffectRoot;

        public Transform topEffectRoot => m_TopEffectRoot;

        public float uiScale
        {
            get { return m_UICamera.orthographicSize * 2 / DEFAULT_UI_HEIGHT; }
        }

        CanvasScaler m_RootCanvasCanvasScaler;

        /// <summary>
        /// 设置竖屏
        /// </summary>
        public void SetPortrait()
        {
            Debug.Log("UIRoot SetPortrait 设置竖屏");
            m_RootCanvasCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            m_RootCanvasCanvasScaler.matchWidthOrHeight = 0.5f; // 设置为0.5，表示宽高比匹配
        }

        /// <summary>
        /// 设置横屏
        /// </summary>
        public void SetLandscape()
        {
            Debug.Log("UIRoot SetLandscape 设置横屏");
            m_RootCanvasCanvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }

        void Awake()
        {
            m_UICamera = transform.Find("UICamera").GetComponent<Camera>();
            m_RootCanvas = transform.Find("RootCanvas").GetComponent<Canvas>();
            m_PanelRoot = transform.Find("RootCanvas/PanelRoot").GetComponent<Transform>();
            m_TopRoot = transform.Find("TopRootCanvas/TopRoot").GetComponent<Transform>();
            m_TopCanvas = transform.Find("TopRootCanvas").GetComponent<Canvas>();
            m_BottomEffectRoot = transform.Find("BottomUIEffect");
            m_TopEffectRoot = transform.Find("TopUIEffect");  
            s_Root = this;
            GameObject.DontDestroyOnLoad(gameObject);
            m_RootCanvasCanvasScaler = m_RootCanvas.GetComponent<CanvasScaler>();
        }
    }
}
