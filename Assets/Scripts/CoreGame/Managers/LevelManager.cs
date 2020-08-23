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

        protected override bool InitActions(GameActionStore actionStore)
        {
            actionStore.RegisterAction("ACTION_DEBUG_CORE",  TAG, (param) =>
            {
                StartDebugCore((GameObject)param[0]);
                return GameActionCallResult.CreateActionCallResult(true);
            }, new string[] { "UnityEngine.GameObject" });
            return base.InitActions(actionStore);
        }
        protected override bool InitStore(Store store)
        {
            InitGlobaShareAndStore(store);
            return base.InitStore(store);
        }
        protected override void InitPre()
        {
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
            return true;
        }






        private void StartDebugCore(GameObject basePrefab)
        {
            GameManager.SetGameBaseCameraVisible(false);
            GameCloneUtils.CloneNewObjectWithParent(basePrefab, GameManager.GameRoot.transform, "DebugFloor");
            GameManager.GameMediator.CallAction(GamePartName.CamManager, "CamStart");
            ResetEnergy();
        }

        private int life = 3;
        private int score = 1000;
        private bool gameCounter = false;

        private void ResetEnergy()
        {
            life = 3;
            score = 1000;
        }
        private void StartCounter()
        {
            gameCounter = true;
        }
        private void StopCounter()
        {
            gameCounter = false;
        }




        private void Update()
        {
            
        }
    }
}
