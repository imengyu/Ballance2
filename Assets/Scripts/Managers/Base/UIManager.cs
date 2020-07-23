using Ballance2.CoreBridge;
using Ballance2.Interfaces;
using Ballance2.UI.BallanceUI;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System;
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
    public class UIManager : BaseManager
    {
        public const string TAG = "UIManager";

        public UIManager() : base(GamePartName.Core, TAG)
        {
            replaceable = false;
        }

        public override bool InitManager()
        {
            GameCanvas = GameManager.GameCanvas;
            UIRoot = GameManager.UIRoot;
            UIFadeManager = UIRoot.AddComponent<UIFadeManager>();

            GlobalFadeMaskWhite = GameCanvas.transform.Find("GlobalFadeMaskWhite").gameObject.GetComponent<Image>();
            GlobalFadeMaskBlack = GameCanvas.transform.Find("GlobalFadeMaskBlack").gameObject.GetComponent<Image>();
            GlobalFadeMaskBlack.gameObject.SetActive(true);

            //退出时的黑
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_BEFORE_GAME_QUIT, TAG, (evtName, param) =>
            {
                MaskBlackFadeIn(0.25f);
                return false;
            });

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
                Destroy(UIRoot.transform.GetChild(i).gameObject);
            return true;
        }

        public GameObject GameCanvas;
        /// <summary>
        /// UI 根
        /// </summary>
        public GameObject UIRoot;
        /// <summary>
        /// 渐变管理器
        /// </summary>
        public UIFadeManager UIFadeManager;

        private void Update()
        {
            if(toastTimeTick > 0 ) {
                toastTimeTick--;
                if (toastTimeTick == 0)
                {
                    UIFadeManager.AddFadeOut(UIToastImage, 0.4f, true);
                    UIFadeManager.AddFadeOut(UIToastText, 0.4f, false);
                    toastNextDelayTimeTick = 60;
                }
            }

            if(toastNextDelayTimeTick > 0)
            {
                toastNextDelayTimeTick--;
                if (toastNextDelayTimeTick == 0) ShowPendingToast();
            }
        }

        //根管理
        private RectTransform TemporarilyRectTransform;
        private RectTransform GlobalWindowRectTransform;
        private RectTransform PagesRectTransform;
        private RectTransform WindowsRectTransform;
        private RectTransform ViewsRectTransform;
        private RectTransform PageContainerRectTransform;
        private RectTransform PageBackgroundRectTransform;

        /// <summary>
        /// UI 根 RectTransform
        /// </summary>
        public RectTransform UIRootRectTransform { get; private set; }

        private void InitAllObects()
        {
            UIRootRectTransform = UIRoot.GetComponent<RectTransform>();
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

            UIToast = GameCloneUtils.CloneNewObjectWithParent(GameManager.FindStaticPrefabs("UIToast"), UIRoot.transform, "UIToast").GetComponent<RectTransform>();
            UIToastImage = UIToast.GetComponent<Image>();
            UIToastText = UIToast.Find("Text").GetComponent<Text>();
            UIToast.gameObject.SetActive(false);
            UIToast.SetAsLastSibling();
            EventTriggerListener.Get(UIToast.gameObject).onClick = (g) => { toastTimeTick = 1;  };
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
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局黑色遮罩隐藏
        /// </summary>
        public void MaskWhiteSet(bool show)
        {
            GlobalFadeMaskWhite.color = new Color(GlobalFadeMaskWhite.color.r,
                GlobalFadeMaskWhite.color.g, GlobalFadeMaskWhite.color.b, show ? 1.0f : 0f);
            GlobalFadeMaskWhite.gameObject.SetActive(show);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局白色遮罩渐变淡入
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeIn(float second)
        {
            UIFadeManager.AddFadeIn(GlobalFadeMaskWhite, second);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局黑色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskBlackFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
            GlobalFadeMaskBlack.transform.SetAsLastSibling();
        }
        /// <summary>
        /// 全局白色遮罩渐变淡出
        /// </summary>
        /// <param name="second">耗时（秒）</param>
        public void MaskWhiteFadeOut(float second)
        {
            UIFadeManager.AddFadeOut(GlobalFadeMaskWhite, second, true);
            GlobalFadeMaskWhite.transform.SetAsLastSibling();
        }

        #endregion

        #region 全局对话框

        private RectTransform UIToast;
        private Image UIToastImage;
        private Text UIToastText;

        private struct ToastData
        {
            public string text;
            public int showTime;

            public ToastData(string t, int i)
            {
                text = t;
                showTime = i;
            }
        }

        private List<ToastData> toastDatas = new List<ToastData>();

        private int toastTimeTick = 0;
        private int toastNextDelayTimeTick = 0;

        /// <summary>
        /// 显示全局 Alert 对话框
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <returns></returns>
        public int GlobalAlert(string text, string title, string okText = "确定")
        {
            string oldPagePath = currentShowPage.PagePath;
            int windowId = GenWindowId();
            UIPage page = RegisterBallanceUIPage("global.alert." + windowId,
                PageGlobalConfirm.text,
                new string[] { "btn.ok:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        OnGlobalDialogClicked("alert", true, windowId, oldPagePath);
                        DestroyUIPage("global.alert." + windowId);
                        return true;
                    },
                }, "Default");

            GotoUIPage("global.alert." + windowId);

            UIElement textEle = page.ContentContainer.FindElementByName("text.main");
            UIElement btnOkEle = page.ContentContainer.FindElementByName("btn.ok");
            if (textEle != null) textEle.SetProperty("text", text);
            if (btnOkEle != null)  btnOkEle.SetProperty("text", okText);
    
            return windowId;
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
            string oldPagePath = currentShowPage.PagePath;
            int windowId = GenWindowId();
            UIPage page = RegisterBallanceUIPage("global.confirm." + windowId,
                PageGlobalConfirm.text,
                new string[] { "btn.ok:click", "btn.cancel:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        OnGlobalDialogClicked("confirm", true, windowId, oldPagePath);
                        DestroyUIPage("global.confirm." + windowId);
                        return true;
                    },
                    (evtName, param) => {
                        OnGlobalDialogClicked("confirm", false, windowId, oldPagePath);
                        DestroyUIPage("global.confirm." + windowId);
                        return true;
                    }
                }, "Default");

            GotoUIPage("global.confirm." + windowId);

            UIElement textEle = page.ContentContainer.FindElementByName("text.main");
            UIElement btnOkEle = page.ContentContainer.FindElementByName("btn.ok");
            UIElement btnCancelEle = page.ContentContainer.FindElementByName("btn.cancel");
            if (textEle != null) textEle.SetProperty("text", text);
            if (btnOkEle != null) btnOkEle.SetProperty("text", okText);
            if (btnCancelEle != null) btnCancelEle.SetProperty("text", cancelText);

            return windowId;
        }
        /// <summary>
        /// 显示全局土司提示
        /// </summary>
        /// <param name="text"></param>
        public void GlobalToast(string text)
        {
            if (toastTimeTick == 0) ShowToast(text, (int)((text.Length / 50.0f) * 1000));
            else toastDatas.Add(new ToastData(text, (int)((text.Length / 50.0f) * 1000)));
        }

        private void ShowPendingToast()
        {
            if(toastDatas.Count > 0)
            {
                ShowToast(toastDatas[0].text, toastDatas[0].showTime);
                toastDatas.RemoveAt(0);
            }
        }
        private void ShowToast(string text, int time)
        {
            UIToastText.text = text;
            float h = UIToastText.preferredHeight;
            UIToast.sizeDelta = new Vector2(UIToast.sizeDelta.x, h > 50 ? h : 50);
            UIToast.gameObject.SetActive(true);
            UIToast.SetAsLastSibling();

            UIFadeManager.AddFadeIn(UIToastImage, 0.8f);
            UIFadeManager.AddFadeIn(UIToastText, 0.9f);
            toastTimeTick = time;
        }

        private void OnGlobalDialogClicked(string type, bool isOk, int dialogId, string oldPagePath)
        {
            CloseUIPage("global." + type + "." + dialogId);

            if (oldPagePath != null) GotoUIPage(oldPagePath as string);

            GameManager.GameMediator.DispatchGlobalEvent(
                GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                dialogId, isOk);
        }

        /// <summary>
        /// 显示全局 Alert 对话框（窗口模式）
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <returns></returns>
        public int GlobalAlertWindow(string text, string title, string okText = "确定")
        {
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIAlertWindow, WindowsRectTransform.transform, "");
            RectTransform rectTransform = windowGo.GetComponent<RectTransform>();
            Button btnOk = rectTransform.Find("UIButton").GetComponent<Button>();
            rectTransform.Find("UIDialogText").GetComponent<Text>().text = text;
            rectTransform.Find("UIButton/Text").GetComponent<Text>().text = okText;
            UIWindow window = CreateWindow(title, rectTransform);
            RegisterWindow(window);
            window.CanClose = true;
            window.CanDrag = true;
            window.CanResize = true;
            window.SetMinSize(300, 250);
            window.Show();
            btnOk.onClick.AddListener(() => {
                window.Close();
            });
            window.onClose = (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    id, false);
            };
            return windowId;
        }
        /// <summary>
        /// 显示全局 Confirm 对话框（窗口模式）
        /// </summary>
        /// <param name="text">内容</param>
        /// <param name="title">标题</param>
        /// <param name="okText">OK 按钮文字</param>
        /// <param name="cancelText">Cancel 按钮文字</param>
        /// <returns></returns>
        public int GlobalConfirmWindow(string text, string title, string okText = "确定", string cancelText = "取消")
        {
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIConfirmWindow, WindowsRectTransform.transform, "");
            RectTransform rectTransform = windowGo.GetComponent<RectTransform>();
            Button btnYes = rectTransform.Find("UIButtonYes").GetComponent<Button>();
            Button btnNo = rectTransform.Find("UIButtonNo").GetComponent<Button>();
            rectTransform.Find("UIButtonYes/Text").GetComponent<Text>().text = okText;
            rectTransform.Find("UIButtonNo/Text").GetComponent<Text>().text = cancelText;
            rectTransform.Find("UIDialogText").GetComponent<Text>().text = text;
            UIWindow window = CreateWindow(title, rectTransform);
            window.CanClose = false;
            window.CanDrag = true;
            window.CanResize = false;
            RegisterWindow(window);
            window.Show();
            window.onClose = (id) =>
            {
                PagesRectTransform.gameObject.SetActive(true);
                WindowsRectTransform.gameObject.SetActive(true);
            };
            btnYes.onClick.AddListener(() =>
            {
                window.Close();
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    window.GetWindowId(), true);
            });
            btnNo.onClick.AddListener(() => {
                window.Close();
                GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, "*",
                    window.GetWindowId(), false);
            });
            return window.GetWindowId();
        }

        #endregion

        #region 窗口管理

        private GameObject PrefabUIAlertWindow;
        private GameObject PrefabUIConfirmWindow;
        private GameObject PrefabUIWindow;

        private void InitWindowManagement()
        {
            managedWindows = new List<UIWindow>();

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
                foreach (UIWindow w in managedWindows)
                    w.Destroy();
                managedWindows.Clear();
                managedWindows = null;
            }
        }

        //窗口

        private List<UIWindow> managedWindows = null;

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
            window.MoveToCenter();
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
        public UIWindow FindWindowById(int windowId)
        {
            foreach (UIWindow w in managedWindows)
                if (w.GetWindowId() == windowId)
                    return w;
            return null;
        }

        private UIWindow currentVisibleWindowAlert = null;

        public void ShowWindow(UIWindow window)
        {
            switch (window.GetWindowType())
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
        public void HideWindow(UIWindow window) { window.SetVisible(false); }
        public void CloseWindow(UIWindow window) { window.Destroy(); managedWindows.Remove(window); }

        private int windowId = 0;
        public int GenWindowId()
        {
            if (windowId < 2048) windowId++;
            else windowId = 0;
            return windowId;
        }

        #endregion

        #region 页管理

        //页
        public List<UIPage> managedPages = null;
        public List<UIPrefab> pagePrefabs = null;
        public List<UIPrefab> elementPrefabs = null;

        public List<UIPage> ManagedPages { get { return managedPages; } set { } }
        public List<UIPrefab> PagePrefabs { get { return pagePrefabs; } set { } }
        public List<UIPrefab> ElementPrefabs { get { return elementPrefabs; } set { } }

        [Serializable]
        public class UIPrefab
        {
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
        [SerializeField, SetProperty("CurrentShowPage")]
        private UIPage currentShowPage = null;

        public UIPage CurrentShowPage { get { return currentShowPage; } }

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
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, SLua.LuaFunction[] handlers, SLua.LuaTable self, string backgroundPrefabName = "Default", string[] initialProps = null)
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            UILayout layoutContainer = UILayoutBuilder.BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers, self, initialProps);
            if (layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.AnchorX = UIAnchor.Stretch;
            layoutContainer.MinSize = new Vector2(0, UIRootRectTransform.rect.height);
            UIPage page = RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer);
            if (page.ContentContainer != null)
                page.ContentContainer.MinSize = layoutContainer.MinSize;
            return page;
        }
        /// <summary>
        /// 注册 Ballance 样式的 UI 页
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">UI事件接收器模板</param>
        /// <returns></returns>
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, GameEventHandlerDelegate[] handlers, string backgroundPrefabName = "Default", string[] initialProps = null)
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }
            UILayout layoutContainer = UILayoutBuilder.BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers, initialProps);
            if (layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.AnchorX = UIAnchor.Stretch;
            layoutContainer.MinSize = new Vector2(0, UIRootRectTransform.rect.height);
            UIPage page = RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer);
            if (page.ContentContainer != null)
                page.ContentContainer.MinSize = layoutContainer.MinSize;
            return page;
        }
        /// <summary>
        /// 注册 Ballance 样式的 UI 页
        /// </summary>
        /// <param name="pagePath"></param>
        /// <param name="template">UI模板</param>
        /// <param name="handlers">UI事件接收器模板</param>
        /// <returns></returns>
        public UIPage RegisterBallanceUIPage(string pagePath, string templateXml, string[] handlerNames, string[] handlers, string backgroundPrefabName = "Default", string[] initialProps = null)
        {
            if (FindUIPage(pagePath) != null)
            {
                GameErrorManager.LastError = GameError.AlredayRegistered;
                return null;
            }

            UILayout layoutContainer = UILayoutBuilder.BuildLayoutByTemplate(pagePath, templateXml, handlerNames, handlers, initialProps);
            if (layoutContainer == null)
            {
                GameErrorManager.LastError = GameError.LayoutBuildFailed;
                return null;
            }

            layoutContainer.MinSize = new Vector2(0, UIRootRectTransform.rect.height);
            layoutContainer.AnchorX = UIAnchor.Stretch;
            UIPage page = RegisterUIPage(pagePath, "Default", backgroundPrefabName, layoutContainer);
            if (page.ContentContainer != null)
                page.ContentContainer.MinSize = layoutContainer.MinSize;
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
            page.PagePath = pagePath;
            page.PagePrefabName = prefabName;
            //添加内容
            if (page.ContentRectTransform == null)
                page.ContentRectTransform = page.RectTransform;
            if (page.ContentContainer.RectTransform == null)
                page.ContentContainer.RectTransform = page.ContentContainer.gameObject.GetComponent<RectTransform>();
            page.ContentContainer.AddElement(content, false);

            content.RectTransform.anchoredPosition = Vector2.zero;

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
            page.PagePrefabName = prefabName;

            if (content.parent != page.ContentRectTransform)
            {
                content.SetParent(page.ContentRectTransform);
                UIAnchorPosUtils.SetUIAnchor(content, UIAnchor.Stretch, UIAnchor.Stretch);
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
                page.PageBackgroundPrefabName = backgroundPrefabName;
                page.PageBackground = FindPageBackgroundPrefab(backgroundPrefabName);
                if (page.PageBackground == null)
                    GameLogger.Error(TAG, "Not found PageBackground prefab {0}", backgroundPrefabName);
            }
        }

        internal void RequestRelayoutForScreenSizeChaged()
        {
            foreach(UIPage p in managedPages)
            {
                if(p.PagePrefabName == "Default" && p.ContentContainer != null)
                {
                    p.ContentContainer.MinSize = new Vector2(0, UIRootRectTransform.rect.height);
                    if (p.ContentContainer.Elements.Count > 0 && p.ContentContainer.Elements[0].IsRootLayout)
                        p.ContentContainer.Elements[0].MinSize = p.ContentContainer.MinSize;
                    p.ContentContainer.PostDoLayout();
                }
            }
        }

        /// <summary>
        /// 查找已注册页
        /// </summary>
        /// <param name="pagePath">页路径</param>
        /// <returns></returns>
        public UIPage FindUIPage(string pagePath)
        {
            foreach (UIPage p in managedPages)
                if (p.PagePath == pagePath)
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
            if (p != null)
            {
                p.gameObject.SetActive(false);
                p.OnHide();
                managedPages.Remove(p);
                return true;
            }
            else
            {
                GameErrorManager.LastError = GameError.NotRegister;
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
                if (p != currentShowPage)
                {
                    if (currentShowPage != null)
                    {
                        currentShowPage.gameObject.SetActive(false);
                        if (currentShowPage.PageBackground != null)
                            currentShowPage.PageBackground.SetActive(false);
                        currentShowPage.OnHide();
                    }
                    p.gameObject.SetActive(true);
                    if (p.PageBackground != null)
                        p.PageBackground.SetActive(true);
                    currentShowPage = p;
                    currentShowPage.OnShow();

                    GameLogger.Log(TAG, "GotoUIPage {0}", pagePath);
                }
                return true;
            }
            else
            {
                GameLogger.Warning(TAG, "GotoUIPage 未找到指定页 {0}", pagePath);
                GameErrorManager.LastError = GameError.NotRegister;
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
                p.OnHide();
                if (p.PageBackground != null)
                    p.PageBackground.SetActive(false);
                return true;
            }
            else
            {
                GameErrorManager.LastError = GameError.NotRegister;
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
                        GameErrorManager.LastError = GameError.NotRegister;
                    }
                }
            }
            return false;
        }

        #endregion

        #region 外壳模板

        private TextAsset PageGlobalConfirm;
        private TextAsset PageGlobalAlert;

        private UILayoutBuilder uILayoutBuilder = null;

        private void InitUIPrefabs()
        {
            uILayoutBuilder = new UILayoutBuilder(this);

            PageGlobalConfirm = GameManager.FindStaticAssets<TextAsset>("PageGlobalConfirm");
            PageGlobalAlert = GameManager.FindStaticAssets<TextAsset>("PageGlobalAlert");

            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISpace"), "UISpace");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISmallButton"), "UISmallButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIMainButton"), "UIMainButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIMenuButton"), "UIMenuButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UILevButton"), "UILevButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIButtonBack"), "UIButtonBack");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UILinearLayout"), "UILinearLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIRelativeLayout"), "UIRelativeLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIHorizontalLinearLayout"), "UIHorizontalLinearLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIVertcialLinearLayout"), "UIVertcialLinearLayout");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISpace"), "UISpace");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIText"), "UIText");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIDropdown"), "UIDropdown");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIKeyChooseButton"), "UIKeyChooseButton");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UISlider"), "UISlider");
            RegisterElementPrefab(GameManager.FindStaticPrefabs("UIToggleButton"), "UIToggleButton");
            
        }

        /// <summary>
        /// 注册UI元素的外壳Prefab
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="name"></param>
        public bool RegisterElementPrefab(GameObject prefab, string name)
        {
            if (FindRegisterElementPrefab(name) != null)
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
            GameErrorManager.LastError = GameError.NotRegister;
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
            if (prefab == null)
            {
                GameErrorManager.LastError = GameError.PrefabNotFound;
                return null;
            }

            return GameCloneUtils.CloneNewObjectWithParent(prefab, parent, prefabName + ":" + name);
        }

        /// <summary>
        /// 获取 UI 布局构建器实例
        /// </summary>
        public UILayoutBuilder UILayoutBuilder { get { return uILayoutBuilder; } }

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
            for (int i = pagePrefabs.Count - 1; i >= 0; i--)
            {
                if (pagePrefabs[i].Name == name)
                {
                    pagePrefabs.RemoveAt(i);
                }
            }
            GameErrorManager.LastError = GameError.NotRegister;
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
            GameErrorManager.LastError = GameError.NotRegister;
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

        #region 调试

        private IDebugManager DebugManager;

        private void InitUIDebug()
        {
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED, TAG, (evtName, param) =>
            {
                if (param[0].ToString() == "DebugManager")
                {
                    DebugManager = (IDebugManager)GameManager.GetManager("DebugManager");
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
            if (!int.TryParse(idstr, out id))
            {
                GameLogger.Error(TAG, "参数 1 必须是 int 类型 {0} : ", idstr);
                return false;
            }

            UIWindow w = FindWindowById(id);
            if (w == null)
            {
                GameLogger.Error(TAG, "未找到 ID 为 {0} 的窗口", id);
                return false;
            }

            switch (act)
            {
                case "show": ShowWindow(w); break;
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
