using Ballance2.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/*
* Copyright (c) 2020  mengyu
* 
* 模块名：   
* GameLogger.cs
* 
* 用途：
* 全局日志类
* 用于输出日志至控制台或文件。
* 
* 作者：
* mengyu
* 
* 更改历史：
* 2020-4-12 创建
* 
*/

namespace Ballance2
{
    /// <summary>
    /// 全局日志类。
    /// 此类可以将日志写入控制台或是文件
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameLogger
    {
        private static bool on = true;
        private static bool logToFile = false;
        private static StreamWriter logFile = null;

        /// <summary>
        /// 初始化日志记录器。
        /// 请勿调用，此方法由 GameManager 调用。
        /// </summary>
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
        /// <summary>
        /// 销毁日志记录器。
        /// 请勿调用，此方法由 GameManager 调用。
        /// </summary>
        public static void DestroyLogger()
        {
            on = false;
            if (logFile != null)
            {
                logFile = null;
                logFile.Close();
            }
            if (logDatas != null)
                logDatas.Clear();
            logDatas = null;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Log(string tag, string message)
        {
            WriteLog(LogType.Text, tag, message);
        }
        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Warning(string tag, string message)
        {
            WriteLog(LogType.Warning, tag, message);
        }
        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Error(string tag, string message)
        {
            WriteLog(LogType.Error, tag, message);
        }
        /// <summary>
        /// 写入信息日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Info(string tag, string message)
        {
            WriteLog(LogType.Info, tag, message);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Log(string tag, object message)
        {
            WriteLog(LogType.Text, tag, message.ToString());
        }
        /// <summary>
        /// 写入警告日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Warning(string tag, object message)
        {
            WriteLog(LogType.Warning, tag, message.ToString());
        }
        /// <summary>
        /// 写入错误日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Error(string tag, object message)
        {
            WriteLog(LogType.Error, tag, message.ToString());
        }
        /// <summary>
        /// 写入异常日志
        /// </summary>
        /// <param name="e"></param>
        public static void Exception(Exception e)
        {
            WriteLog(LogType.Assert, "", e.ToString());
        }
        /// <summary>
        /// 写入信息日志
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息字符串</param>
        public static void Info(string tag, object message)
        {
            WriteLog(LogType.Info, tag, message.ToString());
        }

        /// <summary>
        /// 写入日志（格式化字符串）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息格式化字符串</param>
        /// <param name="param"></param>
        public static void Log(string tag, string message, params object []param)
        {
            message = string.Format(message, param);
            WriteLog(LogType.Text, tag, message, param);
        }
        /// <summary>
        /// 写入警告日志（格式化字符串）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息格式化字符串</param>
        /// <param name="param"></param>
        public static void Warning(string tag, string message, params object[] param)
        {
            message = string.Format(message, param);
            WriteLog(LogType.Warning, tag, message, param);
        }
        /// <summary>
        /// 写入错误日志（格式化字符串）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息格式化字符串</param>
        /// <param name="param"></param>
        public static void Error(string tag, string message, params object[] param)
        {
            message = string.Format(message, param);
            WriteLog(LogType.Error, tag, message, param);
        }
        /// <summary>
        /// 写入信息日志（格式化字符串）
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="message">信息格式化字符串</param>
        /// <param name="param"></param>
        public static void Info(string tag, string message, params object[] param)
        {
            message = string.Format(message, param);
            WriteLog(LogType.Info, tag, message, param);
        }

        /// <summary>
        /// 设置日志记录器是否启用
        /// </summary>
        /// <param name="enabled">是否启用</param>
        public static void SetLoggerEnabled(bool enabled)
        {
            on = enabled;
        }

        /// <summary>
        /// 日志类型
        /// </summary>
        [SLua.CustomLuaClass]
        public enum LogType
        {
            /// <summary>
            /// 文本
            /// </summary>
            Text = 3,
            /// <summary>
            /// 信息
            /// </summary>
            Info = 5,
            /// <summary>
            /// 警告
            /// </summary>
            Warning = 2,
            /// <summary>
            /// 错误
            /// </summary>
            Error = 0,
            /// <summary>
            /// 断言错误
            /// </summary>
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
        private static bool unityLogLock = false;

        /// <summary>
        /// 获取某种类型的日志记录条数
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <returns>返回条数</returns>
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

        internal static bool GetUnityLogLock()
        {
            if (unityLogLock)
            {
                unityLogLock = false;
                return true;
            }
            return false;
        }
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
                data.Data = string.Format("[{0}] [{1}] {2}", GetNowDateString(), tag, message);//追加时间和tag
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

#if UNITY_EDITOR
                unityLogLock = true;
                string unityOutPutLog = string.Format("[{0}] {1}", tag, message);
                switch (type)
                {
                    case LogType.Text: UnityEngine.Debug.Log(unityOutPutLog); break;
                    case LogType.Error: UnityEngine.Debug.LogError(unityOutPutLog); break;
                    case LogType.Assert: UnityEngine.Debug.LogAssertion(unityOutPutLog); break;
                    case LogType.Info: UnityEngine.Debug.Log(unityOutPutLog); break;
                    case LogType.Warning: UnityEngine.Debug.LogWarning(unityOutPutLog); break;
                }
#endif

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

        //删除超出最大条数的日志
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
