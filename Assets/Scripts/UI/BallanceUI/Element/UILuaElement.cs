using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using SLua;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Element
{
    /// <summary>
    /// UI 元素  LUA 承载基类
    /// </summary>
    [CustomLuaClass]
    public class UILuaElement : UIElement
    {
        private const string TAG = "UILuaElement";

        public GameLuaObjectHost LuaObjectHost { get; private set; }
        public LuaTable LuaSelf { get { return lua_self; } }

        private void Start()
        {
            InitLua();
        }
        private void Awake()
        {
            if (LuaObjectHost == null)
                InitLua();
        }

        private LuaTable lua_self;
        private LuaFunction lua_Init;
        private LuaFunction lua_SetEventHandler;
        private LuaFunction lua_RemoveEventHandler;
        private LuaFunction lua_SolveXml;

        private void InitLua()
        {
            LuaObjectHost = GetComponent<GameLuaObjectHost>();
            if (LuaObjectHost != null)
            {
                lua_Init = LuaObjectHost.GetLuaFun("Init");
                lua_SetEventHandler = LuaObjectHost.GetLuaFun("SetEventHandler");
                lua_RemoveEventHandler = LuaObjectHost.GetLuaFun("RemoveEventHandler");
                lua_SolveXml = LuaObjectHost.GetLuaFun("SolveXml");
                lua_self = LuaObjectHost.LuaSelf;
                InitLua(LuaObjectHost);
            }
            else GameLogger.Log(TAG, "Not found GameLuaObjectHost in UILuaElement ! name : {0}", name);
        }
        protected virtual void InitLua(GameLuaObjectHost luaObjectHost)
        {

        }

        public void SetBaseName(string baseName)
        {
            base.baseName = baseName;
        }

        public override void Init(string name, string xml)
        {
            base.Init(name, xml);
            if(lua_Init != null)
            {
                if (lua_self != null) lua_Init.call(lua_self, name, xml);
                else lua_Init.call(name, xml);
            }
        }
        public override void Init(string name, XmlNode xml)
        {
            base.Init(name, xml);
            if (lua_Init != null)
            {
                if (lua_self != null) lua_Init.call(lua_self, name, xml);
                else lua_Init.call(name, xml);
            }
        }
        public override void SetEventHandler(string name, GameHandler handler)
        {
            base.SetEventHandler(name, handler);
            if (lua_SetEventHandler != null)
            {
                if (lua_SetEventHandler != null) lua_SetEventHandler.call(lua_self, name, handler);
                else lua_SetEventHandler.call(name, handler);
            }
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            base.RemoveEventHandler(name, handler);
            if (lua_RemoveEventHandler != null)
            {
                if (lua_RemoveEventHandler != null) lua_RemoveEventHandler.call(lua_self, name, handler);
                else lua_RemoveEventHandler.call(name, handler);
            }
        }
        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);
            if (lua_SolveXml != null)
            {
                if (lua_SolveXml != null) lua_SolveXml.call(lua_self, xml);
                else lua_SolveXml.call(xml);
            }
        }


    }
}
