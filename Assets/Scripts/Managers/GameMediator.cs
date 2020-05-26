using Ballance2.Managers.CoreBridge;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ballance2.Managers
{
    /// <summary>
    /// 游戏全局中介者
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameMediator : BaseManagerSingleton
    {
        public const string TAG = "GameMediator";

        public GameMediator() : base(TAG)
        {
        }

        public override bool InitManager()
        {
            InitAllEvents();
               
            return true;
        }
        public override bool ReleaseManager()
        {
            UnLoadAllEvents();
            return true;
        }
        public override void ReloadData()
        {
            base.ReloadData();
        }

        #region 全局事件控制器

        private List<GameEvent> events = null;

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public bool RegisterGlobalEvent(string evtName)
        {
            if (!IsGlobalEventRegistered(evtName))
            {
                GameEvent gameEvent = new GameEvent(evtName);
                events.Add(gameEvent);
                return true;
            }
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 已注册", evtName);
                GameErrorManager.LastError = GameError.AlredayRegistered;
            }
            return false;
        }
        /// <summary>
        /// 取消注册事件
        /// </summary>
        /// <param name="evtName">事件名称</param>
        public void UnRegisterGlobalEvent(string evtName)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                gameEvent.Dispose();
                events.Remove(gameEvent);
            }
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.Unregistered;
            }
        }
        /// <summary>
        /// 获取事件是否注册
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <returns>是否注册</returns>
        public bool IsGlobalEventRegistered(string evtName)
        {
            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent.EventName == evtName)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取事件是否注册，如果已注册，则返回实例
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="e">返回的事件实例</param>
        /// <returns>是否注册</returns>
        public bool IsGlobalEventRegistered(string evtName, out GameEvent e)
        {
            foreach (GameEvent gameEvent in events)
            {
                if (gameEvent.EventName == evtName)
                {
                    e = gameEvent;
                    return true;
                }
            }
            e = null;
            return false;
        }
        /// <summary>
        /// 执行事件分发
        /// </summary>
        /// <param name="id">事件名称</param>
        public void DispatchGlobalEvent(string evtName, string handlerFilter, params object[] pararms)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                foreach (GameHandler gameHandler in gameEvent.EventHandlers)
                {
                    if (Regex.IsMatch(gameHandler.Name, handlerFilter))
                        if (gameHandler.Call(evtName, pararms))
                        {
                            GameLogger.Instance.Log(TAG, "Event {0} was interrupted by : {1}", evtName, gameHandler.Name);
                            break;
                        }
                }
            }
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.Unregistered;
            }
        }

        //卸载所有命令
        private void UnLoadAllEvents()
        {
            if (events != null)
            {
                foreach (GameEvent gameEvent in events)
                    gameEvent.Dispose();
                events.Clear();
                events = null;
            }
        }
        private void InitAllEvents()
        {
            events = new List<GameEvent>();

            //注册基础事件
            RegisterGlobalEvent(GameEventNames.EVENT_BASE_INIT_FINISHED);
        }

        /// <summary>
        /// 注册命令接收器（C#使用）
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="name">接收器名字</param>
        /// <param name="gameHandlerDelegate">回调</param>
        /// <returns></returns>
        public GameHandler RegisterEventKernalHandler(string evtName, string name, GameHandlerDelegate gameHandlerDelegate)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                GameHandler gameHandler = new GameHandler(name, gameHandlerDelegate);
                gameEvent.EventHandlers.Add(gameHandler);
                return gameHandler;
            }
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.Unregistered;
            }
            return null;
        }
        /// <summary>
        /// 注册事件接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="name">接收器名字</param>
        /// <param name="luaModulHandler">模块接收器函数标识符</param>
        /// <returns>返回接收器类</returns>
        public GameHandler RegisterEventHandler(string evtName, string name, string luaModulHandler)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
            {
                GameHandler gameHandler = new GameHandler(name, luaModulHandler);
                gameEvent.EventHandlers.Add(gameHandler);
                return gameHandler;
            }
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.Unregistered;
            }
            return null;
        }
        /// <summary>
        /// 取消注册事件接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="handler">接收器类</param>
        public void UnRegisterEventHandler(string evtName, GameHandler handler)
        {
            GameEvent gameEvent = null;
            if (IsGlobalEventRegistered(evtName, out gameEvent))
                gameEvent.EventHandlers.Remove(handler);
            else
            {
                GameLogger.Instance.Warning(TAG, "事件 {0} 未注册", evtName);
                GameErrorManager.LastError = GameError.Unregistered;
            }
        }

        #endregion




    }
}
