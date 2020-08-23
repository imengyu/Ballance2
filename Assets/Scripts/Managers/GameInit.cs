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
using System.IO;

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
            initStore = false;
        }

        protected override bool InitActions(GameActionStore actionStore)
        {
            actionStore.RegisterAction("ACTION_GAME_INIT", TAG, (param) => {
                StartCoroutine(GameInitCore());
                return GameActionCallResult.SuccessResult;
            }, null);
            return base.InitActions(actionStore);
        }
        protected override void InitPre()
        {
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
            {
                LoadGameInitBase();
                StartCoroutine(LoadGameInitUI());
                InitSettings();
                InitVideoSettings();
                return false;
            });
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_ENTER_MENULEVEL);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_TAKE_OVER_CONTROL);
            base.InitPre();
        }
        public override bool InitManager()
        {
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

        private void LoadGameInitBase()
        {
            ModManager = (IModManager)GameManager.GetManager("ModManager");
            SoundManager = (ISoundManager)GameManager.GetManager("SoundManager");
            DebugManager = (IDebugManager)GameManager.GetManager("DebugManager");

            IntroAudio = SoundManager.RegisterSoundPlayer(GameSoundType.UI, GameManager.FindStaticAssets<AudioClip>("IntroMusic"));
        }
        private IEnumerator LoadGameInitUI()
        {
            gameInitMod = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "core.gameinit.ballance"), false);
            gameInitMod.IsModInitByGameinit = true;
            
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

        private GameModRunMask currentLoadMask = GameModRunMask.None;

        //加载主函数
        private IEnumerator GameInitCore()
        {
            GameLogger.Log(TAG, "Gameinit start");

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
                case GameMode.Game: currentLoadMask = GameModRunMask.GameBase; break;
                case GameMode.Level: currentLoadMask = GameModRunMask.Level | GameModRunMask.LevelLoader; break;
                case GameMode.LevelEditor: currentLoadMask = GameModRunMask.LevelEditor | GameModRunMask.Level; break;
                case GameMode.MinimumDebug: currentLoadMask = GameModRunMask.GameBase; break;
                case GameMode.LoaderDebug: currentLoadMask = GameModRunMask.Level | GameModRunMask.LevelLoader; break;
                case GameMode.CoreDebug: currentLoadMask = GameModRunMask.GameCore; break;
            }

            UIProgressText.text = "Loading";

            //加载 core.gameinit.txt 获得要加载的模块
            string gameInitTxt = "";
#if UNITY_EDITOR // 编辑器中直接加载
            TextAsset gameInitEditorAsset = null;
            if (DebugSettings.Instance.GameInitLoadInEditor
                && (gameInitEditorAsset =
                UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
                    GamePathManager.DEBUG_MOD_FOLDER + "/core.gameinit.txt")) != null)
            {
                gameInitTxt = gameInitEditorAsset.text;
                GameLogger.Log(TAG, "Load gameinit table in Editor : \n" + gameInitTxt);
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
            //加载关卡信息
            yield return StartCoroutine(GameInitLevels());

            UIProgressText.text = "Loading";

            //加载游戏内核管理器
            GameManager.RegisterManager(typeof(LevelLoader), false);
            GameManager.RegisterManager(typeof(LevelManager), false);
            GameManager.ICManager = (ICManager)GameManager.RegisterManager(typeof(ICManager), false);

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
            {
                yield return new WaitUntil(IsGameInitAnimPlayend);

                //hide cp
                GameManager.UIManager.UIFadeManager.AddFadeOut(GameObject.Find("GlobalCopyrightText").GetComponent<Text>(), 1.3f, true);

                IntroAnimator.Play("IntroAnimationHide");
                IntroAudio.Stop();

                yield return new WaitForSeconds(0.8f);
            }

            yield return new WaitUntil(GameManager.IsManagersInitFinished);

            //初始化模组启动代码（游戏初始化完成）
            ModManager.ExecuteModEntry(GameModEntryCodeExecutionAt.AtStart);

            yield return new WaitUntil(GameManager.IsManagersInitFinished);

            //分发接管事件
            int hC = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_TAKE_OVER_CONTROL, "*", (BooleanDelegate)GameInitContinueInit);
            if (hC == 0)//无接管
            {
                if(GameInitContinueInit())
                {
                    //正常模式，加载menulevel
                    GameManager.NotifyGameCurrentScenseChanged(GameCurrentScense.MenuLevel);

                    yield return new WaitUntil(ModManager.IsNoneModLoading);

                    int initEventHandledCount = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_ENTER_MENULEVEL, "*");
                    GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_ENTER_MENULEVEL);

                    if (initEventHandledCount == 0)
                    {
                        GameErrorManager.ThrowGameError(GameError.HandlerLost, "未找到 EVENT_ENTER_MENULEVEL 的下一步事件接收器\n此错误出现原因可能是配置不正确");
                        GameLogger.Warning(TAG, "None EVENT_GAME_INIT_FINISH handler was found, the game will not continue.");
                    }
                    else GameInitHideGameInitUi(true);
                }
            }
        }
        //加载GameInit模块
        private IEnumerator GameInitPackages(string GameInitTable)
        {
            StringSpliter sp = new StringSpliter(GameInitTable, '\n');
            if (sp.Count >= 1)
            {
                int loadedCount = 0;
                string[] args;

                GameLogger.Log(TAG, "Gameinit table : {0}", sp.Count);

                foreach (string ar in sp.Result)
                {
                    if (ar.StartsWith(":")) continue;

                    bool required = false;
                    string packageName = "";
                    GameModRunMask mask = GameModRunMask.GameBase;
                    args = ar.Split(':');

                    if (args.Length >= 3)
                    {
                        required = args[2] == "Required";
                        packageName = args[0];
                        System.Enum.TryParse(args[1], out mask);
                    }

                    bool modNeedRun = (mask & currentLoadMask) != GameModRunMask.None;

                    //状态
                    loadedCount++;
                    GameInitSetUIProgressValue(loadedCount / (float)sp.Count * 0.6f);
                    UIProgressText.text = "Loading " + packageName;

                    //加载
                    GameMod mod = ModManager.LoadGameModByPackageName(packageName, modNeedRun);
                    if (mod != null)
                    {
                        mod.IsModInitByGameinit = true;

                        yield return new WaitUntil(mod.IsLoadComplete);
                    }

                    if ((mod == null || (modNeedRun && mod.LoadStatus != GameModStatus.InitializeSuccess)) && required)
                        GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载模块  " + packageName + " 时发生错误");
                }
            }

            UIProgressText.text = "Loading";
            GameInitSetUIProgressValue(0.6f);
        }
        //加载用户模组
        private IEnumerator GameInitUserMods()
        {
            //加载mod文件夹下所有模组
            string modFolderPath = GamePathManager.GetResRealPath("mod", "");
            if (Directory.Exists(modFolderPath))
            {
                DirectoryInfo direction = new DirectoryInfo(modFolderPath);
                FileInfo[] files = direction.GetFiles("*.ballance", SearchOption.TopDirectoryOnly);
                for (int i = 0, c = files.Length; i < c; i++)
                {
                    ModManager.LoadGameMod(GamePathManager.GetResRealPath("mod", files[i].Name), false);

                    GameInitSetUIProgressValue(0.6f + i / (float)c * 0.2f);
                    UIProgressText.text = "Loading " + files[i].Name;
                }
            }

            //根据用户设置启用对应模组
            string[] enableMods = ModManager.GetModEnableStatusList();
            foreach (string packageName in enableMods) 
            {
                GameMod m = ModManager.FindGameMod(packageName);
                if (m != null)
                {
                    ModManager.InitializeLoadGameMod(m);

                    yield return new WaitUntil(m.IsLoadComplete);
                }
            }

            UIProgressText.text = "Loading";
            GameInitSetUIProgressValue(0.8f);
            yield break;
        }
        //加载所有关卡信息
        private IEnumerator GameInitLevels()
        {
            //加载levels文件夹下所有模组
            string levelFolderPath = GamePathManager.GetResRealPath("level", "");
            if (Directory.Exists(levelFolderPath))
            {
                DirectoryInfo direction = new DirectoryInfo(levelFolderPath);
                FileInfo[] files = direction.GetFiles("*.ballance", SearchOption.AllDirectories);
                for (int i = 0, c = files.Length; i < c; i++)
                {
                    ModManager.RegisterLevel(files[i].FullName);

                    GameInitSetUIProgressValue(0.8f + i / (float)c * 0.2f);
                    UIProgressText.text = "Loading " + files[i].Name;
                }
            }

            UIProgressText.text = "Loading";
            GameInitSetUIProgressValue(1);
            yield break;
        }

        private bool GameInitContinueInit()
        {
            //模式
            switch (GameManager.Mode)
            {
                case GameMode.Game: return true;
                case GameMode.LoaderDebug:
                    GameManager.GameMediator.CallAction(GamePartName.LevelLoader, "ACTION_DEBUG_LEVEL_LOADER", Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(false);
                    break;
                case GameMode.CoreDebug:
                    GameManager.GameMediator.CallAction(GamePartName.LevelManager, "ACTION_DEBUG_CORE", Main.Main.Instance.CoreDebugBase);
                    GameInitHideGameInitUi(false);
                    break;
                case GameMode.Level:
                    GameManager.GameMediator.CallAction(GamePartName.LevelLoader, "LoadLevel", Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(true);
                    break;
                case GameMode.LevelEditor:
                    LevelEditor levelEditor = (LevelEditor)GameManager.RegisterManager(typeof(LevelEditor), false);
                    levelEditor.StartDebugLevelEditor(Main.Main.Instance.LevelDebugTarget);
                    GameInitHideGameInitUi(true);
                    break;
            }
            return false;
        }
        private void GameInitHideGameInitUi(bool showBlack)
        {
            GameManager.UIManager.MaskBlackSet(showBlack);
            gameInitUI.SetActive(false);
        }
        private void GameInitSetUIProgressValue(float val)
        {
            UIProgressValue.sizeDelta = new Vector2(val * UIProgress.sizeDelta.x, UIProgressValue.sizeDelta.y);
        }

        #endregion

        #region Base Settings Control

        private GameSettingsActuator GameSettings = null;
        private Resolution[] resolutions = null;
        private int defaultResolution = 0;

        private void InitSettings()
        {
            DebugManager.RegisterCommand("resetsettings", (string keyword, string fullCmd, string[] args) =>
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

        
    }

    [SLua.CustomLuaClass]
    public enum GameModRunMask
    {
        None = 0,
        /// <summary>
        /// 仅在MenuLevel运行
        /// </summary>
        MenuLevel = 0x2,
        /// <summary>
        /// 仅在关卡加载器和关卡运行
        /// </summary>
        Level = 0x4,
        /// <summary>
        /// 仅在关卡加载器运行，加载完毕后卸载
        /// </summary>
        LevelLoader = 0x8,
        /// <summary>
        /// 仅在关卡编辑器运行
        /// </summary>
        LevelEditor = 0x10,
        /// <summary>
        /// 仅在关卡查看器运行
        /// </summary>
        LevelViewer = 0x20,
        /// <summary>
        /// 仅在内核运行
        /// </summary>
        GameCore = 0x40,
        /// <summary>
        /// 整个游戏都运行
        /// </summary>
        GameBase = MenuLevel | Level | LevelLoader | LevelEditor | GameCore | LevelViewer,
    }
}
