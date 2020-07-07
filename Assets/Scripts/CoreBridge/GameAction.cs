using Ballance2.Utils;
using System.Collections.Generic;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 全局操作
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameAction
    {
        public GameAction(string name, GameHandler gameHandler, string[] callTypeCheck)
        {
            Name = name;
            GameHandler = gameHandler;
            CallTypeCheck = callTypeCheck;
        }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 操作接收器
        /// </summary>
        public GameHandler GameHandler { get; private set; }
        /// <summary>
        /// 操作类型检查
        /// </summary>
        public string[] CallTypeCheck { get; private set; }

        /// <summary>
        /// 空
        /// </summary>
        public static GameAction Empty { get; } = new GameAction("internal.empty", null, null);

        public void Dispose()
        {
            CallTypeCheck = null;
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

        /// <summary>
        /// 成功的无其他参数的调用返回结果
        /// </summary>
        public static GameActionCallResult SuccessResult = new GameActionCallResult(true, null);
        /// <summary>
        /// 失败的无其他参数的调用返回结果
        /// </summary>
        public static GameActionCallResult FailResult = new GameActionCallResult(false, null);
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 游戏内部操作说明
    /// </summary>
    public static class GameActionNames
    {
        /// <summary>
        /// 游戏内核操作
        /// </summary>
        public static Dictionary<string, string> CoreActions = new Dictionary<string, string>() {
            /// <summary>
            /// 退出游戏操作
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "QuitGame", GamePartName.Core + ".QuitGame" },
            /// <summary>
            /// 显示设置页面
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string : 关闭后返回的 UI 页
            /// </remarks>
            { "ShowSettings", GamePartName.Core + ".ShowSettings" },
            /// <summary>
            /// 打开编辑器编辑关卡
            /// </summary>
            /// 参数：
            /// [0] string : 目标关卡名称或路径
            /// </remarks>
            { "EditLevel", GamePartName.Core + ".EditLevel" },

            { "ACTION_GAME_INIT", GamePartName.Core + ".game_init" },
            { "ACTION_DEBUG_LEVEL_LOADER", GamePartName.Core + ".debug_level_loader" },
            { "ACTION_DEBUG_CORE", GamePartName.Core + ".debug_core" },
        };
        /// <summary>
        /// 自定义操作名字。你可以往这里添加名字，然后来快速索引（须动态添加）
        /// </summary>
        public static Dictionary<string, string> CustomActions = new Dictionary<string, string>() {
           
        };
        /// <summary>
        /// LevelLoader 操作
        /// </summary>
        public static Dictionary<string, string> LevelLoader = new Dictionary<string, string>() {
            /// <summary>
            /// 加载关卡
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string : 目标关卡名称或路径
            /// </remarks>
            { "LoadLevel", GamePartName.LevelLoader + ".LoadLevel" },
            /// <summary>
            /// 卸载关卡
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "UnLoadLevel", GamePartName.LevelLoader + ".UnLoadLevel" },
        };
        /// <summary>
        /// 球管理器操作
        /// </summary>
        public static Dictionary<string, string> BallManager = new Dictionary<string, string>() {
            /// <summary>
            /// 球管理器开始控制
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "StartControll", GamePartName.BallManager + ".StartControll" },
            /// <summary>
            /// 球管理器停止控制
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "EndControll", GamePartName.BallManager + ".EndControll" },
            /// <summary>
            /// 球管理器播放球的烟雾动画
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "PlaySmoke", GamePartName.BallManager + ".PlaySmoke" },
            /// <summary>
            /// 球管理器播放球的出生闪电动画
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] bool smallToBig : 是否由小变大
            /// [1] bool lightAnim : 是否播放相对应的 Light 灯光
            /// </remarks>
            { "PlayLighting", GamePartName.BallManager + ".PlayLighting" },
            /// <summary>
            /// 指定球速度清零
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall ball :  指定球
            /// </remarks>
            { "RemoveBallSpeed", GamePartName.BallManager + ".RemoveBallSpeed" },
            /// <summary>
            /// 添加球推动方向
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] BallPushType t : 球推动方向
            /// </remarks>
            { "AddBallPush", GamePartName.BallManager + ".AddBallPush" },
            /// <summary>
            /// 去除球推动方向
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] BallPushType t : 球推动方向
            /// </remarks>
            { "RemoveBallPush", GamePartName.BallManager + ".RemoveBallPush" },
            /// <summary>
            /// 设置球下次激活的位置
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 下次激活的位置
            /// </remarks>
            { "RecoverSetPos", GamePartName.BallManager + ".RecoverSetPos" },
            /// <summary>
            /// 重新设置当前球在默认位置并激活
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "RecoverBallDef", GamePartName.BallManager + ".RecoverBallDef" },
            /// <summary>
            /// 重新设置当前球位置并激活
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 目标位置
            /// </remarks>
            { "RecoverBallAtPos", GamePartName.BallManager + ".RecoverBallAtPos" },
            /// <summary>
            /// 激活指定的球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string type :  球名字
            /// </remarks>
            { "ActiveBall", GamePartName.BallManager + ".ActiveBall" },
            /// <summary>
            /// 激活默认球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "ActiveBallDef", GamePartName.BallManager + ".ActiveBallDef" },
            /// <summary>
            /// 清除已激活的球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "ClearActiveBall", GamePartName.BallManager + ".ClearActiveBall" },
            /// <summary>
            /// 平滑移动当前球至指定位置
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] Vector3 pos : 指定位置
            /// [1] float off = 2f ：耗时（秒）
            /// </remarks>
            { "SmoothMoveBallToPos", GamePartName.BallManager + ".SmoothMoveBallToPos" },
            /// <summary>
            /// 抛出指定球碎片
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall 或 string ball : 球的实例或球的名字
            /// </remarks>
            { "ThrowPieces", GamePartName.BallManager + ".ThrowPieces" },
            /// <summary>
            /// 恢复指定球碎片
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] GameBall 或 string ball : 球的实例或球的名字
            /// </remarks>
            { "RecoverPieces", GamePartName.BallManager + ".RecoverPieces" },
            /// <summary>
            /// 注册球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string name：球类型名称
            /// [1] GameBall ball：附加了GameBall组件的球实例
            /// [2] GameObject pieces：球碎片组
            /// </remarks>
            { "RegisterBall", GamePartName.BallManager + ".RegisterBall" },
            /// <summary>
            /// 取消注册球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0]  string ball : 球的名字
            /// </remarks>
            { "UnRegisterBall", GamePartName.BallManager + ".UnRegisterBall" },
            /// <summary>
            /// 获取已注册的球
            /// </summary>
            /// <remarks>
            /// 参数：
            /// [0] string name : 球的名字
            /// 返回值：
            /// [0] GameBall：找到的球的实例，如果未找到，为null
            /// </remarks>
            { "GetRegisteredBall", GamePartName.BallManager + ".GetRegisteredBall" },

        };
        /// <summary>
        /// 摄像机管理器操作
        /// </summary>
        public static Dictionary<string, string> CamManager = new Dictionary<string, string>()
        {
            /// <summary>
            /// 将摄像机开启
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamStart", GamePartName.CamManager + ".CamStart" },
            /// <summary>
            /// 将摄像机关闭
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamClose", GamePartName.CamManager + ".CamClose" },
            /// <summary>
            /// 让摄像机不看着球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamSetNoLookAtBall", GamePartName.CamManager + ".CamSetNoLookAtBall" },
            /// <summary>
            /// 让摄像机看着球
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamSetLookAtBall", GamePartName.CamManager + ".CamSetLookAtBall" },
            /// <summary>
            /// 让摄像机只看着球（不跟随）
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamSetJustLookAtBall", GamePartName.CamManager + ".CamSetJustLookAtBall" },
            /// <summary>
            /// 摄像机向左旋转
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamRoteLeft", GamePartName.CamManager + ".CamRoteLeft" },
            /// <summary>
            /// 摄像机向右旋转
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamRoteRight", GamePartName.CamManager + ".CamRoteRight" },
            /// <summary>
            /// 摄像机 按住 空格键 上升
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamRoteSpace", GamePartName.CamManager + ".CamRoteSpace" },
            /// <summary>
            /// 摄像机 放开 空格键 下降
            /// </summary>
            /// <remarks>
            /// 参数：无
            /// </remarks>
            { "CamRoteSpaceBack", GamePartName.CamManager + ".CamRoteSpaceBack" },
        };
            

    }
}
