using Ballance2.Interfaces;
using Ballance2.ModBase;
using Ballance2.Utils;
using SLua;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 简易 Lua 脚本承载组件
    /// </summary>
    /// <remarks>
    /// 使用方法：
    /// ★ 可以直接绑定此组件至你的 Prefab 上，填写 LuaClassName 与 LuaModName，
    /// Instantiate Prefab 后GameLuaObjectHost会自动找到模块并加载 LUA 文件并执行
    /// ★ 也可以在 GameMod 中直接调用 RegisterLuaObject 注册一个 Lua 对象
    /// ☆ 以上两种方法都可以在 GameMod 中使用 FindLuaObject 找到你注册的 Lua 对象
    /// </remarks>
    [CustomLuaClass]
    public class GameLuaObjectHost : MonoBehaviour
    {
        public const string TAG = "GameLuaObjectHost";

        /// <summary>
        /// lua self
        /// </summary>
        public LuaTable LuaSelf { get { return self; } }
        /// <summary>
        /// 获取当前虚拟机
        /// </summary>
        public LuaState LuaState { get; set; }
        /// <summary>
        /// 获取或设置 Lua类的文件名（eg MenuLevel）
        /// </summary>
        public string LuaClassName;
        /// <summary>
        /// 获取或设置 Lua 类所在的模块包名（改模块必须是 ModPack 并可运行）
        /// </summary>
        public string LuaModName;
        /// <summary>
        /// LUA 对象名字，用于 FindLuaObject 查找
        /// </summary>
        public string Name;
        /// <summary>
        /// 获取对应 模组包
        /// </summary>
        public GameMod GameMod { get; set; }

        private IModManager ModManager;

        private LuaTable self = null;
        private LuaVoidDelegate update = null;
        private LuaStartDelegate start = null;
        private LuaVoidDelegate awake = null;
        private LuaVoidDelegate onGUI = null;
        private LuaVoidDelegate onDestory = null;
        private LuaCollisionDelegate onCollisionEnter = null;
        private LuaCollisionDelegate onCollisionExit = null;
        private LuaCollisionDelegate onCollisionStay = null;

        private void Start()
        {
            ModManager = ((IModManager)GameManager.GetManager("ModManager"));
            if (!LuaInit())
            {
                enabled = false;
                GameLogger.Warning(TAG + ":" + Name, "LuaObject disabled because {0} load error", Name);
            }

            if (start != null) start(self, gameObject);
        }
        private void Awake()
        {
            if (awake != null) awake(self);
        }
        private void Update()
        {
            if (update != null) update(self);
        }

        private void OnDestroy()
        {
            if (onDestory != null) onDestory(self);
            StopLuaEvents();
            GameMod.RemoveLuaObject(this);
        }
        private void OnGUI()
        {
            if (onGUI != null) onGUI(self);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (onCollisionEnter != null) onCollisionEnter(self, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            if (onCollisionExit != null) onCollisionExit(self, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            if (onCollisionStay != null) onCollisionStay(self, collision);
        }

        private bool LuaInit()
        {
            if(GameMod ==  null)
            {
                if(string.IsNullOrEmpty(LuaModName))
                {
                    GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error :  LuaModName not provide ", Name);
                    GameErrorManager.LastError = GameError.ParamNotProvide;
                    return false;
                }

                GameMod = ModManager.FindGameModByName(LuaModName); 

                if (GameMod == null)
                {
                    GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error :  LuaModName not found : {1}", Name, LuaModName);
                    GameErrorManager.LastError = GameError.NotRegister;
                    return false;
                }

                GameMod.AddeLuaObject(this);

                LuaState = GameMod.ModLuaState;
                if(LuaState == null)
                {
                    GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error :  Mod can not run : {1}", Name, LuaModName);
                    GameErrorManager.LastError = GameError.ModCanNotRun;
                    return false;
                }
            }

            LuaFunction classInit = GameMod.RequireLuaClass(LuaClassName);
            if (classInit == null)
            {
                GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error :  class not found : {1}", Name, LuaClassName);
                GameErrorManager.LastError = GameError.FunctionNotFound;
                return false;
            }

            object o = classInit.call();
            if (o != null && o is LuaTable) self = o as LuaTable;
            else
            {
                GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error : table not return ", Name);
                GameErrorManager.LastError = GameError.NotReturn;
                return false;
            }

            InitLuaEvents();
            return true;
        }
        private void InitLuaEvents()
        {
            LuaFunction fun = null;

            fun = self["Start"] as LuaFunction;
            if (fun != null) start = fun.cast<LuaStartDelegate>();

            fun = self["Update"] as LuaFunction;
            if (fun != null) update = fun.cast<LuaVoidDelegate>();

            fun = self["Awake"] as LuaFunction;
            if (fun != null) awake = fun.cast<LuaVoidDelegate>();

            fun = self["OnGUI"] as LuaFunction;
            if (fun != null) onGUI = fun.cast<LuaVoidDelegate>();

            fun = self["OnGUI"] as LuaFunction;
            if (fun != null) onGUI = fun.cast<LuaVoidDelegate>();

            fun = self["OnCollisionEnter"] as LuaFunction;
            if (fun != null) onCollisionEnter = fun.cast<LuaCollisionDelegate>();

            fun = self["OnCollisionExit"] as LuaFunction;
            if (fun != null) onCollisionExit = fun.cast<LuaCollisionDelegate>();

            fun = self["onCollisionStay"] as LuaFunction;
            if (fun != null) onCollisionStay = fun.cast<LuaCollisionDelegate>();
        }
        private void StopLuaEvents()
        {
            update = null;
            start = null;
            awake = null;
            onGUI = null;
            onDestory = null;
        }

        /// <summary>
        /// 获取当前 Lua 类
        /// </summary>
        /// <returns></returns>
        public LuaTable GetLuaClass()
        {
            return self;
        }
        /// <summary>
        /// 获取当前 Object 的指定函数
        /// </summary>
        /// <param name="funName">函数名</param>
        /// <returns>返回函数，未找到返回null</returns>
        public LuaFunction GetLuaFun(string funName)
        {
            LuaFunction f = null;
            if (self != null) f = self[funName] as LuaFunction;
            else f = LuaState.getFunction(funName);
            return f;
        }
        /// <summary>
        /// 调用的lua无参函数
        /// </summary>
        /// <param name="funName">lua函数名称</param>
        public void CallLuaFun(string funName)
        {
            LuaFunction f = GetLuaFun(funName);
            if (f != null) f.call(self);
        }
        /// <summary>
        /// 调用的lua函数
        /// </summary>
        /// <param name="funName">lua函数名称</param>
        /// <param name="pararms">参数</param>
        public void CallLuaFun(string funName, params object[] pararms)
        {
            LuaFunction f = GetLuaFun(funName);
            if (f != null) f.call(self, pararms);
        }
    }
}
