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

        public LuaTable LuaSelf { get { return self; } }

        private LuaTable self;
        private LuaFunction update;
        private LuaFunction start;
        private LuaFunction awake;
        private LuaFunction onGUI;
        private LuaFunction onDestory;
        private LuaFunction onCollisionEnter;
        private LuaFunction onCollisionExit;
        private LuaFunction onCollisionStay;

        private void Start()
        {
            LuaInit();
            if (start != null) start.call(self, gameObject);
        }
        private void Awake()
        {
            if (awake != null) awake.call(self);
        }
        private void Update()
        {
            if (update != null) update.call(self);
        }

        private void OnDestroy()
        {
            if (onDestory != null) onDestory.call(self);
            StopLuaEvents();
            GameMod.RemoveLuaObject(this);
        }
        private void OnGUI()
        {
            if (onGUI != null) onGUI.call(self);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (onCollisionEnter != null) onCollisionEnter.call(self, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            if (onCollisionExit != null) onCollisionExit.call(self, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            if (onCollisionStay != null) onCollisionStay.call(self, collision);
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

            start = self["Start"] as LuaFunction;
            update = self["Update"] as LuaFunction;
            awake = self["Awake"] as LuaFunction;
            onGUI = self["OnGUI"] as LuaFunction;
            onDestory = self["OnDestroy"] as LuaFunction;
            onCollisionEnter = self["OnCollisionEnter"] as LuaFunction;
            onCollisionExit = self["OnCollisionExit"] as LuaFunction;
            onCollisionStay = self["OnCollisionStay"] as LuaFunction;
        }
        private void StopLuaEvents()
        {
            if (update != null) update.Dispose();
            if (start != null) start.Dispose();
            if (awake != null) awake.Dispose();
            if (onGUI != null) onGUI.Dispose();
            if (onDestory != null) onDestory.Dispose();
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
