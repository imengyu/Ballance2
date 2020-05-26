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

            GlobalFadeMaskWhite = GameObject.Find("GlobalFadeMaskWhite").GetComponent<Image>();
            GlobalFadeMaskBlack = GameObject.Find("GlobalFadeMaskBlack").GetComponent<Image>();

            InitAllObects();
            InitWindowManagement();

            GameLogger.Instance.Log(TAG, "InitManager");
            return true;
        }
        public override bool ReleaseManager()
        {
            DestroyWindowManagement();

            GameLogger.Instance.Log(TAG, "Destroy {0} ui objects", UIRoot.transform.childCount);
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
            TemporarilyRectTransform = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.PrefabUIEmpty, UIRoot.transform, "GameUITemporarily").GetComponent<RectTransform>();
            GlobalWindowRectTransform = GameCloneUtils.CloneNewObjectWithParent(
               GameManager.PrefabUIEmpty, UIRoot.transform, "GameUIGlobalWindow").GetComponent<RectTransform>();
            PagesRectTransform = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.PrefabUIEmpty, UIRoot.transform, "GameUIPages").GetComponent<RectTransform>();
            WindowsRectTransform = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.PrefabUIEmpty, UIRoot.transform, "GameUIWindow").GetComponent<RectTransform>();

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




        #endregion

        #region 窗口管理

        private GameObject PrefabUIAlertWindow;
        private GameObject PrefabUIConfirmWindow;
        private GameObject PrefabUIWindow;

        private void InitWindowManagement()
        {
            managedWindows = new List<IWindow>();

            PrefabUIAlertWindow = (GameObject)Resources.Load("Prefabs/UI/UIAlertWindow.prefab");
            PrefabUIConfirmWindow = (GameObject)Resources.Load("Prefabs/UI/UIConfirmWindow.prefab");
            PrefabUIWindow = (GameObject)Resources.Load("Prefabs/UI/UIWindow.prefab");

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
            GameObject windowGo = GameCloneUtils.CloneNewObjectWithParent(PrefabUIWindow, WindowsRectTransform.transform, "UIWindow:" + title);
            UIWindow window = windowGo.GetComponent<UIWindow>();
            window.windowId = GenWindowId();
            window.Title = title;
            window.SetPos(x, y);
            window.SetView(customView);
            if (w != 0 && h != 0) window.SetSize(w, h);
            if (show) window.Show();
            return window;
        }

        private IWindow currentVisibleWindowAlert = null;

        public void ShowWindow(IWindow window)
        {
            switch(window.GetWindowType())
            {
                case WindowType.GlobalAlert:
                    break;
                case WindowType.Normal:
                    break;
                case WindowType.Page:
                    break;
            }
            window.SetVisible(true);
        }
        public void HideWindow(IWindow window)
        {
            window.SetVisible(false);
        }
        public void CloseWindow(IWindow window)
        {

        }

        private int windowId = 0;
        private int GenWindowId()
        {
            if (windowId < 2048) windowId++;
            return windowId;
        }

        #endregion

        public void SetViewToTemporarily(RectTransform view)
        {
            view.SetParent(TemporarilyRectTransform.gameObject.transform);
        }
    }
}
