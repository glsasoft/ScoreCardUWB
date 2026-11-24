using UnityEngine;

namespace LetsPiu
{
    /// <summary>
    /// LeftPanel 相关组件仅在 Android 真机生效（Editor / PC 跳过）。
    /// </summary>
    public static class LeftPanelPlatform
    {
        public static bool IsAndroidPlayer =>
            Application.platform == RuntimePlatform.Android;
    }
}
