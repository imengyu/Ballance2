using Ballance2.CoreBridge;
using Ballance2.Interfaces;
using Ballance2.Utils;
using SLua;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

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
            initActionStore = false;
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

        [SerializeField, SetProperty("Events")]
        private Dictionary<string, GameEvent> events = null;

        public Dictionary<string, GameEvent> Events { get { return events; } }

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
            RegisterGlobalEvent(GameEventNames.EVENT_ENTER_SCENSE);
            RegisterGlobalEvent(GameEventNames.EVENT_BEFORE_LEAVE_SCENSE);
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

        [SerializeField, SetProperty("Actions")]
        private Dictionary<string, GameActionStore> actionStores = null;

        /// <summary>
        /// 注册全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns>如果注册成功，返回池对象；如果已经注册，则返回已经注册的池对象</returns>
        public GameActionStore RegisterActionStore(string packageName)
        {
            GameActionStore store;
            if (string.IsNullOrEmpty(packageName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "RegisterGlobalDataStore name 参数未提供");
                return null;
            }
            if (actionStores.ContainsKey(packageName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.AlredayRegistered, TAG,
                    "共享操作仓库 {0} 已经注册", packageName);
                store = actionStores[packageName];
                return store;
            }

            store = new GameActionStore(packageName);
            actionStores.Add(packageName, store);
            return store;
        }
        /// <summary>
        /// 获取全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public GameActionStore GetActionStore(string packageName)
        {
            GameActionStore s;
            actionStores.TryGetValue(packageName, out s);
            return s;
        }
        /// <summary>
        /// 释放已注册的全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public bool UnRegisterActionStore(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ParamNotProvide, TAG,
                    "UnRegisterActionStore name 参数未提供");
                return false;
            }
            if (!actionStores.ContainsKey(packageName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG,
                    "共享操作仓库 {0} 未注册", packageName);
                return false;
            }

            actionStores[packageName].Destroy();
            actionStores.Remove(packageName);
            return false;
        }
        /// <summary>
        /// 释放已注册的全局共享数据存储池
        /// </summary>
        /// <param name="name">池名称</param>
        /// <returns></returns>
        public bool UnRegisterActionStore(GameActionStore store)
        {
            if (!actionStores.ContainsKey(store.PackageName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG,
                    "actionStores {0} 未注册", store.PackageName);
                return false;
            }
            actionStores[store.PackageName].Destroy();
            globalStore.Remove(store.PackageName);
            return false;
        }

        public GameActionCallResult CallAction(string storeName, string name, params object[] param)
        {
            if (!actionStores.ContainsKey(storeName))
            {
                GameErrorManager.SetLastErrorAndLog(GameError.NotRegister, TAG,
                    "共享操作仓库 {0} 未注册", storeName);
                return GameActionCallResult.FailResult;
            }
            return CallAction(actionStores[storeName], name, param);
        }
        public GameActionCallResult CallAction(GameActionStore store, string name, params object[] param)
        {
            return store.CallAction(name, param);
        }
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
                            GameLogger.Warning(TAG, "操作 {0} 参数 {1} 不能为null", name, i);
                            return result;
                        }
                    }
                    else
                    {
                        typeName = param[i].GetType().Name;
                        if (allowType != typeName &&
                            (!allowType.Contains("/") && !allowType.Contains(typeName)))
                        {
                            GameLogger.Warning(TAG, "操作 {0} 参数 {1} 类型必须是 {2}", name, i, action.CallTypeCheck[i]);
                            return result;
                        }
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

        public GameActionStore CoreActinoStore { get; private set; }

        private void UnLoadAllActions()
        {
            if (actionStores != null)
            {
                foreach (var action in actionStores)
                    action.Value.Destroy();
                actionStores.Clear();
                actionStores = null;
            }
        }
        private void InitAllActions()
        {
            actionStores = new Dictionary<string, GameActionStore>();

            //注册内置事件
            CoreActinoStore = RegisterActionStore(GamePartName.Core);
            CoreActinoStore.RegisterAction("QuitGame", "GameManager", (param) =>
            {
                GameManager.QuitGame();
                return GameActionCallResult.SuccessResult;
            }, null);
        }

        #endregion

        #region 全局共享数据共享池

        [SerializeField, SetProperty("GlobalStore")]
        private Dictionary<string, Store> globalStore;

        public Dictionary<string, Store> GlobalStore { get { return globalStore; } }

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

            globalStore[name].Destroy();
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
           
            globalStore.Remove(store.PoolName);
            store.Destroy();

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
                    DebugManager.RegisterCommand("callaction", OnCommandCallAction, 2, "调用操作 [操作所在仓库] [操作名称] [参数...]");
                    DebugManager.RegisterCommand("actions", OnCommandShowActions, 0, "[showName:string] 显示全局操作 [要显示的仓库,为空则显示所有仓库名称]");
                    DebugManager.RegisterCommand("events", OnCommandShowEvents, 0, "[showHandlers:true/false] 显示全局事件 [是否显示事件接收器]");
                    DebugManager.RegisterCommand("showstore", OnCommandShowStores, 0, "[showParams:true/false] 显示全局数据共享池 [是否显示共享池内所有参数]");
                    DebugManager.RegisterCommand("storedata", OnCommandShowStoreData, 1, "[storeName:string] [paramName:string] 显示全局数据共享池内的参数 [池名称] [要显示的参数名称，如果为空则显示全部] ");

                }
                return false;
            });
        }

        private bool OnCommandCallAction(string keyword, string fullCmd, string[] args)
        {
            string[] arrParams = new string[args.Length - 2];
            for (int i = 2; i < args.Length; i++) arrParams[i - 2] = args[i];
            GameActionCallResult rs = CallAction(args[0], args[1],
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
            string showStore = "";
            if (args != null && args.Length > 0)
                showStore = args[0];

            if (showStore == "")
            {
                StringBuilder s = new StringBuilder("Action Stores: ");
                foreach (var a in actionStores)
                {
                    s.Append('\n');
                    s.Append(a.Key);
                    s.Append(' ');
                }
                GameLogger.Log(TAG, "\nCount {0} : \n{1}", actionStores.Count, s.ToString());
            }
            else
            {
                GameActionStore store = GetActionStore(showStore);
                if(store == null)
                {
                    GameLogger.Error(TAG, "Not found store {0}", showStore);
                    return false;
                }

                StringBuilder s = new StringBuilder("Action in store: ");
                s.Append(showStore);
                foreach (var a in store.Actions)
                {
                    s.Append('\n');
                    s.Append(a.Key);
                    s.Append(' ');
                    s.Append(a.Value.GameHandler.Name);
                }

                GameLogger.Log(TAG, "\nCount {0} : \n{1}", actionStores.Count, s.ToString());
            }

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
