namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 渐隐渐显动画Marker参数
    /// </summary>
    public class IconMarkerFadeInOutParams
    {
        /// <summary>
        /// 动画的坐标位置（世界坐标或屏幕坐标）。
        /// </summary>
        public MapPoint point;

        /// <summary>
        /// 帧动画图片的 URL 或路径。图片应为纵向排列的多帧图像。
        /// </summary>
        public string imageUrl;
        
        /// <summary>
        /// 动画的显示宽度（以像素或米为单位，取决于具体使用场景）。
        /// </summary>
        public float width;

        /// <summary>
        /// 动画的显示高度（以像素或米为单位，取决于具体使用场景）。
        /// </summary>
        public float height;

        /// <summary>
        /// 是否重复播放动画。如果为 true，动画将循环播放；如果为 false，动画播放一次后停止。
        /// </summary>
        public bool repeat;
        
        /// <summary>
        /// 闪烁完整的一次时长
        /// </summary>
        public int durationMs = 1000;
        
        /// <summary>
        /// 索引
        /// </summary>
        public int zIndex;
        /// <summary>
        /// 旋转角度
        /// </summary>
        public double? rotation;
    }
}