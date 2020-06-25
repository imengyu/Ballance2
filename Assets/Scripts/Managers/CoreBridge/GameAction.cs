using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.Managers.CoreBridge
{
    /// <summary>
    /// 全局操作
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameAction
    {
        public GameAction(string name, GameHandler gameHandler)
        {
            Name = name;
            GameHandler = gameHandler;
        }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 操作接收器
        /// </summary>
        public GameHandler GameHandler { get; private set; }

        public void Dispose()
        {
            GameHandler.Dispose();
            GameHandler = null;
        }
    }

    /// <summary>
    /// 操作调用结果
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameActionCallResult
    {
        /// <summary>
        /// 创建操作调用结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="returnParams">返回的数据</param>
        /// <returns></returns>
        public static GameActionCallResult CreateActionCallResult(bool success, object[] returnParams = null)
        {
            return new GameActionCallResult(success, returnParams);
        }

        public GameActionCallResult(bool success, object[] returnParams)
        {
            Success = success;
            ReturnParams = returnParams;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 返回的数据
        /// </summary>
        public object[] ReturnParams { get; private set; }
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 游戏内部操作说明
    /// </summary>
    public static class GameActionNames
    {
        /// <summary>
        /// 退出游戏操作
        /// </summary>
        /// <remarks>
        /// 参数：无
        /// </remarks>
        public const string ACTION_QUIT = "core.quit_game";
        /// <summary>
        /// 显示设置页面
        /// </summary>
        /// <remarks>
        /// 参数：
        /// [0] 关闭后返回的 UI 页
        /// </remarks>
        public const string ACTION_SHOW_SETTINGS = "core.ui.show_settings";

    }
}
