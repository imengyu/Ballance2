using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.ModBase;
using System.Collections;
using System.Collections.Generic;

namespace Ballance2.Interfaces
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组管理器
    /// </summary>
    public interface IModManager
    {
        /// <summary>
        /// 获取所有已注册模组数
        /// </summary>
        /// <returns></returns>
        int GetAllModCount();
        /// <summary>
        /// 获取已经加载的模组数量
        /// </summary>
        /// <returns></returns>
        int GetLoadedModCount();

        /// <summary>
        /// 获取当前正在加载的模组
        /// </summary>
        List<GameMod> CurrentLoadingMod { get; }

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="packagePath">模组包路径</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包实例，如果加载失败或找不到文件，返回null</returns>
        GameMod LoadGameMod(string packagePath, bool initialize = true);
        /// <summary>
        /// 通过包名加载模组包
        /// </summary>
        /// <param name="packageName">模组包包名</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包实例，如果加载失败或找不到文件，返回null</returns>
        GameMod LoadGameModByPackageName(string packageName, bool initialize = true);
        /// <summary>
        /// 通过路径查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        GameMod FindGameModByPath(string packagePath);
        /// <summary>
        /// 通过包名查找模组包
        /// </summary>
        /// <param name="packageName">包名</param>
        /// <returns></returns>
        GameMod FindGameMod(string packageName);
        /// <summary>
        /// 通过资源定义字符串查找模组包
        /// </summary>
        /// <param name="modStrIndef">模组包UID或资源定义字符串</param>
        /// <returns></returns>
        GameMod FindGameModByAssetStr(string modStrIndef);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="packageName">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool UnLoadGameMod(string packageName);
        /// <summary>
        /// 获取模组包是否正在加载
        /// </summary>
        /// <param name="packageName">模组包名</param>
        /// <returns>返回操作是否成功</returns>
        bool IsGameModLoading(string packageName);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        bool UnLoadGameMod(GameMod mod);
        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        bool InitializeLoadGameMod(GameMod mod);
        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="modPackageName">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool InitializeLoadGameMod(string modPackageName);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        bool UnInitializeLoadGameMod(GameMod mod);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modPackageName">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool UnInitializeLoadGameMod(string modPackageName);

        /// <summary>
        /// 执行模组包代码
        /// </summary>
        /// <param name="packageName">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool RunGameMod(string packageName);
        /// <summary>
        /// 执行模组包代码
        /// </summary>
        /// <param name="mod">模组包实例</param>
        /// <returns>返回操作是否成功</returns>
        bool RunGameMod(GameMod mod);

        /// <summary>
        /// 获取模组是否被用户启用
        /// </summary>
        /// <param name="mod">模组</param>
        /// <returns></returns>
        bool IsModEnabled(GameMod mod);
        /// <summary>
        /// 获取模组是否被用户启用
        /// </summary>
        /// <param name="packageName">模组包名</param>
        /// <returns></returns>
        bool IsModEnabled(string packageName);

        void ExecuteModEntry(GameModEntryCodeExecutionAt at);
        string[] GetModEnableStatusList();
        IEnumerator FlushModEnableStatus(bool unloadNotInMask);
        void UnLoadNotUsedMod(bool unloadNotInMask);
        void OnModLoadFinished(GameMod m);
        bool IsModNotInCurrentRunningMask(GameMod mod);
        bool IsAnyModLoading();
        bool IsNoneModLoading();

        Dictionary<string, ModDefCustomPropSolveDelegate> ModDefCustomPropertySolver { get; }
        Dictionary<string, LevelDefCustomPropSolveDelegate> LevelDefCustomPropertySolver { get; }

        /// <summary>
        /// 获取所有已注册模组数
        /// </summary>
        /// <returns></returns>
        int GetAllLevelCount();
        /// <summary>
        /// 注册一个关卡文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        GameLevel RegisterLevel(string path);
        /// <summary>
        /// 查找已注册的关卡
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        GameLevel FindLevel(string path);
        /// <summary>
        /// 移除已注册的关卡
        /// </summary>
        /// <param name="path">文件路径</param>
        void RemoveLevel(string path);
    }
}
