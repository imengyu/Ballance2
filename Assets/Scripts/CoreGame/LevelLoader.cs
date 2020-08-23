using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.Interfaces;
using Ballance2.Managers;
using Ballance2.ModBase;
using Ballance2.Utils;
using Boo.Lang;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelLoader : BaseManager, ILevelLoader
    {
        public const string TAG = "LevelLoader";

        public LevelLoader() : base(GamePartName.LevelLoader, TAG, "Singleton")
        {
            replaceable = false;
        }

        protected override bool InitActions(GameActionStore actionStore)
        {
            actionStore.RegisterAction("LoadLevel",
              TAG, OnCallLoadLevel, new string[] { "System,String" });
            actionStore.RegisterAction("UnLoadLevel",
                TAG, OnCallUnLoadLevel, null);
            actionStore.RegisterAction("ACTION_DEBUG_LEVEL_LOADER",
                TAG, OnCallStartDebugLevelLoader, new string[] { "System,String" });
            return base.InitActions(actionStore);
        }
        protected override void InitPre()
        {
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_ENTER_LEVEL_LOADER);
            base.InitPre();
        }

        public override bool InitManager()
        {
            floorGroups = new List<LevelFloorGroup>();
            modulGroups = new List<LevelModulGroup>();
            floorColSounds = new List<LevelFloorColSound>();
            return true;
        }
        public override bool ReleaseManager()
        {
            if (levelLoadStatus == LevelLoadStatus.Loaded)
                OnCallLoadLevel();

            if (UILevelLoader != null)
            {
                Destroy(UILevelLoader);
                UILevelLoader = null;
            }
            if (floorGroups != null)
            {
                floorGroups.Clear();
                floorGroups = null;
            }
            if (modulGroups != null)
            {
                modulGroups.Clear();
                modulGroups = null;
            }
            if (floorColSounds != null)
            {
                floorColSounds.Clear();
                floorColSounds = null;
            }

            return true;
        }

        private GameActionCallResult OnCallLoadLevel(params object[] param)
        {
            if (levelLoadStatus == LevelLoadStatus.Loading || levelLoadStatus == LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (levelLoadStatus != LevelLoadStatus.NotLoad)
            {
                GameErrorManager.LastError = GameError.AlredayLoaded;
                return GameActionCallResult.CreateActionCallResult(false);
            }

            string pathOrName = (string)param[0];
            string levelPath = "";
            int number;
            if (int.TryParse(pathOrName, out number))
            {
                if (number < 15)
                    levelPath = GamePathManager.GetResRealPath("core", "levels/Level" + number + ".ballance");
            }
            else
            {
                if (GamePathManager.IsAbsolutePath(pathOrName)) levelPath = pathOrName;
                else levelPath = GamePathManager.GetResRealPath("level", pathOrName);
            }

            EnterLoader();
            StartCoroutine(LoadLevel(levelPath));
            return GameActionCallResult.CreateActionCallResult(true);
        }
        private GameActionCallResult OnCallUnLoadLevel(params object[] param)
        {
            if (levelLoadStatus == LevelLoadStatus.Loading || levelLoadStatus == LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (levelLoadStatus == LevelLoadStatus.Loaded)
            {
                QuitLoader();


                return GameActionCallResult.CreateActionCallResult(true);
            }
            else
            {
                GameErrorManager.LastError = GameError.NotLoad;
                return GameActionCallResult.CreateActionCallResult(false);
            }
        }
        private GameActionCallResult OnCallStartDebugLevelLoader(params object[] param)
        {
            //...
            return GameActionCallResult.CreateActionCallResult(true);
        }

        private LevelLoadStatus levelLoadStatus = LevelLoadStatus.NotLoad;

        #region 加载UI控制

        private GameObject UILevelLoader;
        private Text LoaderErrorContent;
        private RectTransform LoaderProgressOuter;
        private RectTransform LoaderProgress;
        private Image LoaderProgressImage;
        private GameObject LoaderError;
        private Button BtnContinue;
        private Button BtnQuit;

        private void QuitLoader()
        {
            if (UILevelLoader != null)
            {
                UILevelLoader.SetActive(false);
            }

            GameManager.UIManager.MaskBlackFadeIn(0.5f);
            GameManager.NotifyGameCurrentScenseChanged(GameCurrentScense.MenuLevel);
        }
        private void EnterLoader()
        {
            if (UILevelLoader == null)
            {
                UILevelLoader = GameCloneUtils.CloneNewObject(GameManager.FindStaticPrefabs("UILevelLoader"), "UILevelLoader");
                GameManager.UIManager.AttatchViewToCanvas(UILevelLoader.GetComponent<RectTransform>());

                LoaderProgressOuter = UILevelLoader.transform.Find("LoaderProgressOuter").GetComponent<RectTransform>();
                LoaderError = UILevelLoader.transform.Find("LoaderError").gameObject;
                LoaderProgress = LoaderProgressOuter.transform.Find("LoaderProgress").GetComponent<RectTransform>();
                LoaderProgressImage = LoaderProgress.GetComponent<Image>();
                LoaderErrorContent = UILevelLoader.transform.Find("LoaderError/Scroll View/Viewport/LoaderErrorContent").GetComponent<Text>();

                BtnContinue = UILevelLoader.transform.Find("LoaderError/Continue").GetComponent<Button>();
                BtnQuit = UILevelLoader.transform.Find("LoaderError/Quit").GetComponent<Button>();

                BtnQuit.onClick.AddListener(() => QuitLoader());
            }

            LoaderProgress.gameObject.SetActive(true);
            LoaderError.SetActive(false);
            UpdateLoaderProgress(0.01f);

            GameManager.NotifyGameCurrentScenseChanged(GameCurrentScense.LevelLoader);
        }

        public void UpdateLoaderProgress(float v)
        {
            LoaderProgress.sizeDelta = new Vector2(LoaderProgressOuter.rect.width * v, LoaderProgress.sizeDelta.y);
            LoaderProgressImage.color = new Color(1, 1, 1, v);
        }
        public void LoadeErrorFail(string errText)
        {
            LoaderError.SetActive(true);
            LoaderProgress.gameObject.SetActive(false);
            LoaderErrorContent.text = errText;
            BtnContinue.gameObject.SetActive(false);
            BtnQuit.gameObject.SetActive(true);
        }
        public void LoaderReportError(string errText)
        {
            LoaderError.SetActive(true);
            LoaderErrorContent.text = errText;
            BtnContinue.gameObject.SetActive(currentLevel.ErrorSolve != LevelErrorSolveType.Break);
            BtnQuit.gameObject.SetActive(true);
        }

        #endregion

        public GameLevel GetCurrentLevel() { return currentLevel; }




        private GameLevel currentLevel = null;
        private GameObject currentLevelPrefab = null;

        private IEnumerator LoadLevel(string levelFullPath)
        {
            GameLevel currentLevel = GameManager.ModManager.FindLevel(levelFullPath);
            if (currentLevel == null)
            {
                LoadeErrorFail("加载关卡失败：\n未找到文件：" + levelFullPath + "，请确保关卡文件已放在 Levels 文件夹下。");
                yield break;
            }

            //加载关卡中的模组
            yield return new WaitUntil(GameManager.ModManager.IsNoneModLoading);
            UpdateLoaderProgress(0.1f);

            //通知模组加载
            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_ENTER_LEVEL_LOADER, "*", (ILevelLoader)this);

            //加载关卡本体
            yield return StartCoroutine(currentLevel.Load());
            UpdateLoaderProgress(0.2f);

            if (currentLevel.LoadStatus != ModBase.GameModStatus.InitializeSuccess)
            {
                LoadeErrorFail("加载关卡 " + currentLevel.Name + " 失败：\n错误信息：" + currentLevel.LoadError);
                yield break;
            }

            //加载基础prefab
            if(string.IsNullOrEmpty(currentLevel.BasePrefab))
            {
                LoadeErrorFail("加载关卡失败：\nBasePrefab 不能为空");
                yield break;
            }
            currentLevelPrefab = currentLevel.LevelAssetBundle.LoadAsset<GameObject>(currentLevel.BasePrefab);
            if(currentLevelPrefab == null)
            {
                LoadeErrorFail("加载关卡失败：\n未找到 BasePrefab ：" + currentLevel.BasePrefab);
                yield break;
            }
            currentLevelPrefab = GameCloneUtils.CloneNewObjectWithParent(currentLevelPrefab, transform, currentLevel.Name);
            UpdateLoaderProgress(0.23f);






            yield break;
        }


        #region 组管理

        private List<LevelFloorGroup> floorGroups = null;
        private List<LevelModulGroup> modulGroups = null;
        private List<LevelFloorColSound> floorColSounds = null;

        public LevelFloorGroup RegisterFloorType(LevelFloorGroup floorGroup)
        {
            LevelFloorGroup g = FindFloorType(floorGroup.GroupName);
            if (g == null)
            {
                g = floorGroup;
                floorGroups.Add(g);
            }
            else
            {
                GameLogger.Warning(TAG, "RegisterFloorType : floor type {0} alreday exists ! ", floorGroup.GroupName);
            }
            return g;
        }
        public LevelFloorGroup FindFloorType(string name)
        {
            foreach (LevelFloorGroup g in floorGroups)
                if (g.GroupName == name)
                    return g;
            return null;
        }
        public void UnRegisterFloorType(LevelFloorGroup floorGroup)
        {
            floorGroups.Remove(floorGroup);
        }




        #endregion
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 关卡加载状态
    /// </summary>
    public enum LevelLoadStatus
    {
        NotLoad,
        Loading,
        LoadFailed,
        Loaded,
        UnLoading,
    }


    internal class LevelFloorColSound
    {
        public string BallType = "";
        public string FloorType = "";
        public AudioClip RollSound = null;
        public AudioClip HitSound = null;
    }

    [SLua.CustomLuaClass]
    public class LevelFloorGroup
    {
        public string GroupName = "";
        public PhysicMaterial PhysicMaterial = null;
        public LevelFloorSolveDelegate CustomSolve = null;
    }

    [SLua.CustomLuaClass]
    public class LevelModulGroup
    {
        public string GroupName = "";
        public GameMod ModPackage = null;
        public LevelModulSolveDelegate CustomSolve = null;
    }
}
