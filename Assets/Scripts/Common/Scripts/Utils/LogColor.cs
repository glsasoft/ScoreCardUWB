namespace Common.Utils
{
    public static class LogColor
    {
        public const string Request = "#00FFFF";    // 青色
        public const string ResponseOk = "#00FF00"; // 绿色
        public const string ResponseErr = "#FF5555"; // 红色
        public const string Info = "#FFA500";       // 橙色

        public static string Colorize(string msg, string hex) => $"<color={hex}>{msg}</color>";
    }
}