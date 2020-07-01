using Ballance2.CoreBridge;
using UnityEngine;

namespace Ballance2.Interfaces
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 调试管理器接口
    /// </summary>
    public interface IDebugManager
    {
        /// <summary>
        /// 添加调试工具菜单
        /// </summary>
        /// <param name="text">文字</param>
        /// <param name="callbackHandler">点击回调</param>
        void AddCustomDebugToolItem(string text, GameHandler callbackHandler);

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        /// <param name="title">错误对话框标题</param>
        /// <param name="message">错误字符串</param>
        /// <param name="type">错误类型</param>
        void ShowExceptionDialog(string title, string message, LogType type);

        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="cmd">命令字符串</param>
        /// <returns>返回是否成功</returns>
        bool RunCommand(string cmd);

        /*
        * 注册命令说明
        * 
        * 支持在LUA端直接注册控制台指令，你可以用于模块调试
        * eg: 
        *     local DebugManager = Slua.As(DebugManager, GameManager:GetManager("DebugManager"))
        *     DebugManager:RegisterCommand("test", "TestModul:Modul:CmdTestHandler", 1, "测试指令帮助文字")
        * 命令处理器 回调格式
        *    func(keyword, argCount, stringList)
        *         keyword 用户输入的命令单词
        *         argCount 用户输入的参数个数（您也可以通过RegisterCommand中的limitArgCount参数指定最低参数个数）
        *         stringList 参数数组，string[] 类型
        * 
        */

        /// <summary>
        /// 注册命令 (lua使用)
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <param name="handler">命令处理器。命令处理器格式与通用格式相同。LUA接受函数须满足定义：func(keyword, argCount, stringList)</param>
        /// <param name="limitArgCount">命令最低参数，默认 0 表示无参数或不限制</param>
        /// <param name="helpText">命令帮助文字</param>
        /// <returns>成功返回命令ID，不成功返回-1</returns>
        int RegisterCommand(string keyword, string handler, int limitArgCount, string helpText);
        /// <summary>
        /// 注册命令（delegate）
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <param name="kernelCallback">命令回调</param>
        /// <param name="limitArgCount">命令最低参数，默认 0 表示无参数或不限制</param>
        /// <param name="helpText">命令帮助文字</param>
        /// <returns>成功返回命令ID，不成功返回-1</returns>
        /// <returns></returns>
        int RegisterCommand(string keyword, CommandDelegate kernelCallback, int limitArgCount, string helpText);
        /// <summary>
        /// 取消注册命令
        /// </summary>
        /// <param name="cmdId">命令ID</param>
        void UnRegisterCommand(string keyword);
        /// <summary>
        /// 获取命令是否注册
        /// </summary>
        /// <param name="keyword">命令单词</param>
        /// <returns></returns>
        bool IsCommandRegistered(string keyword);
    }

    /// <summary>
    /// 调试命令回调
    /// </summary>
    /// <param name="keyword">命令单词</param>
    /// <param name="fullCmd">完整命令</param>
    /// <param name="args">命令参数</param>
    /// <returns></returns>
    [SLua.CustomLuaClass]
    public delegate bool CommandDelegate(string keyword, string fullCmd, string[] args);
}
