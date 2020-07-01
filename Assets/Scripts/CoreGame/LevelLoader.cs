using Ballance2.CoreBridge;
using Ballance2.CoreGame.Interfaces;
using Ballance2.Managers;
using Ballance2.Utils;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelLoader : BaseManager, ILevelLoader
    {
        public const string TAG = "LevelLoader";

        public LevelLoader() : base(TAG, "Singleton")
        {

        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.ACTION_LOAD_LEVEL, 
                TAG, OnCallLoadLevel);
            GameManager.GameMediator.RegisterAction(GameActionNames.ACTION_UNLOAD_LEVEL,
                TAG, OnCallLoadLevel);
            GameManager.GameMediator.RegisterAction(GameActionNames.ACTION_DEBUG_LEVEL_LOADER,
                TAG, OnCallStartDebugLevelLoader);
            return true;
        }
        public override bool ReleaseManager()
        {
            if (LevelLoadStatus == LevelLoadStatus.Loaded)
                UnLoadLevel(true);
            return true;
        }

        private GameActionCallResult OnCallLoadLevel(params object[] param)
        {
            if (LevelLoadStatus != LevelLoadStatus.Loading || LevelLoadStatus != LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (LevelLoadStatus != LevelLoadStatus.NotLoad)
            {
                GameErrorManager.LastError = GameError.AlredayLoaded;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if(param == null || param.Length < 1)
            {
                GameErrorManager.LastError = GameError.ParamNotProvide;
                return GameActionCallResult.CreateActionCallResult(false);
            }

            bool rs = StartLoadLevel((string)param[0]);
            return GameActionCallResult.CreateActionCallResult(rs);
        }
        private GameActionCallResult OnCallUnLoadLevel(params object[] param)
        {
            if (LevelLoadStatus != LevelLoadStatus.Loading || LevelLoadStatus != LevelLoadStatus.UnLoading)
            {
                GameErrorManager.LastError = GameError.InProgress;
                return GameActionCallResult.CreateActionCallResult(false);
            }
            if (LevelLoadStatus == LevelLoadStatus.Loaded)
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

        /// <summary>
        /// 关卡加载状态
        /// </summary>
        public LevelLoadStatus LevelLoadStatus { get; private set; } = LevelLoadStatus.NotLoad;

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
