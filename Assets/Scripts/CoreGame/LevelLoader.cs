using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelLoader : BaseManager
    {
        public const string TAG = "LevelLoader";

        public LevelLoader() : base(GamePartName.LevelLoader, TAG, "Singleton")
        {

        }

        protected override void InitPre()
        {
            InitActions();
            base.InitPre();
        }

        public override bool InitManager()
        {
          
            return true;
        }
        public override bool ReleaseManager()
        {
            if (levelLoadStatus == LevelLoadStatus.Loaded)
                UnLoadLevel(true);
            UnInitActions();
            return true;
        }

        private void InitActions()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.LevelLoader["LoadLevel"],
              TAG, OnCallLoadLevel, new string[] { "System,String" });
            GameManager.GameMediator.RegisterAction(GameActionNames.LevelLoader["UnLoadLevel"],
                TAG, OnCallLoadLevel, null);
            GameManager.GameMediator.RegisterAction(GameActionNames.CoreActions["ACTION_DEBUG_LEVEL_LOADER"],
                TAG, OnCallStartDebugLevelLoader, new string[] { "System,String" });
        }
        private void UnInitActions()
        {
            GameManager.GameMediator.UnRegisterActions(GameActionNames.LevelLoader);
            GameManager.GameMediator.UnRegisterAction(GameActionNames.CoreActions["ACTION_DEBUG_LEVEL_LOADER"]);
        }

        private GameActionCallResult OnCallLoadLevel(params object[] param)
        {
            if (levelLoadStatus != LevelLoadStatus.Loading || levelLoadStatus != LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (levelLoadStatus != LevelLoadStatus.NotLoad)
            {
                GameErrorManager.LastError = GameError.AlredayLoaded;
                return GameActionCallResult.CreateActionCallResult(false);
            }

            bool rs = StartLoadLevel((string)param[0]);
            return GameActionCallResult.CreateActionCallResult(rs);
        }
        private GameActionCallResult OnCallUnLoadLevel(params object[] param)
        {
            if (levelLoadStatus != LevelLoadStatus.Loading || levelLoadStatus != LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (levelLoadStatus == LevelLoadStatus.Loaded)
                return GameActionCallResult.CreateActionCallResult(UnLoadLevel(false));
            else
            {
                GameErrorManager.LastError = GameError.NotLoad;
                return GameActionCallResult.CreateActionCallResult(false);
            }
        }
        private GameActionCallResult OnCallStartDebugLevelLoader(params object[] param)
        {
            StartDebugLevelLoader((string)param[0]);
            return GameActionCallResult.CreateActionCallResult(true);
        }

        private LevelLoadStatus levelLoadStatus = LevelLoadStatus.NotLoad;

        private bool StartLoadLevel(string pathOrName)
        {


            return false;
        }
        private bool UnLoadLevel(bool force)
        {


            return false;
        }

        private void StartDebugLevelLoader(string levelFile)
        {

        }
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
}
