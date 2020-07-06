using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using UnityEngine;

namespace Ballance2.Managers
{
    [CustomLuaClass]
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
            loadIndex = CommonUtils.GenAutoIncrementID();
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
            loadIndex = CommonUtils.GenAutoIncrementID();
            currentContext = CommonUtils.GenRandomID();
        }
        /// <summary>
        /// 创建（lua）空管理器，初始化以后才可注册
        /// </summary>
        public BaseManager()
        {
            isEmpty = true;
        }

        protected int currentContext = 0;

        private new string name = "";
        private bool isSingleton = false;
        private string subName = "";
        private string packageName = "";

        internal int loadIndex = 0;
        internal bool initialized = false;
        internal bool preInitialized = false;
        internal bool replaceable = true;
        internal bool isEmpty = false;
        private bool isLuaModul = false;
        private Store store = null;

        /// <summary>
        /// 获取是否可替换
        /// </summary>
        public bool Replaceable { get { return replaceable; } }
        /// <summary>
        /// 获取是否初始化
        /// </summary>
        public bool Initialized { get { return initialized; } }
        /// <summary>
        /// 获取是否预初始化
        /// </summary>
        public bool PreInitialized { get { return preInitialized; } }
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
        public Store Store { get; private set; }

        private LuaReturnBoolDelegate fnInitManager;
        private LuaVoidDelegate fnPreInitManager;
        private LuaReturnBoolDelegate fnReleaseManager;
        private GameLuaObjectHost luaObjectHost;

        private void InitStore()
        {
            Store = GameManager.GameMediator.RegisterGlobalDataStore(packageName);
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
            f = luaObjectHost.GetLuaFun("PreInitManager");
            if (f != null) fnPreInitManager = f.cast<LuaVoidDelegate>();
            f = luaObjectHost.GetLuaFun("ReleaseManager");
            if (f != null) fnReleaseManager = f.cast<LuaReturnBoolDelegate>();
        }
        private void DestroyStore()
        {
            if (Store != null)
            {
                GameManager.GameMediator.UnRegisterGlobalDataStore(Store);
                Store = null;
            }
        }

        [DoNotToLua]
        /// <summary>
        /// 管理器预初始化
        /// </summary>
        /// <returns></returns>
        public virtual void PreInitManager()
        {
            InitStore();
            if (luaObjectHost == null)
            {
                GameLogger.Error("BaseManager", "LuaModul can oly use when GameLuaObjectHost is bind ! ");
                return;
            }
            if (IsLuaModul && fnPreInitManager != null) fnPreInitManager(luaObjectHost.LuaSelf);
        }
        [DoNotToLua]
        /// <summary>
        /// 当管理器第一次初始化时（场景进入）
        /// </summary>
        /// <returns></returns>
        public virtual  bool InitManager()
        {
            if (IsLuaModul)
            {
                if (luaObjectHost == null)
                {
                    GameLogger.Error(GetFullName(), "LuaModul can oly use when GameLuaObjectHost is bind ! ");
                    return false;
                }
                if (fnInitManager != null) return fnInitManager(luaObjectHost.LuaSelf);
            }
            return false;
        }
        [DoNotToLua]
        /// <summary>
        /// 当管理器卸载时（场景卸载）
        /// </summary>
        /// <returns></returns>
        public virtual bool ReleaseManager()
        {
            if(IsLuaModul && fnReleaseManager != null) return fnReleaseManager(luaObjectHost.LuaSelf);
            return false;
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
        public string GetFullName()
        {
            return packageName + "." + name + ":" + subName;
        }
    }
}
