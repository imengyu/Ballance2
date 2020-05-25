using Ballance2.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2
{
    /// <summary>
    /// 日志和输出
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameLogger
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static GameLogger Instance { get; private set; }

        public static void Init()
        {
            Instance = new GameLogger();
        }
        public static void Destroy()
        {
            Instance.DestroyLogger();
        }

        public GameLogger()
        {
            InitLogger();
        }

        private bool on = true;
        private bool logToFile = false;
        private StreamWriter logFile = null;

        public void InitLogger()
        {
            on = GameConst.GameLoggerOn;
            if (on)
            {
                if(GameConst.GameLoggerLogToFile && !string.IsNullOrEmpty(GameConst.GameLoggerLogFile))
                {
                    try
                    {
                        logFile = new StreamWriter(GameConst.GameLoggerLogFile);
                        logFile.AutoFlush = true;
                        logToFile = true;
                    }
                    catch(Exception e)
                    {
                        Exception(e);
                    }
                }
            }
        }
        public void DestroyLogger()
        {
            if(logFile != null)
            {
                logFile = null;
                logFile.Close();
            }
            logDatas.Clear();
            logDatas = null;
        }

        public void Log(object message)
        {
            Debug.Log(message);
        }
        public void Warning(object message)
        {
            Debug.LogWarning(message);
        }
        public void Error(object message)
        {
            Debug.LogError(message);
        }
        public void Exception(Exception e)
        {
            Debug.LogException(e);
        }
        public void Info(object e)
        {
            Debug.Log(e);
        }

        public void Log(string message, params object []param)
        {
            Debug.LogFormat(message, param);
        }
        public void Warning(string message, params object[] param)
        {
            Debug.LogWarningFormat(message, param);
        }
        public void Error(string message, params object[] param)
        {
            Debug.LogErrorFormat(message, param);
        }
        public void Info(string message, params object[] param)
        {
            Debug.LogFormat(message, param);
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        public enum LogType
        {
            Text,
            Info,
            Warning,
            Error,
            Assert
        }

        private struct LogData
        {
            public LogType Type;
            public string Data;
        }
        private string GetNowDateString()
        {
            return DateTime.Now.ToString("o");
        }
        private List<LogData> logDatas = new List<LogData>();

        public void LoggerGUI()
        {

        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">内容</param>
        public void WriteLog(LogType type, object message)
        {
            LogData data = new LogData();
            data.Type = type;
            data.Data = string.Format("[{0}/{1}] {2}", GetNowDateString(), type, message.ToString());
            logDatas.Add(data);
            logFile.WriteLine(data.Data);
        }
        /// <summary>
        /// 格式化写入日志
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">内容</param>
        /// <param name="param">可变参数</param>
        public void WriteLog(LogType type, string message, params object[] param)
        {
            WriteLog(type, string.Format(message, param));
        }
    }
}
