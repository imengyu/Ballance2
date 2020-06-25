using UnityEngine;

namespace Ballance2.Config
{
    /// <summary>
    /// 设置管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GameSettingsManager
    {
        /// <summary>
        /// 获取设置
        /// </summary>
        /// <param name="packageName">当前包名</param>
        /// <returns></returns>
        public static GameSettingsActuator GetSettings(string packageName)
        {
            return new GameSettingsActuator(packageName);
        }
    }

    [SLua.CustomLuaClass]
    public class GameSettingsActuator
    {
        private string basePackName = "unknow";

        public GameSettingsActuator(string packageName)
        {
            basePackName = packageName;
        }

        public void SetFloat(string key, int value)
        {
            PlayerPrefs.SetInt(basePackName + "." + key, value);
        }
        public int GetFloat(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(basePackName + "." + key, defaultValue);
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(basePackName + "." + key, value);
        }
        public string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(basePackName + "." + key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(basePackName + "." + key, value);
        }
        public float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(basePackName + "." + key, defaultValue);
        }

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetString(basePackName + "." + key, value.ToString());
        }
        public bool GetBool(string key, bool defaultValue = false)
        {
            return bool.Parse(PlayerPrefs.GetString(basePackName + "." + key, defaultValue.ToString()));
        }
    }
}
