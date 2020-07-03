using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.CoreBridge
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
            ReturnParams = LuaUtils.LuaTableArrayToObjectArray(returnParams);
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
        /// [0] string : 关闭后返回的 UI 页
        /// </remarks>
        public const string ACTION_SHOW_SETTINGS = "core.ui.show_settings";
        /// <summary>
        /// 加载关卡
        /// </summary>
        /// <remarks>
        /// 参数：
        /// [0] string : 目标关卡名称或路径
        /// </remarks>
        public const string ACTION_LOAD_LEVEL = "core.load_level";
        /// <summary>
        /// 卸载关卡
        /// </summary>
        /// <remarks>
        /// 参数：无
        /// </remarks>
        public const string ACTION_UNLOAD_LEVEL = "core.unload_level";
        /// <summary>
        /// 打开编辑器编辑关卡
        /// </summary>
        /// 参数：
        /// [0] string : 目标关卡名称或路径
        /// </remarks>
        public const string ACTION_EDIT_LEVEL = "core.edit_level";

        public const string ACTION_DEBUG_LEVEL_LOADER = "core.debug_level_loader";
        public const string ACTION_DEBUG_CORE = "core.debug";

        /// <summary>
        /// 球管理器操作
        /// </summary>
        public static class BallManager
        {
            public const string BASE = "core.ballmgr";

            /// <summary>
            /// 球管理器开始控制
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string StartControll = BASE + ".StartControll";
            /// <summary>
            /// 球管理器停止控制
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string EndControll = BASE + ".EndControll";
            /// <summary>
            /// 球管理器播放球的烟雾动画
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string PlaySmoke = BASE + ".PlaySmoke";
            /// <summary>
            /// 球管理器播放球的出生闪电动画
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] bool smallToBig : 是否由小变大
            /// [1] bool lightAnim : 是否播放相对应的 Light 灯光
            /// </remarks>
            public const string PlayLighting = BASE + ".PlayLighting";
            /// <summary>
            /// 指定球速度清零
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall ball :  指定球
            /// </remarks>
            public const string RemoveBallSpeed = BASE + ".RemoveBallSpeed";
            /// <summary>
            /// 添加球推动方向
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] BallPushType t : 球推动方向
            /// </remarks>
            public const string AddBallPush = BASE + ".AddBallPush";
            /// <summary>
            /// 去除球推动方向
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] BallPushType t : 球推动方向
            /// </remarks>
            public const string RemoveBallPush = BASE + ".RemoveBallPush";
            /// <summary>
            /// 设置球下次激活的位置
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 下次激活的位置
            /// </remarks>
            public const string RecoverSetPos = BASE + ".RecoverSetPos";
            /// <summary>
            /// 重新设置当前球在默认位置并激活
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string RecoverBallDef = BASE + ".RecoverBallDef";
            /// <summary>
            /// 重新设置当前球位置并激活
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 目标位置
            /// </remarks>
            public const string RecoverBallAtPos = BASE + ".RecoverBallAtPos";
            /// <summary>
            /// 激活默认球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string ActiveBallDef = BASE + ".ActiveBallDef";
            /// <summary>
            /// 激活指定的球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string type :  球名字
            /// </remarks>
            public const string ActiveBall = BASE + ".ActiveBall";
            /// <summary>
            /// 清除已激活的球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string ClearActiveBall = BASE + ".ClearActiveBall";
            /// <summary>
            /// 清除已激活的球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 
            /// [1] float off = 2f
            /// </remarks>
            public const string SmoothMoveBallToPos = BASE + ".SmoothMoveBallToPos";
            /// <summary>
            /// 抛出指定球碎片
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall 或 string ball : 球的实例或球的名字
            /// </remarks>
            public const string ThrowPieces = BASE + ".ThrowPieces";
            /// <summary>
            /// 恢复指定球碎片
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall 或 string ball : 球的实例或球的名字
            /// </remarks>
            public const string RecoverPieces = BASE + ".RecoverPieces";
        }
        /// <summary>
        /// 摄像机管理器操作
        /// </summary>
        public static class CamManager
        {
            public const string BASE = "core.cammgr";

            /// <summary>
            /// 将摄像机开启
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamStart = BASE + ".CamStart";
            /// <summary>
            /// 将摄像机关闭
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamClose = BASE + ".CamClose";
            /// <summary>
            /// 让摄像机不看着球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamSetNoLookAtBall = BASE + ".CamSetNoLookAtBall";
            /// <summary>
            /// 让摄像机看着球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamSetLookAtBall = BASE + ".CamSetLookAtBall";
            /// <summary>
            /// 让摄像机只看着球（不跟随）
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamSetJustLookAtBall = BASE + ".CamSetJustLookAtBall";
            /// <summary>
            /// 摄像机向左旋转
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamRoteLeft = BASE + ".CamRoteLeft";
            /// <summary>
            /// 摄像机向右旋转
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamRoteRight = BASE + ".CamRoteRight";
            /// <summary>
            /// 摄像机 按住 空格键 上升
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamRoteSpace = BASE + ".CamRoteSpace";
            /// <summary>
            /// 摄像机 放开 空格键 下降
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            public const string CamRoteSpaceBack = BASE + ".CamRoteSpaceBack";

        }

    }
}
