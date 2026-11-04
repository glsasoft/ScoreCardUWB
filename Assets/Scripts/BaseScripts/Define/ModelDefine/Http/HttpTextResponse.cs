using System;

namespace Common.Network.Model.Http
{
    public class HttpTextResponse
    {
        public Exception error = null;
        public bool success = false;
        public string result;
        public long responseCode;
    }
}