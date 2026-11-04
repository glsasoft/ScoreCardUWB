using System;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LetsPiu.Base.BaseScripts.Utils
{
    public static class LongLogger
    {
        // Unity控制台单条日志最大长度限制（实际为16382，这里取保守值）
        private const int MAX_CONSOLE_LOG_LENGTH = 15000;
        
        // 默认日志文件路径（Application.persistentDataPath + "/Logs/log.txt"）
        public static string LogFilePath { get; set; } 
            = Path.Combine(Application.persistentDataPath, "Logs", "log.txt");
        
        // 是否启用文件日志记录
        public static bool EnableFileLogging { get; set; } = false;
        
        /// <summary>
        /// 打印长日志（自动分割超长内容）
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="context">关联对象（用于Unity点击日志定位）</param>
        public static void Log(string message, Object context = null)
        {
            if (string.IsNullOrEmpty(message)) return;

            // 控制台日志分割输出
            if (message.Length <= MAX_CONSOLE_LOG_LENGTH)
            {
                Debug.Log(message, context);
            }
            else
            {
                Debug.Log("===== 长日志开始 =====", context);
                
                int chunkCount = 0;
                for (int i = 0; i < message.Length; i += MAX_CONSOLE_LOG_LENGTH)
                {
                    int length = Math.Min(MAX_CONSOLE_LOG_LENGTH, message.Length - i);
                    string chunk = $"[Part {++chunkCount}]\n" + message.Substring(i, length);
                    Debug.Log(chunk, context);
                }
                
                Debug.Log($"===== 长日志结束（共{chunkCount}部分） =====", context);
            }

            // 文件日志记录（完整内容）
            if (EnableFileLogging)
            {
                WriteToFile(message);
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        private static void WriteToFile(string message)
        {
            try
            {
                // 确保日志目录存在
                string logDir = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // 添加时间戳和换行
                string logEntry = $"\n\n[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}]\n{message}";
                
                // 写入文件
                File.AppendAllText(LogFilePath, logEntry, Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"日志文件写入失败: {e.Message}");
            }
        }

        /// <summary>
        /// 清空日志文件
        /// </summary>
        public static void ClearLogFile()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.WriteAllText(LogFilePath, string.Empty);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"清空日志文件失败: {e.Message}");
            }
        }
    }
}
