namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 地图贴图动画
    /// </summary>
    public class GroundFadeInAnimParams
    {
        // <summary>
        /// 动画的坐标位置（世界坐标或屏幕坐标）。
        /// </summary>
        public MapPoint point;

        /// <summary>
        /// 帧动画图片的 URL 或路径。图片应为纵向排列的多帧图像。
        /// </summary>
        public string imageUrl;

        /// <summary>
        /// 动画的宽度（以米为单位）。
        /// </summary>
        public float width;

        /// <summary>
        /// 动画的高度（以米为单位）。
        /// </summary>
        public float height;
        
        /// <summary>
        /// 最大透明度
        /// </summary>
        public float maxAlpha = 1;

        /// <summary>
        /// 渐显后自动隐藏
        /// </summary>
        public bool autoHide;
        
        /// <summary>
        /// 布尔(是否启用大小从0到1的缩放效果)
        /// </summary>
        public bool enableScale = false;
        
        /// <summary>
        /// 渐显时间
        /// </summary>
        public int fadeInDurationMs = 500;
        /// <summary>
        /// 
        /// </summary>
        public int fadeOutDurationMs = 500;
    }
}