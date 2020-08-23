using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡编辑器
    /// 【待完成，看看后期有没有时间做，如果没有的话，这一块就算了吧（毕竟用Unity直接做关卡更方便）】    
    /// </summary>
    public class LevelEditor : BaseManager
    {
        public const string TAG = "LevelEditor";

        public LevelEditor() : base("ext.level_editor", TAG, "Singleton")
        {

        }

        protected override bool InitActions(GameActionStore actionStore)
        {
            actionStore.RegisterAction("EditLevel", TAG, OnCallLoadLevel, new string[] { "System.String" });
            return base.InitActions(actionStore);
        }
        protected override void InitPre()
        {
            
            base.InitPre();
        }
        public override bool InitManager()
        {
            
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private GameActionCallResult OnCallLoadLevel(params object[] param)
        {
            GameErrorManager.LastError = GameError.NotFinished;
            return GameActionCallResult.FailResult;
        }

        internal void StartDebugLevelEditor(string targetFileName)
        {
        }
    }
}
