﻿using Ballance2.CoreBridge;
using System.Collections.Generic;
using UnityEngine;

/*
* Copyright (c) 2020  mengyu
* 
* 模块名：     
* GameSettingsManager.cs
* 
* 用途：
* 游戏设置管理器，管理整个游戏的设置
* 
* 通过 GameSettingsManager.GetSettings("com.your.packagename") 获取设置执行器
* 设置执行器可调用 RegisterSettingsUpdateCallback 注册设置更改信息
* UI通过 NotifySettingsUpdate 通知设置更改信息
* 
* 作者：
* mengyu
* 
* 更改历史：
* 2020-1-12 创建
* 
*/

namespace Ballance2.Config
{
    /// <summary>
    /// 设置管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameSettingsManager
    {
        private const string TAG = "GameSettingsManager";

        private static Dictionary<string, GameSettingsActuator> settingsActuators = null;

        /// <summary>
        /// 获取设置执行器
        /// </summary>
        /// <param name="packageName">设置所使用包名</param>
        /// <returns></returns>
        public static GameSettingsActuator GetSettings(string packageName)
        {
            GameSettingsActuator gameSettingsActuator = null;
            if (!settingsActuators.TryGetValue(packageName, out gameSettingsActuator))
            {
                gameSettingsActuator = new GameSettingsActuator(packageName);
                settingsActuators.Add(packageName, gameSettingsActuator);
            }
            return gameSettingsActuator;
        }
        /// <summary>
        /// 还原默认设置
        /// </summary>
        public static void ResetDefaultSettings()
        {
            PlayerPrefs.DeleteAll();
        }

        internal static void Init()
        {
            settingsActuators = new Dictionary<string, GameSettingsActuator>();
        }
        internal static void Destroy()
        {
            foreach(var key in settingsActuators.Keys)
                settingsActuators[key].Destroy();
            settingsActuators.Clear();
            settingsActuators = null;
        }
    }

    /// <summary>
    /// 设置执行器
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameSettingsActuator
    {
        private const string TAG = "GameSettingsActuator";
        private string basePackName = "unknow";

        /// <summary>
        /// 创建设置执行器
        /// </summary>
        /// <param name="packageName">设置所在包名</param>
        public GameSettingsActuator(string packageName)
        {
            basePackName = packageName;
        }

        public virtual void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(basePackName + "." + key, value);
        }
        public virtual int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(basePackName + "." + key, defaultValue);
        }

        public virtual void SetString(string key, string value)
        {
            PlayerPrefs.SetString(basePackName + "." + key, value);
        }
        public virtual string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(basePackName + "." + key, defaultValue);
        }

        public virtual void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(basePackName + "." + key, value);
        }
        public virtual float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(basePackName + "." + key, defaultValue);
        }

        public virtual void SetBool(string key, bool value)
        {
            PlayerPrefs.SetString(basePackName + "." + key, value.ToString());
        }
        public virtual bool GetBool(string key, bool defaultValue = false)
        {
            return bool.Parse(PlayerPrefs.GetString(basePackName + "." + key, defaultValue.ToString()));
        }

        private List<SettingUpdateCallbackData> settingUpdateCallbacks = new List<SettingUpdateCallbackData>();
        private struct SettingUpdateCallbackData
        {
            public string groupName;
            public GameHandler handler;

            public SettingUpdateCallbackData(string groupName, GameHandler handler)
            {
                this.groupName = groupName;
                this.handler = handler;
            }
        }

        internal void Destroy()
        {
            if (settingUpdateCallbacks != null)
            {
                foreach (var v in settingUpdateCallbacks)
                    v.handler.Dispose();
                settingUpdateCallbacks.Clear();
                settingUpdateCallbacks = null;
            }
        }

        /// <summary>
        /// 通知设置组加载更新
        /// </summary>
        /// <param name="groupName">组名称</param>
        public void RequireSettingsLoad(string groupName)
        {
            foreach (var d in settingUpdateCallbacks)
                if (d.groupName == groupName)
                    if (d.handler.CallEventHandler("SettingsLoad")) break;
        }
        /// <summary>
        /// 通知设置组更新
        /// </summary>
        /// <param name="groupName">组名称</param>
        public void NotifySettingsUpdate(string groupName)
        {
            foreach (var d in settingUpdateCallbacks)
                if (d.groupName == groupName)
                    d.handler.CallEventHandler("SettingsUpdate");
        }
        /// <summary>
        /// 注册设置组更新回调
        /// </summary>
        /// <param name="groupName">组名称</param>
        /// <param name="handler">回调</param>
        public void RegisterSettingsUpdateCallback(string groupName, GameHandler handler)
        {
            settingUpdateCallbacks.Add(new SettingUpdateCallbackData(groupName, handler));
        }
    }
}
