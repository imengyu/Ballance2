using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using Ballance2.Utils;
using Ballance2.CoreBridge;
using Ballance2.Config;
using Ballance2.ModBase;
using Ballance2.GameCore;
using Ballance2.Interfaces;

namespace Ballance2.Managers
{
    /// <summary>
    /// 游戏初始化管理器
    /// </summary>
    public class GameInit : BaseManager
    {
        public const string TAG = "GameInit";

        public GameInit() : base("core.gameinit", TAG, "Singleton")
        {
        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    LoadGameInitBase();
                    StartCoroutine(LoadGameInitUI());
                    InitSettings();
                    InitVideoSettings();
                    return false;
                });
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_TAKE_OVER_CONTROL);
            GameManager.GameMediator.RegisterAction(GameActionNames.CoreActions["ACTION_GAME_INIT"], TAG, (param) => {
                StartCoroutine(GameInitCore());
                return GameActionCallResult.SuccessResult;
            }, null);
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private IDebugManager DebugManager;
        private IModManager ModManager;
        private ISoundManager SoundManager;

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
            ModManager = (IModManager)GameManager.GetManager("ModManager");
            SoundManager = (ISoundManager)GameManager.GetManager("SoundManager");
            DebugManager = (IDebugManager)GameManager.GetManager("DebugManager");

            IntroAudio = SoundManager.RegisterSoundPlayer(GameSoundType.UI, GameManager.FindStaticAssets<AudioClip>("IntroMusic"));

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);
        }
        private IEnumerator LoadGameInitUI()
        {
            int modUid = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "core.gameinit.ballance"), false);
            gameInitMod = ModManager.FindGameMod(modUid);
            yield return StartCoroutine(gameInitMod.LoadInternal());

            if (gameInitMod.LoadStatus != GameModStatus.InitializeSuccess)
            {
                GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载 GameInit 资源包失败 ");
                StopAllCoroutines();
            }

            gameInitUI = GameManager.UIManager.InitViewToCanvas(gameInitMod.GetPrefabAsset("UIGameInit.prefab"), "GameInitUI").gameObject;

            GameManager.UIManager.MaskBlackSet(false);

            UIProgress = gameInitUI.transform.Find("UIProgress").GetComponent<RectTransform>();
            UIProgressValue = gameInitUI.transform.Find("UIProgress/UIValue").GetComponent<RectTransform>();
            UIProgressText = gameInitUI.transform.Find("Text").GetComponent<Text>();
            IntroAnimator = gameInitUI.GetComponent<Animator>();
            TextError = gameInitUI.transform.Find("TextError").GetComponent<Text>();

            loadedGameInitUI = true;
        }

        private LoadMask currentLoadMask = LoadMask.None;

        //加载模块
        private IEnumerator GameInitCore()
        {
            yield return new WaitUntil(IsGameInitUILoaded);

            //播放音乐和动画
            if (GameManager.Mode == GameMode.Game)
            {
                IntroAnimator.Play("IntroAnimation");
                IntroAudio.Play();
            }

            //选择加载包模式
            switch (GameManager.Mode)
            {
                case GameMode.Game: currentLoadMask = LoadMask.Game;  break;
                case GameMode.Level: currentLoadMask = LoadMask.Level | LoadMask.LevelLoader;  break;
                case GameMode.LevelEditor: currentLoadMask = LoadMask.LevelEditor | LoadMask.Level;  break;
                case GameMode.MinimumDebug: currentLoadMask = LoadMask.GameBase; break;
                case GameMode.LoaderDebug: currentLoadMask = LoadMask.Level | LoadMask.LevelLoader; break;
                case GameMode.CoreDebug: currentLoadMask = LoadMask.GameCore;  break;
            }

            UIProgressText.text = "Loading";

            //加载 core.gameinit.txt 获得要加载的模块
            string gameInitTxt = "";
#if UNITY_EDITOR
            TextAsset gameInitEditorAsset = null;
            if (GameManager.AssetsPreferEditor
                && (gameInitEditorAsset =
                UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
                    GamePathManager.DEBUG_MOD_FOLDER + "/core.gameinit.txt")) != null)
            {
                gameInitTxt = gameInitEditorAsset.text;
                GameLogger.Log(TAG, "Load gameinit table in Editor");
            }
#else
            if(false) { }
#endif
            else
            {
                //加载 gameinit
                string gameinit_txt_path = GamePathManager.GetResRealPath("gameinit", "");
                UnityWebRequest request = UnityWebRequest.Get(gameinit_txt_path);
                yield return request.SendWebRequest();

                if (!string.IsNullOrEmpty(request.error))
                {
                    GameErrorManager.ThrowGameError(GameError.GameInitReadFailed, "加载 GameInit.txt  " + gameinit_txt_path + " 时发生错误：" + request.error);
                    yield break;
                }

                gameInitTxt = request.downloadHandler.text;
            }

            //加载包
            yield return StartCoroutine(GameInitPackages(gameInitTxt));
            //加载模组
            yield return StartCoroutine(GameInitUserMods());

            UIProgressText.text = "Loading";

            //加载游戏内核管理器
            GameManager.RegisterManager(typeof(LevelLoader), false);
            GameManager.RegisterManager(typeof(LevelManager), false);
            GameManager.RegisterManager(typeof(ICManager), false);

            BaseManager ballManager = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.FindStaticPrefabs("BallManager"), 
                GameManager.GameRoot.transform).GetComponent<BaseManager>();
            BaseManager camManager = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.FindStaticPrefabs("CamManager"), 
                GameManager.GameRoot.transform).GetComponent<BaseManager>();

            GameManager.RegisterManager(ballManager, false);
            GameManager.RegisterManager(camManager, false);

            //初始化管理器
            GameManager.RequestAllManagerInitialization();

            //正常情况下，等待动画播放完成
            if (GameManager.Mode == GameMode.Game)
                yield return new WaitUntil(IsGameInitAnimPlayend);

            yield return new WaitUntil(GameManager.IsManagerInitFinished);

            //初始化模组启动代码（游戏初始化完成）
            ModManager.ExecuteModEntry(GameModEntryCodeExecutionAt.AtStart);

            yield return new WaitUntil(GameManager.IsManagerInitFinished);

            //分发接管事件
            int hC = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_TAKE_OVER_CONTROL, "*", (VoidDelegate)GameInitContinueInit);
            if (hC == 0) //无接管
                GameInitContinueInit();
        }
        private IEnumerator GameInitPackages(string GameInitTable)
        {
            StringSpliter sp = new StringSpliter(GameInitTable, '\n');
            if (sp.Count >= 1)
            {
                int loadedCount = 0;
                string[] args = null;

                GameLogger.Log(TAG, "Gameinit table : {0}", sp.Count);

                foreach (string ar in sp.Result)
                {
                    if (ar.StartsWith(":")) continue;

                    bool required = false;
                    string packageName = "";
                    LoadMask mask = LoadMask.Game;
                    args = ar.Split(':');

                    if(args.Length >= 3) {
                        required = args[2] == "Required";
                        packageName = args[0];
                        System.Enum.TryParse(args[1], out mask);
                    }

                    //跳过不需要加载的模块
                    if((mask & currentLoadMask) == LoadMask.None)
                        continue;

                    //状态
                    loadedCount++;
                    LoadGameInitUIProgressValue(loadedCount / (float)sp.Count / 2);
                    UIProgressText.text = "Loading " + packageName;

                    //加载
                    int modUid = ModManager.LoadGameModByPackageName(packageName, false);
                    GameMod mod = ModManager.FindGameMod(modUid);
                    //等待加载
                    yield return StartCoroutine(mod.LoadInternal());

                    if (mod.LoadStatus == GameModStatus.InitializeSuccess)
                        continue;
                    else if (required)
                        GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载模块  " + packageName + " 时发生错误");
                    else GameLogger.Warning(TAG, "加载模块  {0} 时发生错误", packageName);
                }

                LoadGameInitUIProgressValue(1);
                UIProgressText.text = "Loading";
            }
        }
        private IEnumerator GameInitUserMods()
        {

            yield break;
        }

        private void GameInitContinueInit()
        {
            //模式
            switch (GameManager.Mode)
            {
                case GameMode.Game: GameInitContinueGoGame(); break;
                case GameMode.LoaderDebug:
                    GameManager.GameMediator.CallAction(GameActionNames.CoreActions["ACTION_DEBUG_LEVEL_LOADER"], Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(false);
                    break;
                case GameMode.CoreDebug:
                    GameManager.GameMediator.CallAction(GameActionNames.CoreActions["ACTION_DEBUG_CORE"], Main.Main.Instance.CoreDebugBase);
                    GameInitHideGameInitUi(false);
                    break;
                case GameMode.Level:
                    GameManager.GameMediator.CallAction(GameActionNames.LevelLoader["LoadLevel"], Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(true);
                    break;
                case GameMode.LevelEditor:
                    LevelEditor levelEditor = (LevelEditor)GameManager.RegisterManager(typeof(LevelEditor), false);
                    levelEditor.StartDebugLevelEditor(Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(true);
                    break;
            }
        }
        private void GameInitHideGameInitUi(bool showBlack)
        {
            GameManager.UIManager.MaskBlackSet(showBlack);
            gameInitUI.SetActive(false);
        }
        private void GameInitContinueGoGame()
        {
            int initEventHandledCount = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH, "*");
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);

            if (initEventHandledCount == 0)
            {
                GameErrorManager.ThrowGameError(GameError.HandlerLost, "未找到 EVENT_GAME_INIT_FINISH 的下一步事件接收器\n此错误出现原因可能是配置不正确");
                GameLogger.Warning(TAG, "None EVENT_GAME_INIT_FINISH handler was found, the game will not continue.");
            }
            else GameInitHideGameInitUi(true);
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
             GameLogger.Log(TAG, "设置已还原默认");
             return true;
         }, 0, "[还原默认设置]");
        }
        private void InitVideoSettings()
        {
            //屏幕大小事件
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_SCREEN_SIZE_CHANGED);

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
            bool fullScreen = GameSettings.GetBool("video.fullScreen", Screen.fullScreen);
            int quality = GameSettings.GetInt("video.quality", QualitySettings.GetQualityLevel());
            int vSync = GameSettings.GetInt("video.vsync", QualitySettings.vSyncCount);

            GameLogger.Log(TAG, "OnVideoSettingsUpdated:\nresolutionsSet: {0}\nfullScreen: {1}" +
                "\nquality: {2}\nvSync : {3}", resolutionsSet, fullScreen, quality, vSync);

            Screen.SetResolution(resolutions[resolutionsSet].width, resolutions[resolutionsSet].height, true);
            Screen.fullScreen = fullScreen;
            Screen.fullScreenMode = fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            QualitySettings.SetQualityLevel(quality, true);
            QualitySettings.vSyncCount = vSync;
            
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_SCREEN_SIZE_CHANGED, "*",
                resolutions[resolutionsSet].width, resolutions[resolutionsSet].height);
            GameManager.UIManager.RequestRelayoutForScreenSizeChaged();

            return true;
        }

        #endregion

        private enum LoadMask
        {
            None = 0,
            GameBase = 0x1,
            MenuLevel = 0x2,
            Level = 0x4,
            LevelLoader = 0x8,
            LevelEditor = 0x10,
            GameCore = 0x20,
            Game = GameBase | MenuLevel| Level| LevelLoader | LevelEditor | GameCore,
        }
    }
}
