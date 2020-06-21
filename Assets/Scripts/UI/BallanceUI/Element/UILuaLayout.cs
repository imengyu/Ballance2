using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using SLua;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Element
{
    /// <summary>
    /// UI 布局  LUA 承载基类
    /// </summary>
    [CustomLuaClass]
    public class UILuaLayout : UILuaElement, ILayoutContainer
    {
        private const string TAG = "UILuaLayout";

        private LuaFunction lua_DoLayout;
        private LuaFunction lua_AddElement;
        private LuaFunction lua_RemoveElement;
        private LuaFunction lua_InsertElement;
        private List<UIElement> elements = null;

        public List<UIElement> Elements { get { return elements; } }

        private void Start()
        {
            elements = new List<UIElement>();
        }
        private void OnDestroy()
        {
            if (elements != null)
            {
                elements.Clear();
                elements = null;
            }
        }

        protected override void InitLua(GameLuaObjectHost luaObjectHost)
        {
            if (luaObjectHost != null)
            {
                lua_DoLayout = LuaObjectHost.GetLuaFun("DoLayout");
                lua_AddElement = LuaObjectHost.GetLuaFun("AddElement");
                lua_RemoveElement = LuaObjectHost.GetLuaFun("RemoveElement");
                lua_InsertElement = LuaObjectHost.GetLuaFun("InsertElement");
            }
        }

        public UIElement AddElement(UIElement element, bool doLayout = true)
        {
            object result = null;
            if (lua_AddElement != null)
            {
                if (lua_AddElement != null) result = lua_AddElement.call(LuaSelf, element, doLayout);
                else result = lua_AddElement.call(element, doLayout);
            }
            if (result is UIElement)
                return (UIElement)result;
            return null;
        }
        public void RemoveElement(UIElement element, bool destroy = true, bool doLayout = true)
        {
            if (lua_RemoveElement != null)
            {
                if (lua_RemoveElement != null) lua_RemoveElement.call(LuaSelf, element, destroy, doLayout);
                else lua_RemoveElement.call(element, destroy, doLayout);
            }
        }
        public UIElement InsertElement(UIElement element, int index, bool doLayout = true)
        {
            object result = null;
            if (lua_InsertElement != null)
            {
                if (lua_InsertElement != null) result = lua_InsertElement.call(LuaSelf, element, index, doLayout);
                else result = lua_InsertElement.call(element, index, doLayout);
            }
            if (result is UIElement)
                return (UIElement)result;
            return null;
        }
        public void DoLayout(int startChildIndex)
        {
            if (lua_DoLayout != null)
            {
                if (lua_DoLayout != null) lua_DoLayout.call(LuaSelf, startChildIndex);
                else lua_DoLayout.call(startChildIndex);
            }
        }
        public UIElement FindElementByName(string name)
        {
            foreach (UIElement u in elements)
            {
                if (u.Name == name) return u;
                else if (u is ILayoutContainer)
                {
                    UIElement rs = (u as ILayoutContainer).FindElementByName(name);
                    if (rs != null) return rs;
                }
            }
            return null;
        }

        int loopUpdate = 0;

        private void Update()
        {
            if (loopUpdate > 0)
            {
                loopUpdate--;
                if (loopUpdate == 0)
                    DoLayout(0);
            }

        }

        public void PostDoLayout()
        {
            loopUpdate = 30;
        }


    }
}
