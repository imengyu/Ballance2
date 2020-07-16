using Ballance2.Config;
using Ballance2.Managers;
using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using System;
using System.Collections.Generic;
using UnityEngine;
using Ballance2.Managers.Base;
using System.Collections;
using Ballance2.Interfaces;

namespace Ballance2
{
    /// <summary>
    /// 游戏主管理器
    /// </summary>
    [CustomLuaClass]
    public static class GameManager
    {
        private const string TAG = "GameManager";

        #region 管理器控制

        /// <summary>
        /// 获取管理器实例
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="subName">二级名称</param>
        /// <returns>管理器</returns>
        public static BaseManager GetManager(string name, string subName)
        {
            foreach (BaseManager m in GameManagerWorker.managers)
                if (m.GetName() == name && m.GetSubName() == subName)
                    return m;
            return null;
        }
        /// <summary>
        /// 获取管理器实例（单例模式）
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static BaseManager GetManager(string name)
        {
            return GetManager(name, "Singleton");
        }
        /// <summary>
        /// 注册自定义管理器（内置类，创建新实例并注册）
        /// </summary>
        /// <param name="classInstance">（内置类）</param>
        /// <param name="initializeNow">是否立即初始化</param>
        /// <returns></returns>
        public static BaseManager RegisterManager(Type classType, bool initializeNow = true)
        {
            BaseManager classInstance = (BaseManager)GameCloneUtils.CreateEmptyObjectWithParent(
                GameRoot.transform, classType.Name).AddComponent(classType);
            classInstance.name = classInstance.GetName();
            return RegisterManager(classInstance, initializeNow);
        }
        /// <summary>
        /// 注册管理器（已创建实例注册）
        /// </summary>
        /// <param name="baseManager">管理器类</param>
        /// <param name="initializeNow">是否立即初始化</param>
        /// <returns></returns>
        public static BaseManager RegisterManager(BaseManager baseManager, bool initializeNow)
        {
            string name = baseManager.GetName();
            if (GetManager(name) != null)
            {
                if (baseManager.GetIsSingleton())
                {
                    GameLogger.Warning(TAG, "RegisterManager 失败，管理器 {0} 已注册", name);
                    GameErrorManager.LastError = GameError.AlredayRegistered;
                    return null;
                }
                else if (GetManager(name, baseManager.GetSubName()) != null)
                {
                    GameLogger.Warning(TAG, "RegisterManager 失败，管理器 {0}:{1} 已注册", name, baseManager.GetSubName());
                    GameErrorManager.LastError = GameError.AlredayRegistered;
                    return null;
                }
            }
            if (baseManager.gameObject.name != baseManager.GetName())
            {
                baseManager.gameObject.name = baseManager.GetName();
                baseManager.transform.SetParent(GameRoot.transform);
            }
            if (baseManager.isEmpty)
            {
                GameLogger.Warning(TAG, "ReplaceManager 失败，LUA 必须初始化");
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }

            GameManagerWorker.managers.Add(baseManager);
            GameLogger.Log(TAG, "Manager registered : {0}:{1}", baseManager.GetName(), baseManager.GetSubName());

            if (initializeNow)
            {
                GameManagerWorker.nextInitManages.Add(baseManager);
                GameManagerWorker.nextInitManagerTick = 30;
            }
            return baseManager;
        }

        /// <summary>
        /// 替换管理器
        /// </summary>
        /// <param name="newClass">新的管理器实例</param>
        /// <param name="oldName">要替换的管理器名称</param>
        /// <returns></returns>
        public static BaseManager ReplaceManager(BaseManager newClass, string oldName) { return ReplaceManager(newClass, oldName, "Singleton"); }
        /// <summary>
        /// 替换管理器
        /// </summary>
        /// <param name="newClass">新的管理器实例</param>
        /// <param name="oldName">要替换的管理器名称</param>
        /// <param name="subName">要替换的管理器子名称</param>
        /// <returns></returns>
        public static BaseManager ReplaceManager(BaseManager newClass, string oldName, string subName)
        {
            BaseManager old = GetManager(oldName, subName);
            if (old == null)
            {
                GameLogger.Warning(TAG, "ReplaceManager 失败，管理器 {0}:{1} 未注册", oldName, subName);
                GameErrorManager.LastError = GameError.NotRegister;
                return null;
            }
            if (old.Replaceable == false)
            {
                GameLogger.Warning(TAG, "ReplaceManager 失败，管理器 {0} 为基础模块，不能替换", old.GetFullName());
                GameErrorManager.LastError = GameError.BaseModuleCannotReplace;
                return null;
            }

            if (newClass.GetName() != oldName || newClass.GetSubName() != subName)
            {
                GameLogger.Warning(TAG, "ReplaceManager 失败，要替换的管理器 {0}:{1} 与 传入的管理器名称不符", oldName, subName);
                GameErrorManager.LastError = GameError.ModConflict;
                return null;
            }
            if (newClass.isEmpty)
            {
                GameLogger.Warning(TAG, "ReplaceManager 失败，LUA 必须初始化");
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }

            newClass.OldManager = old;

            GameManagerWorker.managers.Remove(old);
            GameManagerWorker.managers.Add(newClass);

            RequestManagerInitialization(newClass);

            GameLogger.Log(TAG, "Manager was replaced : {0} -> {1}",
                old.GetFullName(), newClass.GetFullName());

            return old;
        }

        /// <summary>
        /// 注册管理器就绪回调。
        /// 在这里来引用管理器实例
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="subName">二级名称</param>
        /// <param name="managerRedayDelegate">回调</param>
        /// <returns>返回一个ID，可以使用 UnRegisterManagerRedayCallback 取消注册回调</returns>
        public static int RegisterManagerRedayCallback(string name, string subName, LuaManagerRedayDelegate managerRedayDelegate, LuaTable self = null)
        {
            return GameManagerWorker.RegisterManagerRedayCallback(name, subName, managerRedayDelegate, self);
        }
        /// <summary>
        /// 注册管理器就绪回调（单例）。
        /// 在这里来引用管理器实例
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="managerRedayDelegate">回调</param>
        /// <returns>返回一个ID，可以使用 UnRegisterManagerRedayCallback 取消注册回调</returns>
        public static int RegisterManagerRedayCallback(string name, LuaManagerRedayDelegate managerRedayDelegate, LuaTable self = null)
        {
            return RegisterManagerRedayCallback(name, "Singleton", managerRedayDelegate, self);
        }
        /// <summary>
        /// 取消注册管理器就绪回调
        /// </summary>
        /// <param name="id">注册时返回的ID</param>
        public static void UnRegisterManagerRedayCallback(int id)
        {
            GameManagerWorker.UnRegisterManagerRedayCallback(id);
        }

        public static bool IsManagerInitFinished() { return GameManagerWorker.IsManagerInitFinished(); }

        /// <summary>
        /// 请求所有管理器初始化
        /// </summary>
        /// <param name="isPre">是否是预初始化</param>
        public static void RequestAllManagerInitialization()
        {
            foreach (BaseManager m in GameManagerWorker.managers)
            {
                if (!m.initialized || !m.preIinitialized)
                {
                    GameManagerWorker.nextInitManages.Add(m);
                    GameManagerWorker.nextInitManagerTick = 5;
                }
            }
        }
        /// <summary>
        /// 请求管理器初始化
        /// </summary>
        /// <param name="manager">目标管理器</param>
        /// <param name="isPre">是否是预初始化</param>
        public static void RequestManagerInitialization(BaseManager manager)
        {
            GameManagerWorker.nextInitManages.Add(manager);
            GameManagerWorker.nextInitManagerTick = 10;
        }
        /// <summary>
        /// 请求管理器立即初始化
        /// </summary>
        /// <param name="manager"></param>
        public static void RequestManagerInitializationImmediately(BaseManager manager)
        {
            if (!manager.preIinitialized)
                manager.DoPreInit();
            if (!manager.initialized)
                GameManagerWorker.InitManager(manager);
        }
        /// <summary>
        /// 手动释放管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static bool DestroyManager(string name)
        {
            foreach (BaseManager m in GameManagerWorker.managers)
                if (m.GetName() == name)
                {
                    if (m.OldManager != null)
                        m.OldManager.ReleaseManager();
                    m.ReleaseManager();
                    GameManagerWorker.managers.Remove(m);
                    UnityEngine.Object.Destroy(m);
                    GameLogger.Log(TAG, "Manager destroyed : {0}:{1}", m.GetName(), m.GetSubName());
                    return true;
                }
            GameLogger.Warning(TAG, "DestroyManager 失败，管理器 {0} 未注册", name);
            GameErrorManager.LastError = GameError.NotRegister;
            return false;
        }

        #endregion

        #region  游戏总体初始化例程

        /// <summary>
        /// 获取当前游戏状态模式
        /// </summary>
        public static GameMode Mode { get; private set; }
        /// <summary>
        /// 获取当前游戏工作场景
        /// </summary>
        public static GameCurrentScense CurrentScense { get; private set; }
        /// <summary>
        /// 游戏根，所有游戏部件在这里托管
        /// </summary>
        public static GameObject GameRoot { get; private set; }
        /// <summary>
        /// GameManager 元素
        /// </summary>
        public static GameObject GameManagerObject { get; private set; }
        /// <summary>
        /// 游戏UI根
        /// </summary>
        public static GameObject GameCanvas { get; private set; }
        /// <summary>
        /// 根相机
        /// </summary>
        public static Camera GameBaseCamera { get; private set; }
        /// <summary>
        /// 静态引入资源
        /// </summary>
        public static List<GameObjectInfo> GamePrefab { get; private set; }
        /// <summary>
        /// 游戏全局 Lua 虚拟机
        /// </summary>
        public static LuaSvr.MainState GameMainLuaState { get; set; }
        /// <summary>
        /// 静态资源引入
        /// </summary>
        public static List<GameAssetsInfo> GameAssets { get; private set; }
        [DoNotToLua]
        /// <summary>
        /// 在开始时暂停（通常用于调试）
        /// </summary>
        public static bool BreakAtStart { get; set; } = false;

        [Serializable]
        public class GameObjectInfo
        {
            public GameObject Object;
            public string Name;

            public override string ToString() { return Name; }
        }
        [Serializable]
        public class GameAssetsInfo
        {
            public UnityEngine.Object Object;
            public string Name;

            public override string ToString() { return Name; }
        }

        private static int gameManagerAlertDialogId = 0;
        private static GameManagerWorker GameManagerWorker = null;

        private static bool gameMediatorInitFinished = false;
        private static bool gameBaseInitFinished = false;

        /// <summary>
        /// 获取底层管理器是否初始化完成
        /// </summary>
        /// <returns></returns>
        public static bool IsGameBaseInitFinished()
        {
            return gameBaseInitFinished;
        }
        /// <summary>
        /// 获取中介者是否初始化完成。
        /// 通常中介者初始化完成后可以注册全局内核事件
        /// </summary>
        /// <returns></returns>
        public static bool IsGameMediatorInitFinished()
        {
            return gameMediatorInitFinished;
        }

        internal static IEnumerator Init(GameMode mode, GameObject gameRoot, GameObject gameCanvas, List<GameObjectInfo> gamePrefab, List<GameAssetsInfo> gameAssets)
        {
            CurrentScense = GameCurrentScense.None;
            Mode = mode;
            GameRoot = gameRoot;
            GameCanvas = gameCanvas;
            GamePrefab = gamePrefab;
            GameAssets = gameAssets;

            InitStaticPrefab();
            GameBaseCamera = GameObject.Find("GameBaseCamera").GetComponent<Camera>();
            GameManagerObject = GameCloneUtils.CreateEmptyObjectWithParent(GameRoot.transform, TAG);
            GameManagerWorker = GameManagerObject.AddComponent<GameManagerWorker>();

            //程序事件
            Application.wantsToQuit += Application_wantsToQuit;
            Application.lowMemory += Application_lowMemory;

            //错误提示
            GameObject GlobalGameErrorPanel = GameCanvas.transform.Find("GlobalGameErrorPanel").gameObject;
            GameErrorManager.SetGameErrorUI(GlobalGameErrorPanel.GetComponent<GameGlobalErrorUI>());

            GameLogger.Log(TAG, "Init game Version : {0}({1}) Platform : {2}", GameConst.GameVersion, 
                GameConst.GameBulidVersion, GameConst.GamePlatform);
            GameSettingsManager.Init();

            yield return new WaitForSeconds(0.2f);

            if (Mode != GameMode.None)
            {
                //初始化基础管理器
                GameMediator = (GameMediator)RegisterManager(typeof(GameMediator));
                RequestManagerInitializationImmediately(GameMediator);//中介者需要立即初始化
                gameMediatorInitFinished = true;
                UIManager = (UIManager)RegisterManager(typeof(UIManager));
                RegisterManager(typeof(DebugManager));
                ModManager = (ModManager)RegisterManager(typeof(ModManager)); 
                RegisterManager(typeof(SoundManager));

                if (Mode != GameMode.MinimumDebug)
                {
                    //游戏加载器
                    RegisterManager(typeof(GameInit));
                    //Lua
                    GameMainLuaState = new LuaSvr.MainState();
                }

                yield return new WaitUntil(IsManagerInitFinished);

                //初始化完成
                gameBaseInitFinished = true;
                GameLogger.Log(TAG, "All manager initialization complete");
                GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BASE_INIT_FINISHED, "*", null);

                if (Mode == GameMode.MinimumDebug)
                {
                    GameLogger.Log(TAG, "MinimumLoad Break");
                    gameManagerAlertDialogId = UIManager.GlobalAlertWindow("MinimumLoad<br/>当前是最小加载模式。", "提示", "关闭");
                }
                else if (BreakAtStart) //启动时暂停
                {
                    GameLogger.Log(TAG, "Game break at start");
                    gameManagerAlertDialogId = UIManager.GlobalAlertWindow("BreakAtStart<br/>您可以点击“继续运行”", "BreakAtStart", "继续运行");
                    GameMediator.RegisterEventHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, TAG, (evtName, param) =>
                    {
                        if ((int)param[0] == gameManagerAlertDialogId)
                            CallGameInit();
                        return false;
                    });
                }
                else CallGameInit();
            }
            else
            {
                GameLogger.Error(TAG, "GameMode not set. ");
                GameErrorManager.LastError = GameError.BadMode;
                GameErrorManager.ThrowGameError(GameError.BadMode, "错误的模式，请确定启动模式已设置");
            }
        }
        internal static void Destroy()
        {
            Debug.Log("[" + TAG + " ] Destroy game");
            GameSettingsManager.Destroy();
        }

        private static void Application_lowMemory()
        {
            GameLogger.Log(TAG, "lowMemory !");
            if (ModManager != null)
            {
                ModManager.UnLoadNotUsedMod();
            }
        }
        private static bool Application_wantsToQuit()
        {
            if (!gameIsQuitEmitByGameManager)
                DoQuit();
            return true;
        }
        private static void DoQuit() 
        {
            GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BEFORE_GAME_QUIT, "*", null);
            GameManagerWorker.ReqDestroyManagers();
            GameManagerWorker.ReqGameQuit();
        }
        //通知进行下一步内核加载
        private static void CallGameInit()
        {
            GameActionCallResult rs = GameMediator.CallAction(GameActionNames.CoreActions["ACTION_GAME_INIT"]);
            if (rs == null)
            {
                GameLogger.Error(TAG, "Not found handler for ACTION_GAME_INIT!");
                GameErrorManager.ThrowGameError(GameError.HandlerLost, "Not found handler for ACTION_GAME_INIT");
            }
            else if (rs.Success)
            {
                CurrentScense = GameCurrentScense.Intro;
            }
        }

        private static bool gameIsQuitEmitByGameManager = false;

        /// <summary>
        /// 设置基础摄像机状态
        /// </summary>
        /// <param name="visible">是否显示</param>
        public static void SetGameBaseCameraVisible(bool visible)
        {
            GameBaseCamera.gameObject.SetActive(visible);
        }
        /// <summary>
        /// 立即退出游戏
        /// </summary>
        public static void QuitGame()
        {
            GameLogger.Log(TAG, "Quiting game");
            gameIsQuitEmitByGameManager = true;
            DoQuit();
        }
        /// <summary>
        /// 强制中断游戏，此操作会立即停止所有物体运行。此操作由错误管理器进行控制
        /// </summary>
        public static void ForceInterruptGame()
        {
            GameLogger.Log(TAG, "Force interrupt game");
            foreach (Camera c in Camera.allCameras)
                c.gameObject.SetActive(false);
            for (int i = 0, c = GameCanvas.transform.childCount; i < c; i++)
            {
                GameObject go = GameCanvas.transform.GetChild(i).gameObject;
                if (go.name == "GameUIWindow")
                {
                    GameObject go1 = null;
                    for (int j = 0, c1 = go.transform.childCount; j < c1; j++)
                    {
                        go1 = go.transform.GetChild(j).gameObject;
                        if(go1.name != "GameUIWindow_Debug Window")
                            go1.SetActive(false);
                    }
                }
                else if(go.name != "GameUIDebugToolBar" && go.name != "GameUIWindow_Debug Window")
                    go.SetActive(false);
            }
            for (int i = 0, c = GameRoot.transform.childCount; i < c; i++)
                GameRoot.transform.GetChild(i).gameObject.SetActive(false);
            GameBaseCamera.gameObject.SetActive(true);
        }
        /// <summary>
        /// 关闭 GameManager 产生的全局对话框（调试用）
        /// </summary>
        public static void CloseGameManagerAlert()
        {
            UIManager.CloseWindow(UIManager.FindWindowById(gameManagerAlertDialogId));
        }

        #endregion

        #region  静态资源管理

        public static GameObject PrefabEmpty { get; private set; }
        public static GameObject PrefabUIEmpty { get; private set; }

        /// <summary>
        /// 在静态引入资源中查找
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns></returns>
        public static GameObject FindStaticPrefabs(string name)
        {
            foreach (GameObjectInfo gameObjectInfo in GamePrefab)
            {
                if (gameObjectInfo.Name == name)
                    return gameObjectInfo.Object;
            }
            return null;
        }
        /// <summary>
        /// 在静态引入资源中查找
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns></returns>
        public static T FindStaticAssets<T>(string name) where T : UnityEngine.Object
        {
            foreach (GameAssetsInfo gameAssetsInfo in GameAssets)
            {
                if (gameAssetsInfo.Name == name)
                    return (T)gameAssetsInfo.Object;
            }
            return null;
        }
        /// <summary>
        /// 在静态引入资源中查找
        /// </summary>
        /// <param name="name">资源名称</param>
        /// <returns></returns>
        public static UnityEngine.Object FindStaticAssets(string name)
        {
            foreach (GameAssetsInfo gameAssetsInfo in GameAssets)
            {
                if (gameAssetsInfo.Name == name)
                    return gameAssetsInfo.Object;
            }
            return null;
        }

        private static void InitStaticPrefab()
        {
            PrefabEmpty = FindStaticPrefabs("PrefabEmpty");
            PrefabUIEmpty = FindStaticPrefabs("PrefabUIEmpty");
        }

        #endregion

        #region  游戏基础管理器快速索引

        public static IModManager ModManager { get; private set; }
        public static UIManager UIManager { get; private set; }
        public static GameMediator GameMediator { get; private set; }

        #endregion
    }

    [CustomLuaClass]
    /// <summary>
    /// 指定游戏状态模式
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        /// <summary>
        /// 普通游戏状态，此状态包括 主界面 以及 Level
        /// </summary>
        Game,
        /// <summary>
        /// 关卡模式
        /// </summary>
        Level,
        /// <summary>
        /// 关卡编辑器模式
        /// </summary>
        LevelEditor,
        /// <summary>
        /// 调试时使用的最小化加载模式
        /// </summary>
        MinimumDebug,
        /// <summary>
        /// 加载器调试
        /// </summary>
        LoaderDebug,
        /// <summary>
        /// 内核调试
        /// </summary>
        CoreDebug,

    }

    [CustomLuaClass]
    /// <summary>
    /// 指定游戏状态当前工作场景
    /// </summary>
    public enum GameCurrentScense
    {
        None,
        Intro,
        Level,
        LevelLoader,
        LevelEditor,
        LevelViewer,
        MenuLevel,
    }
}
