using Ballance2.CoreBridge;
using Ballance2.Managers;
using Ballance2.Utils;
using UnityEngine;

namespace Ballance2.GameCore
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 关卡加载器
    /// </summary>
    public class LevelManager : BaseManagerBindable
    {
        public const string TAG = "LevelManager";

        public LevelManager() : base(TAG, "Singleton")
        {

        }

        public override bool InitManager()
        {
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        internal void StartDebugCore(GameObject basePrefab)
        {

        }
    }
}
