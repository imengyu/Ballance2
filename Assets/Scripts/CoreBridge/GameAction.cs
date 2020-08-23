using Ballance2.Utils;
using System;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 全局操作
    /// </summary>
    [SLua.CustomLuaClass]
    [Serializable]
    public class GameAction
    {
        public GameAction(string name, GameHandler gameHandler, string[] callTypeCheck)
        {
            _Name = name;
            _GameHandler = gameHandler;
            _CallTypeCheck = callTypeCheck;
        }

        [SerializeField, SetProperty("Name")]
        private string _Name;
        [SerializeField, SetProperty("GameHandler")]
        private GameHandler _GameHandler;
        [SerializeField, SetProperty("CallTypeCheck")]
        private string[] _CallTypeCheck;

        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get { return _Name; } }
        /// <summary>
        /// 操作接收器
        /// </summary>
        public GameHandler GameHandler { get { return _GameHandler; } }
        /// <summary>
        /// 操作类型检查
        /// </summary>
        public string[] CallTypeCheck { get { return _CallTypeCheck; } }

        /// <summary>
        /// 空
        /// </summary>
        public static GameAction Empty { get; } = new GameAction("internal.empty", null, null);

        public void Dispose()
        {
            _CallTypeCheck = null;
            _GameHandler.Dispose();
            _GameHandler = null;
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
}
