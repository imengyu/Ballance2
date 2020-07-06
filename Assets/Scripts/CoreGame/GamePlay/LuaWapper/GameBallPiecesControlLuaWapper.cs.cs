using Ballance2.CoreBridge;
using SLua;
using System;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 球碎片控制器 LUA 包装
    /// </summary>
    [CustomLuaClass]
    public class GameBallPiecesControlLuaWapper : GameBallPiecesControl
    {
        private LuaReturnBoolDelegate fnRecoverPieces = null;
        private LuaReturnBoolDelegate fnThrowPieces = null;

        private GameLuaObjectHost GameLuaObjectHost;

        private LuaTable self = null;

        private void Start()
        {
            GameLuaObjectHost = GetComponent<GameLuaObjectHost>();
            if (GameLuaObjectHost == null)
                throw new Exception("GameBallLuaWapper can only use when GameLuaObjectHost is bind ! ");

            InitLuaFun();
        }
        private void InitLuaFun()
        {
            self = GameLuaObjectHost.LuaSelf;
            LuaFunction fn = self["RecoverPieces"] as LuaFunction;
            if (fn != null) fnRecoverPieces = fn.cast<LuaReturnBoolDelegate>();
            fn = self["ThrowPieces"] as LuaFunction;
            if (fn != null) fnThrowPieces = fn.cast<LuaReturnBoolDelegate>();
        }

        [DoNotToLua]
        public override bool RecoverPieces()
        {
            if (fnRecoverPieces != null) return fnRecoverPieces(self);
            return false;
        }
        [DoNotToLua]
        public override bool ThrowPieces()
        {
            if (fnThrowPieces != null) return fnThrowPieces(self);
            return false;
        }
    }
}
