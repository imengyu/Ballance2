﻿
using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameManagerWorker.cs
 * 用途：
 * GameManager 静态类的辅助工作类
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

namespace Ballance2.Managers.Base
{
    /// <summary>
    /// 管理器初始化管理.
    /// 这个类是 GameManager 静态类的辅助工作类
    /// </summary>
    class GameManagerWorker : MonoBehaviour
    {
        private void Update()
        {
            //按时间进行任务执行
            if (nextInitManagerTick >= 0)
            {
                nextInitManagerTick--;
                if (nextInitManagerTick == 0)
                    InitManagers();
            }
            if (initManagers)
            {
                DoInitManagers();
            }
            if (nextGameQuitTick >= 0)
            {
                nextGameQuitTick--;
                if (nextGameQuitTick == 10)
                    GameManager.ClearScense();
                if (nextGameQuitTick == 0)
                    QuitGame();
            }
            if (nextDestroyTick >= 0)
            {
                nextDestroyTick--;
                if (nextDestroyTick == 0)
                    DestroyManagers();
            }
        }
        private void OnDestroy()
        {
            if(redayCallbacks != null)
            {
                redayCallbacks.Clear();
                redayCallbacks = null;
            }
        }

        private int loadIndex = 0;
        private bool initManagers = false;

        public int nextInitManagerTick = 0;
        /// <summary>
        /// 下一次要加载的管理器
        /// </summary>
        public List<BaseManager> nextInitManages = new List<BaseManager>();
        /// <summary>
        /// 全局管理器单例数组
        /// </summary>
        public List<BaseManager> managers = new List<BaseManager>();

        /// <summary>
        /// 获取管理器是否初始化完成（可用于WaitUntil）
        /// </summary>
        /// <returns></returns>
        public bool IsManagersInitFinished() { return nextInitManagerTick <= 0 && !initManagers;  }
        /// <summary>
        /// 强制停止加载
        /// </summary>
        public void ForceStopLoad()
        {
            initManagers = false;
            nextDestroyTick = -1;
            nextGameQuitTick = -1;
            nextInitManages.Clear();
        }

        private void DoInitManagers()
        {
            if(nextInitManages.Count > 0)
            {
                BaseManager m = nextInitManages[0];
                if (!m.initialized)
                    InitManager(m);
                nextInitManages.RemoveAt(0);
            }
            else initManagers = false;
        }
        private void InitManagers()
        {
            initManagers = true;
            foreach (BaseManager m in nextInitManages)
            {
                if (!m.preIinitialized) m.DoPreInit();
            }
        }

        private int nextGameQuitTick = -1;
        private int nextDestroyTick = -1;

        /// <summary>
        /// 请求游戏退出
        /// </summary>
        public void ReqGameQuit()
        {
            nextGameQuitTick = 60;
        }
        /// <summary>
        /// 请求销毁所有管理器
        /// </summary>
        public void ReqDestroyManagers()
        {
            nextDestroyTick = 35;
        }
        /// <summary>
        /// 请求通知场景更改事件
        /// </summary>
        public void ReqNotifyScenseChanged()
        {
            StartCoroutine(NotifyScenseChanged());
        }

        private IEnumerator NotifyScenseChanged()
        {
            yield return new WaitUntil(GameManager.ModManager.IsNoneModLoading);

            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_ENTER_SCENSE, "*", GameManager.CurrentScense);
        }
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public class ManagerRedayCallback
        {
            public int id;
            public LuaManagerRedayDelegate redayDelegate;
            public LuaTable self;
            public string name;
        }
        public List<ManagerRedayCallback> redayCallbacks = new List<ManagerRedayCallback>();

        /// <summary>
        /// 注册管理器就绪回调
        /// </summary>
        /// <param name="name">管理器名称</param>
        /// <param name="subName">管理器二级名称</param>
        /// <param name="managerRedayDelegate">回调</param>
        /// <param name="self"></param>
        /// <returns>返回回调ID</returns>
        public int RegisterManagerRedayCallback(string name, string subName, LuaManagerRedayDelegate managerRedayDelegate, LuaTable self = null)
        {
            ManagerRedayCallback callback = new ManagerRedayCallback();
            callback.name = name + ":" + subName;
            callback.self = self;
            callback.id = CommonUtils.GenRandomID();
            callback.redayDelegate = managerRedayDelegate;
            redayCallbacks.Add(callback);
            return callback.id;
        }
        /// <summary>
        /// 取消注册管理器就绪回调
        /// </summary>
        /// <param name="id">回调ID</param>
        public void UnRegisterManagerRedayCallback(int id)
        {
            for (int i = redayCallbacks.Count - 1; i >= 0; i--)
            {
                if (redayCallbacks[i].id == id)
                {
                    redayCallbacks.RemoveAt(i);
                    break;
                }
            }
        }
        /// <summary>
        /// 初始化管理器
        /// </summary>
        /// <param name="manager">目标管理器</param>
        public void InitManager(BaseManager manager)
        {
            if (!manager.Initialized)
            {
                try
                {
                    if (!manager.InitManager())
                    {
                        GameLogger.Error("GameManager", "InitManager 失败，管理器 {0}:{1} 初始化失败", manager.GetName(), manager.GetSubName());
                        GameErrorManager.LastError = GameError.InitializationFailed;
                    }
                    else
                    {
                        manager.initialized = true;
                        manager.loadIndex = ++loadIndex;

                        GameLogger.Log("GameManager", "{0}:{1} Inited ", manager.GetName(), manager.GetSubName());

                        CallManagerRedayCallback(manager, manager.GetNameWithSub());

                        if (GameManager.GameMediator != null)
                            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED, "*", manager.GetName(), manager.GetSubName());
                    }
                }
                catch(Exception e)
                {
                    GameLogger.Error("GameManager", (object)("管理器 " + manager.GetName() + ":" + 
                        manager.GetSubName() + " 初始化失败: " + e.Message + "\n" + e));
                }
            }
        }
        /// <summary>
        /// 销毁所有管理器
        /// </summary>
        public void DestroyManagers()
        {
            //降序排列销毁（加载顺序反过来）
            managers.Sort((m1, m2) => m1.loadIndex.CompareTo(m2.loadIndex));

            bool b = false;
            if (managers != null)
            {
                for (int i = managers.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        b = managers[i].ReleaseManager();
                        Debug.LogFormat("[GameManagerWorker] Destroy manager {0}:{1}",
                             managers[i].GetName(), managers[i].GetSubName());
                        if (!b) Debug.LogWarningFormat("[GameManagerWorker] Failed to release manager {0}:{1} . ",
                             managers[i].GetName(), managers[i].GetSubName());
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarningFormat("[GameManagerWorker] Failed to release manager {0}:{1} ." +
                            " An exception occurred in the release:\n{2}",
                             managers[i].GetName(), managers[i].GetSubName(), e.ToString());
                    }
                }
                managers.Clear();
                managers = null;
            }

            nextInitManages.Clear();
        }

        //调用就绪回调
        private void CallManagerRedayCallback(BaseManager manager, string name)
        {
            foreach (ManagerRedayCallback c in redayCallbacks)
                if (c.name == name)
                    c.redayDelegate(c.self, manager.Store, manager.ActionStore, manager);
        }
    }
}
