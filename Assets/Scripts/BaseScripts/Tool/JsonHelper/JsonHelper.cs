using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LetsPiu.Base
{
    public static class JsonHelper
    {
        static JsonSerializerSettings m_JsonSetting;

        static JsonSerializerSettings JSON_SETTING
        {
            get
            {
                if (m_JsonSetting == null)
                {
                    m_JsonSetting = new JsonSerializerSettings
                    {
                        DateFormatString = "yyyy-MM-dd HH:mm:ss.fff",
                        Formatting = Formatting.Indented,
                        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                    m_JsonSetting.Converters.Add(new Vector3Converter());
                    m_JsonSetting.Converters.Add(new QuaternionConverter());
                    m_JsonSetting.Converters.Add(new IsoDateTimeConverter() { DateTimeStyles = DateTimeStyles.AssumeUniversal  });
                }
                return m_JsonSetting;
            }
        }

        public static string SerializeObject(object jsonData)
        {
            return JsonConvert.SerializeObject(jsonData, JSON_SETTING);
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JSON_SETTING);
        }
    }
}