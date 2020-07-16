using Ballance2.ModBase;

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
        GameMod CurrentLoadingMod { get; }

        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="packagePath">模组包路径</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包UID，如果加载失败或找不到文件，返回0</returns>
        int LoadGameMod(string packagePath, bool initialize = true);
        /// <summary>
        /// 通过包名加载模组包
        /// </summary>
        /// <param name="packageName">模组包包名</param>
        /// <param name="initialize">是否立即初始化模组包</param>
        /// <returns>返回模组包UID，如果加载失败或找不到文件，返回0</returns>
        int LoadGameModByPackageName(string packageName, bool initialize = true);
        /// <summary>
        /// 通过路径查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        GameMod FindGameModByPath(string packagePath);
        /// <summary>
        /// 通过包名查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        GameMod FindGameModByName(string packageName);
        /// <summary>
        /// 通过包名查找模组包
        /// </summary>
        /// <param name="packagePath">路径</param>
        /// <returns>模组包</returns>
        GameMod[] FindAllGameModByName(string packageName);
        /// <summary>
        /// 通过UID查找模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns></returns>
        GameMod FindGameMod(int modUid);
        /// <summary>
        /// 通过资源定义字符串查找模组包
        /// </summary>
        /// <param name="modStrIndef">模组包UID或资源定义字符串</param>
        /// <returns></returns>
        GameMod FindGameModByAssetStr(string modStrIndef);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        bool UnLoadGameMod(int modUid);
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
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        bool InitializeLoadGameMod(int modUid);
        /// <summary>
        /// 加载模组包
        /// </summary>
        /// <param name="modUid">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool InitializeLoadGameMod(string modPackageName);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        bool UnInitializeLoadGameMod(int modUid);
        /// <summary>
        /// 卸载模组包
        /// </summary>
        /// <param name="modUid">模组包包名</param>
        /// <returns>返回操作是否成功</returns>
        bool UnInitializeLoadGameMod(string modPackageName);

        /// <summary>
        /// 执行模组包代码
        /// </summary>
        /// <param name="modUid">模组包UID</param>
        /// <returns>返回操作是否成功</returns>
        bool RunLoadGameMod(int modUid);

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
        void UnLoadNotUsedMod();
    }
}
