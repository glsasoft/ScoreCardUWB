using System;
using System.Collections.Generic;
// using Plugins.IOSDiskSpaceUtil;
using UnityEngine;

namespace LetsPiu
{
    
    /// <summary>
    /// 目前安全区适配仍然存在的问题：如果安全区x为0，但是屏幕右边有安全区的情况下， 并没有适配!
    /// </summary>
    
    public class UIScreenAdapter : MonoBehaviour
    {
        [SerializeField] Canvas m_RootCanvas;
        [SerializeField, Range(0, 200)]
        int m_OffsetX;
        int m_OffsetY;

        List<RectTransform> m_PanelRoots = new List<RectTransform>();

        static int m_CurrentOffsetX;
        static int m_CurrentOffsetY;
        
        static ScreenOrientation m_CurrentOrientation;

        int m_ScreenWidth;
        int m_ScreenHeight;
        private float m_RootCanvasScaleFactor = 1f;

        public static int offsetX
        {
            get { return m_CurrentOffsetX; }
        }
        
        public static int offsetY
        {
            get { return m_CurrentOffsetY; }
        }

        public static ScreenOrientation orientation
        {
            get { return m_CurrentOrientation; }
        }

        void Awake()
        {
            m_PanelRoots.Add(this.transform.Find("RootCanvas/PanelRoot").GetComponent<RectTransform>());
            m_PanelRoots.Add(this.transform.Find("TopRootCanvas/TopRoot").GetComponent<RectTransform>());
            UpdateScreen();
        }

        void Update()
        {
            UpdateScreen();
        }

        void UpdateScreen()
        {
            UpdateOffset();

            if (m_CurrentOffsetX == m_OffsetX 
                && m_CurrentOrientation == Screen.orientation
                && m_CurrentOffsetY == m_OffsetY
                && Math.Abs(m_RootCanvasScaleFactor - m_RootCanvas.scaleFactor) < 0.001f)
            {
                return;
            }
            
            Debug.LogWarning("UIScreenAdapter UpdateScreen 屏幕方向旋转了 m_OffsetX="+m_OffsetX+" m_OffsetY="+m_OffsetY+" Screen.orientation="+Screen.orientation);
            
            m_CurrentOrientation = Screen.orientation;

            m_CurrentOffsetX = m_OffsetX;
            m_CurrentOffsetY = m_OffsetY;
            m_RootCanvasScaleFactor = m_RootCanvas.scaleFactor;

            for (int i = 0; i < m_PanelRoots.Count; ++i)
            {
                RectTransform panelRoot = m_PanelRoots[i];
                if (panelRoot == null)
                {
                    continue;
                }
                
                /*
                 offsetMin 表示锚点左下角的偏移（left, bottom）。
                offsetMax 表示锚点右上角的偏移（right, top）。
                
                offsetMin.x：距离左边的偏移（left），正值表示向右偏移，负值表示向左扩展。
                offsetMin.y：距离下边的偏移（bottom），正值表示向上偏移，负值表示向下扩展。
                offsetMax.x：距离右边的偏移（right），正值表示向左偏移，负值表示向右扩展。
                offsetMax.y：距离上边的偏移（top），正值表示向下偏移，负值表示向上扩展。

                 */

                if (m_CurrentOrientation is ScreenOrientation.LandscapeLeft or ScreenOrientation.LandscapeRight)
                {
                    panelRoot.offsetMin = new Vector2(m_CurrentOffsetX, 0); //left,bottom
                    panelRoot.offsetMax = new Vector2(-m_CurrentOffsetX, 0); //right,top
                    Debug.Log("UIScreenAdapter UpdateScreen 屏幕方向旋转了 设置横屏安全区1 m_CurrentOffsetX=" + m_CurrentOffsetX + " m_CurrentOffsetY=" + m_CurrentOffsetY+" panelRoot.offsetMax=" + panelRoot.offsetMax + " panelRoot.offsetMin=" + panelRoot.offsetMin);
                }
                else if (m_CurrentOrientation is ScreenOrientation.Portrait or ScreenOrientation.PortraitUpsideDown)
                {
                    panelRoot.offsetMin = new Vector2(0, m_CurrentOffsetY);//left,bottom
                    panelRoot.offsetMax = new Vector2(0, -m_CurrentOffsetY);//right,top
                    Debug.Log("UIScreenAdapter UpdateScreen 屏幕方向旋转了 设置竖屏安全区2 m_CurrentOffsetX=" + m_CurrentOffsetX + " m_CurrentOffsetY=" + m_CurrentOffsetY+" panelRoot.offsetMax=" + panelRoot.offsetMax+" panelRoot.offsetMin=" + panelRoot.offsetMin);
                }
                else
                {
                    panelRoot.offsetMax = Vector2.zero;
                    panelRoot.offsetMin = Vector2.zero;
                    Debug.LogWarning("UIScreenAdapter UpdateScreen 屏幕方向旋转了 安全区设置为0 m_CurrentOffsetX=" + m_CurrentOffsetX + " m_CurrentOffsetY=" + m_CurrentOffsetY+" panelRoot.offsetMax=" + panelRoot.offsetMax);
                }

                UIBreakSafeArea[] breaks = panelRoot.GetComponentsInChildren<UIBreakSafeArea>(true);
                for (int j = 0; j < breaks.Length; ++j)
                {
                    breaks[j].Refresh();
                }
            }
        }

        void UpdateOffset()
        {
            if (m_ScreenWidth == Screen.width && m_ScreenHeight == Screen.height && m_CurrentOrientation == Screen.orientation
                && Math.Abs(m_RootCanvasScaleFactor - m_RootCanvas.scaleFactor) < 0.001f)
            {
                return;
            }

            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
            m_RootCanvasScaleFactor = m_RootCanvas.scaleFactor;

            if (HasNotchInScreen())
            {
                m_OffsetX = Mathf.CeilToInt(GetScreenSafeArea().x / m_RootCanvas.scaleFactor);
                m_OffsetY = Mathf.CeilToInt(GetScreenSafeArea().y / m_RootCanvas.scaleFactor); }
            else
            {
                m_OffsetX = 0;
                m_OffsetY = 0;
            }
        }

        bool HasNotchInScreen()
        {
            var safeArea = GetScreenSafeArea();
            return safeArea.x > 0 || safeArea.y > 0;
        }
        
        public static Rect GetScreenSafeArea()
        {
            /*
             在MainActivity中有以下变量，代表原生安全的安全区边界：
            public static int safeInsetsLeft = 0;
            public static int safeInsetsTop = 0;
            public static int safeInsetsRight = 0;
            public static int safeInsetsBottom = 0;

            在Unity中，需要通过AndroidJavaObject来获取这些值。
            并根据这些值计算出新的Unity安全区 Rect(x,y,width,height)
             */
            var safeArea = Screen.safeArea;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using (var mainActivity = new AndroidJavaClass("cn.com.glsa.app.fightgame.MainActivity"))
                    {
                        int left = mainActivity.GetStatic<int>("safeInsetsLeft");
                        int top = mainActivity.GetStatic<int>("safeInsetsTop");
                        int right = mainActivity.GetStatic<int>("safeInsetsRight");
                        int bottom = mainActivity.GetStatic<int>("safeInsetsBottom");

                        int screenWidth = Screen.width;
                        int screenHeight = Screen.height;
                        // 计算安全区Rect
                        safeArea = new Rect(
                            left,
                            top,
                            screenWidth - left - right,
                            screenHeight - top - bottom
                        );
                        Debug.Log($"UIScreenAdapter Android原生屏幕安全区: left={left}, top={top}, right={right}, bottom={bottom}, screenWidth={screenWidth}, screenHeight={screenHeight}, safeArea={safeArea}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("UIScreenAdapter Android原生屏幕安全区 GetScreenSafeArea异常: " + e);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            try 
            {
                float left =IOSDiskSpaceUtil.GetSafeAreaLeft_ios();
                float top =IOSDiskSpaceUtil.GetSafeAreaTop_ios();
                float right = IOSDiskSpaceUtil.GetSafeAreaRight_ios();
                float bottom = IOSDiskSpaceUtil.GetSafeAreaBottom_ios();
                
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                
                // 将物理像素转换为逻辑像素
                float scale = Screen.dpi / 160f;
                safeArea = new Rect(
                    left * scale,
                    top * scale,
                    screenWidth - (left + right) * scale,
                    screenHeight - (top + bottom) * scale
                );
                
                Debug.Log($"UIScreenAdapter iOS原生屏幕安全区: left={left}, top={top}, right={right}, bottom={bottom}, screenWidth={screenWidth}, screenHeight={screenHeight}, safeArea={safeArea}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("UIScreenAdapter iOS原生屏幕安全区 GetScreenSafeArea异常: " + e);
            }
#endif
            
            //修正安全区，左右安全区取对大值设置为x、上下安全区取最大值设置为y
            
            Debug.Log("UIScreenAdapter GetScreenSafeArea() safeArea>>> x=" + safeArea.x + " y=" + safeArea.y + " width=" + safeArea.width + " height=" + safeArea.height);
            
            var safeAreaLeft = safeArea.x;
            var safeAreaTop = safeArea.y;
            var safeAreaRight = Screen.width - (safeArea.x + safeArea.width);
            var safeAreaBottom = Screen.height - (safeArea.y + safeArea.height);
            
            Debug.Log("UIScreenAdapter GetScreenSafeArea() safeAreaLeft=" + safeAreaLeft + " safeAreaTop=" + safeAreaTop + " safeAreaRight=" + safeAreaRight + " safeAreaBottom=" + safeAreaBottom+" screen.width=" + Screen.width + " screen.height=" + Screen.height);
            
            var safeAreaHorizontal = Math.Max(safeAreaLeft, safeAreaRight);
            var safeAreaVertical = Math.Max(safeAreaTop, safeAreaBottom);
            

            Debug.Log("UIScreenAdapter GetScreenSafeArea() safeAreaHorizontal=" + safeAreaHorizontal + " safeAreaVertical=" + safeAreaVertical);
            
            safeArea.x = safeAreaHorizontal;
            safeArea.y = safeAreaVertical;
            
            Debug.Log("UIScreenAdapter GetScreenSafeArea() safeArea.x=" + safeArea.x + " safeArea.y=" + safeArea.y);
            
            return safeArea;
        }
    }
}
