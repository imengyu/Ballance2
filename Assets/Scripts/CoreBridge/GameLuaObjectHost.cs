using Ballance2.Interfaces;
using Ballance2.ModBase;
using Ballance2.Utils;
using SLua;
using SubjectNerd.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 简易 Lua 脚本承载组件
    /// </summary>
    /// <remarks>
    /// ✧使用方法：
    ///     ★ 可以直接绑定此组件至你的 Prefab 上，填写 LuaClassName 与 LuaModName，
    ///     Instantiate Prefab 后GameLuaObjectHost会自动找到模块并加载 LUA 文件并执行。
    ///     如果找不到模块或LUA 文件，将会抛出错误。
    ///     ★ 也可以在 GameMod 中直接调用 RegisterLuaObject 注册一个 Lua 对象
    ///     ☆ 以上两种方法都可以在 GameMod 中使用 FindLuaObject 找到你注册的 Lua 对象
    /// 
    /// ✧参数引入
    ///     可以在编辑器中设置 LuaInitialVars 添加你想引入的参数，承载组件会自动将的参数设置
    ///     到你的 Lua脚本 self 上，通过 self.参数名 就可以访问了。
    /// 
    /// </remarks>
    [CustomLuaClass]
    [AddComponentMenu("Ballance/Lua/GameLuaObjectHost")]

    public class GameLuaObjectHost : MonoBehaviour
    {
        public const string TAG = "GameLuaObjectHost";

        /// <summary>
        /// LUA 对象名字，用于 FindLuaObject 查找
        /// </summary>
        [Tooltip("LUA 对象名字，用于 FindLuaObject 查找")]
        public string Name;

        /// <summary>
        /// 获取或设置 Lua类的文件名（eg MenuLevel）
        /// </summary>
        [Tooltip("设置 Lua类的文件名（eg MenuLevel）")]
        public string LuaClassName;
        /// <summary>
        /// 获取或设置 Lua 类所在的模块包名（改模块必须是 ModPack 并可运行）。设置后该对象会自动注册到 LuaObject 中
        /// </summary>
        [Tooltip("设置 Lua 类所在的模块包名（改模块必须是 ModPack 并可运行）。设置后该对象会自动注册到 LuaObject 中")]
        public string LuaModName;
        /// <summary>
        /// 设置 LUA 初始参数，用于方便地从 Unity 编辑器直接引入初始参数至 Lua，这些变量会设置到 Lua self 上，可直接获取。
        /// </summary>
        [Tooltip("设置 LUA 初始参数，用于方便地从 Unity 编辑器直接引入初始参数至 Lua，这些变量会设置到 Lua self 上，可直接获取。")]
        [SerializeField]
        public List<LuaVarObjectInfo> LuaInitialVars = new List<LuaVarObjectInfo>();


        /// <summary>
        /// lua self
        /// </summary>
        public LuaTable LuaSelf { get { return self; } }
        /// <summary>
        /// 获取当前虚拟机
        /// </summary>
        public LuaState LuaState { get; set; }
        /// <summary>
        /// 获取对应 模组包
        /// </summary>
        public GameMod GameMod { get; set; }

        private IModManager ModManager;

        private LuaTable self = null;
        private LuaVoidDelegate update = null;
        private LuaVoidDelegate fixedUpdate = null;
        private LuaVoidDelegate lateUpdate = null;
        private LuaStartDelegate start = null;
        private LuaVoidDelegate awake = null;
        private LuaVoidDelegate onGUI = null;
        private LuaVoidDelegate onDestory = null;
        private LuaVoidDelegate onEnable = null;
        private LuaVoidDelegate onDisable = null;
        private LuaVoidDelegate reset = null;

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
        private void FixedUpdate()
        {
            if (fixedUpdate != null) fixedUpdate(self);
        }
        private void LateUpdate()
        {
            if (lateUpdate != null) lateUpdate(self);
        }

        private void OnGUI()
        {
            if (onGUI != null) onGUI(self);
        }
        private void OnDestroy()
        {
            if (onDestory != null) onDestory(self);
            StopLuaEvents();
            GameMod.RemoveLuaObject(this);
        }
        private void OnDisable()
        {
            if (onDisable != null) onDisable(self);
        }
        private void OnEnable()
        {
            if (onEnable != null) onEnable(self);
        }

        private void Reset()
        {
            if (reset != null) reset(self);
        }

        // Init and get
        // ===========================

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

            InitLuaInternalVars();
            InitLuaVars(); //初始化引入参数
            //调用其他LUA初始化脚本
            SendMessage("OnInitLua", gameObject, SendMessageOptions.DontRequireReceiver);
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

            fun = self["FixedUpdate"] as LuaFunction;
            if (fun != null) fixedUpdate = fun.cast<LuaVoidDelegate>();

            fun = self["LateUpdate"] as LuaFunction;
            if (fun != null) lateUpdate = fun.cast<LuaVoidDelegate>();

            fun = self["onEnable"] as LuaFunction;
            if (fun != null) onEnable = fun.cast<LuaVoidDelegate>();

            fun = self["OnDisable"] as LuaFunction;
            if (fun != null) onDisable = fun.cast<LuaVoidDelegate>();

            fun = self["Reset"] as LuaFunction;
            if (fun != null) reset = fun.cast<LuaVoidDelegate>();
        }
        private void InitLuaInternalVars()
        {
            LuaSelf["transform"] = transform;
            LuaSelf["gameObject"] = gameObject;
        }
        private void InitLuaVars()
        {
            foreach(LuaVarObjectInfo v in LuaInitialVars)
            {
                if (!string.IsNullOrEmpty(v.Name))
                {
                    switch (v.Type)
                    {
                        case LuaVarObjectType.None: LuaSelf[v.Name] = null; break;
                        case LuaVarObjectType.Vector2: LuaSelf[v.Name] = v.Vector2(); break;
                        case LuaVarObjectType.Vector2Int: LuaSelf[v.Name] = v.Vector2Int(); break;
                        case LuaVarObjectType.Vector3: LuaSelf[v.Name] = v.Vector3(); break;
                        case LuaVarObjectType.Vector3Int: LuaSelf[v.Name] = v.Vector3Int(); break;
                        case LuaVarObjectType.Vector4: LuaSelf[v.Name] = v.Vector4(); break;
                        case LuaVarObjectType.Rect: LuaSelf[v.Name] = v.Rect(); break;
                        case LuaVarObjectType.RectInt: LuaSelf[v.Name] = v.RectInt(); break;
                        case LuaVarObjectType.Gradient: LuaSelf[v.Name] = v.Gradient(); break;
                        case LuaVarObjectType.Layer: LuaSelf[v.Name] = v.Layer(); break;
                        case LuaVarObjectType.Curve: LuaSelf[v.Name] = v.Curve(); break;
                        case LuaVarObjectType.Color: LuaSelf[v.Name] = v.Color(); break;
                        case LuaVarObjectType.BoundsInt: LuaSelf[v.Name] = v.BoundsInt(); break;
                        case LuaVarObjectType.Bounds: LuaSelf[v.Name] = v.Bounds(); break;
                        case LuaVarObjectType.Object: LuaSelf[v.Name] = v.Object(); break;
                        case LuaVarObjectType.GameObject: LuaSelf[v.Name] = v.GameObject(); break;
                        case LuaVarObjectType.Long: LuaSelf[v.Name] = v.Long(); break;
                        case LuaVarObjectType.Int: LuaSelf[v.Name] = v.Int(); break;
                        case LuaVarObjectType.String: LuaSelf[v.Name] = v.String(); break;
                        case LuaVarObjectType.Double: LuaSelf[v.Name] = v.Double(); break;
                        case LuaVarObjectType.Bool: LuaSelf[v.Name] = v.Bool(); break;
                    }
                }
            }
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
