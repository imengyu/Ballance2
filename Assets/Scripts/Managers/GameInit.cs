using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ballance2.Utils;
using Ballance2.Managers.CoreBridge;
using Ballance2.Config;
using Ballance2.Managers.ModBase;

namespace Ballance2.Managers
{
    /// <summary>
    /// 游戏初始化管理器
    /// </summary>
    public class GameInit : BaseManagerBindable
    {
        public const string TAG = "GameInit";

        public GameInit() : base(TAG, "Singleton")
        {
        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterEventHandler(
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    LoadGameInitBase();
                    StartCoroutine(LoadGameInitUI());
                    InitSettings();
                    InitVideoSettings();
                    return false;
                });
            GameManager.GameMediator.RegisterEventHandler(
                GameEventNames.EVENT_GAME_INIT_ENTRY, TAG, (e, p) =>
                { StartCoroutine(GameInitModuls()); return false; });
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private DebugManager DebugManager;
        private ModManager ModManager;
        private SoundManager SoundManager;

        #region GameInit Control

        private GameMod gameInitMod;
        private GameObject gameInitUI;

        private RectTransform UIProgress;
        private RectTransform UIProgressValue;
        private Text UIProgressText;
        private Text TextError;
        private Animator IntroAnimator;
        private AudioSource IntroAudio;

        private bool loadedGameInitUI = false;
        private bool IsGameInitUILoaded() { return loadedGameInitUI; }
        private bool IsGameInitAnimPlayend() { return IntroAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; }

        private void LoadGameInitUIProgressValue(float val)
        {
            UIProgressValue.sizeDelta = new Vector2(val * UIProgress.sizeDelta.x, UIProgressValue.sizeDelta.y);
        }
        private void LoadGameInitBase()
        {
            ModManager = (ModManager)GameManager.GetManager(ModManager.TAG);
            SoundManager = (SoundManager)GameManager.GetManager(SoundManager.TAG);
            DebugManager = (DebugManager)GameManager.GetManager(DebugManager.TAG);

            IntroAudio = SoundManager.RegisterSoundPlayer(GameSoundType.UI, GameManager.FindStaticAssets<AudioClip>("IntroMusic"));
        }
        private IEnumerator LoadGameInitUI()
        {
            int modUid = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "core.gameinit.ballance"), false);
            gameInitMod = ModManager.FindGameMod(modUid);
            yield return StartCoroutine(gameInitMod.LoadInternal());

            if (gameInitMod.LoadStatus !=  GameModStatus.InitializeSuccess)
            {
                GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载 GameInit 资源包失败 ");
                StopAllCoroutines();
            }

            gameInitUI = GameManager.UIManager.InitViewToCanvas(gameInitMod.GetPrefabAsset("Assets/Mods/GameInit/UIGameInit.prefab"), "GameInitUI").gameObject;

            GameManager.UIManager.MaskBlackSet(false);

            UIProgress = gameInitUI.transform.Find("UIProgress").GetComponent<RectTransform>();
            UIProgressValue = gameInitUI.transform.Find("UIProgress/UIValue").GetComponent<RectTransform>();
            UIProgressText = gameInitUI.transform.Find("Text").GetComponent<Text>();
            IntroAnimator = gameInitUI.GetComponent<Animator>();
            TextError = gameInitUI.transform.Find("TextError").GetComponent<Text>();

            loadedGameInitUI = true;
        }
        private IEnumerator GameInitModuls()
        {
            yield return new WaitUntil(IsGameInitUILoaded);

            //播放音乐
            IntroAudio.Play();

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);
            ModManager.RegisterOnUpdateCurrentLoadingModCallback((m) =>
            {
                LoadGameInitUIProgressValue((ModManager.GetLoadedModCount() + 1) / 
                    (float)ModManager.GetAllModCount());
                UIProgressText.text = m.PackagePath;
            });
            
            UIProgressText.text = "Loading";

            int modUid = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "core.gamepackages.ballance"), false);
            GameMod gamePackagesMod = ModManager.FindGameMod(modUid);
            yield return StartCoroutine(gamePackagesMod.LoadInternal());

            if (gamePackagesMod.LoadStatus != GameModStatus.InitializeSuccess)
            {
                GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载 GamePackages 资源包失败 ");
                StopAllCoroutines();
            }

            yield return new WaitUntil(IsGameInitAnimPlayend);

            //初始化模组启动代码（游戏初始化完成）
            ModManager.ExecuteModEntryCodeAtStart();

            int initEventHandledCount = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH, "*");
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);
           
            if (initEventHandledCount == 0)
            {
                GameErrorManager.ThrowGameError(GameError.HandlerLost, "未找到 EVENT_GAME_INIT_FINISH 的下一步事件接收器\n此错误出现原因可能是配置不正确");
                GameLogger.Warning(TAG, "None EVENT_GAME_INIT_FINISH handler was found, the game will not continue.");
            }
            else
            {
                GameManager.UIManager.MaskBlackSet(true);
                gameInitUI.SetActive(false);
            }
        }

        #endregion

        #region Base Settings Control

        private GameSettingsActuator GameSettings = null;
        private Resolution[] resolutions = null;
        private int defaultResolution = 0;

        private void InitSettings()
        {
            DebugManager.RegisterCommand("restsettings", (string keyword, string fullCmd, string[] args) =>
            {
                GameSettingsManager.ResetDefaultSettings();
                return true;
            }, 0, "[还原默认设置]");
        }
        private void InitVideoSettings()
        {
            resolutions = Screen.resolutions;

            for (int i = 0; i < resolutions.Length; i++)
                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                {
                    defaultResolution = i;
                    break;
                }

            //设置更新事件
            GameSettings = GameSettingsManager.GetSettings("core");
            GameSettings.RegisterSettingsUpdateCallback("video", new GameHandler(TAG, OnVideoSettingsUpdated));
            GameSettings.RequireSettingsLoad("video");
        }

        private bool OnVideoSettingsUpdated(string evtName, params object[] param)
        {
            int resolutionsSet = GameSettings.GetInt("video.resolution", defaultResolution);
            bool fullScreen = GameSettings.GetBool("video.fullScreen", false);
            int quality = GameSettings.GetInt("video.quality", 2);
            int vSync = GameSettings.GetInt("video.vsync", 0);

            Screen.SetResolution(resolutions[resolutionsSet].width, resolutions[resolutionsSet].height, true);
            Screen.fullScreen = fullScreen;
            QualitySettings.SetQualityLevel(quality, true);
            QualitySettings.vSyncCount = vSync;

            return true;
        }

        #endregion
    }
}
