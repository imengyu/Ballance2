﻿namespace Ballance2.ModBase
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组加载状态
    /// </summary>
    public enum GameModStatus
    {
        /// <summary>
        /// 已经释放
        /// </summary>
        Destroyed,
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
    public enum GameModFileType
    {
        NotSet,
        AssetBundle,
        BallanceZipPack,
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 模组类型
    /// </summary>
    public enum GameModType
    {
        NotSet,
        /// <summary>
        /// 资源包，提供资源给其他代码模块调用
        /// 不包含代码（如果包中没有代码，可设置为资源包，可节省初始化环境的开销）
        /// </summary>
        AssetPack,
        /// <summary>
        /// 游戏代码模块模组。包含完整代码运行环境。
        /// </summary>
        ModulePack,
        /// <summary>
        /// 关卡包
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
