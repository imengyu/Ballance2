using Ballance2.Managers.CoreBridge;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.Managers
{
    /// <summary>
    /// UI 管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIManager : BaseManagerSingleton
    {
        public const string TAG = "UIManager";

        public UIManager() : base(TAG)
        {

        }

        public override bool InitManager()
        {
            UIRoot = GameManager.GameCanvas;
            UIFadeManager = UIRoot.AddComponent<UIFadeManager>();

            GlobalFadeMaskWhite = UIRoot.transform.Find("GlobalFadeMaskWhite").gameObject.GetComponent<Image>();
            GlobalFadeMaskBlack = UIRoot.transform.Find("GlobalFadeMaskBlack").gameObject.GetComponent<Image>();
            GlobalFadeMaskBlack.gameObject.SetActive(true);

            InitAllObects();
            InitWindowManagement();
            InitPageManagement();
            InitUIPrefabs();
            InitUIDebug();

            return true;
        }
        public override bool ReleaseManager()
        {
            DestroyWindowManagement();
            DestroyPageManagement();

            GameLogger.Log(TAG, "Destroy {0} ui objects", UIRoot.transform.childCount);
            for (int i = 0, c = UIRoot.transform.childCount; i < c; i++)
                UnityEngine.Object.Destroy(UIRoot.transform.GetChild(i).gameObject);
            return true;
        }

        /// <summary>
        /// UI 根
        /// </summary>
        public GameObject UIRoot;
        /// <summary>
        /// 渐变管理器
        /// </summary>
        public UIFadeManager UIFadeManager;

        //根管理
        private RectTransform TemporarilyRectTransform;
        private RectTransform GlobalWindowRectTransform;
        private RectTransform PagesRectTransform;
        private RectTransform WindowsRectTransform;
        private RectTransform ViewsRectTransform;
        private RectTransform PageContainerRectTransform;
        private RectTransform PageBackgroundRectTransform;

        private void InitAllObects()
        {
            TemporarilyRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUITemporarily").GetComponent<RectTransform>();
            TemporarilyRectTransform.gameObject.SetActive(false);
            GlobalWindowRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIGlobalWindow").GetComponent<RectTransform>();
            PagesRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIPages").GetComponent<RectTransform>();
            WindowsRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIWindow").GetComponent<RectTransform>();
            ViewsRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "ViewsRectTransform").GetComponent<RectTransform>();
            UIAnchorPosUtils.SetUIAnchor(ViewsRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(ViewsRectTransform, 0, 0, 0, 0);
            UIAnchorPosUtils.SetUIAnchor(PagesRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(PagesRectTransform, 0, 0, 0, 0);

            PageBackgroundRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(PagesRectTransform, "PageBackground").GetComponent<RectTransform>();
            PageContainerRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(PagesRectTransform, "PageContainer").GetComponent<RectTransform>();

            UIAnchorPosUtils.SetUIAnchor(PageContainerRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(PageContainerRectTransform, 0, 0, 0, 0);
            UIAnchorPosUtils.SetUIAnchor(PageBackgroundRectTransform, UIAnchor.Stretch, UIAnchor.Stretch);
            UIAnchorPosUtils.SetUIPos(PageBackgroundRectTransform, 0, 0, 0, 0);
        }

        #region 全局渐变遮罩

        private Image GlobalFadeMaskWhite;
        private Image GlobalFadeMaskBlack;

        /// <summary>
        /// 全局黑色遮罩隐藏
        /// </summary>
        public void MaskBlackSet(bool show)
        {
            GlobalFadeMaskBlack.color = new Color(GlobalFadeMaskBlack.color.r,
                   GlobalFadeMaskBlack.color.g, GlobalFadeMaskBlack.color.b, show ? 1.0f : 0f);
            GlobalFadeMaskBlack.gameObject.SetActive(show);
        }
        /// <summary>
        /// 全局黑色遮罩隐藏
        /// </summary>
        public void MaskWhiteSet(bool show)
        {
            GlobalFadeMaskWhite.color = new Color(GlobalFadeMaskWhite.color.r,
                GlobalFadeMaskWhite.color.g, GlobalFadeMaskWhite.color.b, show ? 1.0f : 0f);
            GlobalFadeMaskWhite.gameObject.SetActive(show);
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
        }
        /// <summary>
        /// 全局白色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskWhite, second);
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
        }
        /// <summary>
        /// 全局白色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskWhite, second, true);
        }

        #endregion

        #region 全局对话框

        /// <summary>
        /// 显示全局 Alert 对话框
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <returns></returns>
        public int GlobalAlert(string text, string title, string okText = "确定" )
        {
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIAlertWindow, WindowsRectTransform.transform, "");
            UIAlertWindow window = windowGo.GetComponent<UIAlertWindow>();
            RegisterWindow(window);
            window.Show(text, title, okText);
            window.onClose = (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    id, false);
            };
            return window.GetWindowId();
        }
        /// <summary>
        /// 显示全局 Confirm 对话框
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <param name="cancelText">Cancel 按钮文字</param>
        /// <returns></returns>
        public int GlobalConfirm(string text, string title, string okText = "确定", string cancelText = "取消")
        {
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIConfirmWindow, WindowsRectTransform.transform, "");
            UIConfirmWindow window = windowGo.GetComponent<UIConfirmWindow>();
            RegisterWindow(window);
            window.Show(text, title, okText, cancelText);
            window.onClose = (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    id, window.IsConfirmed);
            };
            return window.GetWindowId();
        }

        #endregion

        #region 窗口管理

        private GameObject PrefabUIAlertWindow;
        private GameObject PrefabUIConfirmWindow;
        private GameObject PrefabUIWindow;

        private void InitWindowManagement()
        {
            managedWindows = new List<IWindow>();

            PrefabUIAlertWindow = GameManager.FindStaticPrefabs("UIAlertWindow");
            PrefabUIConfirmWindow = GameManager.FindStaticPrefabs("UIConfirmWindow");
            PrefabUIWindow = GameManager.FindStaticPrefabs("UIWindow");

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE);
        }
        private void DestroyWindowManagement()
        {
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE);

            if (managedWindows != null)
            {
                foreach (IWindow w in managedWindows)
                    w.Destroy();
                managedWindows.Clear();
                managedWindows = null;
            }
        }

        //窗口
        private List<IWindow> managedWindows = null;

        /// <summary>
        /// 创建自定义窗口（默认不显示）
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="customView">窗口自定义View</param>
        /// <returns></returns>
        public UIWindow CreateWindow(string title, RectTransform customView)
        {
            return CreateWindow(title, customView, false);
        }
        /// <summary>
        /// 创建自定义窗口
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="show">创建后是否立即显示</param>
        /// <param name="customView">窗口自定义View</param>
        /// <returns></returns>
        public UIWindow CreateWindow(string title, RectTransform customView, bool show)
        {
            return CreateWindow(title, customView, false, 0, 0, 0, 0);
        }
        /// <summary>
        /// 创建自定义窗口
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="show">创建后是否立即显示</param>
        /// <param name="customView">窗口自定义View</param>
        /// <param name="x">X 坐标</param>
        /// <param name="y">Y 坐标</param>
        /// <param name="w">宽度，0 使用默认</param>
        /// <param name="h">高度，0 使用默认</param>
        /// <returns></returns>
        public UIWindow CreateWindow(string title, RectTransform customView, bool show, float x, float y, float w, float h)
        {
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIWindow, WindowsRectTransform.transform, "GameUIWindow_" + title);
            UIWindow window = windowGo.GetComponent<UIWindow>();
            window.Title = title;
            window.SetPos(x, y);
            window.SetView(customView);
            if (w != 0 && h != 0) window.SetSize(w, h);
            if (show) window.Show();
            RegisterWindow(window);
            return window;
        }
        /// <summary>
        /// 注册窗口到管理器中
        /// </summary>
        /// <param name="window">窗口</param>
        /// <returns></returns>
        public UIWindow RegisterWindow(UIWindow window)
        {
            window.name = "GameUIWindow_" + window.Title;
            managedWindows.Add(window);
            return window;
        }
        /// <summary>
        /// 通过 ID 查找窗口
        /// </summary>
        /// <param name="windowId">窗口ID</param>
        /// <returns></returns>
        public IWindow FindWindowById(int windowId)
        {
            foreach (IWindow w in managedWindows)
                if (w.GetWindowId() == windowId)
                    return w;
            return null;
        }

        private IWindow currentVisibleWindowAlert = null;

        public void ShowWindow(IWindow window)
        {
            switch(window.GetWindowType())
            {
                case WindowType.GlobalAlert:
                    window.GetRectTransform().transform.SetParent(GlobalWindowRectTransform.transform);
                    PagesRectTransform.gameObject.SetActive(false);
                    WindowsRectTransform.gameObject.SetActive(false);
                    WindowsRectTransform.SetAsLastSibling();
                    currentVisibleWindowAlert = window;
                    break;
                case WindowType.Normal:
                    window.GetRectTransform().transform.SetParent(WindowsRectTransform.transform);
                    WindowsRectTransform.SetAsLastSibling();
                    break;
                case WindowType.Page:
                    window.GetRectTransform().transform.SetParent(PagesRectTransform.transform);
                    PagesRectTransform.SetAsLastSibling();
                    break;
            }
            window.SetVisible(true);
        }
        public void HideWindow(IWindow window) { window.SetVisible(false);  }
        public void CloseWindow(IWindow window)  { window.Destroy(); managedWindows.Remove(window);  }

        private int windowId = 0;
        public int GenWindowId()
        {
            if (windowId < 2048) windowId++;
            return windowId;
        }

        #endregion

        #region 页管理

        //页
        private List<UIPage> managedPages = null;
        private List<UIPrefab> pagePrefabs = null;
        private List<UIPrefab> elementPrefabs = null;

        private struct UIPrefab {
            public UIPrefab(GameObject o, string n)
            {
                Prefab = o;
                Name = n;
                LuaObjectHost = o.GetComponent<GameLuaObjectHost>();
                IsLuaPrefab = LuaObjectHost != null;
            }

            public GameObject Prefab;
            public string Name;
            public bool IsLuaPrefab;
            public GameLuaObjectHost LuaObjectHost;
        }
        private UIPage currentShowPage = null;

        private void InitPageManagement()
        {
            managedPages = new List<UIPage>();
            pagePrefabs = new List<UIPrefab>();
            elementPrefabs = new List<UIPrefab>();

            GameObject UIPagePrefabDefault = GameManager.FindStaticPrefabs("UIPagePrefabDefault");
            GameObject UIPageBackgroundDefault = GameManager.FindStaticPrefabs("UIPageBackgroundDefault");

            RegisterPageBackgroundPrefab(UIPageBackgroundDefault, "Default");
            RegisterPagePrefab(UIPagePrefabDefault, "Default");
        }
        private void DestroyPageManagement()
        {
            if (managedPages != null)
            {
                foreach (UIPage w in managedPages)
                    w.Destroy();
                managedPages.Clear();
                managedPages = null;
            }
            if (pagePrefabs != null)
            {
                pagePrefabs.Clear();
                pagePrefabs = null;
            }
            if (elementPrefabs != null)
            {
                elementPrefabs.Clear();
                elementPrefabs = null;
            }
        }

        /// <summary>
        /// 注册 Ballance 样式的 UI 页
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="template">UI模板</param>
        /// <param name="handlerNames">UI事件接收器目标列表</param>
        /// <param name="handlers">UI事件接收器函数</param>
        /// <param name="self">UI事件接收器接收类</param>
        /// <returns></returns>
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, SLua.LuaFunction[] handlers, SLua.LuaTable self, string backgroundPrefabName = "Default")
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            UILayout layoutContainer = BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers, self);
            if(layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.MinSize = new Vector2(0, Screen.height);
            return RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer);
        }
        /// <summary>
        /// 注册 Ballance 样式的 UI 页
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">UI事件接收器模板</param>
        /// <returns></returns>
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, GameHandlerDelegate[] handlers, string backgroundPrefabName = "Default")
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }
            UILayout layoutContainer = BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers);
            if (layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.MinSize = new Vector2(0, Screen.height);
            return RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer);
        }
        /// <summary>
        /// 注册 Ballance 样式的 UI 页
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">UI事件接收器模板</param>
        /// <returns></returns>
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, string[] handlers, string backgroundPrefabName = "Default")
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            UILayout layoutContainer = BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers);
            if (layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.MinSize = new Vector2(0, Screen.height);
            return RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer); 
        }
        /// <summary>
        /// 注册 UI 页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <param name="prefabName">注册的 Prefab 名称</param>
        /// <param name="backgroundPrefabName">注册的背景 Prefab 名称</param>
        /// <param name="content">页内容</param>
        /// <returns></returns>
        public UIPage RegisterUIPage(string pagePath, string prefabName, string backgroundPrefabName, UILayout content)
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            //找到预制体
            GameObject prefab = FindRegisterPagePrefab(prefabName);
            if (prefab == null)
            {
                GameLogger.Error(TAG, "RegisterUIPage failed, not found prefab {0}", prefabName);
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }

            //实例化
            GameObject pageGo = GameCloneUtils.CloneNewObjectWithParent(prefab, PageContainerRectTransform, pagePath, false);
            UIPage page = pageGo.GetComponent<UIPage>();
            page.RectTransform = pageGo.GetComponent<RectTransform>();
            if (page.ContentRectTransform == null)
                page.ContentRectTransform = page.RectTransform;
            page.PagePath = pagePath;
            if (page.ContentContainer.RectTransform == null)
                page.ContentContainer.RectTransform = page.ContentContainer.gameObject.GetComponent<RectTransform>();
            page.ContentContainer.AddElement(content, false);

            //背景
            SetPageBackground(page, backgroundPrefabName);

            managedPages.Add(page);
            return page;
        }
        /// <summary>
        /// 注册 UI 页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <param name="prefabName">注册的 Prefab 名称</param>
        /// <param name="backgroundPrefabName">注册的背景 Prefab 名称</param>
        /// <param name="content">页内容</param>
        /// <returns></returns>
        public UIPage RegisterUIPage(string pagePath, string prefabName, string backgroundPrefabName, RectTransform content)
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            //找到预制体
            GameObject prefab = FindRegisterPagePrefab(prefabName);
            if (prefab == null)
            {
                GameLogger.Error(TAG, "RegisterUIPage failed, not found prefab {0}", prefabName);
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }

            //实例化
            GameObject pageGo = GameCloneUtils.CloneNewObjectWithParent(prefab, PageContainerRectTransform, pagePath, false);
            UIPage page = pageGo.GetComponent<UIPage>();
            page.RectTransform = pageGo.GetComponent<RectTransform>();
            if (page.ContentRectTransform == null)
                page.ContentRectTransform = page.RectTransform;
            page.PagePath = pagePath;
            if (content.parent != page.ContentRectTransform) {
                content.SetParent(page.ContentRectTransform);
                UIAnchorPosUtils.SetUIPos(content, 0, 0, 0, 0);
            }

            //背景
            SetPageBackground(page, backgroundPrefabName);

            managedPages.Add(page);
            return page;
        }

        private void SetPageBackground(UIPage page, string backgroundPrefabName)
        {
            if (!string.IsNullOrEmpty(backgroundPrefabName) && backgroundPrefabName != "None")
            {
                page.PageBackground = FindPageBackgroundPrefab(backgroundPrefabName);
                if (page.PageBackground == null)
                    GameLogger.Error(TAG, "Not found PageBackground prefab {0}", backgroundPrefabName);
            }
        }

        /// <summary>
        /// 查找已注册页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <returns></returns>
        public UIPage FindUIPage(string pagePath)
        {
            foreach(UIPage p in managedPages)
                if(p.PagePath == pagePath)
                    return p;
            return null;
        }
        /// <summary>
        /// 销毁已注册页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <returns></returns>
        public bool DestroyUIPage(string pagePath)
        {
            UIPage p = FindUIPage(pagePath);
            if(p != null)
            {
                p.gameObject.SetActive(false);
                managedPages.Remove(p);
                return true;
            }
            else {
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }
        }
        /// <summary>
        /// 跳转到某个页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <returns></returns>
        public bool GotoUIPage(string pagePath)
        {
            UIPage p = FindUIPage(pagePath);
            if (p != null)
            {
                if(p != currentShowPage)
                {
                    if (currentShowPage != null)
                    {
                        currentShowPage.gameObject.SetActive(false);
                        if (currentShowPage.PageBackground != null)
                            currentShowPage.PageBackground.SetActive(false);
                    }
                    p.gameObject.SetActive(true);
                    if (p.PageBackground != null)
                        p.PageBackground.SetActive(true);
                    currentShowPage = p;

                    GameLogger.Log(TAG, "GotoUIPage {0}", pagePath);
                }
                return true;
            }
            else
            {
                GameLogger.Warning(TAG, "GotoUIPage 未找到指定页 {0}", pagePath);
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }
        }
        /// <summary>
        /// 隐藏页
        /// </summary>
        /// <param name="pagePath">页路径，如果为空则隐藏当前显示的页</param>
        /// <returns></returns>
        public bool CloseUIPage(string pagePath)
        {
            UIPage p = string.IsNullOrEmpty(pagePath) ? currentShowPage : FindUIPage(pagePath);
            if (p != null)
            {
                p.gameObject.SetActive(false);
                if (p.PageBackground != null)
                    p.PageBackground.SetActive(false);
                return true;
            }
            else
            {
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }
        }
        /// <summary>
        /// 从当前页退回上级页，如果当前没有显示的页或当前页已经是顶级，返回 false
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <returns>如果当前没有显示的页，返回 false</returns>
        public bool BackUIPage()
        {
            if (currentShowPage != null)
            {
                string pagePath = currentShowPage.PagePath;
                if (pagePath.Contains("."))
                {
                    pagePath = pagePath.Substring(0, pagePath.LastIndexOf('.'));

                    UIPage p = FindUIPage(pagePath);
                    if (p != null)
                    {
                        if (p != currentShowPage)
                        {
                            currentShowPage.gameObject.SetActive(false);
                            if (currentShowPage.PageBackground != null)
                                currentShowPage.PageBackground.SetActive(false);
                            p.gameObject.SetActive(true);
                            if (p.PageBackground != null)
                                p.PageBackground.SetActive(true);
                            currentShowPage = p;

                            GameLogger.Log(TAG, "BackUIPage {0}", currentShowPage.PagePath);
                        }
                        return true;
                    }
                    else
                    {
                        GameLogger.Log(TAG, "BackUIPage 未找到上级页 {0}", pagePath);
                        GameErrorManager.LastError = GameError.Unregistered;
                    }
                }
            }
            return false;
        }

        #region 自动布局生成器

        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlerNames">UI事件接收器目标列表</param>
        /// <param name="handlers">UI事件接收器函数</param>
        /// <param name="self">UI事件接收器接收类</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, SLua.LuaFunction[] handlers, SLua.LuaTable self)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            for (int i = 0; i < handlerNames.Length; i++)
                handlerList.Add(handlerNames[i], new GameHandler(name, handlers[i], self));
            return BuildLayoutByTemplate(name, templateXml, handlerList);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">LUA接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, string[] handlers)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            string h = "";
            for (int i = 0; i < handlerNames.Length; i++)
            {
                h = handlers[i];
                handlerList.Add(handlerNames[i], new GameHandler(name + ":" + h, h));
            }
            
            return BuildLayoutByTemplate(name, templateXml, handlerList);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">LUA接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, string[] handlerNames, GameHandlerDelegate[] handlers)
        {
            Dictionary<string, GameHandler> handlerList = new Dictionary<string, GameHandler>();
            GameHandlerDelegate h = null;
            for (int i = 0; i < handlerNames.Length; i++)
            {
                h = handlers[i];
                handlerList.Add(handlerNames[i], new GameHandler(name + ":" + h.Method.Name, h));
            }
            return BuildLayoutByTemplate(name, templateXml, handlerList);
        }
        /// <summary>
        /// 生成 垂直自动布局
        /// </summary>
        /// <param name="name">布局名称</param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">接收器模板</param>
        /// <returns></returns>
        public UILayout BuildLayoutByTemplate(string name, string templateXml, Dictionary<string, GameHandler> handlers)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(templateXml);
            return BuildLayoutByTemplateInternal(name, xmlDocument.DocumentElement, handlers, null);
        }

        private UILayout BuildLayoutByTemplateInternal(string name, XmlNode templateXml, Dictionary<string, GameHandler> handlers, UILayout parent)
        {
            //实例化UI预制体
            string prefabName = templateXml.Name;
            GameObject prefab = FindRegisterElementPrefab(prefabName);
            if(prefab == null)
            {
                GameLogger.Log(TAG, "BuildLayoutByTemplate failed, not found prefab {0}", prefabName);
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }

            //获取预制体上的脚本
            UILayout ilayout = prefab.GetComponent<UILayout>();            
            if(ilayout == null) //该方法必须实例化UI容器
            {
                GameLogger.Log(TAG, "BuildLayoutByTemplate with prefab {0} failed, root must be a container", prefabName);
                GameErrorManager.LastError = GameError.MustBeContainer;
                return null;
            }

            GameObject newCon = GameCloneUtils.CloneNewObjectWithParent(prefab,
                parent == null ? UIRoot.transform : parent.RectTransform);
            ilayout = newCon.GetComponent<UILayout>();
            if (ilayout == null)
            {
                GameLogger.Error(TAG, "BuildLayoutByTemplate failed, root prefab {0} does not contain UIElement", prefabName);
                return null;
            }
            ilayout.RectTransform = newCon.GetComponent<RectTransform>();
            ilayout.LateInit(name, templateXml); //容器的XML读取

            GameObject newEle = null;
            UIElement uIElement = null;

            //子元素
            for (int i = 0, c = templateXml.ChildNodes.Count; i < c; i++)
            {
                //xml 属性读取
                XmlNode eleNode = templateXml.ChildNodes[i];
                string eleName = "";
                foreach (XmlAttribute a in eleNode.Attributes) {
                    if (a.Name == "name") eleName = a.Value;
                }

                //预制体
                prefab = FindRegisterElementPrefab(eleNode.Name);
                if (prefab == null)
                {
                    GameLogger.Error(TAG, "BuildLayoutByTemplate failed, not found prefab {0}", prefabName);
                    continue;
                }
                if (prefab.GetComponent<UILayout>() != null) //这是UI容器
                    return BuildLayoutByTemplateInternal(eleName, eleNode, handlers, ilayout);//递归构建

                //构建子元素
                newEle = GameCloneUtils.CloneNewObjectWithParent(prefab, ilayout.RectTransform, eleName);

                uIElement = newEle.GetComponent<UIElement>();
                uIElement.RectTransform = newEle.GetComponent<RectTransform>();
                uIElement.LateInit(eleName, eleNode);

                //初始化子元素的事件接收器
                Dictionary<string, GameHandler> lateInitHandlers = new Dictionary<string, GameHandler>();
                foreach (string key in handlers.Keys)
                {
                    string[] sp = key.Split(':');
                    if (sp.Length >= 2 && sp[0] == uIElement.Name)
                    {
                        try {  lateInitHandlers.Add(sp[1], handlers[key]); }
                        catch(System.ArgumentException e)
                        {
                            GameLogger.Warning(TAG, "Add event for {0} -> {1} failed {2}", key, handlers[key].Name, e.Message);
                            continue;
                        }
                    }
                }
                if (lateInitHandlers.Count > 0) uIElement.LateInitHandlers(lateInitHandlers);

                ilayout.AddElement(uIElement, false);
            }

            ilayout.PostDoLayout();
            return ilayout;
        }

        #endregion

        #region 外壳模板

        private void InitUIPrefabs()
        {
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISpace"), "UISpace");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISmallButton"), "UISmallButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIMainButton"), "UIMainButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UILevButton"), "UILevButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIButtonBack"), "UIButtonBack");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIHorizontalLinearLayout"), "UIHorizontalLinearLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIVertcialLinearLayout"), "UIVertcialLinearLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISpace"), "UISpace");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIText"), "UIText");
        }

        /// <summary>
        /// 注册UI元素的外壳Prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="name"></param>
        public bool RegisterElementPrefab(GameObject prefab, string name)
        {
            if(FindRegisterElementPrefab(name) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return false;
            }
            elementPrefabs.Add(new UIPrefab(prefab, name));
            return true;
        }
        /// <summary>
        /// 取消注册UI元素的外壳Prefab
        /// </summary>
        /// <param name="name"></param>
        public bool UnRegisterElementPrefab(string name)
        {
            for (int i = elementPrefabs.Count - 1; i >= 0; i--)
            {
                if (elementPrefabs[i].Name == name)
                {
                    elementPrefabs.RemoveAt(i);
                    return true;
                }
            }
            GameErrorManager.LastError = GameError.Unregistered;
            return false;
        }
        /// <summary>
        /// 获取注册的UI元素的外壳Prefab
        /// </summary>
        /// <param name="name"></param>
        public GameObject FindRegisterElementPrefab(string name)
        {
            for (int i = elementPrefabs.Count - 1; i >= 0; i--)
            {
                if (elementPrefabs[i].Name == name)
                    return elementPrefabs[i].Prefab;
            }
            return null;
        }
        /// <summary>
        /// 实例化已经注册的UI预制体
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="parent">父元素</param>
        /// <param name="name">新元素名称</param>
        public GameObject InstanceElement(string prefabName, RectTransform parent, string name)
        {
            GameObject prefab = FindRegisterElementPrefab(name);
            if(prefab == null)
            {
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }
            
            return GameCloneUtils.CloneNewObjectWithParent(prefab, parent, prefabName + ":" +name);
        }

        /// <summary>
        /// 注册UI页的外壳Prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="name"></param>
        public bool RegisterPagePrefab(GameObject prefab, string name)
        {
            if (FindRegisterPagePrefab(name) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return false;
            }
            pagePrefabs.Add(new UIPrefab(prefab, name));
            return true;
        }
        /// <summary>
        /// 取消注册UI页的外壳Prefab
        /// </summary>
        /// <param name="name"></param>
        public bool UnRegisterPagePrefab(string name)
        {
            for(int i = pagePrefabs.Count - 1; i>=0;i--)
            {
                if (pagePrefabs[i].Name == name)
                {
                    pagePrefabs.RemoveAt(i);
                }
            }
            GameErrorManager.LastError = GameError.Unregistered;
            return true;
        }
        /// <summary>
        /// 获取注册的UI页的外壳Prefab
        /// </summary>
        /// <param name="name"></param>
        public GameObject FindRegisterPagePrefab(string name)
        {
            for (int i = pagePrefabs.Count - 1; i >= 0; i--)
            {
                if (pagePrefabs[i].Name == name)
                    return pagePrefabs[i].Prefab;
            }
            return null;
        }
        /// <summary>
        /// 注册UI页的背景Prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="name"></param>
        public bool RegisterPageBackgroundPrefab(GameObject prefab, string name)
        {
            if (FindPageBackgroundPrefab(name) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return false;
            }
            GameCloneUtils.CloneNewObjectWithParent(prefab, PageBackgroundRectTransform, name, false);
            return true;
        }
        /// <summary>
        /// 取消注册UI页的背景Prefab
        /// </summary>
        /// <param name="name"></param>
        public bool UnRegisterPageBackgroundPrefab(string name)
        {
            GameObject go = null;
            for (int i = PageBackgroundRectTransform.childCount - 1; i >= 0; i--)
            {
                go = PageBackgroundRectTransform.GetChild(i).gameObject;
                if (go.name == name)
                {
                    UnityEngine.Object.Destroy(go);
                    return true;
                }
            }
            GameErrorManager.LastError = GameError.Unregistered;
            return false;
        }
        /// <summary>
        /// 获取注册的UI页的背景Prefab
        /// </summary>
        /// <param name="name"></param>
        public GameObject FindPageBackgroundPrefab(string name)
        {
            GameObject go = null;
            for (int i = PageBackgroundRectTransform.childCount - 1; i >= 0; i--)
            {
                go = PageBackgroundRectTransform.GetChild(i).gameObject;
                if (go.name == name)
                    break;
            }
            return go;
        }

        #endregion

        #endregion

        #region 调试

        private DebugManager DebugManager;

        private void InitUIDebug()
        {
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED, TAG, (evtName, param) =>
            {
                if (param[0].ToString() == DebugManager.TAG)
                {
                    DebugManager = (DebugManager)GameManager.GetManager(DebugManager.TAG);
                    DebugManager.RegisterCommand("pages", OnCommandShowPages, 0, "显示页");
                    DebugManager.RegisterCommand("windows", OnCommandShowWindows, 0, "显示页窗口");
                    DebugManager.RegisterCommand("gouipage", OnCommandGoUIPage, 1, "跳转到页");
                    DebugManager.RegisterCommand("closeuipage", OnCommandCloseUIPage, 1, "关闭已经显示的页");
                    DebugManager.RegisterCommand("backuipage", OnCommandBackUIPage, 0, "返回上级页");

                    DebugManager.RegisterCommand("showwindow", OnCommandShowWindow, 1, "显示窗口");
                    DebugManager.RegisterCommand("closewindow", OnCommandCloseWindow, 1, "关闭窗口");
                    DebugManager.RegisterCommand("hidewindow", OnCommandHideWindow, 1, "隐藏窗口");
                }
                return false;
            });
        }

        private bool OnCommandCloseWindow(string keyword, string fullCmd, string[] args)
        {
            return OnCommandShowHideCloseWindow(args[0], "close");
        }
        private bool OnCommandHideWindow(string keyword, string fullCmd, string[] args)
        {
            return OnCommandShowHideCloseWindow(args[0], "hise");
        }
        private bool OnCommandShowWindow(string keyword, string fullCmd, string[] args)
        {
            return OnCommandShowHideCloseWindow(args[0], "show");
        }
        private bool OnCommandShowHideCloseWindow(string idstr, string act)
        {
            int id = 0;
            if(!int.TryParse(idstr, out id))
            {
                GameLogger.Error(TAG, "参数 1 必须是 int 类型 {0} : ", idstr);
                return false;
            }

            IWindow w = FindWindowById(id);
            if(w == null)
            {
                GameLogger.Error(TAG, "未找到 ID 为 {0} 的窗口", id);
                return false;
            }

            switch(act)
            {
                case "show": ShowWindow(w);  break;
                case "hide": HideWindow(w); break;
                case "close": CloseWindow(w); break;
            }

            return true;
        }
        private bool OnCommandCloseUIPage(string keyword, string fullCmd, string[] args)
        {
            return CloseUIPage(args[0]);
        }
        private bool OnCommandBackUIPage(string keyword, string fullCmd, string[] args)
        {
            return BackUIPage();
        }
        private bool OnCommandGoUIPage(string keyword, string fullCmd, string[] args)
        {
            return GotoUIPage(args[0]);
        }
        private bool OnCommandShowWindows(string keyword, string fullCmd, string[] args)
        {
            StringBuilder s = new StringBuilder();
            foreach (UIWindow w in managedWindows)
            {
                s.Append("\n(");
                s.Append(w.GetWindowId());
                s.Append(')');
                s.Append(w.Title);
            }
            GameLogger.Log(TAG, "All windows count {0} : \n{1}", managedWindows.Count, s.ToString());
            return true;
        }
        private bool OnCommandShowPages(string keyword, string fullCmd, string[] args)
        {
            StringBuilder s = new StringBuilder();
            foreach (UIPage p in managedPages)
            {
                s.Append('\n');
                s.Append(p.PagePath);
                s.Append(" /");
                s.Append(p.PageBackground);
            }
            GameLogger.Log(TAG, "All Pages count {0} : \n{1}", managedPages.Count, s.ToString());
            return true;
        }

        #endregion

        #region 通用管理

        public void SetViewToTemporarily(RectTransform view)
        {
            view.SetParent(TemporarilyRectTransform.gameObject.transform);
        }
        public void AttatchViewToCanvas(RectTransform view)
        {
            view.SetParent(UIRoot.gameObject.transform);
        }
        public RectTransform InitViewToCanvas(GameObject prefab, string name)
        {
            GameObject go = GameCloneUtils.CloneNewObjectWithParent(prefab,
                ViewsRectTransform.transform, name);
            RectTransform view = go.GetComponent<RectTransform>();
            view.SetParent(ViewsRectTransform.gameObject.transform);
            return view;
        }

        #endregion
    }
}
