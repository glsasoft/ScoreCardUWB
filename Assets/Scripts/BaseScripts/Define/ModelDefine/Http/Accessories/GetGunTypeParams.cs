using Newtonsoft.Json;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    public class GetGunTypeParams
    {
        /// <summary>
        /// 枪的subType 不传查询所有
        /// </summary>
        [JsonProperty("id")]
        public int? id;
    }
}
