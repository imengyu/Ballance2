using System.Collections.Generic;

namespace Ballance2.Managers.CoreBridge
{
    /// <summary>
    /// 全局事件存储类
    /// </summary>
    public class GameEvent
    {
        public GameEvent(string evtName)
        {
            EventName = evtName; EventHandlers = new List<GameHandler>();
        }

        public void Dispose()
        {
            EventHandlers.Clear();
            EventHandlers = null;
        }

        public string EventName { get; private set; }
        public List<GameHandler> EventHandlers { get; private set; }
    }

    /// <summary>
    /// 游戏内部事件说明
    /// </summary>
    public static class GameEventNames
    {
        /// <summary>
        /// 全局（基础管理器）全部初始化完成时触发该事件
        /// </summary>
        /// <remarks>
        /// 事件参数：无
        /// </remarks>
        public const string EVENT_BASE_INIT_FINISHED = "e:base_init_finished";

        /// <summary>
        /// 全局对话框（Alert，Confirm）关闭时触发该事件
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对话框ID
        /// 【1】用户是否选择了 Confirm（对于 Alert 永远是false）
        /// </remarks>
        public const string EVENT_GLOBAL_ALERT_CLOSE = "e:ui:global_Alert_close";
    }
}
