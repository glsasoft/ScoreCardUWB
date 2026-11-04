using System.Collections.Generic;
using System.Linq;
using LetsPiu.Define;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    public class PointInfo
    {
        /// <summary>
        /// 当前编辑的形状
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EditShape currentType;
        /// <summary>
        /// 当前电子栅栏中心点
        /// </summary>
        public MapPoint center;
        /// <summary>
        /// 当前形状对应坐标点
        /// </summary>
        public List<MapPoint> points = new List<MapPoint>();
        /// <summary>
        /// 半径/边长的一半
        /// </summary>
        public float radius = 500;
        /// <summary>
        /// 旋转度数
        /// </summary>
        public float rotation;
        /// <summary>
        /// 是否是合法的多边形
        /// </summary>
        public bool isValidPolygon;
        
        public PointInfo()
        {
        }
    }
}