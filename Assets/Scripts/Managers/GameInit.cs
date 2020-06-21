using UnityEngine;
using Ballance2.Managers.CoreBridge;
using System.Collections;
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
            GameManager.GameMediator.RegisterEventHandler(
                GameEventNames.EVENT_BASE_INIT_FINISHED, TAG, (e, p) =>
                {
                    LoadGameInitBase();
                    StartCoroutine(LoadGameInitUI());
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
            int modUid = ModManager.LoadGameMod(GamePathManager.GetResRealPath("core", "core.gameinit.ballance"), false);
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
            if (gamePackagesMod == null)
            {
                GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载 GamePackages 资源包失败 ");
                StopAllCoroutines();
            }

            yield return StartCoroutine(gamePackagesMod.LoadInternal());

            yield return new WaitUntil(IsGameInitAnimPlayend);

            //初始化模组启动代码（游戏初始化完成）
            ModManager.ExecuteModEntryCodeAtStart();

            int initEventHandledCount = GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH, "*");
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);
           
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
