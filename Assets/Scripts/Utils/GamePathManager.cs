﻿using Ballance2.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.Utils
{
    /// <summary>
    /// 路径管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GamePathManager
    {
        /// <summary>
        /// 调试模组包存放路径
        /// </summary>
        public const string DEBUG_MOD_FOLDER = "Assets/Mods";

        /// <summary>
        /// 调试路径（输出目录）<c>（您在调试时请点击菜单 "Ballance">"开发设置">"Debug Settings" 将其更改为自己调试输出存放目录）</c>
        /// </summary>
        public static string DEBUG_PATH 
        {
            get
            {
                DebugSettings debugSettings = DebugSettings.Instance;
                if (debugSettings != null)
                    return debugSettings.DebugFolder.Replace("\\", "/");
                return "";
            }
        }
        /// <summary>
        /// 调试路径（模组目录）
        /// </summary>
        public static string DEBUG_MODS_PATH { get { return DEBUG_PATH + "Mods/"; } }
        /// <summary>
        /// 调试路径（关卡目录）
        /// </summary>
        public static string DEBUG_LEVELS_PATH { get { return DEBUG_PATH + "Levels/"; } }

        /// <summary>
        /// 安卓系统数据目录
        /// </summary>
        public const string ANDROID_FOLDER_PATH = "/sdcard/games/com.imengyu.ballance2/";
        /// <summary>
        /// 安卓系统模组目录
        /// </summary>
        public const string ANDROID_MODS_PATH = ANDROID_FOLDER_PATH + "Mods/";
        /// <summary>
        /// 安卓系统关卡目录
        /// </summary>
        public const string ANDROID_LEVELS_PATH = ANDROID_FOLDER_PATH + "Levels/";






        /// <summary>
        /// 检测是否是绝对路径
        /// </summary>
        /// <param name="path">路径</param>
        public static bool IsAbsolutePath(string path)
        {
#if (UNITY_EDITOR && UNITY_EDITOR_WIN) || UNITY_STANDALONE_WIN
            if (path.Length > 2)
                return path[1] == ':' && ((path.Length > 3 && path[2] == '\\' && path[3] == '\\') || path[2] == '/');
#elif (UNITY_EDITOR && UNITY_EDITOR_OSX) || UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            return path.StartsWith("/");
#elif UNITY_IOS
            return path.StartsWith("/");
#endif
            return false;
        }
        /// <summary>
        /// 将资源的相对路径转为资源真实路径
        /// </summary>
        /// <param name="type">资源种类（gameinit、core: 核心文件、level：关卡、mod：模组）</param>
        /// <param name="pathorname">相对路径或名称</param>
        /// <param name="replacePlatform">是否替换文件路径中的[Platform]</param>
        /// <returns></returns>
        public static string GetResRealPath(string type, string pathorname, bool replacePlatform = true)
        {
            string pathbuf = "";
            string[] spbuf = null;

            if (replacePlatform && pathorname.Contains("[Platform]"))
                pathorname = pathorname.Replace("[Platform]", GameConst.GamePlatformIdentifier);

            spbuf = SplitResourceIdentifier(pathorname, out pathbuf);

            if (type == "" && pathorname.Contains(":"))
                type = spbuf[0].ToLower();

            if (type == "gameinit")
            {
#if UNITY_EDITOR
                return DEBUG_PATH + "core/core.gameinit.txt";
#elif UNITY_STANDALONE || UNITY_ANDROID
                    return Application.dataPath + "/core/core.gameinit.txt";
#elif UNITY_IOS
                    return Application.streamingAssetsPath + "/core/core.gameinit.txt";
#endif
            }
            else if (type == "level") return GetLevelRealPath(pathbuf);
            else if (type == "mod")
            {
                if (pathbuf.Contains(":"))
                {
                    if (IsAbsolutePath(pathbuf)) return pathbuf;
#if UNITY_EDITOR
                    pathbuf = DEBUG_MODS_PATH + pathbuf;
#elif UNITY_STANDALONE
                    pathbuf= Application.dataPath + "/mods/" + pathbuf;
#elif UNITY_ANDROID
                    pathbuf = ANDROID_MODS_PATH + pathbuf;
#elif UNITY_IOS
                    pathbuf = pathbuf;
#endif
                    return ReplacePathInResourceIdentifier(pathbuf, ref spbuf);
                }
                else
                {
#if UNITY_EDITOR
                    return DEBUG_MODS_PATH + pathbuf;
#elif UNITY_STANDALONE
                    return Application.dataPath + "/mods/" + pathbuf;
#elif UNITY_ANDROID
                    return ANDROID_MODS_PATH + pathbuf;
#elif UNITY_IOS
                    return pathorname;
#endif
                }
            }
            else if (type == "core")
            {
                if (pathbuf.Contains(":"))
                {
                    if (IsAbsolutePath(pathbuf)) return pathbuf;
#if UNITY_EDITOR
                    pathbuf = DEBUG_PATH + "core/" + pathbuf;
#elif UNITY_STANDALONE || UNITY_ANDROID
                    pathbuf = Application.dataPath + "/core/" + pathbuf;
#elif UNITY_IOS
                    pathbuf = Application.streamingAssetsPath + "/core/" + pathbuf;
#endif
                    return ReplacePathInResourceIdentifier(pathbuf, ref spbuf);
                }
                else
                {
#if UNITY_EDITOR
                    return DEBUG_PATH + "core/" + pathbuf;
#elif UNITY_STANDALONE || UNITY_ANDROID
                    return Application.dataPath + "/core/" + pathbuf;
#elif UNITY_IOS
                    return Application.streamingAssetsPath + "/core/" + pathbuf;
#endif
                }
            }
            return pathorname;
        }
        /// <summary>
        /// 将关卡资源的相对路径转为关卡资源真实路径
        /// </summary>
        /// <param name="pathorname">关卡的相对路径或名称</param>
        /// <returns></returns>
        public static string GetLevelRealPath(string pathorname)
        {
            string pathbuf = "";
            string[] spbuf = null;

            if (pathorname.Contains(":"))
            {
                spbuf = SplitResourceIdentifier(pathorname, out pathbuf);

                if (IsAbsolutePath(pathbuf)) return pathbuf;
#if UNITY_EDITOR
                pathbuf = DEBUG_LEVELS_PATH + pathbuf;
#elif UNITY_STANDALONE
                pathbuf= Application.dataPath + "/levels/" + pathbuf;
#elif UNITY_ANDROID
                pathbuf= ANDROID_LEVELS_PATH + pathbuf;
#elif UNITY_IOS
                pathbuf = pathbuf;
#endif
                return ReplacePathInResourceIdentifier(pathbuf, ref spbuf);
            }
            else
            {
#if UNITY_EDITOR
                return DEBUG_LEVELS_PATH + pathorname;
#elif UNITY_STANDALONE
                return Application.dataPath + "/levels/" + pathorname;
#elif UNITY_ANDROID
                return ANDROID_LEVELS_PATH + pathorname;
#elif UNITY_IOS
                return pathorname;
#endif
            }
        }
        /// <summary>
        /// Replace Path In Resource Identifier (Identifier:Path:Arg0:Arg1)
        /// </summary>
        /// <param name="oldIdentifier">Identifier Want br replace</param>
        /// <param name="newPath"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static string ReplacePathInResourceIdentifier(string newPath, ref string[] buf)
        {
            if (buf.Length > 1)
            {
                buf[1] = newPath;
                string s = "";
                foreach (string si in buf)
                    s += ":" + si;
                return s.Remove(0, 1);
            }
            return newPath;
        }
        /// <summary>
        /// 分割资源标识符
        /// </summary>
        /// <param name="oldIdentifier">资源标识符</param>
        /// <param name="outPath">输出资源路径</param>
        /// <returns></returns>
        public static string[] SplitResourceIdentifier(string oldIdentifier, out string outPath)
        {
            string[] buf = oldIdentifier.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (buf.Length > 2)
            {
                if (buf[1].Length == 1 && (buf[2].StartsWith("/") || buf[2].StartsWith("\\")))
                {
                    string[] newbuf = new string[buf.Length - 1];
                    newbuf[0] = buf[0];
                    newbuf[1] = buf[1] + buf[2];
                    for (int i = 2; i < newbuf.Length; i++)
                        newbuf[i] = buf[i + 1];
                    buf = newbuf;
                }
            }
            if (buf.Length > 1) outPath = buf[1];
            else outPath = oldIdentifier;
            return buf;
        }
        /// <summary>
        /// 获取路径中文件名（不包括后缀）
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string GetFileNameWithoutExt(string path)
        {
            bool chooseRight = path.Contains("/") && !path.Contains("\\");
            int lastPos = path.LastIndexOf(chooseRight ? '/' : '\\');
            if (lastPos > 0)
            {
                if (lastPos == path.Length - 1)
                {
                    path = path.Remove(path.Length - 1);
                    lastPos = path.LastIndexOf(chooseRight ? '/' : '\\');
                }
                path = path.Substring(lastPos + 1);
                if (path.Contains("."))
                {
                    lastPos = path.LastIndexOf('.');
                    path = path.Substring(0, lastPos);
                }
            }
            return path;
        }
        /// <summary>
        /// 获取路径中文件名（包括后缀）
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            bool chooseRight = path.Contains("/") && !path.Contains("\\");
            int lastPos = path.LastIndexOf(chooseRight ? '/' : '\\');
            if (lastPos > 0)
            {
                if (lastPos == path.Length - 1)
                {
                    path = path.Remove(path.Length - 1);
                    lastPos = path.LastIndexOf(chooseRight ? '/' : '\\');
                }
                path = path.Substring(lastPos + 1);
            }
            return path;
        }
    }
}
