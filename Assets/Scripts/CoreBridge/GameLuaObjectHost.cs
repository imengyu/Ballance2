using Ballance2.ModBase;
using SLua;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 简易 Lua 脚本承载组件
    /// </summary>
    [CustomLuaClass]
    public class GameLuaObjectHost : MonoBehaviour
    {
        public const string TAG = "GameLuaObjectHost";

        public LuaState LuaState { get; set; }
        public string LuaClassName;
        public string Name;
        public GameMod GameMod { get; set; }

        /// <summary>
        /// lua self
        /// </summary>
        public LuaTable LuaSelf { get { return self; } }

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
            LuaInit();
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

        private void LuaInit()
        {
            LuaFunction classInit = GameMod.RequireLuaClass(LuaClassName);
            if (classInit == null)
            {
                GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error :  class not found : {1}", Name, LuaClassName);
                Destroy(this);
                return;
            }

            object o = classInit.call();
            if (o != null && o is LuaTable) self = o as LuaTable;
            else
            {
                GameLogger.Error(TAG + ":" + Name, "LuaObject {0} load error : table not return ", Name);
                Destroy(this);
                return;
            }

            InitLuaEvents();
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
