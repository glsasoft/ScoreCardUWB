using System;

namespace Common.Network.Model.Http
{
    /// <summary>
    /// http请求返回的基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class HttpJsonResponse<T>
    {
        public int return_code;
        public bool success;
        public string msg;
        public string version;
        public T data;
    }
}