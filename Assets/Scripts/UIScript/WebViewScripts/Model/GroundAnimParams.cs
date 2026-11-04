namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 地图贴图动画
    /// </summary>
    public class GroundAnimParams
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
        /// 单帧的高度（以像素为单位）。
        /// </summary>
        public int frameHeight;

        /// <summary>
        /// 帧率（Frames Per Second），表示每秒播放的帧数。
        /// </summary>
        public int fps;

        /// <summary>
        /// 动画的宽度（以米为单位）。
        /// </summary>
        public float width;

        /// <summary>
        /// 动画的高度（以米为单位）。
        /// </summary>
        public float height;

        /// <summary>
        /// 是否重复播放动画。如果为 true，动画将循环播放；如果为 false，动画播放一次后停止。
        /// </summary>
        public bool repeat;
    }
}