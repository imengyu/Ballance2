using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;
using UnityEngine;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡主管理器
    /// </summary>
    public class LevelManager : BaseManager
    {
        public const string TAG = "LevelManager";

        public LevelManager() : base(GamePartName.LevelManager, TAG, "Singleton")
        {

        }

        protected override bool InitStore(Store store)
        {
            InitGlobaShareAndStore(store);
            return base.InitStore(store);
        }
        protected override void InitPre()
        {
            InitActions();
            base.InitPre();
        }

        #region 全局数据共享

        //私有控制数据
        private StoreData sd_life = null;
        private StoreData sd_score = null;

        private void InitGlobaShareAndStore(Store store)
        {
            sd_life = store.AddParameter("life", StoreDataAccess.Get, StoreDataType.Integer);
            sd_score = store.AddParameter("score", StoreDataAccess.Get, StoreDataType.Integer);

            //Get
            sd_life.SetDataProvider(currentContext, () => life);
            sd_score.SetDataProvider(currentContext, () => score);
        }

        #endregion

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
            ResetEnergy();
        }

        private int life = 3;
        private int score = 1000;

        private void ResetEnergy()
        {
            life = 3;
            score = 1000;
        }
        private void StartCounter()
        {
            
        }
    }
}
