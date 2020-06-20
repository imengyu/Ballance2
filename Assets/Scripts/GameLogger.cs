using Ballance2.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Ballance2
{
    /// <summary>
    /// 日志和输出
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameLogger
    {
        private static bool on = true;
        private static bool logToFile = false;
        private static StreamWriter logFile = null;

        public static void InitLogger()
        {
            logDatas = new List<LogData>();
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
        public static void DestroyLogger()
        {
            if (logFile != null)
            {
                logFile = null;
                logFile.Close();
            }
            if (logDatas != null)
                logDatas.Clear();
            logDatas = null;
        }

        public static void Log(string tag, object message)
        {
            WriteLog(LogType.Text, tag, message.ToString());
        }
        public static void Warning(string tag, object message)
        {
            WriteLog(LogType.Warning, tag, message.ToString());
        }
        public static void Error(string tag, object message)
        {
            WriteLog(LogType.Error, tag, message.ToString());
        }
        public static void Exception(Exception e)
        {
            WriteLog(LogType.Assert, "", e.ToString());
        }
        public static void Info(string tag, object message)
        {
            WriteLog(LogType.Info, tag, message.ToString());
        }

        public static void Log(string tag, string message, params object []param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            WriteLog(LogType.Text, tag, format, param);
        }
        public static void Warning(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            WriteLog(LogType.Warning, tag, format, param);
        }
        public static void Error(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            WriteLog(LogType.Error, tag, format, param);
        }
        public static void Info(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            WriteLog(LogType.Info, tag, format, param);
        }

        [SLua.CustomLuaClass]
        /// <summary>
        /// 日志类型
        /// </summary>
        public enum LogType
        {
            Text = 3,
            Info = 5,
            Warning = 2,
            Error = 0,
            Assert = 1,
            Max = 6,
        }

        internal struct LogData
        {
            public LogType Type;
            public string Data;
            public string StackTrace;
        }
        internal static string GetNowDateString()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
        private static List<LogData> logDatas = null;
        private static LogCallback logCallback = null;
        private static int countError = 0;
        private static int countWarning = 0;
        private static int countInfo = 0;

        public static int GetLogCount(LogType type)
        {
            switch (type)
            {
                case LogType.Text: return countInfo;
                case LogType.Error:
                case LogType.Assert: return countError;
                case LogType.Info: return countInfo;
                case LogType.Warning: return countWarning;
            }
            return 0;
        }

        internal delegate void LogCallback(LogData data);
        internal static List<LogData> GetLogData()  { return logDatas;  }

        internal static void RegisterLogCallback(LogCallback logCallback)
        {
            GameLogger.logCallback = logCallback;
        }
        internal static void UnRegisterLogCallback()
        {
            GameLogger.logCallback = null;
        }
        internal static void ClearAllLogs()
        {
            countError = 0;
            countWarning = 0;
            countInfo = 0;
            logDatas.Clear();
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">内容</param>
        public static void WriteLog(LogType type, string tag, string message)
        {
            if (on)
            {
                LogData data = new LogData();
                data.Type = type;
                data.Data = string.Format("[{0}] {1}", GetNowDateString(), message);
                data.StackTrace = new StackTrace(true).ToString();

                if (logToFile)
                    logFile.WriteLine(data.Data);
                if (logCallback != null)
                    logCallback(data);
                logDatas.Add(data);
                DeleteExcessLogs();
                if (logDatas.Count > GameConst.GameLoggerBufferMax)
                    logDatas.RemoveAt(0);

                switch (type)
                {
                    case LogType.Text: countInfo++; break;
                    case LogType.Error:
                    case LogType.Assert: countError++; break;
                    case LogType.Info: countInfo++; break;
                    case LogType.Warning: countWarning++; break;
                }

            }
        }
        /// <summary>
        /// 格式化写入日志
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="message">内容</param>
        /// <param name="param">可变参数</param>
        public static void WriteLog(LogType type, string tag, string message, params object[] param)
        {
            WriteLog(type, tag, string.Format(message, param));
        }

        private static void DeleteExcessLogs()
        {
            var amountToRemove = Mathf.Max(logDatas.Count -
                GameConst.GameLoggerBufferMax, 0);
            if (amountToRemove == 0)
                return;
            for (int i = amountToRemove - 1; i >= 0; i--)
            {
                switch (logDatas[i].Type)
                {
                    case LogType.Text: countInfo--; break;
                    case LogType.Error:
                    case LogType.Assert: countError--; break;
                    case LogType.Info: countInfo--; break;
                    case LogType.Warning: countWarning--; break;
                }
                logDatas.RemoveAt(i);
            }
           
        }
    }
}
