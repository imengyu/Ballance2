using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.Utils
{
    /// <summary>
    /// 错误管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameErrorManager
    {
        /// <summary>
        /// 游戏上一个操作的错误
        /// </summary>
        public static GameError LastError { get; set; }

        private static GameGlobalErrorUI gameGlobalErrorUI;
        internal static void SetGameErrorUI(GameGlobalErrorUI gameGlobalErrorUI)
        {
            GameErrorManager.gameGlobalErrorUI = gameGlobalErrorUI;
        }

        /// <summary>
        /// 抛出游戏异常，此操作会直接
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public static void ThrowGameError(GameError code, string message)
        {       
            StringBuilder stringBuilder = new StringBuilder("错误代码：");
            stringBuilder.Append(code.ToString());
            stringBuilder.Append("\n");
            stringBuilder.Append(message);
            stringBuilder.Append("\n");

            //打印堆栈
            var stacktrace = new StackTrace();
            for (var i = 0; i < stacktrace.FrameCount; i++)
            {
                var method = stacktrace.GetFrame(i).GetMethod();
                stringBuilder.Append("\n[");
                stringBuilder.Append(i);
                stringBuilder.Append("] ");
                stringBuilder.Append(method.Name);
            }

            GameManager.ForceInterruptGame();
            gameGlobalErrorUI.ShowErrorUI(stringBuilder.ToString());
        }
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 游戏发生错误的枚举
    /// </summary>
    public enum GameError
    {
        /// <summary>
        /// 无错误
        /// </summary>
        None,
        /// <summary>
        /// 错误的模式
        /// </summary>
        BadMode,
        /// <summary>
        /// 全局模块异常
        /// </summary>
        GlobalException,
        /// <summary>
        /// GameInit 文件找不到了
        /// </summary>
        GameInitReadFailed,
        /// <summary>
        /// GameInit 中的某个模块加载失败
        /// </summary>
        GameInitPartLoadFailed,
        /// <summary>
        /// 下一步操作对应接收器丢失，无法继续流程
        /// </summary>
        HandlerLost,
        /// <summary>
        /// 部件、模组、模块已经注册，不能重复注册
        /// </summary>
        AlredayRegistered,
        /// <summary>
        /// 部件、模组、模块未注册
        /// </summary>
        Unregistered,
        /// <summary>
        /// 需要初始化的 Prefab 未找到，可能是未注册
        /// </summary>
        PrefabNotFound,
        /// <summary>
        /// 初始化失败
        /// </summary>
        InitializationFailed,
        /// <summary>
        /// 无效资源包
        /// </summary>
        BadAssetBundle,
        /// <summary>
        /// 未找到文件
        /// </summary>
        FileNotFound,
        /// <summary>
        /// 模组包冲突
        /// </summary>
        ModConflict,
        /// <summary>
        /// 模组包必要依赖项加载失败
        /// </summary>
        ModDependenciesLoadFailed,
        /// <summary>
        /// 模组不能运行
        /// </summary>
        ModCanNotRun,
        /// <summary>
        /// 模组包版本不兼容
        /// </summary>
        BadMod,
        /// <summary>
        /// 网络错误
        /// </summary>
        NetworkError,
        /// <summary>
        /// 未初始化
        /// </summary>
        NotInitialize,
        /// <summary>
        /// 实例化根必须是容器
        /// </summary>
        MustBeContainer,
    }
}
