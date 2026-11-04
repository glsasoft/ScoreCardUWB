using JetBrains.Annotations;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 带边框图像Marker参数
    /// </summary>
    public class ImageMarkerParams
    {
        public string imageUrl;
        public MapPoint point = null;
        public int width = 0;
        public int height = 0;
        public int zindex = 0;
        public int borderWidth = 0;
        public string borderColor = "white";
        public int borderRadius = 0;
        /// <summary>
        /// 旋转度数
        /// </summary>
        public int? rotation = null;
    }
}