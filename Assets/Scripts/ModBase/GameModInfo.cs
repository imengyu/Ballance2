namespace Ballance2.ModBase
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组加载状态
    /// </summary>
    public enum GameModStatus
    {
        /// <summary>
        /// 已注册但未初始化
        /// </summary>
        NotInitialize,
        /// <summary>
        /// 初始化成功
        /// </summary>
        InitializeSuccess,
        /// <summary>
        /// 正在加载
        /// </summary>
        Loading,
        /// <summary>
        /// 初始化失败
        /// </summary>
        InitializeFailed,
        /// <summary>
        /// 版本不兼容的模组
        /// </summary>
        BadMod,
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组类型
    /// </summary>
    public enum GameModType
    {
        NotSet,
        /// <summary>
        /// 仅资源包
        /// </summary>
        AssetPack,
        /// <summary>
        /// 代码模组和资源包
        /// </summary>
        ModPack,
        /// <summary>
        /// 关卡文件
        /// </summary>
        Level,
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组启动代码执行时机
    /// </summary>
    public enum GameModEntryCodeExecutionAt
    {
        /// <summary>
        /// 手动
        /// </summary>
        Manual,
        /// <summary>
        /// 模组加载完毕后
        /// </summary>
        AfterLoaded,
        /// <summary>
        /// 游戏加载完毕时
        /// </summary>
        AtStart,
        /// <summary>
        /// 关卡加载时
        /// </summary>
        AtLevelLoading,
        /// <summary>
        /// 关卡加载完成时
        /// </summary>
        AfterLevelLoad,
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组代码类型
    /// </summary>
    public enum GameModCodeType
    {
        /// <summary>
        /// Lua 脚本（默认）
        /// </summary>
        Lua,
        /// <summary>
        /// C# dll （仅momo可用，il2cpp使用此模块会报错）
        /// </summary>
        CSharp,
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组信息
    /// </summary>
    public struct GameModInfo
    {
        public GameModInfo(string name)
        {
            Name = name;
            Author = "未知";
            Introduction = "未填写介绍";
            Logo = "";
            Version = "未知";
        }

        public string Name;
        public string Author;
        public string Introduction;
        public string Logo;
        public string Version;
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组适配信息
    /// </summary>
    public struct GameCompatibilityInfo
    {
        public GameCompatibilityInfo(int mi, int t, int ma)
        {
            MinVersion = mi;
            TargetVersion = t;
            MaxVersion = ma;
        }

        public int MinVersion;
        public int TargetVersion;
        public int MaxVersion;
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组依赖信息
    /// </summary>
    public struct GameDependencyInfo
    {
        public GameDependencyInfo(string name, string mi)
        {
            MinVersion = mi;
            PackageName = name;
            Loaded = false;
            MustLoad = false;
        }

        public string PackageName;
        public string MinVersion;
        public bool Loaded;
        public bool MustLoad;
    }
}
