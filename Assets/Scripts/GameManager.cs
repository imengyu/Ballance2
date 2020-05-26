﻿using Ballance2.Managers;
using Ballance2.Managers.CoreBridge;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
{
    /// <summary>
    /// 游戏主管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameManager
    {
        private const string TAG = "GameManager";

        #region 管理器控制

        /// <summary>
        /// 全局管理器单例数组
        /// </summary>
        private static List<IManager> managers = null;

        /// <summary>
        /// 获取管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static IManager GetManager(string name, string subName = "Singleton")
        {
            foreach (IManager m in managers)
                if (m.GetName() == name && m.GetSubName() == subName)
                    return m;
            return null;
        }
        /// <summary>
        /// 注册自定义管理器
        /// </summary>
        /// <param name="classInstance">实例</param>
        /// <returns></returns>
        public static IManager RegisterManager(IManager classInstance)
        {
            string name = classInstance.GetName();
            if (GetManager(name) != null)
            {
                if (classInstance.GetIsSingleton())
                {
                    GameLogger.Instance.Warning(TAG, "RegisterManager 失败，管理器 {0} 已注册", name);
                    GameErrorManager.LastError = GameError.AlredayRegistered;
                    return null;
                }
                else if(GetManager(name, classInstance.GetSubName()) != null)
                {
                    GameLogger.Instance.Warning(TAG, "RegisterManager 失败，管理器 {0}:{1} 已注册", name, classInstance.GetSubName());
                    GameErrorManager.LastError = GameError.AlredayRegistered;
                    return null;
                }
            }
            managers.Add(classInstance);
            GameLogger.Instance.Log(TAG, "Manager registered : {0}:{1}", classInstance.GetName(), classInstance.GetSubName());

            if (!classInstance.InitManager())
            {
                GameLogger.Instance.Warning(TAG, "RegisterManager 失败，管理器 {0} 初始化失败", name);
                GameErrorManager.LastError = GameError.InitializationFailed;
                return null;
            }
            return classInstance;
        }
        
        /// <summary>
        /// 手动释放管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static bool DestroyManager(string name)
        {
            foreach (IManager m in managers)
                if (m.GetName() == name)
                {
                    m.ReleaseManager();
                    managers.Remove(m);
                    GameLogger.Instance.Log(TAG, "Manager destroyed : {0}:{1}", m.GetName(), m.GetSubName());
                    return true;
                }
            GameLogger.Instance.Warning(TAG, "DestroyManager 失败，管理器 {0} 未注册", name);
            return false;
        }

        #endregion

        #region  游戏总体初始化例程

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
            /// 调试时使用的最小化加载模式
            /// </summary>
            MinimumLoad,
            /// <summary>
            /// 关卡模式
            /// </summary>
            Level,
            /// <summary>
            /// 关卡编辑器模式
            /// </summary>
            LevelEditor,
        }

        /// <summary>
        /// 获取当前游戏状态模式
        /// </summary>
        public static GameMode Mode { get; private set; }
        /// <summary>
        /// 游戏根，所有游戏部件在这里托管
        /// </summary>
        public static GameObject GameRoot { get; private set; }
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

        [Serializable]
        public class GameObjectInfo
        {
            public GameObject Object;
            public string Name;
        }

        private static bool gameMediatorInitFinished = false;
        private static bool gameBaseInitFinished = false;
        private static bool gameBreakAtStart = false;

        public static bool IsGameBaseInitFinished()
        {
            return gameBaseInitFinished;
        }
        public static bool IsGameMediatorInitFinished()
        {
            return gameMediatorInitFinished;
        }

        internal static bool Init(GameMode mode, GameObject gameRoot, 
            GameObject gameCanvas, List<GameObjectInfo> gamePrefab, bool breakAtStart)
        {
            bool result = false;

            Mode = mode;
            GameRoot = gameRoot;
            GameCanvas = gameCanvas;
            GamePrefab = gamePrefab;

            InitStaticPrefab();
            GameBaseCamera = GameObject.Find("GameBaseCamera").GetComponent<Camera>();
            gameBreakAtStart = breakAtStart;
            managers = new List<IManager>();
            

            //错误提示
            GameObject GlobalGameErrorPanel = GameCanvas.transform.Find("GlobalGameErrorPanel").gameObject;
            GameErrorManager.SetGameErrorUI(GlobalGameErrorPanel.GetComponent<GameGlobalErrorUI>());

            GameLogger.Instance.Log(TAG, "Init game");

            if (Mode != GameMode.None)
            {
                try
                {
                    //初始化各个管理器
                    GameMediator = (GameMediator)RegisterManager(new GameMediator());
                    gameMediatorInitFinished = true;

                    UIManager =  (UIManager)RegisterManager(new UIManager());
                    RegisterManager(GameCloneUtils.CreateEmptyObjectWithParent(GameRoot.transform, 
                        DebugManager.TAG).AddComponent<DebugManager>());

                    //初始化完成
                    gameBaseInitFinished = true;
                    GameLogger.Instance.Log(TAG, "All manager initialization complete");
                    GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BASE_INIT_FINISHED, "*", null);

                    //启动时暂停
                    if (gameBreakAtStart)
                    {
                        GameLogger.Instance.Log(TAG, "Game break at start");
                        UIManager.GlobalAlert("BreakAtStart<br/>您可以点击“继续运行”", "BreakAtStart", "继续运行");
                        GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE, TAG, (evtName, param) =>
                        {
                            
                            return false;
                        });
                    }
                    else
                    {

                    }
                }
                catch(Exception e)
                {
                    GameLogger.Instance.Error(TAG, "Global Exception was captured in initialization. ");
                    GameLogger.Instance.Exception(e);
                    GameErrorManager.LastError = GameError.GlobalException;
                    GameErrorManager.ThrowGameError(GameError.GlobalException, "初始化失败：\n" + e.ToString());
                }
            }
            else
            {
                GameLogger.Instance.Error(TAG, "GameMode not set. ");
                GameErrorManager.LastError = GameError.BadMode;
                GameErrorManager.ThrowGameError(GameError.BadMode, "错误的模式，请确定启动模式已设置");
            }

            return result;
        }
        internal static bool Destroy()
        {
            UnityEngine.Debug.Log("[" + TAG + " ] Destroy game");
            bool result = DestryAllManagers();
            managers.Clear();
            managers = null;
            return result;
        }
        private static bool DestryAllManagers()
        {
            bool b = false;
            for (int i = managers.Count - 1; i >= 0; i--)
            {
                b = managers[i].ReleaseManager();
                if (!b) UnityEngine.Debug.LogWarningFormat("[" + TAG +" ] Failed to release manager {0}:{1} . ",
                     managers[i].GetName(), managers[i].GetSubName());
            }
            return b;
        }

        /// <summary>
        /// 立即退出游戏
        /// </summary>
        public static void QuitGame()
        {
            GameLogger.Instance.Log(TAG, "Quiting game");
            GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BEFORE_GAME_QUIT, "*", null);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        /// <summary>
        /// 强制中断游戏，此操作会立即停止所有物体运行。此操作由错误管理器进行控制
        /// </summary>
        public static void ForceInterruptGame()
        {
            GameLogger.Instance.Log(TAG, "Force interrupt game");
            foreach (Camera c in Camera.allCameras)
                c.gameObject.SetActive(false);
            for (int i = 0, c = GameCanvas.transform.childCount; i < c; i++)
                GameCanvas.transform.GetChild(i).gameObject.SetActive(false);
            for (int i = 0, c = GameRoot.transform.childCount; i < c; i++)
                GameRoot.transform.GetChild(i).gameObject.SetActive(false);
            GameBaseCamera.gameObject.SetActive(true);
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

        private static void InitStaticPrefab()
        {
            GameLogger.Instance.Log(TAG, "Init static prefab, count : {0}", GamePrefab.Count);
            PrefabEmpty = FindStaticPrefabs("PrefabEmpty");
            PrefabUIEmpty = FindStaticPrefabs("PrefabUIEmpty");
        }

        #endregion

        #region  游戏基础管理器快速索引

        public static UIManager UIManager { get; private set; }
        public static GameMediator GameMediator { get; private set; }

        #endregion
    }
}