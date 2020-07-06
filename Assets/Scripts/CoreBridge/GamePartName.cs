using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 游戏内置模块名称
    /// </summary>
    [SLua.CustomLuaClass]
    public static class GamePartName
    {
        public const string Core = "core";
        public const string ICManager = "core.icmgr";
        public const string SoundManager = "core.soundmgr";
        public const string BallManager = "core.ballmgr";
        public const string CamManager = "core.cammgr";
        public const string LevelManager = "core.levelmgr";
        public const string LevelLoader = "core.levelloader";
    }
}
