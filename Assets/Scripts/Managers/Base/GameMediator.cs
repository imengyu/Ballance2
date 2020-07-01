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
    /// 游戏全局中介者
    /// </summary>
    [CustomLuaClass]
    public class GameMediator : BaseManager
    {
        public const string TAG = "GameMediator";

        public GameMediator() : base(TAG)
        {
        }

        public override bool InitManager()
        {
            InitAllEvents();
            InitAllActions();
            InitModDebug();
            return true;
        }
        public override bool ReleaseManager()
        {
            UnLoadAllEvents();
            UnLoadAllActions();
            return true;
        }

        #region 全局事件控制器

        private List<GameEvent> events = null;

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public GameEvent RegisterGlobalEvent(string evtName)
        {
            if (!IsGlobalEventRegistered(evtName))
            {
                GameEvent gameEvent = new GameEvent(evtName);
                events.Add(gameEvent);
                return gameEvent;
            }
            else
            {
                GameLogger.Warning(TAG, "事件 {0} 已注册", evtName);
                GameErrorManager.LastError = GameError.AlredayRegistered;
            }
            return null;
        }
        /// <summary>
        /// 取消注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public void UnRegisterGlobalEvent(string evtName)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                gameEvent.Dispose();
                events.Remove(gameEvent);
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
            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent.EventName == evtName)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取事件是否注册，如果已注册，则返回实例
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="e">返回的事件实例</param>
        /// <returns>是否注册</returns>
        public bool IsGlobalEventRegistered(string evtName, out GameEvent e)
        {
            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent.EventName == evtName)
                {
                    e = gameEvent;
                    return true;
                }
            }
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
            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent.EventName == evtName)
                    return gameEvent;
            }
            return null;
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
            if (gameEvent != null)
            {
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
                foreach (GameEvent gameEvent in events)
                    gameEvent.Dispose();
                events.Clear();
                events = null;
            }
        }
        private void InitAllEvents()
        {
            events = new List<GameEvent>();

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

        private List<GameAction> actions = null;

        /// <summary>
        /// 注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handlerName">接收器名称</param>
        /// <param name="handler">接收函数</param>
        /// <returns></returns>
        public GameAction RegisterAction(string name, string handlerName, GameActionHandlerDelegate handler)
        {
            return RegisterAction(name, new GameHandler(handlerName, handler));
        }
        /// <summary>
        /// 注册操作(LUA)
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="handlerName">接收器名称</param>
        /// <param name="luaFunction">LUA接收函数</param>
        /// <param name="self">LUA Self</param>
        /// <returns></returns>
        public GameAction RegisterAction(string name, string handlerName, LuaFunction luaFunction, LuaTable self)
        {
            return RegisterAction(name, new GameHandler(handlerName, luaFunction, self));
        }
        /// <summary>
        /// 注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        public GameAction RegisterAction(string name, GameHandler handler)
        {
            if (!IsActionRegistered(name))
            {
                GameAction gameAction = new GameAction(name, handler);
                actions.Add(gameAction);
                return gameAction;
            }
            else
            {
                GameLogger.Warning(TAG, "操作 {0} 已注册", name);
                GameErrorManager.LastError = GameError.AlredayRegistered;
            }
            return null;
        }
        /// <summary>
        /// 取消注册操作
        /// </summary>
        /// <param name="name">操作名称</param>
        public void UnRegisterAction(string name)
        {
            GameAction gameAction = null;
            if (IsActionRegistered(name, out gameAction))
            {
                gameAction.Dispose();
                actions.Remove(gameAction);
            }
            else
            {
                GameLogger.Warning(TAG, "操作 {0} 未注册", name);
                GameErrorManager.LastError = GameError.NotRegister;
            }
        }
        /// <summary>
        /// 获取操作是否注册
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <returns>是否注册</returns>
        public bool IsActionRegistered(string name)
        {
            foreach (GameAction gameAction in actions)
            {
                if (gameAction.Name == name)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取操作是否注册，如果已注册，则返回实例
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="e">返回的操作实例</param>
        /// <returns>是否注册</returns>
        public bool IsActionRegistered(string name, out GameAction e)
        {
            foreach (GameAction gameAction in actions)
            {
                if (gameAction.Name == name)
                {
                    e = gameAction;
                    return true;
                }
            }
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
            foreach (GameAction a in actions)
            {
                if (a.Name == name)
                    return a;
            }
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
            if (IsActionRegistered(name, out gameAction)) return CallAction(gameAction, param);
            else
            {
                GameLogger.Warning(TAG, "操作 {0} 未注册", name);
                GameErrorManager.LastError = GameError.NotRegister;
            }
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
            GameActionCallResult result = null;

            param = LuaUtils.LuaTableArrayToObjectArray(param);

            GameLogger.Log(TAG, "CallAction {0} -> {1}", action.Name, StringUtils.ValueArrayToString(param));

            result = action.GameHandler.CallActionHandler(param);
            if (!result.Success)
                GameLogger.Warning(TAG, "操作 {0} 执行失败 {1}", name, GameErrorManager.LastError);

            return result;
        }

        private void UnLoadAllActions()
        {
            if (actions != null)
            {
                foreach (GameAction action in actions)
                    action.Dispose();
                actions.Clear();
                actions = null;
            }
        }
        private void InitAllActions()
        {
            actions = new List<GameAction>();

            //注册内置事件
            RegisterAction(GameActionNames.ACTION_QUIT, "GameManager", (param) =>
            {
                GameManager.QuitGame();
                return GameActionCallResult.CreateActionCallResult(true);
            });
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
            foreach (GameAction a in actions)
            {
                s.Append('\n');
                s.Append(a.Name);
                s.Append(a.GameHandler.Name);
            }
            GameLogger.Log(TAG, "GameActions count {0} : \n{1}", actions.Count, s.ToString());
            return true;
        }
        private bool OnCommandShowEvents(string keyword, string fullCmd, string[] args)
        {
            bool showHandlers = args != null && args.Length > 0 && args[0] == "true";

            StringBuilder s = new StringBuilder();
            foreach (GameEvent e in events)
            {
                s.Append('\n');
                s.Append(e.EventName);
                s.Append("   Handler count:  ");
                s.Append(e.EventHandlers.Count);
                if (showHandlers && e.EventHandlers.Count > 0)
                {
                    s.Append("\n    Handlers : ");
                    foreach (GameHandler h in e.EventHandlers)
                    {
                        s.Append("\n  ");
                        s.Append(h.ToString());
                    }
                }
            }
            GameLogger.Log(TAG, "GameEvents count {0} : \n{1}", events.Count, s.ToString());
            return true;
        }

        #endregion
    }
}
