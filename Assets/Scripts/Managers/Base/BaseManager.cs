using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using UnityEngine;

namespace Ballance2.Managers
{
    [CustomLuaClass]
    [AddComponentMenu("Ballance/BaseManager")]
    /// <summary>
    /// 管理器接口
    /// </summary>
    public class BaseManager : MonoBehaviour
    {
        /// <summary>
        /// 创建管理器（单例）
        /// </summary>
        /// <param name="name">标识符名称</param>
        public BaseManager(string packageName, string name)
        {
            this.name = name;
            this.packageName = packageName;
            subName = "Singleton";
            currentContext = CommonUtils.GenRandomID();
        }
        /// <summary>
        /// 创建管理器（多例）
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="subName">二级名称，用于区分多个管理器</param>
        public BaseManager(string packageName, string name, string subName)
        {
            this.name = name;
            this.subName = subName;
            this.packageName = packageName;
            isSingleton = subName == "Singleton";
            currentContext = CommonUtils.GenRandomID();
        }
        /// <summary>
        /// 创建（lua）空管理器，初始化以后才可注册
        /// </summary>
        public BaseManager()
        {
            isEmpty = true;
        }

        /// <summary>
        /// 获取当前管理器的上下文
        /// </summary>
        protected int currentContext = 0;

        private new string name = "";
        private bool isSingleton = false;
        private string subName = "";
        private string packageName = "";
        private bool isLuaModul = false;
        private Store store = null;
        private GameActionStore actionStore = null;

        internal int loadIndex = 0;
        internal bool initialized = false;
        internal bool preIinitialized = false;
        internal bool isEmpty = false;

        //指示管理器是否可以被替换
        internal bool replaceable = true;
        //指示管理器是否自动初始化对应共享数据仓库
        [Tooltip("管理器是否自动初始化对应共享数据仓库")]
        public bool initStore = true;
        //指示管理器是否自动初始化对应共享操作仓库
        [Tooltip("管理器是否自动初始化对应共享操作仓库")]
        public bool initActionStore = true;

        /// <summary>
        /// 获取是否可替换
        /// </summary>
        public bool Replaceable { get { return replaceable; } }
        /// <summary>
        /// 获取是否初始化
        /// </summary>
        public bool Initialized { get { return initialized; } }
        /// <summary>
        /// 获取或所在这个管理器是不是 Lua 模块的
        /// </summary>
        public bool IsLuaModul
        {
            get { return isLuaModul; }
            internal set
            {
                isLuaModul = value;
            }
        }
        /// <summary>
        /// 如果是 Lua 模块，可用获取 LUA 承载类
        /// </summary>
        public GameLuaObjectHost LuaObjectHost
        {
            get { return luaObjectHost; }
        }
        /// <summary>
        /// 获取当前管理器的全局共享数据仓库
        /// </summary>
        public Store Store { get { return store; } }
        /// <summary>
        /// 获取当前管理器的全局操作仓库
        /// </summary>
        public GameActionStore ActionStore { get { return actionStore; } }
        /// <summary>
        /// 获取这个管理器替换掉的旧管理器
        /// </summary>
        public BaseManager OldManager { get; internal set; }

        private LuaReturnBoolDelegate fnInitManager;
        private LuaReturnBoolDelegate fnReleaseManager;
        private LuaStoreReturnBoolDelegate fnInitStore;
        private LuaActionStoreReturnBoolDelegate fnInitActions;
        private LuaVoidDelegate fnInitPre;
        private GameLuaObjectHost luaObjectHost;

        private bool DoInitStore()
        {
            bool rest = false;
            if (initStore)
            {
                store = GameManager.GameMediator.RegisterGlobalDataStore(packageName);
                rest = InitStore(store);
            }
            if (initActionStore)
            {
                actionStore = GameManager.GameMediator.RegisterActionStore(packageName);
                rest = InitActions(actionStore);
            }
            return rest;
        }
        private void DestroyStore()
        {
            if (store != null)
            {
                GameManager.GameMediator.UnRegisterGlobalDataStore(store);
                store = null;
            }
            if (actionStore != null)
            {
                GameManager.GameMediator.UnRegisterActionStore(actionStore);
                actionStore = null;
            }
        }

        protected bool InitLua()
        {
            luaObjectHost = GetComponent<GameLuaObjectHost>();
            if (luaObjectHost == null || luaObjectHost.LuaSelf == null)
                return false;
            luaObjectHost.LuaSelf["currentContext"] = currentContext;
            luaObjectHost.LuaSelf["store"] = store;
            InitLuaFuns();
            return true;
        }
        protected virtual void InitLuaFuns() {
            LuaFunction f = luaObjectHost.GetLuaFun("InitManager");
            if (f != null) fnInitManager = f.cast<LuaReturnBoolDelegate>();
            f = luaObjectHost.GetLuaFun("InitStore");
            if (f != null) fnInitStore = f.cast<LuaStoreReturnBoolDelegate>();
            f = luaObjectHost.GetLuaFun("InitActions");
            if (f != null) fnInitActions = f.cast<LuaActionStoreReturnBoolDelegate>();
            f = luaObjectHost.GetLuaFun("InitPre");
            if (f != null) fnInitPre = f.cast<LuaVoidDelegate>();
            f = luaObjectHost.GetLuaFun("ReleaseManager");
            if (f != null) fnReleaseManager = f.cast<LuaReturnBoolDelegate>();
        }
        protected virtual bool InitStore(Store store)
        {
            if (fnInitStore != null) return fnInitStore(luaObjectHost.LuaSelf, store);
            return true;
        }
        protected virtual bool InitActions(GameActionStore actionStore)
        {
            if (fnInitActions != null) return fnInitActions(luaObjectHost.LuaSelf, actionStore);
            return true;
        }
        protected virtual void InitPre()
        {
            if (fnInitPre != null) fnInitPre(luaObjectHost.LuaSelf);
        }

        public bool DoPreInit()
        {
            if (preIinitialized)
                return true;

            preIinitialized = true;
            InitPre();

            if (IsLuaModul && luaObjectHost == null)
            {
                GameLogger.Error(GetFullName(), "LuaModul can oly use when GameLuaObjectHost is bind ! ");
                return false;
            }
            return DoInitStore();
        }

        public bool Inited { get; protected set; } = false;
        public bool Released { get; protected set; } = false;

        [DoNotToLua]
        /// <summary>
        /// 当管理器第一次初始化时（场景进入）
        /// </summary>
        /// <returns></returns>
        public virtual bool InitManager()
        {
            if (!Inited)
            {
                Inited = true;
                if (fnInitManager != null) return fnInitManager(luaObjectHost.LuaSelf);
            }
            return true;
        }
        [DoNotToLua]
        /// <summary>
        /// 当管理器卸载时（场景卸载）
        /// </summary>
        /// <returns></returns>
        public virtual bool ReleaseManager()
        {
            if (!Released)
            {
                Released = true;
                DestroyStore();
                if (IsLuaModul && fnReleaseManager != null) return fnReleaseManager(luaObjectHost.LuaSelf);
            }
            return true;
        }

        public string GetName()
        {
            return name;
        }
        public string GetSubName()
        {
            return subName;
        }
        public bool GetIsSingleton()
        {
            return isSingleton;
        }
        public string GetNameWithSub()
        {
            return name + ":" + subName;
        }
        public string GetFullName()
        {
            return packageName + "." + name + ":" + subName;
        }
    }
}
