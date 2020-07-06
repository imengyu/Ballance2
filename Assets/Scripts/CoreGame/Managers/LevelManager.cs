using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;
using UnityEngine;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelManager : BaseManager
    {
        public const string TAG = "LevelManager";

        public LevelManager() : base(GamePartName.LevelManager, TAG, "Singleton")
        {

        }

        public override void PreInitManager()
        {
            base.PreInitManager();
            InitActions();
        }
        public override bool InitManager()
        {
            
            return true;
        }
        public override bool ReleaseManager()
        {
            UnInitActions();
            return true;
        }

        private GameActionCallResult OnCallStartDebugCore(params object[] param)
        {
            StartDebugCore((GameObject)param[0]);
            return GameActionCallResult.CreateActionCallResult(true);
        }

        private void InitActions()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.CoreActions["ACTION_DEBUG_CORE"],
                   TAG, OnCallStartDebugCore, new string[] { "UnityEngine.GameObject" });
        }
        private void UnInitActions()
        {
            GameManager.GameMediator.UnRegisterAction(GameActionNames.CoreActions["ACTION_DEBUG_CORE"]);
        }

        private void StartDebugCore(GameObject basePrefab)
        {
            GameManager.SetGameBaseCameraVisible(false);
            GameCloneUtils.CloneNewObjectWithParent(basePrefab, GameManager.GameRoot.transform, "DebugFloor");
            GameManager.GameMediator.CallAction(GameActionNames.CamManager["CamStart"]);
        }
    }
}
