using JetBrains.Annotations;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 图像Marker参数
    /// </summary>
    public class IconMarkerParams
    {
        [CanBeNull] public string imageUrl;
        [CanBeNull] public MapPoint point;
        public int? offsetX = null;
        public int? offsetY = null;
        public int? width = null;
        public int? height = null;
        public double? rotation = null;
        public string title = null;
        public string titleStyle = null;
        public int? zIndex = null;

        public IconMarkerParams()
        {
        }

        public IconMarkerParams(MapPoint point, string imageUrl, int width, int height)
        {
            this.point = point;
            this.imageUrl = imageUrl;
            this.width = width;
            this.height = height;
            rotation = 0;
            title = null;
            zIndex = 1;
            offsetX = 0;
            offsetY = 0;
            titleStyle = null;
        }
    }
}