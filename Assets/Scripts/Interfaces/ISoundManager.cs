using UnityEngine;

namespace Ballance2.Interfaces
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 声音管理器
    /// </summary>
    public interface ISoundManager
    {
        /// <summary>
        /// 加载模组包中的音乐资源
        /// </summary>
        /// <param name="assets">资源路径（模组包:音乐路径）</param>
        /// <returns></returns>
        AudioClip LoadAudioResource(string assets);
        /// <summary>
        /// 注册 SoundPlayer
        /// </summary>
        /// <param name="assets">音频资源字符串</param>
        /// <returns></returns>
        AudioSource RegisterSoundPlayer(GameSoundType type, string assets, bool playOnAwake = false, bool activeStart = true, string name = "");
        /// <summary>
        /// 注册 SoundPlayer
        /// </summary>
        /// <param name="audioClip">音频源文件</param>
        /// <returns></returns>
        AudioSource RegisterSoundPlayer(GameSoundType type, AudioClip audioClip, bool playOnAwake = false, bool activeStart = true, string name = "");
        /// <summary>
        /// 注册已有 SoundPlayer
        /// </summary>
        /// <param name="type">ui/normal/background</param>
        /// <param name="audioSource">AudioSource</param>
        /// <returns></returns>
        AudioSource RegisterSoundPlayer(GameSoundType type, AudioSource audioSource);
        /// <summary>
        /// 检查指定 SoundPlayer 是否注册
        /// </summary>
        /// <param name="audioSource">AudioSource</param>
        /// <returns></returns>
        bool IsSoundPlayerRegistered(AudioSource audioSource);
        /// <summary>
        /// 销毁 SoundPlayer
        /// </summary>
        /// <param name="assets">SoundPlayer</param>
        bool DestroySoundPlayer(AudioSource assets);

        /// <summary>
        /// 快速播放一个短声音
        /// </summary>
        /// <param name="soundName">声音资源字符串</param>
        /// <param name="type">声音类型</param>
        /// <returns></returns>
        bool PlayFastVoice(string soundName, GameSoundType type);
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 指定声音类型
    /// </summary>
    public enum GameSoundType
    {
        /// <summary>
        /// 普通声音
        /// </summary>
        Normal,
        /// <summary>
        /// 游戏音效 关于球的
        /// </summary>
        BallEffect,
        /// <summary>
        /// 游戏音效 关于模组的
        /// </summary>
        ModulEffect,
        /// <summary>
        /// UI 发出的声音
        /// </summary>
        UI,
        /// <summary>
        /// 背景音乐
        /// </summary>
        Background,
    }
}
