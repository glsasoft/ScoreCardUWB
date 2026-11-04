namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App
{
    public class LatLng
    {
        /// <summary>
        /// 经度
        /// </summary>
        public double longitude;
        /// <summary>
        /// 纬度
        /// </summary>
        public double latitude;

        public LatLng(double lng, double lat)
        {
            longitude = lng;
            latitude = lat;
        }
    }
}