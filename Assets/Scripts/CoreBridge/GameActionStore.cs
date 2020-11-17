﻿using Ballance2.Utils;
using SLua;
using System.Collections.Generic;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameActionStore.cs
 * 用途：
 * 存储操作的类。向每个模块提供统一的操作接口。
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 操作存储库
    /// </summary>
    public class GameActionStore
    {
        /// <summary>
        /// 创建操作存储库
        /// </summary>
        /// <param name="packageName">所属包名</param>
        public GameActionStore(string packageName)
        {
            _PackageName = packageName;
            actions = new Dictionary<string, GameAction>();
        }

        public void Destroy()
        {
            if (actions != null)
            {
                foreach (var v in actions)
                    v.Value.Dispose();
                actions.Clear();
                actions = null;
            }
        }

        private string _PackageName;
        private Dictionary<string, GameAction> actions = null;

        /// <summary>
        /// 标签
        /// </summary>
        public string TAG { get { return _PackageName + ":GameActionStore"; } }
        /// <summary>
        /// 获取此存储库的所有操作
        /// </summary>
        public Dictionary<string, GameAction> Actions { get { return actions; } }
        /// <summary>
        /// 获取此存储库的包名
        /// </summary>
        public string PackageName { get { return _PackageName; } }

        /// <summary>
        /// 注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handlerName">接收器名称</param>
        /// <param name="handler">接收函数</param>
        /// <param name="callTypeCheck">函数参数检查，数组长度规定了操作需要的参数，
        /// 组值是一个或多个允许的类型名字，例如 UnityEngine.GameObject System.String 。
        /// 如果一个参数允许多种类型，可使用/分隔。
        /// 如果不需要参数检查，也可以为null，则当前操作将不会进行类型检查
        /// </param>
        /// <returns>返回注册的操作实例，如果注册失败则返回 null ，请查看 LastError 的值</returns>
        public GameAction RegisterAction(string name, string handlerName, GameActionHandlerDelegate handler, string[] callTypeCheck)
        {
            return RegisterAction(name, new GameHandler(handlerName, handler), callTypeCheck);
        }
        /// <summary>
        /// 注册操作(LUA)
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handlerName">接收器名称</param>
        /// <param name="luaFunction">LUA接收函数</param>
        /// <param name="self">LUA self （当前类，LuaTable），如无可填null</param>
        /// <param name="callTypeCheck">函数参数检查，数组长度规定了操作需要的参数，
        /// 数组值是一个或多个允许的类型名字，例如 UnityEngine.GameObject System.String 。
        /// 如果一个参数允许多种类型，可使用/分隔。
        /// 如果不需要参数检查，也可以为null，则当前操作将不会进行类型检查
        /// </param>
        /// <returns>返回注册的操作实例，如果注册失败则返回 null ，请查看 LastError 的值</returns>
        public GameAction RegisterAction(string name, string handlerName, LuaFunction luaFunction, LuaTable self, string[] callTypeCheck)
        {
            return RegisterAction(name, new GameHandler(handlerName, luaFunction, self), callTypeCheck);
        }
        /// <summary>
        /// 注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handler">接收器</param>
        /// <param name="callTypeCheck">函数参数检查，数组长度规定了操作需要的参数，
        /// 组值是一个或多个允许的类型名字，例如 UnityEngine.GameObject System.String 。
        /// 如果一个参数允许多种类型，可使用/分隔。
        /// 如果不需要参数检查，也可以为null，则当前操作将不会进行类型检查
        /// </param>
        /// <returns>返回注册的操作实例，如果注册失败则返回 null ，请查看 LastError 的值</returns>
        public GameAction RegisterAction(string name, GameHandler handler, string[] callTypeCheck)
        {
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterAction name 参数未提供");
                return null;
            }
            if (handler == null)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterAction handler 参数未提供");
                return null;
            }
            if (IsActionRegistered(name))
            {
                GameLogger.Warning(TAG, "操作 {0} 已注册", name);
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            GameAction gameAction = new GameAction(name, handler, callTypeCheck);
            actions.Add(name, gameAction);
            return gameAction;
        }

        /// <summary>
        /// 取消注册操作
        /// </summary>
        /// <param name="action">操作实例</param>
        public void UnRegisterAction(GameAction action)
        {
            if (action == null)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "UnRegisterAction action 参数未提供");
                return;
            }

            action.Dispose();
            actions.Remove(action.Name);
        }
        /// <summary>
        /// 取消注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        public void UnRegisterAction(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "UnRegisterAction name 参数未提供");
                return;
            }

            GameAction gameAction;
            if (IsActionRegistered(name, out gameAction))
            {
                gameAction.Dispose();
                actions.Remove(name);
            }
            else
            {
                GameLogger.Warning(TAG, "操作 {0} 未注册", name);
                GameErrorManager.LastError = GameError.NotRegister;
            }
        }
        /// <summary>
        /// 取消注册多个操作
        /// </summary>
        /// <param name="name">操作名称</param>
        public void UnRegisterActions(string[] names)
        {
            if (CommonUtils.IsArrayNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "UnRegisterActions names 参数未提供或数组为空");
                return;
            }
            foreach (string s in names)
                UnRegisterAction(s);
        }
        /// <summary>
        /// 获取操作是否注册
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <returns>是否注册</returns>
        public bool IsActionRegistered(string name)
        {
            return actions.ContainsKey(name);
        }
        /// <summary>
        /// 获取操作是否注册，如果已注册，则返回实例
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="e">返回的操作实例</param>
        /// <returns>是否注册</returns>
        public bool IsActionRegistered(string name, out GameAction e)
        {
            if (actions.TryGetValue(name, out e))
                return true;
            e = null;
            return false;
        }
        /// <summary>
        /// 获取操作实例
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <returns>返回的操作实例</returns>
        public GameAction GetRegisteredAction(string name)
        {
            GameAction a;
            if (actions.TryGetValue(name, out a))
                return a;
            return null;
        }

        /// <summary>
        /// 注册多个操作
        /// </summary>
        /// <param name="names">操作名称数组</param>
        /// <param name="handlerNames">接收器名称数组</param>
        /// <param name="handlers">接收函数数组</param>
        /// <param name="callTypeChecks">函数参数检查，如果不需要，也可以为null</param>
        /// <returns>返回注册成功的操作个数</returns>
        public int RegisterActions(string[] names, string[] handlerNames, GameActionHandlerDelegate[] handlers, string[][] callTypeChecks)
        {
            int succCount = 0;

            if (CommonUtils.IsArrayNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 names 数组为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(handlerNames))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlerNames 数组为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(handlers))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlers 数组为空");
                return succCount;
            }
            if (names.Length != handlerNames.Length || handlerNames.Length != handlers.Length
                || (callTypeChecks != null && callTypeChecks.Length != handlers.Length))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterActions 参数数组长度不符\n{0}=={1}=={2}=={3}",
                    names.Length,
                    handlerNames.Length,
                    handlers.Length,
                    callTypeChecks.Length);
                return succCount;
            }

            for (int i = 0, c = names.Length; i < c; i++)
            {
                if (RegisterAction(names[i], new GameHandler(handlerNames[i], handlers[i]),
                    callTypeChecks == null ? null : callTypeChecks[i]) != null)
                    succCount++;
            }

            return succCount;
        }
        /// <summary>
        /// 注册多个操作
        /// </summary>
        /// <param name="names">操作名称数组</param>
        /// <param name="handlerNams">接收器名称</param>
        /// <param name="handlers">接收函数数组</param>
        /// <param name="callTypeChecks">函数参数检查，如果不需要，也可以为null</param>
        /// <returns>返回注册成功的操作个数</returns>
        public int RegisterActions(string[] names, string handlerName, GameActionHandlerDelegate[] handlers, string[][] callTypeChecks)
        {
            int succCount = 0;

            if (CommonUtils.IsArrayNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 names 数组为空");
                return succCount;
            }
            if (string.IsNullOrEmpty(handlerName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlerName 数组为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(handlers))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlers 数组为空");
                return succCount;
            }
            if (names.Length != handlers.Length
                || (callTypeChecks != null && callTypeChecks.Length != handlers.Length))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterActions 参数数组长度不符\n{0}=={1}=={2}",
                    names.Length,
                    handlers.Length,
                    callTypeChecks.Length);
                return succCount;
            }

            for (int i = 0, c = names.Length; i < c; i++)
            {
                if (RegisterAction(names[i], new GameHandler(handlerName, handlers[i]),
                    callTypeChecks == null ? null : callTypeChecks[i]) != null)
                    succCount++;
            }

            return succCount;
        }
        /// <summary>
        /// 注册多个操作
        /// </summary>
        /// <param name="names">操作名称数组</param>
        /// <param name="handlerNames">接收器名称数组</param>
        /// <param name="luaFunctionHandlers">LUA接收函数数组</param>
        /// <param name="self">LUA self （当前类，LuaTable），如无可填null</param>
        /// <param name="callTypeChecks">函数参数检查，如果不需要，也可以为null</param>
        /// <returns>返回注册成功的操作个数</returns>
        public int RegisterActions(string[] names, string[] handlerNames, LuaFunction[] luaFunctionHandlers, LuaTable self, string[][] callTypeChecks)
        {
            int succCount = 0;

            if (CommonUtils.IsArrayNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 names 数组为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(handlerNames))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlerNames 数组为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(luaFunctionHandlers))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 luaFunctionHandlers 数组为空");
                return succCount;
            }
            if (names.Length != handlerNames.Length || handlerNames.Length != luaFunctionHandlers.Length
                 || (callTypeChecks != null && callTypeChecks.Length != luaFunctionHandlers.Length))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterActions 参数数组长度不符\n{0}=={1}=={2}=={3}",
                    names.Length,
                    handlerNames.Length,
                    luaFunctionHandlers.Length,
                    callTypeChecks.Length);
                return succCount;
            }

            for (int i = 0, c = names.Length; i < c; i++)
                if (RegisterAction(names[i], new GameHandler(handlerNames[i], luaFunctionHandlers[i], self),
                    callTypeChecks == null ? null : callTypeChecks[i]) != null)
                    succCount++;

            return succCount;
        }
        /// <summary>
        /// 注册多个操作
        /// </summary>
        /// <param name="names">操作名称数组</param>
        /// <param name="handlerName">接收器名（多个接收器名字一样）</param>
        /// <param name="luaFunctionHandlers">LUA接收函数数组</param>
        /// <param name="self">LUA self （当前类，LuaTable），如无可填null</param>
        /// <param name="callTypeChecks">函数参数检查，如果不需要，也可以为null</param>
        /// <returns>返回注册成功的操作个数</returns>
        public int RegisterActions(string[] names, string handlerName, LuaFunction[] luaFunctionHandlers, LuaTable self, string[][] callTypeChecks)
        {
            int succCount = 0;

            if (CommonUtils.IsArrayNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 names 数组为空");
                return succCount;
            }
            if (string.IsNullOrEmpty(handlerName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlerName 为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(luaFunctionHandlers))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 luaFunctionHandlers 数组为空");
                return succCount;
            }
            if (names.Length != luaFunctionHandlers.Length
                 || (callTypeChecks != null && callTypeChecks.Length != luaFunctionHandlers.Length))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterActions 参数数组长度不符\n{0}=={1}=={2}",
                    names.Length,
                    luaFunctionHandlers.Length,
                    callTypeChecks.Length);
                return succCount;
            }

            for (int i = 0, c = names.Length; i < c; i++)
                if (RegisterAction(names[i], new GameHandler(handlerName, luaFunctionHandlers[i], self),
                    callTypeChecks == null ? null : callTypeChecks[i]) != null)
                    succCount++;

            return succCount;
        }

        /// <summary>
        /// 调用操作
        /// </summary>
        /// <param name="name">目标操作名称</param>
        /// <param name="param">调用参数</param>
        /// <returns>返回操作调用结果，如果未找到操作，则返回 GameActionCallResult.FailResult </returns>
        public GameActionCallResult CallAction(string name, params object[] param)
        {
            GameActionCallResult result = GameActionCallResult.FailResult;
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "CallAction name 参数未提供");
                return result;
            }

            GameAction gameAction;
            if (IsActionRegistered(name, out gameAction)) return CallAction(gameAction, param);
            else GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG, "操作 {0} 未注册", name);
            return result;
        }
        /// <summary>
        /// 调用操作
        /// </summary>
        /// <param name="action">目标操作实例</param>
        /// <param name="param">调用参数</param>
        /// <returns>返回操作调用结果，如果未找到操作，则返回 GameActionCallResult.FailResult </returns>
        public GameActionCallResult CallAction(GameAction action, params object[] param)
        {
            GameErrorManager.LastError = GameError.None;
            GameActionCallResult result = GameActionCallResult.FailResult;

            if (action == null)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "CallAction action 参数为空");
                return result;
            }
            if (action.Name == GameAction.Empty.Name)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.Empty, TAG, "CallAction action 为空");
                return result;
            }
            if (action.CallTypeCheck != null && action.CallTypeCheck.Length > 0)
            {
                //参数类型检查
                int argCount = action.CallTypeCheck.Length;
                if (argCount > param.Length)
                {
                    GameLogger.Warning(TAG, "操作 {0} 至少需要 {1} 个参数", action.Name, argCount);
                    return result;
                }
                string allowType, typeName;
                for (int i = 0; i < argCount; i++)
                {
                    allowType = action.CallTypeCheck[i];
                    if (param[i] == null)
                    {
                        if (allowType != "null" &&
                           (!allowType.Contains("/") && !allowType.Contains("null")))
                        {
                            GameLogger.Warning(TAG, "操作 {0} 参数 {1} 不能为null", action.Name, i);
                            return result;
                        }
                    }
                    else
                    {
                        typeName = param[i].GetType().Name;
                        if (allowType != typeName &&
                            (!allowType.Contains("/") && !allowType.Contains(typeName)))
                        {
                            GameLogger.Warning(TAG, "操作 {0} 参数 {1} 类型必须是 {2}", action.Name, i, action.CallTypeCheck[i]);
                            return result;
                        }
                    }
                }
            }

            param = LuaUtils.LuaTableArrayToObjectArray(param);

            //GameLogger.Log(TAG, "CallAction {0} -> {1}", action.Name, StringUtils.ValueArrayToString(param));

            result = action.GameHandler.CallActionHandler(param);
            if (!result.Success)
                GameLogger.Warning(TAG, "操作 {0} 执行失败 {1}", action.Name, GameErrorManager.LastError);

            return result;
        }
    }
}
