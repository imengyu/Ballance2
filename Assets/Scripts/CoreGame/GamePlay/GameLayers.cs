using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 游戏物理层定义
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameLayers
    {
        public const int LAYER_PHY_FLOOR = 8;
        public const int LAYER_PHY_FLOOR_STOPPER = 9;
        public const int LAYER_PHY_FLOOR_WOOD = 10;
        public const int LAYER_PHY_RAIL = 11;
        public const int LAYER_PHY_MODUL_FLOOR_COL = 12;
        public const int LAYER_PHY_MODUL_NO_FLOOR_COL = 13;
        public const int LAYER_PHY_MODUL_NO_BALL_COL = 14;
        public const int LAYER_PHY_MODUL_NO_COL = 15;
        public const int LAYER_PHY_BALL = 16;
    }
}
