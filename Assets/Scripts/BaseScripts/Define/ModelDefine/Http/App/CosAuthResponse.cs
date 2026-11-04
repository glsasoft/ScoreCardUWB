namespace Common.Network.Model.Http
{
    public class Credentials{
        public string tmpSecretId { get; set; }
        public string tmpSecretKey { get; set; }
        public string sessionToken { get; set; }
    }

    public class CosAuthResponse
    {
        public Credentials credentials { get; set; }
        public string requestId { get; set; }
        public string expiration { get; set; }
        public long startTime { get; set; }
        public long expiredTime { get; set; }
    }
}