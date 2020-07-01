using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;

namespace Ballance2.GameCore
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 关卡编辑器
    /// </summary>
    public class LevelEditor : BaseManager
    {
        public const string TAG = "LevelEditor";

        public LevelEditor() : base(TAG, "Singleton")
        {

        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterAction(GameActionNames.ACTION_EDIT_LEVEL, 
                TAG, OnCallLoadLevel);
            return true;
        }
        public override bool ReleaseManager()
        {
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
