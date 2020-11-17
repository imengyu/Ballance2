﻿using Ballance2.Utils;
using System;
using UnityEngine;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameAction.cs
 * 用途：
 * 全局操作类定义
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 全局操作
    /// </summary>
    [SLua.CustomLuaClass]
    [Serializable]
    public class GameAction
    {
        /// <summary>
        /// 创建全局操作
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="gameHandler">操作接收器</param>
        /// <param name="callTypeCheck">操作调用参数检查</param>
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
        /// 空操作
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
        /// <returns>操作调用结果</returns>
        public static GameActionCallResult CreateActionCallResult(bool success, object[] returnParams = null)
        {
            return new GameActionCallResult(success, returnParams);
        }
        /// <summary>
        /// 创建操作调用结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="returnParams">返回的数据</param>
        public GameActionCallResult(bool success, object[] returnParams)
        {
            Success = success;
            ReturnParams = LuaUtils.LuaTableArrayToObjectArray(returnParams);
        }

        /// <summary>
        /// 获取是否成功
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 获取操作返回的数据
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
