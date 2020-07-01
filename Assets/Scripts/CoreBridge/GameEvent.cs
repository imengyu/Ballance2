using System.Collections.Generic;

namespace Ballance2.CoreBridge
{
    [SLua.CustomLuaClass]
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

    [SLua.CustomLuaClass]
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
        /// 基础管理器初始化完成时触发该事件
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】管理器名称
        /// 【1】管理器二级名称
        /// </remarks>
        public const string EVENT_BASE_MANAGER_INIT_FINISHED = "e:base_manager_init_finished";

        /// <summary>
        /// 全局对话框（Alert，Confirm）关闭时触发该事件
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对话框ID
        /// 【1】用户是否选择了 Confirm（对于 Alert 永远是false）
        /// </remarks>
        public const string EVENT_GLOBAL_ALERT_CLOSE = "e:ui:global_alert_close";

        /// <summary>
        /// 游戏即将退出时触发该事件
        /// </summary>
        /// <remarks>
        /// 事件参数：无
        /// </remarks>
        public const string EVENT_BEFORE_GAME_QUIT = "e:before_game_quit";

        /// <summary>
        /// 游戏底层加载完成，现在开始加载游戏内核
        /// </summary>
        /// <remarks>
        /// 事件参数：无
        /// </remarks>
        public const string EVENT_GAME_INIT_ENTRY = "e:base_game_init";

        /// <summary>
        /// gameinit 完成
        /// </summary>
        /// <remarks>
        /// 事件参数：无
        /// </remarks>
        public const string EVENT_GAME_INIT_FINISH = "e:init:gameinit_finish";

        /// <summary>
        /// 模组加载成功
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对应模组 UID
        /// 【1】对应模组对象
        /// </remarks>
        public const string EVENT_MOD_LOAD_SUCCESS = "e:mod:mod_load_success";

        /// <summary>
        /// 模组加载成功
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对应模组 UID
        /// 【1】对应模组对象
        /// 【2】错误信息
        /// </remarks>
        public const string EVENT_MOD_LOAD_FAILED = "e:mod:mod_load_failed";

        /// <summary>
        /// 模组注册
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对应模组 UID
        /// 【1】对应模组对象
        /// </remarks>
        public const string EVENT_MOD_REGISTERED = "e:mod:mod_registered";

        /// <summary>
        /// 模组卸载
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】对应模组 UID
        /// 【1】对应模组对象
        /// </remarks>
        public const string EVENT_MOD_UNLOAD = "e:mod:mod_unload";

        /// <summary>
        /// 屏幕分辨率变化
        /// </summary>
        /// <remarks>
        /// 事件参数：
        /// 【0】Vector2 屏幕大小
        /// </remarks>
        public const string EVENT_SCREEN_SIZE_CHANGED = "e:core:screen_size_changed";
    }
}
