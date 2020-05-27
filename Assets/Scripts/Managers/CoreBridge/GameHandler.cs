namespace Ballance2.Managers.CoreBridge
{
    /// <summary>
    /// 游戏通用接收器
    /// </summary>
    public class GameHandler
    {
        /// <summary>
        /// 创建游戏内部使用 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="gameHandlerDelegate">回调</param>
        public GameHandler(string name, GameHandlerDelegate gameHandlerDelegate)
        {
            Name = name;
            CSKernelHandler = gameHandlerDelegate;
            Type = GameHandlerType.CSKernel;
        }

        /// <summary>
        /// 创建 LUA 层使用的 Handler
        /// </summary>
        /// <param name="name">接收器名称</param>
        /// <param name="luaModulHandler">LUA Handler （格式：模块名:Modul/lua虚拟脚本名字:主模块函数名称）</param>
        public GameHandler(string name, string luaModulHandler)
        {
            Name = name;
            Type = GameHandlerType.LuaModul;
            LuaModulHandler = luaModulHandler;
            LuaModulHandlerFunc = new GameLuaHandler(luaModulHandler);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            CSKernelHandler = null;
            LuaModulHandlerFunc = null;
        }

        /// <summary>
        /// 调用接收器
        /// </summary>
        /// <param name="evtName">事件名称</param>
        /// <param name="pararms">参数</param>
        /// <returns>返回是否中断剩余事件分发/返回Action是否成功</returns>
        public bool Call(string evtName, params object[] pararms)
        {
            bool result = false;
            if (Type == GameHandlerType.CSKernel)
                result = CSKernelHandler(evtName, pararms);
            else if (Type == GameHandlerType.LuaModul)
                result = LuaModulHandlerFunc.RunEventHandler(evtName, pararms);
            return result;
        }

        /// <summary>
        /// 接收器名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 接收器类型
        /// </summary>
        public GameHandlerType Type { get; private set; }
        /// <summary>
        /// KernelHandler
        /// </summary>
        public GameHandlerDelegate CSKernelHandler { get; private set; }
        /// <summary>
        /// LUA Handler
        /// </summary>
        public string LuaModulHandler { get; private set; }
        /// <summary>
        /// LUA Handler 执行体接收器
        /// </summary>
        public GameLuaHandler LuaModulHandlerFunc { get; private set; }

        public override string ToString()
        {
            return "[" + Type + "] " + Name + (string.IsNullOrEmpty(LuaModulHandler) ?  "" :  (":" + LuaModulHandler));
        }
    }

    /// <summary>
    /// 通用接收器类型
    /// </summary>
    public enum GameHandlerType
    {
        /// <summary>
        /// C# 内核模块使用的
        /// </summary>
        CSKernel,
        /// <summary>
        /// Lua 模块使用的
        /// </summary>
        LuaModul,
    }

    /// <summary>
    /// 接收器内核回调
    /// </summary>
    /// <param name="evtName">事件名称</param>
    /// <param name="pararms">参数</param>
    /// <returns>返回是否中断其他事件的分发</returns>
    public delegate bool GameHandlerDelegate(string evtName, params object[] pararms);
}
