using Newtonsoft.Json;

namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    /// <summary>
    /// 带文本标签的标记点参数
    /// </summary>
    public class LabeledMarkerParams
    {
        /// <summary>
        /// 位置
        /// </summary>
        [JsonProperty("point")]
        public MapPoint point { get; set; }

        /// <summary>
        /// 图标URL
        /// </summary>
        [JsonProperty("iconUrl")]
        public string iconUrl { get; set; }

        /// <summary>
        /// 标签文本
        /// </summary>
        [JsonProperty("labelText")]
        public string labelText { get; set; }

        /// <summary>
        /// 标签颜色
        /// </summary>
        [JsonProperty("labelColor")]
        public string labelColor { get; set; }

        /// <summary>
        /// 标记宽度
        /// </summary>
        [JsonProperty("markerWidth")]
        public int markerWidth { get; set; } = 32;

        /// <summary>
        /// 标记高度
        /// </summary>
        [JsonProperty("markerHeight")]
        public int markerHeight { get; set; } = 40;

        /// <summary>
        /// 字体大小
        /// </summary>
        [JsonProperty("fontSize")]
        public int fontSize { get; set; } = 14;

        /// <summary>
        /// 标签Y轴偏移量
        /// </summary>
        [JsonProperty("labelOffsetY")]
        public int labelOffsetY { get; set; } = 10;

        /// <summary>
        /// 层叠顺序
        /// </summary>
        [JsonProperty("zIndex")]
        public int zIndex { get; set; } = 0;
    }
}