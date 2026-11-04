using System.Collections.Generic;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 绘制折线参数
    /// </summary>
    public class PolyLineParams
    {
        public List<MapPoint> points;
        public int zIndex = 1;
        /// <summary>
        /// 折线颜色 css颜色
        /// </summary>
        public string strokeColor = "blue";
        /// <summary>
        /// 折线的宽度，以像素为单位 
        /// </summary>
        public int strokeWeight = 6;
        /// <summary>
        /// 折线的透明度，取值范围0 - 1 
        /// </summary>
        public float strokeOpacity = 0.5f;
        /// <summary>
        /// 设置是为实线或虚线，solid或dashed 
        /// </summary>
        public string strokeStyle = "dashed";
    }
}