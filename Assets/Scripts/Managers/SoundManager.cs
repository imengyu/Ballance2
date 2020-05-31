using Ballance2.Managers.CoreBridge;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Ballance2.Managers
{
    [SLua.CustomLuaClass]
    public class SoundManager : BaseManagerBindable
    {
        public const string TAG = "SoundManager";

        public SoundManager() : base(TAG, "Singleton") { }

        public override bool InitManager()
        {
            audioSourcePrefab = GameManager.FindStaticPrefabs("AudioSource");
            BackgroundAudioMixer = GameManager.FindStaticAssets<AudioMixer>("BackgroundAudioMixer");
            GameMainAudioMixer = GameManager.FindStaticAssets<AudioMixer>("GameMainAudioMixer");
            GameUIAudioMixer = GameManager.FindStaticAssets<AudioMixer>("GameUIAudioMixer");
            GameManager.GameMediator.RegisterEventKernalHandler(
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    GameLogger.Log(TAG, GameEventNames.EVENT_BASE_INIT_FINISHED);
                    ModManager = (ModManager)GameManager.GetManager(ModManager.TAG);
                    return false;
                }
            );
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private ModManager ModManager;

        private List<AudioGlobalControl> audios = new List<AudioGlobalControl>();
        private GameObject audioSourcePrefab = null;
        private class AudioGlobalControl
        {
            public AudioSource Audio;
            public GameSoundType Type;
        }

        private AudioMixer BackgroundAudioMixer;
        private AudioMixer GameMainAudioMixer;
        private AudioMixer GameUIAudioMixer;

        /// <summary>
        /// 加载模组包中的音乐资源
        /// </summary>
        /// <param name="assets">资源路径（模组包:音乐路径）</param>
        /// <returns></returns>
        public AudioClip LoadAudioResource(string assets)
        {
            string[] names = assets.Split(':');

            GameMod mod = ModManager.FindGameModByAssetStr(names[0]);
            if (mod == null)
            {
                GameLogger.Warning(TAG, "无法加载声音文件 {0} ，因为未找到模组包 {1}", assets, names[0]);
                GameErrorManager.LastError = GameError.Unregistered;
                return null;
            }
            if (mod.LoadStatus != GameModStatus.InitializeSuccess)
            {
                GameLogger.Warning(TAG, "无法加载声音文件 {0} ，因为模组包 {1} ({2}) 未初始化", assets, names[0], mod.Uid);
                GameErrorManager.LastError = GameError.NotInitialize;
                return null;
            }

            AudioClip clip = mod.GetAsset<AudioClip>(names[1]);
            if (clip != null) clip.name = GamePathManager.GetFileNameWithoutExt(names[1]);

            return clip;
        }
        /// <summary>
        /// 注册 SoundPlayer
        /// </summary>
        /// <param name="audioClip">音频源文件</param>
        /// <returns></returns>
        public AudioSource RegisterSoundPlayer(GameSoundType type, AudioClip audioClip, bool playOnAwake = false, bool activeStart = true, string name = "")
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab, gameObject.transform).AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = playOnAwake;
            audioSource.gameObject.name = "AudioSource_" + type + "_" + (name == "" ? GamePathManager.GetFileNameWithoutExt(audioClip.name) : name);

            if (!activeStart)
                audioSource.gameObject.SetActive(false);

            RegisterAudioSource(type, audioSource);
            return audioSource;
        }
        /// <summary>
        /// 注册已有 SoundPlayer
        /// </summary>
        /// <param name="type">ui/normal/background</param>
        /// <param name="audioSource">AudioSource</param>
        /// <returns></returns>
        public AudioSource RegisterSoundPlayer(GameSoundType type, AudioSource audioSource)
        {
            if (!IsSoundPlayerRegistered(audioSource)) 
                RegisterAudioSource(type, audioSource);
            else GameErrorManager.LastError = GameError.AlredayRegistered;
            return audioSource;
        }
        /// <summary>
        /// 检查指定 SoundPlayer 是否注册
        /// </summary>
        /// <param name="audioSource">AudioSource</param>
        /// <returns></returns>
        public bool IsSoundPlayerRegistered(AudioSource audioSource)
        {
            foreach (AudioGlobalControl a in audios)
            {
                if (a.Audio == audioSource)
                    return true;
            }
            return false;
        }

        private void RegisterAudioSource(GameSoundType type, AudioSource audioSource)
        {
            AudioGlobalControl audioGlobalControl = new AudioGlobalControl();
            audioGlobalControl.Audio = audioSource;
            audioGlobalControl.Type = type;

            switch (type)
            {
                case GameSoundType.Background:
                    audioSource.outputAudioMixerGroup = BackgroundAudioMixer.outputAudioMixerGroup;
                    break;
                case GameSoundType.GameEffect:
                    break;
                case GameSoundType.Normal:
                    audioSource.outputAudioMixerGroup = GameMainAudioMixer.outputAudioMixerGroup;
                    break;
                case GameSoundType.UI:
                    audioSource.outputAudioMixerGroup = GameUIAudioMixer.outputAudioMixerGroup;
                    break;
            }


            audios.Add(audioGlobalControl);
        }
        private bool IsSoundPlayerRegistered(AudioSource audioSource, out AudioGlobalControl audioGlobalControl)
        {
            foreach (AudioGlobalControl a in audios)
            {
                if (a.Audio == audioSource)
                {
                    audioGlobalControl = a;
                    return true;
                }
            }
            audioGlobalControl = null;
            return false;
        }

        /// <summary>
        /// 销毁 SoundPlayer
        /// </summary>
        /// <param name="assets">SoundPlayer</param>
        public bool DestroySoundPlayer(AudioSource assets)
        {
            AudioGlobalControl audioGlobalControl = null;
            if (IsSoundPlayerRegistered(assets, out audioGlobalControl))
            {
                audios.Remove(audioGlobalControl);
                Destroy(audioGlobalControl.Audio.gameObject);
                Destroy(audioGlobalControl.Audio);
                return true;
            }
            else
            {
                GameErrorManager.LastError = GameError.Unregistered;
                return false;
            }

        }


    }

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
        /// 游戏音效
        /// </summary>
        GameEffect,
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
