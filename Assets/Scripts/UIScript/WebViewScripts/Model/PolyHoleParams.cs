using System.Collections.Generic;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 绘制围栏参数
    /// </summary>
    public class PolyHoleParams
    {
        public List<MapPoint> points;
        public float fillOpacity = 0.5f;
        /// <summary>
        /// 遮罩范围(米)
        /// </summary>
        public int backgroundSize;
        public int zIndex = 1;
        public string fillColor = "black";
        public string strokeColor = "black";
        /// <summary>
        /// 边框线条宽度
        /// </summary>
        public int strokeWeight = 0;
        public float strokeOpacity = 1;
    }
}