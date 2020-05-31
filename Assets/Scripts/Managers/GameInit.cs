using UnityEngine;
using Ballance2.Managers.CoreBridge;
using System.Collections;
using UnityEngine.Networking;
using Ballance2.Utils;
using UnityEngine.UI;

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
            GameManager.GameMediator.RegisterEventKernalHandler(
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    LoadGameInitBase();
                    StartCoroutine(LoadGameInitUI());
                    return false;
                });
            GameManager.GameMediator.RegisterEventKernalHandler(
                GameEventNames.EVENT_GAME_INIT_ENTRY, TAG, (e, p) =>
                { StartCoroutine(GameInitModuls()); return false; });
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private ModManager ModManager;
        private SoundManager SoundManager;

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
            ModManager = (ModManager)GameManager.GetManager(ModManager.TAG);
            SoundManager = (SoundManager)GameManager.GetManager(SoundManager.TAG);

            IntroAudio = SoundManager.RegisterSoundPlayer(GameSoundType.UI, GameManager.FindStaticAssets<AudioClip>("IntroMusic"));
        }
        private IEnumerator LoadGameInitUI()
        {
            int modUid = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "assets/gameinit_ui.assetbundle"), false);
            gameInitMod = ModManager.FindGameMod(modUid);
            if (gameInitMod == null)
            {
                GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载 GameInit 资源包失败 ");
                StopAllCoroutines();
            }

            yield return StartCoroutine(gameInitMod.LoadInternal());

            gameInitUI = GameManager.UIManager.InitViewToCanvas(gameInitMod.GetPrefabAsset("Assets/Mods/GameInit/UIGameInit.prefab"), "GameInitUI").gameObject;

            GameManager.UIManager.MaskBlackSet(false);

            UIProgress = gameInitUI.transform.Find("UIProgress").GetComponent<RectTransform>();
            UIProgressValue = gameInitUI.transform.Find("UIProgress/UIValue").GetComponent<RectTransform>();
            UIProgressText = gameInitUI.transform.Find("Text").GetComponent<Text>();
            IntroAnimator = gameInitUI.GetComponent<Animator>();
            TextError = gameInitUI.transform.Find("TextError").GetComponent<Text>();

            loadedGameInitUI = true;
        }
        private void LoadGameInitUIProgressValue(float val)
        {
            UIProgressValue.sizeDelta = new Vector2(val * UIProgress.sizeDelta.x, UIProgressValue.sizeDelta.y);
        }

        /*
         * GameInit.txt 格式
         * 
         * 格式：REQUIRED/OPTIONAL:CORE/MOD:资源路径
         * 必须/可选模组       内核(游戏内置)/外部模组    文件路径 可使用 [Platform] 来代表平台，其会替换为
         * 指定平台字符串：win/android/ios 等等
         *
         * eg : REQUIRED:CORE:assets/musics_[Platform].unity3d
         *          REQUIRED:CORE:scenses/menulevel.unity3d
         * 
         */

        //加载 GameInit.txt 中的模块
        private IEnumerator GameInitModuls()
        {
            yield return new WaitUntil(IsGameInitUILoaded);

            //播放音乐
            IntroAudio.Play();

            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);

            string gameinit_txt_path = GamePathManager.GetResRealPath("gameinit", "");
            UnityWebRequest request = UnityWebRequest.Get(gameinit_txt_path);
            yield return request.SendWebRequest();
            if (request.isDone && string.IsNullOrEmpty(request.error))
            {
                string GameInitTable = request.downloadHandler.text;
                StringSpliter sp = new StringSpliter(GameInitTable, '\n');
                if (sp.Count >= 1)
                {
                    int loadedCount = 0;
                    foreach (string ar in sp.Result)
                    {
                        bool required = false;
                        string fullPath = ar;
                        if (ar.StartsWith(":")) continue;
                        else if (ar.StartsWith("REQUIRED:"))
                        {
                            fullPath = GamePathManager.GetResRealPath("", fullPath.Substring(9));
                            required = true;
                        }
                        else if (ar.StartsWith("OPTIONAL:"))
                            fullPath = GamePathManager.GetResRealPath("", fullPath.Substring(10));

                        //状态
                        loadedCount++;
                        LoadGameInitUIProgressValue(loadedCount / (float)sp.Count);
                        UIProgressText.text = "Loading " + fullPath;

                        //加载
                        int modUid = ModManager.LoadGameMod(fullPath, false);
                        GameMod mod = ModManager.FindGameMod(modUid);
                        //等待加载
                        yield return StartCoroutine(mod.LoadInternal());

                        if (mod.LoadStatus == GameModStatus.InitializeSuccess)
                            continue;
                        else if (required)
                            GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载模块  " + fullPath + " 时发生错误");
                        else GameLogger.Warning(TAG, "加载模块  {0} 时发生错误", fullPath);
                    }
                }
            }
            else GameErrorManager.ThrowGameError(GameError.GameInitReadFailed, "加载 GameInit.txt  " + gameinit_txt_path + " 时发生错误：" + request.error);

            UIProgressText.text = "Loading";

            int initEventHandledCount = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH, "*");
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);

            yield return new WaitUntil(IsGameInitAnimPlayend);
            if (initEventHandledCount == 0)
            {
                GameErrorManager.ThrowGameError(GameError.HandlerLost, "未找到 EVENT_GAME_INIT_FINISH 的下一步事件接收器\n此错误出现原因可能是配置不正确");
                GameLogger.Warning(TAG, "None EVENT_GAME_INIT_FINISH handler was found, this game will not continue.");
            }
            else
            {
                GameManager.UIManager.MaskBlackSet(true);
                gameInitUI.SetActive(false);
            }
        }
    }
}
