using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;

namespace Ballance2.GameCore
{
    /// <summary>
    /// 关卡编辑器
    /// </summary>
    public class LevelEditor : BaseManager
    {
        public const string TAG = "LevelEditor";

        public LevelEditor() : base("ext.level_editor", TAG, "Singleton")
        {

        }

        protected override void InitPre()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.CoreActions["EditLevel"],
                   TAG, OnCallLoadLevel, new string[] { "System.String" });
            base.InitPre();
        }
        public override bool InitManager()
        {
            
            return true;
        }
        public override bool ReleaseManager()
        {
            GameManager.GameMediator.UnRegisterAction(GameActionNames.CoreActions["EditLevel"]);
            return true;
        }

        private GameActionCallResult OnCallLoadLevel(params object[] param)
        {
           
            return GameActionCallResult.CreateActionCallResult(true);
        }

        internal void StartDebugLevelEditor(string targetFileName)
        {

        }
    }
}
