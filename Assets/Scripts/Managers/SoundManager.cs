using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.ModBase;
using Ballance2.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Ballance2.Managers
{
    /// <summary>
    /// 声音管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public class SoundManager : BaseManagerBindable
    {
        public const string TAG = "SoundManager";

        public SoundManager() : base(TAG, "Singleton") { }

        private GameSettingsActuator GameSettings = null;

        public override bool InitManager()
        {
            audioSourcePrefab = GameManager.FindStaticPrefabs("AudioSource");
            fastPlayVoices = new Dictionary<string, AudioSource>();

            InitGameAudioMixer();

            GameManager.GameMediator.RegisterEventHandler(
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    GameLogger.Log(TAG, GameEventNames.EVENT_BASE_INIT_FINISHED);
                    ModManager = (ModManager)GameManager.GetManager(ModManager.TAG);
                    return false;
                }
            );

            //设置更新事件
            GameSettings = GameSettingsManager.GetSettings("core");
            GameSettings.RegisterSettingsUpdateCallback("voice", new GameHandler(TAG, OnVoiceSettingsUpdated));
            GameSettings.RequireSettingsLoad("voice");
            return true;
        }
        public override bool ReleaseManager()
        {
            if(null != fastPlayVoices)
            {
                foreach (var v in fastPlayVoices)
                    DestroySoundPlayer(v.Value);
                fastPlayVoices.Clear();
                fastPlayVoices = null;
            }
            if (null != audios)
            {
                for (int i = audios.Count - 1; i >= 0; i--)
                    DestroySoundPlayer(audios[i].Audio);
                audios.Clear();
                audios = null;
            }
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

        #region AudioMixer

        private AudioMixer GameMainAudioMixer;
        private AudioMixer GameUIAudioMixer;

        private AudioMixerGroup GameUIAudioMixerGroupMaster;

        private AudioMixerGroup GameMainAudioMixerGroupMaster;
        private AudioMixerGroup GameMainAudioMixerGroupBallEffect;
        private AudioMixerGroup GameMainAudioMixerGroupModulEffect;
        private AudioMixerGroup GameMainAudioMixerGroupBackgroundMusic;

        private void InitGameAudioMixer()
        {
            GameMainAudioMixer = GameManager.FindStaticAssets<AudioMixer>("GameMainAudioMixer");
            GameUIAudioMixer = GameManager.FindStaticAssets<AudioMixer>("GameUIAudioMixer");

            GameUIAudioMixerGroupMaster = GameUIAudioMixer.FindMatchingGroups("Master")[0];

            GameMainAudioMixerGroupMaster = GameUIAudioMixer.FindMatchingGroups("Master")[0];
            GameMainAudioMixerGroupBallEffect = GameMainAudioMixer.FindMatchingGroups("Master/BallEffect")[0];
            GameMainAudioMixerGroupModulEffect = GameMainAudioMixer.FindMatchingGroups("Master/ModulEffect")[0];
            GameMainAudioMixerGroupBackgroundMusic = GameMainAudioMixer.FindMatchingGroups("Master/BackgroundMusic")[0];
        }

        #endregion

        #region Sound Player

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
                GameErrorManager.LastError = GameError.NotRegistered;
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
            else
            {
                GameLogger.Warning(TAG, "未找到声音文件 {0} ，在模组包 {1}", assets, names[0]);
                GameErrorManager.LastError = GameError.AssetsNotFound;
            }

            return clip;
        }
        /// <summary>
        /// 注册 SoundPlayer
        /// </summary>
        /// <param name="assets">音频资源字符串</param>
        /// <returns></returns>
        public AudioSource RegisterSoundPlayer(GameSoundType type, string assets, bool playOnAwake = false, bool activeStart = true, string name = "")
        {
            AudioClip audioClip = LoadAudioResource(assets);
            if (audioClip == null)
                return null;

            AudioSource audioSource = Instantiate(audioSourcePrefab, gameObject.transform).GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = playOnAwake;
            audioSource.gameObject.name = "AudioSource_" + type + "_" + (name == "" ? GamePathManager.GetFileNameWithoutExt(audioClip.name) : name);

            if (!activeStart)
                audioSource.gameObject.SetActive(false);

            RegisterAudioSource(type, audioSource);
            return audioSource;
        }
        /// <summary>
        /// 注册 SoundPlayer
        /// </summary>
        /// <param name="audioClip">音频源文件</param>
        /// <returns></returns>
        public AudioSource RegisterSoundPlayer(GameSoundType type, AudioClip audioClip, bool playOnAwake = false, bool activeStart = true, string name = "")
        {
            AudioSource audioSource = Instantiate(audioSourcePrefab, gameObject.transform).GetComponent<AudioSource>();
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
                if (audioGlobalControl.Audio.gameObject != null)
                    Destroy(audioGlobalControl.Audio.gameObject);
                return true;
            }
            else
            {
                GameErrorManager.LastError = GameError.NotRegistered;
                return false;
            }

        }

        private void RegisterAudioSource(GameSoundType type, AudioSource audioSource)
        {
            AudioGlobalControl audioGlobalControl = new AudioGlobalControl();
            audioGlobalControl.Audio = audioSource;
            audioGlobalControl.Type = type;

            switch (type)
            {
                case GameSoundType.Background:
                    audioSource.outputAudioMixerGroup = GameMainAudioMixerGroupBackgroundMusic;
                    break;
                case GameSoundType.BallEffect:
                    audioSource.outputAudioMixerGroup = GameMainAudioMixerGroupBallEffect;
                    break;
                case GameSoundType.ModulEffect:
                    audioSource.outputAudioMixerGroup = GameMainAudioMixerGroupModulEffect;
                    break;
                case GameSoundType.Normal:
                    audioSource.outputAudioMixerGroup = GameMainAudioMixerGroupMaster;
                    break;
                case GameSoundType.UI:
                    audioSource.outputAudioMixerGroup = GameUIAudioMixerGroupMaster;
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

        #endregion

        //加载声音设置
        private bool OnVoiceSettingsUpdated(string evtName, params object[] param)
        {
            float volBackground = GameSettings.GetFloat("voice.background", 100);
            float volMain = GameSettings.GetFloat("voice.main", 100);
            float volUI = GameSettings.GetFloat("voice.ui", 100);

            GameUIAudioMixer.SetFloat("UIMasterVolume", volUI <= 1 ? -60 : (20.0f * Mathf.Log10(volUI / 100.0f)));
            GameMainAudioMixer.SetFloat("MasterVolume", volUI <= 1 ? -60 : (20.0f * Mathf.Log10(volMain / 100.0f)));
            GameMainAudioMixer.SetFloat("BackgroundVolume", volUI <= 1 ? -60 : (20.0f * Mathf.Log10(volBackground / 100.0f)));

            return true;
        }

        private Dictionary<string, AudioSource> fastPlayVoices = null;

        /// <summary>
        /// 快速播放一个短声音
        /// </summary>
        /// <param name="soundName">声音资源字符串</param>
        /// <param name="type">声音类型</param>
        /// <returns></returns>
        public bool PlayFastVoice(string soundName, GameSoundType type)
        {
            string key = soundName + "@" + type;
            AudioSource cache = null;
            if(fastPlayVoices.TryGetValue(key, out cache))
            {
                cache.Play();
                return true;
            }
            AudioClip audioClip = LoadAudioResource(soundName);
            if (audioClip == null)
                return false;

            cache = RegisterSoundPlayer(type, audioClip, false, true, key);
            cache.Play();
            return true;
        }

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
