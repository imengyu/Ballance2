﻿
using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.Managers.Base
{
    /// <summary>
    /// 管理器初始化管理
    /// </summary>
    class GameManagerWorker : MonoBehaviour
    {
        private void Update()
        {
            if (nextInitManagerTick > 0)
            {
                nextInitManagerTick--;
                if (nextInitManagerTick == 0)
                    ReqInitManagers();
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

        public int nextInitManagerTick = 0;
        public List<BaseManager> nextInitManages = new List<BaseManager>();

        // 全局管理器单例数组
        public List<BaseManager> managers = new List<BaseManager>();

        public bool IsManagerInitFinished() { return nextInitManagerTick <= 0; }
        private void ReqInitManagers()
        {
            foreach (BaseManager m in nextInitManages)
            {
                if (!m.preIinitialized)
                    m.DoPreInit();
            }
            foreach (BaseManager m in nextInitManages)
            {
                InitManager(m);
            }
            nextInitManages.Clear();
        }

        public class ManagerRedayCallback
        {
            public int id;
            public LuaManagerRedayDelegate redayDelegate;
            public LuaTable self;
            public string name;
        }
        public List<ManagerRedayCallback> redayCallbacks = new List<ManagerRedayCallback>();

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
        public void InitManager(BaseManager manager)
        {
            if (!manager.Initialized)
            {
                if (!manager.InitManager())
                {
                    GameLogger.Warning("GameManager", "InitManager 失败，管理器 {0}:{1} 初始化失败", manager.GetName(), manager.GetSubName());
                    GameErrorManager.LastError = GameError.InitializationFailed;
                }
                else
                {
                    manager.initialized = true;

                    GameLogger.Log("GameManager", "{0}:{1} Inited ", manager.GetName(), manager.GetSubName());

                    CallManagerRedayCallback(manager, manager.GetNameWithSub());

                    if (GameManager.GameMediator != null)
                        GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_BASE_MANAGER_INIT_FINISHED, "*", manager.GetName(), manager.GetSubName());
                }
            }
        }

        public void DestroyManagers()
        {
            //降序排列销毁
            managers.Sort((m1, m2) => -m1.loadIndex.CompareTo(m2.loadIndex));

            bool b = false;
            if (managers != null)
            {
                for (int i = managers.Count - 1; i >= 0; i--)
                {
                    b = managers[i].ReleaseManager();
                    if (!b) Debug.LogWarningFormat("[GameManagerWorker] Failed to release manager {0}:{1} . ",
                         managers[i].GetName(), managers[i].GetSubName());
                }
                managers.Clear();
                managers = null;
            }

            nextInitManages.Clear();
        }

        private void CallManagerRedayCallback(BaseManager manager, string name)
        {
            foreach (ManagerRedayCallback c in redayCallbacks)
                if (c.name == name)
                    c.redayDelegate(c.self, manager.Store, manager);
        }
    }
}
