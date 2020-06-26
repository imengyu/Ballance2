using Ballance2.Managers;
using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// UI 布局构建器
    /// </summary>
    [SLua.CustomLuaClass]
    public class UILayoutBuilder
    {
        private const string TAG = "UILayoutBuilder";

        public UILayoutBuilder(UIManager manager)
        {
            UIManager = manager;
        }

        private UIManager UIManager;

        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlerNames">UI事件接收器目标列表</param>
        /// <param name="handlers">UI事件接收器函数</param>
        /// <param name="self">UI事件接收器接收类</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, SLua.LuaFunction[] handlers, SLua.LuaTable self = null, string[] initialProps = null)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            for (int i = 0; i < handlerNames.Length; i++)
                handlerList.Add(handlerNames[i], new GameHandler(name, handlers[i], self));
            return BuildLayoutByTemplate(name, templateXml, handlerList, initialProps);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">LUA接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, string[] handlers, string[] initialProps)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            string h = "";
            for (int i = 0; i < handlerNames.Length; i++)
            {
                h = handlers[i];
                handlerList.Add(handlerNames[i], new GameHandler(name + ":" + h, h));
            }

            return BuildLayoutByTemplate(name, templateXml, handlerList, initialProps);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">LUA接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, GameEventHandlerDelegate[] handlers, string[] initialProps)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            GameEventHandlerDelegate h = null;
            for (int i = 0; i < handlerNames.Length; i++)
            {
                h = handlers[i];
                handlerList.Add(handlerNames[i], new GameHandler(name + ":" + h.Method.Name, h));
            }
            return BuildLayoutByTemplate(name, templateXml, handlerList, initialProps);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, Dictionary<string, GameHandler> handlers, string[] initialProps)
        {
            if(string.IsNullOrEmpty(templateXml))
            {
                GameLogger.Error(TAG, "BuildLayoutByTemplate {0} failed, templateXml is Empty", name);
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return null;
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(templateXml);

            return BuildLayoutByTemplateInternal(name, xmlDocument.DocumentElement,
                handlers, null, null, initialProps); 
        }

        private UILayout BuildLayoutByTemplateInternal(string name, XmlNode templateXml, 
            Dictionary<string, GameHandler> handlers, 
            UILayout parent, UILayout root, string[] initialProps)
        {
            //实例化UI预制体
            string prefabName = templateXml.Name;
            GameObject prefab = UIManager.FindRegisterElementPrefab(prefabName);
            if (prefab == null)
            {
                GameLogger.Log(TAG, "BuildLayoutByTemplate failed, not found prefab {0}", prefabName);
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }

            //获取预制体上的脚本
            UILayout ilayout = prefab.GetComponent<UILayout>();
            if (ilayout == null) //该方法必须实例化UI容器
            {
                GameLogger.Error(TAG, "BuildLayoutByTemplate with prefab {0} failed, root must be a container", prefabName);
                GameErrorManager.LastError = GameError.MustBeContainer;
                return null;
            }

            GameObject newCon = GameCloneUtils.CloneNewObjectWithParent(prefab,
                parent == null ? UIManager.UIRoot.transform : parent.RectTransform);
            ilayout = newCon.GetComponent<UILayout>();
            ilayout.DoCallStart();
            ilayout.LayoutLock();
            ilayout.RectTransform = newCon.GetComponent<RectTransform>();
            UIAnchorPosUtils.SetUIPivot(ilayout.RectTransform, UIPivot.TopCenter);
            ilayout.RectTransform.anchoredPosition = Vector2.zero;

            if (root == null) root = ilayout;

            GameObject newEle = null;
            UIElement uIElement = null;

            //子元素
            for (int i = 0, c = templateXml.ChildNodes.Count; i < c; i++)
            {
                string eleName = "";
                //xml 属性读取
                XmlNode eleNode = templateXml.ChildNodes[i];
                foreach (XmlAttribute a in eleNode.Attributes)
                    if (a.Name.ToLower() == "name")
                        eleName = a.Value;

                //预制体
                prefab = UIManager.FindRegisterElementPrefab(eleNode.Name);
                if (prefab == null)
                {
                    GameLogger.Error(TAG, "BuildLayoutByTemplate failed, not found prefab {0}", prefabName);
                    continue;
                }
                if (prefab.GetComponent<UILayout>() != null)//这是UI容器
                {
                    UILayout newLayout = BuildLayoutByTemplateInternal(eleName, eleNode, handlers, ilayout, root, initialProps);//递归构建

                    uIElement = newLayout;
                    uIElement.rootContainer = root;
                    uIElement.Name = eleName;
                }
                else
                {
                    //构建子元素
                    newEle = GameCloneUtils.CloneNewObjectWithParent(prefab, ilayout.RectTransform, eleName);

                    uIElement = newEle.GetComponent<UIElement>();
                    uIElement.RectTransform = newEle.GetComponent<RectTransform>();
                    uIElement.DoCallStart();
                    uIElement.Init(eleNode);
                    uIElement.rootContainer = root;
                    uIElement.Name = eleName;
                }

                //初始化
                InitiazeChildElement(uIElement, handlers, initialProps);

                //添加元素
                ilayout.AddElement(uIElement, false);
            }

            ilayout.Init(templateXml); //容器的XML读取
            InitiazeChildElement(ilayout, handlers, initialProps);
            ilayout.LayoutUnLock();
            ilayout.PostDoLayout();

            if (root == ilayout)
                ilayout.DoLayout();

            return ilayout;
        }

        private void InitiazeChildElement(UIElement uIElement, Dictionary<string, GameHandler> handlers, string[] initialProps)
        {
            //初始化子元素的事件接收器
            Dictionary<string, GameHandler> lateInitHandlers = new Dictionary<string, GameHandler>();
            foreach (string key in handlers.Keys)
            {
                string[] sp = key.Split(':');
                if (sp.Length >= 2 && sp[0] == uIElement.Name)
                {
                    try { lateInitHandlers.Add(sp[1], handlers[key]); }
                    catch (System.ArgumentException e)
                    {
                        GameLogger.Warning(TAG, "Add event for {0} -> {1} failed {2}", key, handlers[key].Name, e.Message);
                        continue;
                    }
                }
            }
            if (lateInitHandlers.Count > 0)
                uIElement.InitHandlers(lateInitHandlers);

            if (initialProps != null)
            {
                //初始化一些初始属性参数
                foreach (string v in initialProps)
                {
                    string[] sp = v.Split(':');
                    if (sp.Length >= 3 && sp[0] == uIElement.Name)
                        uIElement.SetProperty(sp[1], sp[2]);
                }
            }
        }

    }
}
