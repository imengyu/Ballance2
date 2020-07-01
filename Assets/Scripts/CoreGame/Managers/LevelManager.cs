using Ballance2.CoreBridge;
using Ballance2.CoreGame.Interfaces;
using Ballance2.Managers;
using Ballance2.Utils;
using UnityEngine;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelManager : BaseManager, ILevelManager
    {
        public const string TAG = "LevelManager";

        public LevelManager() : base(TAG, "Singleton")
        {

        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.ACTION_DEBUG_CORE,
                   TAG, OnCallStartDebugCore);
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private GameActionCallResult OnCallStartDebugCore(params object[] param)
        {
            StartDebugCore((GameObject)param[0]);
            return GameActionCallResult.CreateActionCallResult(true);
        }

        private void StartDebugCore(GameObject basePrefab)
        {
            GameManager.SetGameBaseCameraVisible(false);
            GameCloneUtils.CloneNewObjectWithParent(basePrefab, GameManager.GameRoot.transform, "DebugFloor");
            IBallManager ballManager = (IBallManager)GameManager.GetManager("BallManager");
            ICamManager camManager = (ICamManager)GameManager.GetManager("CamManager");

            camManager.CamStart();
        }
    }
}
