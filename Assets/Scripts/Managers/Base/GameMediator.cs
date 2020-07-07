using Ballance2.CoreBridge;
using Ballance2.Interfaces;
using Ballance2.Utils;
using SLua;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ballance2.Managers
{
    /// <summary>
    /// 游戏全局中介者。
    /// 核心模块。
    /// </summary>
    /// <remarks>
    /// 游戏的架构分为：
    /// 操作，事件，共享数据 》管理器。
    /// 各个模块通过中介者解耦
    /// </remarks>
    [CustomLuaClass]
    public class GameMediator : BaseManager
    {
        public const string TAG = "GameMediator";

        public GameMediator() : base(GamePartName.Core, TAG)
        {
            replaceable = false;
            initStore = false;
        }

        public override bool InitManager()
        {
            InitAllEvents();
            InitAllActions();
            InitModDebug();
            InitStore();
            return true;
        }
        public override bool ReleaseManager()
        {
            UnLoadAllEvents();
            UnLoadAllActions();
            DestroyStore();
            return true;
        }

        #region 全局事件控制器

        private Dictionary<string, GameEvent> events = null;

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public GameEvent RegisterGlobalEvent(string evtName)
        { 
            if (string.IsNullOrEmpty(evtName))
            {
                GameLogger.Warning(TAG, "RegisterGlobalEvent evtName 参数未提供");
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return null;
            }
            if (IsGlobalEventRegistered(evtName))
            {
                GameLogger.Warning(TAG, "事件 {0} 已注册", evtName);
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            GameEvent gameEvent = new GameEvent(evtName);
            events.Add(evtName, gameEvent);
            return gameEvent;
        }
        /// <summary>
        /// 取消注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public void UnRegisterGlobalEvent(string evtName)
        {
            if (string.IsNullOrEmpty(evtName))
            {
                GameLogger.Warning(TAG, "UnRegisterGlobalEvent evtName 参数未提供");
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return;
            }
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                gameEvent.Dispose();
                events.Remove(evtName);
            }
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
        }
        /// <summary>
        /// 获取事件是否注册
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <returns>是否注册</returns>
        public bool IsGlobalEventRegistered(string evtName)
        {
            return events.ContainsKey(evtName);
        }
        /// <summary>
        /// 获取事件是否注册，如果已注册，则返回实例
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="e">返回的事件实例</param>
        /// <returns>是否注册</returns>
        public bool IsGlobalEventRegistered(string evtName, out GameEvent e)
        {
            if (events.TryGetValue(evtName, out e))
                return true;
            e = null;
            return false;
        }
        /// <summary>
        /// 获取事件实例
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <returns>返回的事件实例</returns>
        public GameEvent GetRegisteredGlobalEvent(string evtName)
        {
            GameEvent gameEvent = null;

            if (string.IsNullOrEmpty(evtName))
            {
                GameLogger.Warning(TAG, "GetRegisteredGlobalEvent evtName 参数未提供");
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return gameEvent;
            }

            events.TryGetValue(evtName, out gameEvent);
            return gameEvent;
        }

        /// <summary>
        /// 执行事件分发
        /// </summary>
        /// <param name="gameEvent">事件实例</param>
        /// <param name="handlerFilter">指定事件可以接收到的名字（这里可以用正则）</param>
        /// <param name="pararms">事件参数</param>
        /// <returns>返回已经发送的接收器个数</returns>
        public int DispatchGlobalEvent(GameEvent gameEvent, string handlerFilter, params object[] pararms)
        {
            int handledCount = 0;
            if (gameEvent == null)
            {
                GameLogger.Warning(TAG, "DispatchGlobalEvent gameEvent 参数未提供");
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return handledCount;
            }

            //事件分发
            foreach (GameHandler gameHandler in gameEvent.EventHandlers)
            {
                if (handlerFilter == "*" || Regex.IsMatch(gameHandler.Name, handlerFilter))
                {
                    handledCount++;
                    if (gameHandler.CallEventHandler(gameEvent.EventName, pararms))
                    {
                        GameLogger.Log(TAG, "Event {0} was interrupted by : {1}", gameEvent.EventName, gameHandler.Name);
                        break;
                    }
                }
            }

            return handledCount;
        }
        /// <summary>
        /// 执行事件分发
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="handlerFilter">指定事件可以接收到的名字（这里可以用正则）</param>
        /// <param name="pararms">事件参数</param>
        /// <returns>返回已经发送的接收器个数</returns>
        public int DispatchGlobalEvent(string evtName, string handlerFilter, params object[] pararms)
        {
            int handledCount = 0;
            GameEvent gameEvent = null;

            if (string.IsNullOrEmpty(evtName))
            {
                GameLogger.Warning(TAG, "DispatchGlobalEvent evtName 参数未提供");
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return 0;
            }
            if (IsGlobalEventRegistered(evtName, out gameEvent))
                return DispatchGlobalEvent(gameEvent, handlerFilter, pararms);
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
            return handledCount;
        }

        //卸载所有命令
        private void UnLoadAllEvents()
        {
            if (events != null)
            {
                foreach (var gameEvent in events)
                    gameEvent.Value.Dispose();
                events.Clear();
                events = null;
            }
        }
        private void InitAllEvents()
        {
            events = new Dictionary<string, GameEvent>();

            //注册内置事件
            RegisterGlobalEvent(GameEventNames.EVENT_BASE_INIT_FINISHED);
            RegisterGlobalEvent(GameEventNames.EVENT_BEFORE_GAME_QUIT);
            RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_ENTRY);
            RegisterGlobalEvent(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED);
        }

        /// <summary>
        /// 注册命令接收器（Lua）
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="name">接收器名字</param>
        /// <param name="gameHandlerDelegate">回调</param>
        /// <returns></returns>
        public GameHandler RegisterEventHandler(string evtName, string name, LuaFunction luaFunction)
        {
            if (string.IsNullOrEmpty(evtName)
               || string.IsNullOrEmpty(name)
               || luaFunction == null)
            {
                GameLogger.Warning(TAG, "参数缺失", evtName);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return null;
            }

            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                GameHandler gameHandler = new GameHandler(name, luaFunction);
                gameEvent.EventHandlers.Add(gameHandler);
                return gameHandler;
            }
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
            return null;
        }
        /// <summary>
        /// 注册命令接收器（Delegate）
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="name">接收器名字</param>
        /// <param name="gameHandlerDelegate">回调</param>
        /// <returns></returns>
        public GameHandler RegisterEventHandler(string evtName, string name, GameEventHandlerDelegate gameHandlerDelegate)
        {
            if (string.IsNullOrEmpty(evtName)
               || string.IsNullOrEmpty(name)
               || gameHandlerDelegate == null)
            {
                GameLogger.Warning(TAG, "参数缺失", evtName);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return null;
            }

            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                GameHandler gameHandler = new GameHandler(name, gameHandlerDelegate);
                gameEvent.EventHandlers.Add(gameHandler);
                return gameHandler;
            }
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
            return null;
        }
        /// <summary>
        /// 注册事件接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="name">接收器名字</param>
        /// <param name="luaModulHandler">模块接收器函数标识符</param>
        /// <returns>返回接收器类</returns>
        public GameHandler RegisterEventHandler(string evtName, string name, string luaModulHandler)
        {
            if(string.IsNullOrEmpty(evtName) 
                || string.IsNullOrEmpty(name) 
                || string.IsNullOrEmpty(luaModulHandler)) {
                GameLogger.Warning(TAG, "参数缺失", evtName);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return null;
            }
          
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                GameHandler gameHandler = new GameHandler(name, luaModulHandler);
                gameEvent.EventHandlers.Add(gameHandler);
                return gameHandler;
            }
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
            return null;
        }
        /// <summary>
        /// 取消注册事件接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="handler">接收器类</param>
        public void UnRegisterEventHandler(string evtName, GameHandler handler)
        {
            if (string.IsNullOrEmpty(evtName)
                || handler == null)
            {
                GameLogger.Warning(TAG, "参数缺失", evtName);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return;
            }

            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
                gameEvent.EventHandlers.Remove(handler);
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.NotRegister;
            }
        }

        #endregion

        #region 全局操作控制器

        private Dictionary<string, GameAction> actions = null;

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
        /// <param name="handlerName">接收器名称（多个接收器名字一样）</param>
        /// <param name="handlers">接收函数数组</param>
        /// <param name="callTypeChecks">函数参数检查，如果不需要，也可以为null</param>
        /// <returns>返回注册成功的操作个数</returns>
        public int RegisterActions(Dictionary<string, string> names, string handlerName, GameActionHandlerDelegate[] handlers, string[][] callTypeChecks)
        {
            int succCount = 0;

            if (CommonUtils.IsDictionaryNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 names 数组为空");
                return succCount;
            }
            if (string.IsNullOrEmpty(handlerName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlerName 为空");
                return succCount;
            }
            if (CommonUtils.IsArrayNullOrEmpty(handlers))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "RegisterActions 参数 handlers 数组为空");
                return succCount;
            }
            if (names.Keys.Count != handlers.Length
                 || (callTypeChecks != null && callTypeChecks.Length != handlers.Length))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterActions 参数数组长度不符\n{0}=={1}=={2}",
                    names.Count,
                    handlers.Length,
                    callTypeChecks.Length);
                return succCount;
            }

            int i = 0;
            foreach (string k in names.Keys)
            {
                if (RegisterAction(names[k], new GameHandler(handlerName, handlers[i]),
                    callTypeChecks == null ? null : callTypeChecks[i]) != null)
                    succCount++;
                i++;
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
        /// 注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handlerName">接收器名称</param>
        /// <param name="handler">接收函数</param>
        /// <param name="callTypeCheck">函数参数检查，数组长度规定了操作需要的参数，
        /// 数组值是一个或多个允许的类型名字，例如 UnityEngine.GameObject System.String 。
        /// 如果一个参数允许多种类型，可使用/分隔。
        /// 如果不需要，也可以为null，当前操作将不会进行类型检查
        /// </param>
        /// <returns></returns>
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
        /// 如果不需要，也可以为null，当前操作将不会进行类型检查
        /// </param>
        /// <returns></returns>
        public GameAction RegisterAction(string name, string handlerName, LuaFunction luaFunction, LuaTable self, string[] callTypeCheck)
        {
            return RegisterAction(name, new GameHandler(handlerName, luaFunction, self), callTypeCheck);
        }
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
        /// <param name="name">操作名称</param>
        public void UnRegisterAction(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "UnRegisterAction name 参数未提供");
                return;
            }
            GameAction gameAction = null;
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
        /// 取消注册多个操作
        /// </summary>
        /// <param name="name">操作名称</param>
        public void UnRegisterActions(Dictionary<string, string> names)
        {
            if (CommonUtils.IsDictionaryNullOrEmpty(names))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "UnRegisterActions names 参数未提供或数组为空");
                return;
            }
            foreach (string s in names.Keys)
                UnRegisterAction(names[s]);
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
            if(actions.TryGetValue(name, out e))
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
        /// 调用操作
        /// </summary>
        /// <param name="name">目标操作名称</param>
        /// <param name="param">调用参数</param>
        /// <returns></returns>
        public GameActionCallResult CallAction(string name, params object[] param)
        {
            GameActionCallResult result = null;
            GameAction gameAction = null;
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, "CallAction name 参数未提供");
                return result;
            }
            if (IsActionRegistered(name, out gameAction)) return CallAction(gameAction, param);
            else GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG, "操作 {0} 未注册", name);
            return result;
        }
        /// <summary>
        /// 调用操作
        /// </summary>
        /// <param name="action">目标操作实例</param>
        /// <param name="param">调用参数</param>
        /// <returns></returns>
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
                string allowType = "", typeName = "";
                for (int i = 0; i < argCount; i++)
                {
                    typeName = param[i].GetType().Name;
                    allowType = action.CallTypeCheck[i];
                    if (allowType == typeName ||
                        (allowType.Contains("/") && allowType.Contains(typeName)))
                    {
                        GameLogger.Warning(TAG, "操作 {0} 参数 {1} 类型必须是 {2}", name, i, action.CallTypeCheck[i]);
                        return result;
                    }
               }
            }

            param = LuaUtils.LuaTableArrayToObjectArray(param);

            //GameLogger.Log(TAG, "CallAction {0} -> {1}", action.Name, StringUtils.ValueArrayToString(param));

            result = action.GameHandler.CallActionHandler(param);
            if (!result.Success)
                GameLogger.Warning(TAG, "操作 {0} 执行失败 {1}", name, GameErrorManager.LastError);

            return result;
        }

        private void UnLoadAllActions()
        {
            if (actions != null)
            {
                foreach (var action in actions)
                    action.Value.Dispose();
                actions.Clear();
                actions = null;
            }
        }
        private void InitAllActions()
        {
            actions = new Dictionary<string, GameAction>();

            //注册内置事件
            RegisterAction(GameActionNames.CoreActions["QuitGame"], "GameManager", (param) =>
            {
                GameManager.QuitGame();
                return GameActionCallResult.CreateActionCallResult(true);
            }, null);
        }

        #endregion

        #region 全局共享数据共享池

        private Dictionary<string, Store> globalStore;

        private void InitStore()
        {
            globalStore = new Dictionary<string, Store>();
        }
        private void DestroyStore()
        {
            foreach(var v in globalStore)
                v.Value.Destroy();
            globalStore.Clear();
            globalStore = null;
        }

        /// <summary>
        /// 注册全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns>如果注册成功，返回池对象；如果已经注册，则返回已经注册的池对象</returns>
        public Store RegisterGlobalDataStore(string name)
        {
            Store store = null;
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG, 
                    "RegisterGlobalDataStore name 参数未提供");
                return null;
            }
            if(globalStore.ContainsKey(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.AlredayRegistered, TAG,
                    "数据共享存储池 {0} 已经注册", name);
                store = globalStore[name];
                return store;
            }

            store = new Store(name);
            globalStore.Add(name, store);
            return store;
        }
        /// <summary>
        /// 获取全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public Store GetGlobalDataStore(string name)
        {
            Store s;
            globalStore.TryGetValue(name, out s);
            return s;
        }
        /// <summary>
        /// 释放已注册的全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public bool UnRegisterGlobalDataStore(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "UnRegisterGlobalDataStore name 参数未提供");
                return false;
            }
            if (!globalStore.ContainsKey(name))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG,
                    "数据共享存储池 {0} 未注册", name);
                return false;
            }
            globalStore.Remove(name);
            return false;
        }
        /// <summary>
        /// 释放已注册的全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public bool UnRegisterGlobalDataStore(Store store)
        {
            if (!globalStore.ContainsKey(store.PoolName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG,
                    "数据共享存储池 {0} 未注册", name);
                return false;
            }
            globalStore.Remove(name);
            return false;
        }

        #endregion

        #region GameMediator 调试命令

        private IDebugManager DebugManager;

        private void InitModDebug()
        {
            RegisterEventHandler(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED, TAG, (evtName, param) =>
            {
                if (param[0].ToString() == "DebugManager")
                {
                    DebugManager = (IDebugManager)GameManager.GetManager("DebugManager");
                    DebugManager.RegisterCommand("callaction", OnCommandCallAction, 1, "调用操作 [操作名称] [参数...]");
                    DebugManager.RegisterCommand("actions", OnCommandShowActions, 0, "显示全局操作");
                    DebugManager.RegisterCommand("events", OnCommandShowEvents, 0, "[showHandlers:true/false] 显示全局事件 [是否显示事件接收器]");
                    DebugManager.RegisterCommand("showstore", OnCommandShowStores, 0, "[showParams:true/false] 显示全局数据共享池 [是否显示共享池内所有参数]");
                    DebugManager.RegisterCommand("storedata", OnCommandShowStores, 1, "[storeName:string] [paramName:string] 显示全局数据共享池内的参数 [池名称] [要显示的参数名称，如果为空则显示全部] ");

                }
                return false;
            });
        }

        private bool OnCommandCallAction(string keyword, string fullCmd, string[] args)
        {
            string[] arrParams = new string[args.Length - 1];
            for (int i = 1; i < args.Length; i++) arrParams[i - 1] = args[i];
            GameActionCallResult rs = CallAction(args[0], 
                StringUtils.TryConvertStringArrayToValueArray(arrParams));
            if(rs != null)
            {
                GameLogger.Log(TAG, "Call success : {0}\nCall result: {1}", rs.Success, 
                    StringUtils.ValueArrayToString(rs.ReturnParams));
            }
            return true;
        }
        private bool OnCommandShowActions(string keyword, string fullCmd, string[] args)
        {
            StringBuilder s = new StringBuilder();
            foreach (var a in actions)
            {
                s.Append('\n');
                s.Append(a.Key);
                s.Append(' ');
                s.Append(a.Value.GameHandler.Name);
            }
            GameLogger.Log(TAG, "GameActions count {0} : \n{1}", actions.Count, s.ToString());
            return true;
        }
        private bool OnCommandShowEvents(string keyword, string fullCmd, string[] args)
        {
            bool showHandlers = args != null && args.Length > 0 && args[0] == "true";

            StringBuilder s = new StringBuilder();
            foreach (var e in events)
            {
                s.Append('\n');
                s.Append(e.Key);
                s.Append("   Handler count:  ");
                s.Append(e.Value.EventHandlers.Count);
                if (showHandlers && e.Value.EventHandlers.Count > 0)
                {
                    s.Append("\n    Handlers : ");
                    foreach (GameHandler h in e.Value.EventHandlers)
                    {
                        s.Append("\n  ");
                        s.Append(h.ToString());
                    }
                }
            }
            GameLogger.Log(TAG, "GameEvents count {0} : \n{1}", events.Count, s.ToString());
            return true;
        }
        private bool OnCommandShowStores(string keyword, string fullCmd, string[] args)
        {
            bool showParams = args != null && args.Length > 0 && args[0] == "true";

            StringBuilder s = new StringBuilder();
            foreach (var e in globalStore)
            {
                s.Append('\n');
                s.Append(e.Key);
                s.Append("   Params count:  ");
                s.Append(e.Value.PoolDatas.Count);
                if (showParams && e.Value.PoolDatas.Count > 0)
                {
                    s.Append("\n    Params : ");
                    foreach (var p in e.Value.PoolDatas)
                    {
                        s.Append("\n  ");
                        s.Append(p.ToString());
                    }
                }
            }
            GameLogger.Log(TAG, "Global Store data count {0} : \n{1}", globalStore.Count, s.ToString());
            return true;
        }
        private bool OnCommandShowStoreData(string keyword, string fullCmd, string[] args)
        {
            string storeName = args != null && args.Length > 0 ? args[0] : "";
            string paramName = args != null && args.Length > 1 ? args[1] : "";

            Store s = GetGlobalDataStore(storeName);
            if (s == null)
            {
                GameLogger.Error(TAG, "未找到 Store {0}", storeName);
                return false;
            }

            if(paramName == "")
            {
                StringBuilder sb = new StringBuilder(storeName);
                sb.Append("  All paramas: ");
                foreach (var e in s.PoolDatas)
                {
                    sb.Append('\n');
                    sb.Append(e.Key);
                    sb.Append("   ");
                    sb.Append(e.Value.ToString());
                }
                GameLogger.Log(TAG, sb.ToString());
            }
            else
            {
                StoreData sd = s.GetParameter(paramName);
                if(sd == null)
                {
                    GameLogger.Error(TAG, "未找到 StoreData :  {0} -> {1}", storeName, paramName);
                    return false;
                }

                GameLogger.Log(TAG, "{0} -> {1} : ", storeName, paramName);
                GameLogger.Log(TAG, sd.ToString());
                if (sd.DataType == StoreDataType.Array)
                {
                    int i = 0;
                    StringBuilder sb = new StringBuilder(storeName);
                    sb.Append("  Childs: ");
                    foreach (var e in sd.DataArray)
                    {
                        sb.Append('\n');
                        sb.Append(i);
                        sb.Append("   ");
                        sb.Append(e.ToString());
                        i++;
                    }
                    GameLogger.Log(TAG, sb.ToString());
                }
            }
           
            return true;
        }

        #endregion
    }
}
