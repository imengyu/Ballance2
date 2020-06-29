using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ballance2.Utils;
using Ballance2.CoreBridge;
using Ballance2.Config;
using Ballance2.ModBase;
using Ballance2.GameCore;
using Ballance2.CoreGame.Managers;
using UnityEngine.Networking;

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
                { StartCoroutine(GameInitCore()); return false; });
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

            UIProgressText.text = "Loading";

            //加载 core.gameinit.txt 获得要加载的模块
            string gameInitTxt = "";
#if UNITY_EDITOR
            TextAsset gameInitEditorAsset = null;
            if (GameManager.AssetsPreferEditor
                && (gameInitEditorAsset =
                UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(
                    GamePathManager.DEBUG_MODS_PATH + "/core.gameinit.txt")) != null)
                gameInitTxt = gameInitEditorAsset.text;
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

            UIProgressText.text = "Loading";

            //加载游戏内核管理器
            LevelLoader levelLoader = (LevelLoader)GameManager.RegisterManager(typeof(LevelLoader), false);
            LevelManager levelManager = (LevelManager)GameManager.RegisterManager(typeof(LevelManager), false);

            BallManager ballManager = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.FindStaticPrefabs("BallManager"), GameManager.GameRoot.transform).GetComponent<BallManager>();
            CamManager camManager = GameCloneUtils.CloneNewObjectWithParent(
                GameManager.FindStaticPrefabs("CamManager"), GameManager.GameRoot.transform).GetComponent<CamManager>();

            GameManager.RegisterManager(ballManager, false);
            GameManager.RegisterManager(camManager, false);

            //初始化管理器
            GameManager.RequestAllManagerInitialization();
            //初始化模组启动代码（游戏初始化完成）
            ModManager.ExecuteModEntryCodeAtStart();

            //正常情况下，等待动画播放完成
            if(GameManager.Mode == GameMode.Game)
                yield return new WaitUntil(IsGameInitAnimPlayend);
            
            //模式
            switch (GameManager.Mode)
            {
                case GameMode.Game: GameInitContinueGoGame();  break;
                case GameMode.LoaderDebug: levelLoader.StartDebugLevelLoader(Main.Main.Instance.LevelDebugTarget); break;
                case GameMode.CoreDebug: levelManager.StartDebugCore(Main.Main.Instance.CoreDebugBase); break;
                case GameMode.Level:
                    GameManager.GameMediator.CallAction(GameActionNames.ACTION_LOAD_LEVEL, Main.Main.Instance.LevelDebugTarget);
                    break;
                case GameMode.LevelEditor:
                    LevelEditor levelEditor = (LevelEditor)GameManager.RegisterManager(typeof(LevelEditor), false);
                    levelEditor.StartDebugLevelEditor(Main.Main.Instance.LevelDebugTarget);
                    break;
            }                
        }
        private IEnumerator GameInitPackages(string GameInitTable)
        {
            StringSpliter sp = new StringSpliter(GameInitTable, '\n');
            if (sp.Count >= 1)
            {
                int loadedCount = 0;
                string[] args = null;
                foreach (string ar in sp.Result)
                {
                    if (ar.StartsWith(":")) continue;

                    bool required = false;
                    string packageName = "";
                    args = ar.Split(':');

                    if(args.Length >= 3) {
                        required = args[1] == "Required";
                        packageName = args[0];
                    }

                    //状态
                    loadedCount++;
                    LoadGameInitUIProgressValue(loadedCount / (float)sp.Count);
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
            }
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
