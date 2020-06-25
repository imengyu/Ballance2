﻿using SLua;

namespace Ballance2.Managers.CoreBridge
{
    [CustomLuaClass]
    /// <summary>
    /// 游戏通用接收器
    /// </summary>
    public class GameHandler
    {
        /// <summary>
        /// 创建游戏内部使用 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="gameHandlerDelegate">回调</param>
        public GameHandler(string name, GameActionHandlerDelegate gameActionHandlerDelegate)
        {
            Name = name;
            DelegateActionHandler = gameActionHandlerDelegate;
            Type = GameHandlerType.CSKernel;
        }
        /// <summary>
        /// 创建游戏内部使用 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="gameHandlerDelegate">回调</param>
        public GameHandler(string name, GameEventHandlerDelegate gameEventHandlerDelegate)
        {
            Name = name;
            DelegateEventHandler = gameEventHandlerDelegate;
            Type = GameHandlerType.CSKernel;
        }
        /// <summary>
        /// 创建 LUA 层使用的 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="luaModulHandler">LUA Handler （格式：模块名:Modul/lua虚拟脚本名字:主模块函数名称:附带参数）</param>
        public GameHandler(string name, string luaModulHandler)
        {
            Name = name;
            Type = GameHandlerType.LuaModul;
            LuaModulHandler = luaModulHandler;
            LuaModulHandlerFunc = new GameLuaHandler(luaModulHandler);
        }
        /// <summary>
        /// 创建 LUA 层使用的 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="luaFunction">LUA 函数</param>
        public GameHandler(string name, LuaFunction luaFunction, LuaTable self = null)
        {
            Name = name;
            Type = GameHandlerType.LuaFun;
            LuaFunction = luaFunction;
            LuaSelf = self;
        }

        /// <summary>
        /// 创建 LUA 层使用的 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="luaModulHandler">LUA Handler （格式：模块名:Modul/lua虚拟脚本名字:主模块函数名称:附带参数）</param>
        public static GameHandler CreateLuaGameHandler(string name, string luaModulHandler)
        {
            return new GameHandler(name, luaModulHandler);
        }
        /// <summary>
        /// 创建 LUA 层使用的 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="luaFunction">LUA 函数</param>
        public static GameHandler CreateLuaGameHandler(string name, LuaFunction luaFunction, LuaTable self = null)
        {
            return new GameHandler(name, luaFunction, self);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            DelegateActionHandler = null;
            DelegateEventHandler = null;
            LuaModulHandlerFunc = null;
        }

        /// <summary>
        /// 调用接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="pararms">参数</param>
        /// <returns>返回是否中断剩余事件分发/返回Action是否成功</returns>
        public bool CallEventHandler(string evtName, params object[] pararms)
        {
            bool result = false;
            if (Type == GameHandlerType.CSKernel)
                result = DelegateEventHandler(evtName, pararms);
            else if (Type == GameHandlerType.LuaModul)
                result = LuaModulHandlerFunc.RunEventHandler(evtName, pararms);
            else if (Type == GameHandlerType.LuaFun)
            {
                object rs = null;
                object[] pararms2 = new object[pararms.Length + 1];
                pararms2[0] = evtName;
                for (int i = 0; i < pararms.Length; i++)
                    pararms2[i + 1] = pararms[i];
                if (LuaSelf != null) rs = LuaFunction.call(LuaSelf, pararms2);
                else rs = LuaFunction.call(pararms2);
                if (rs is bool)
                    result = (bool)rs;
            }
            return result;
        }
        /// <summary>
        /// 调用操作接收器
        /// </summary>
        /// <param name="pararms">参数</param>
        /// <returns>返回是否中断剩余事件分发/返回Action是否成功</returns>
        public GameActionCallResult CallActionHandler(params object[] pararms)
        {
            GameActionCallResult result = null;
            object rso = null;
            if (Type == GameHandlerType.CSKernel)
                result = DelegateActionHandler(pararms);
            else if (Type == GameHandlerType.LuaModul)
                result = LuaModulHandlerFunc.RunActionHandler(pararms);
            else if (Type == GameHandlerType.LuaFun)
            {
                if (LuaSelf != null) rso = LuaFunction.call(LuaSelf, pararms);
                else rso = LuaFunction.call(pararms);
                if (rso != null && rso is GameActionCallResult)
                    result = rso as GameActionCallResult;
            }
            if (result == null)
                return GameActionCallResult.CreateActionCallResult(false);
            return result;
        }

        /// <summary>
        /// 接收器名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 接收器类型
        /// </summary>
        public GameHandlerType Type { get; private set; }

        public GameActionHandlerDelegate DelegateActionHandler { get; private set; }
        public GameEventHandlerDelegate DelegateEventHandler { get; private set; }

        /// <summary>
        /// LUA Handler
        /// </summary>
        public string LuaModulHandler { get; private set; }
        /// <summary>
        /// LUA Handler 执行体接收器
        /// </summary>
        public GameLuaHandler LuaModulHandlerFunc { get; private set; }
        /// <summary>
        /// LUA Handler 函数
        /// </summary>
        public LuaFunction LuaFunction { get; private set; }
        /// <summary>
        /// LUA Handler 函数的self
        /// </summary>
        public LuaTable LuaSelf { get; private set; }

        public override string ToString()
        {
            return "[" + Type + "] " + Name + (string.IsNullOrEmpty(LuaModulHandler) ?  "" :  (":" + LuaModulHandler));
        }
    }

    [CustomLuaClass]
    /// <summary>
    /// 通用接收器类型
    /// </summary>
    public enum GameHandlerType
    {
        /// <summary>
        /// C# 内核模块使用的
        /// </summary>
        CSKernel,
        /// <summary>
        /// Lua 模块使用的
        /// </summary>
        LuaModul,
        LuaFun
    }

    [CustomLuaClass]
    /// <summary>
    /// 事件接收器内核回调
    /// </summary>
    /// <param name="evtName">事件名称</param>
    /// <param name="pararms">参数</param>
    /// <returns>返回是否中断其他事件的分发</returns>
    public delegate bool GameEventHandlerDelegate(string evtName, params object[] pararms);
    [CustomLuaClass]
    /// <summary>
    /// 操作接收器内核回调
    /// </summary>
    /// <param name="pararms">参数</param>
    /// <returns>返回事件数据</returns>
    public delegate GameActionCallResult GameActionHandlerDelegate(params object[] pararms);

}
