namespace LetsPiu.UI.UIScript.WebViewScripts.Model
{
    public class MapPoint
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double lng;
        /// <summary>
        /// 纬度
        /// </summary>
        public double lat;

        public MapPoint(double lng, double lat)
        {
            this.lng = lng;
            this.lat = lat;
        }
    }
}