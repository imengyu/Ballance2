using Ballance2.Managers.CoreBridge;
using Ballance2.UI.BallanceUI;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            InitAllObects();
            InitWindowManagement();

            GameLogger.Log(TAG, "InitManager");
            return true;
        }
        public override bool ReleaseManager()
        {
            DestroyWindowManagement();

            GameLogger.Log(TAG, "Destroy {0} ui objects", UIRoot.transform.childCount);
            for (int i = 0, c = UIRoot.transform.childCount; i < c; i++)
                UnityEngine.Object.Destroy(UIRoot.transform.GetChild(i).gameObject);
            return true;
        }
        public override void ReloadData()
        {
            base.ReloadData();
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

        private void InitAllObects()
        {
            TemporarilyRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUITemporarily").GetComponent<RectTransform>();
            TemporarilyRectTransform.gameObject.SetActive(false);
            GlobalWindowRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIGlobalWindow").GetComponent<RectTransform>();
            PagesRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIPages").GetComponent<RectTransform>();
            WindowsRectTransform = GameCloneUtils.CreateEmptyUIObjectWithParent(UIRoot.transform, "GameUIWindow").GetComponent<RectTransform>();
        }

        #region 全局渐变遮罩

        private Image GlobalFadeMaskWhite;
        private Image GlobalFadeMaskBlack;

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
            UIFadeManager.AddFadeIn(GlobalFadeMaskBlack, second);
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
            UIFadeManager.AddFadeOut(GlobalFadeMaskBlack, second, true);
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

        #region 通用管理

        public void SetViewToTemporarily(RectTransform view)
        {
            view.SetParent(TemporarilyRectTransform.gameObject.transform);
        }
        public void AttatchViewToCanvas(RectTransform view)
        {
            view.SetParent(UIRoot.gameObject.transform);
        }

        #endregion
    }
}
