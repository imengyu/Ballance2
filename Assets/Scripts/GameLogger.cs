using Ballance2.Config;
using System;
using System.Collections.Generic;
using System.IO;
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
            logDatas = new List<LogData>();
            logDatasNotSend = new List<LogData>();
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
            if (logFile != null)
            {
                logFile = null;
                logFile.Close();
            }
            if (logDatasNotSend != null)
                logDatasNotSend.Clear();
            logDatasNotSend = null;
            if (logDatas != null)
                logDatas.Clear();
            logDatas = null;
        }

        public void Log(string tag, object message)
        {
            UnityEngine.Debug.Log(string.Format("[{0}] {1}", tag, message));
            WriteLog(LogType.Text, tag, message.ToString());
        }
        public void Warning(string tag, object message)
        {
            UnityEngine.Debug.LogWarning(string.Format("[{0}] {1}", tag, message));
            WriteLog(LogType.Warning, tag, message.ToString());
        }
        public void Error(string tag, object message)
        {
            UnityEngine.Debug.LogError(string.Format("[{0}] {1}", tag, message));
            WriteLog(LogType.Error, tag, message.ToString());
        }
        public void Exception(Exception e)
        {
            UnityEngine.Debug.LogException(e);
            WriteLog(LogType.Assert, "", e.ToString());
        }
        public void Info(string tag, object message)
        {
            UnityEngine.Debug.Log(string.Format("[{0}] {1}", tag, message));
            WriteLog(LogType.Info, tag, message.ToString());
        }

        public void Log(string tag, string message, params object []param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            UnityEngine.Debug.LogFormat(format, param);
            WriteLog(LogType.Text, tag, format, param);
        }
        public void Warning(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            UnityEngine.Debug.LogWarningFormat(format, param);
            WriteLog(LogType.Warning, tag, format, param);
        }
        public void Error(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            UnityEngine.Debug.LogErrorFormat(format, param);
            WriteLog(LogType.Error, tag, format, param);
        }
        public void Info(string tag, string message, params object[] param)
        {
            string format = string.Format("[{0}] {1}", tag, message);
            UnityEngine.Debug.LogFormat(format, param);
            WriteLog(LogType.Info, tag, format, param);
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

        internal struct LogData
        {
            public LogType Type;
            public string Data;
            public bool Received;
        }
        internal string GetNowDateString()
        {
            return DateTime.Now.ToString("H:i:s");
        }
        private List<LogData> logDatas = null;
        private List<LogData> logDatasNotSend = null;
        private LogCallback logCallback = null;
        private int countError = 0;
        private int countWarning = 0;
        private int countInfo = 0;

        public int GetLogCount(LogType type)
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

        internal delegate void LogCallback(LogType type, string content);
        internal List<LogData> GetLogData()  { return logDatas;  }

        internal void ReSendNotReceivedLogs()
        {
            if(logCallback != null)
            {
                for (int i = 0; i < logDatasNotSend.Count; i++)
                    logCallback(logDatasNotSend[i].Type, logDatasNotSend[i].Data);
                logDatasNotSend.Clear();
            }
        }
        internal void RegisterLogCallback(LogCallback logCallback)
        {
            this.logCallback = logCallback;
        }
        internal void UnRegisterLogCallback()
        {
            this.logCallback = null;
        }
        internal void ClearAllLogs()
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
        public void WriteLog(LogType type, string tag, string message)
        {
            if (on)
            {
                LogData data = new LogData();
                data.Type = type;
                data.Received = false;
                data.Data = string.Format("[{0}/{1}] [{2}] {3}", GetNowDateString(), type, tag, message.ToString());
               
                if(logToFile)
                    logFile.WriteLine(data.Data);
                if (logCallback != null)
                {
                    logCallback(type, data.Data);
                    logDatas.Add(data);
                    DeleteExcessLogs();
                }
                else if(logDatasNotSend.Count < GameConst.GameLoggerBufferMax)
                    logDatasNotSend.Add(data);
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
        public void WriteLog(LogType type, string tag, string message, params object[] param)
        {
            WriteLog(type, tag, string.Format(message, param));
        }

        private void DeleteExcessLogs()
        {
            var amountToRemove = Mathf.Max(logDatas.Count -
                GameConst.GameLoggerBufferMax, 0);
            if (amountToRemove == 0)
                return;
            for (int i = amountToRemove; i >= 0; i--)
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
