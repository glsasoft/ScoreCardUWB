using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class FileLogger
{
    private static ConcurrentQueue<LogData> _logQueue = new ConcurrentQueue<LogData>();
    private static Thread _writeThread;
    private static bool _isRunning = true;
    private static string _logFilePath;
    private static readonly object _fileLock = new object();

    private struct LogData
    {
        public string message;
        public string stackTrace;
        public LogType logType;
        public string timeStamp;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Application.isEditor)
            return;

        string logPath = Path.Combine(Application.persistentDataPath, "PIU_Log.txt");
        string lastLogPath = Path.Combine(Application.persistentDataPath, "PIU_LastLog.txt");

        if(File.Exists(lastLogPath))
        {
            File.Delete(lastLogPath);
        }
        if (File.Exists(logPath))
        {
            File.Move(logPath, lastLogPath);
        }

        _logFilePath = logPath;

        _writeThread = new Thread(WriteLogToFile)
        {
            IsBackground = true,
            Priority = System.Threading.ThreadPriority.BelowNormal
        };
        _writeThread.Start();

        Application.logMessageReceivedThreaded += HandleUnityLog;
        Application.quitting += () => 
        {
            Application.logMessageReceivedThreaded -= HandleUnityLog;
            _isRunning = false;
            _writeThread?.Join(1000);
        };
    }

    private static void HandleUnityLog(string condition, string stackTrace, LogType type)
    {
        _logQueue.Enqueue(new LogData
        {
            message = condition,
            stackTrace = stackTrace,
            logType = type,
            timeStamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    private static void WriteLogToFile()
    {
        try
        {
            using (StreamWriter _writer = new StreamWriter(_logFilePath, true, new System.Text.UTF8Encoding(false)))
            {
                while (_isRunning || !_logQueue.IsEmpty)
                {
                    if (_logQueue.TryDequeue(out LogData log))
                    {
                        lock (_fileLock)
                        {
                            string logEntry = $"[{log.timeStamp}] [{log.logType}] {log.message}";
                            if (log.logType == LogType.Error || log.logType == LogType.Exception)
                            {
                                logEntry += $"\nStackTrace: {log.stackTrace}";
                            }

                            _writer.WriteLine(logEntry);
                            if (_logQueue.Count % 10 == 0)
                            {
                                _writer.Flush();
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
                _writer.Flush();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"FileLogger error: {ex.Message}");
        }
    }
}
