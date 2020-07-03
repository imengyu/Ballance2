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
        public BaseManager(string name)
        {
            this.name = name;
            subName = "Singleton";
            loadIndex = CommonUtils.GenAutoIncrementID();
        }
        /// <summary>
        /// 创建管理器（多例）
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="subName">二级名称，用于区分多个管理器</param>
        public BaseManager(string name, string subName)
        {
            this.name = name;
            this.subName = subName;
            isSingleton = subName == "Singleton";
            loadIndex = CommonUtils.GenAutoIncrementID();
        }

        private new string name = "";
        private bool isSingleton = false;
        private string subName = "";

        internal int loadIndex = 0;
        internal bool initialized = false;
        internal bool preInitialized = false;
        internal bool replaceable = true;
        private bool isLuaModul = false;

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
            set
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

        private LuaReturnBoolDelegate fnInitManager;
        private LuaVoidDelegate fnPreInitManager;
        private LuaReturnBoolDelegate fnReleaseManager;
        private GameLuaObjectHost luaObjectHost;

        private bool InitLua()
        {
            luaObjectHost = GetComponent<GameLuaObjectHost>();
            if (luaObjectHost == null)
                return false;
            InitLuaFuns();
            return true;
        }
        protected virtual void InitLuaFuns() {
            LuaFunction f = luaObjectHost.GetLuaFun("InitManager");
            if (f != null) fnInitManager = f.cast<LuaReturnBoolDelegate>();
            f = luaObjectHost.GetLuaFun("ReleaseManager");
            if (f != null) fnReleaseManager = f.cast<LuaReturnBoolDelegate>();
        }

        [DoNotToLua]
        /// <summary>
        /// 管理器预初始化
        /// </summary>
        /// <returns></returns>
        public virtual void PreInitManager()
        {
            if (IsLuaModul)
            {
                if (!InitLua())
                    GameLogger.Error("BaseManager", "LuaModul can oly use when GameLuaObjectHost is bind ! ");
                if (fnPreInitManager != null) fnPreInitManager(luaObjectHost.LuaSelf);
            }
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
                    GameLogger.Error("BaseManager", "LuaModul can oly use when GameLuaObjectHost is bind ! ");
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
    }
}
