using Ballance2.Managers.CoreBridge;
using System.Collections.Generic;

namespace Assets.Scripts.Managers.CoreBridge
{
    [SLua.CustomLuaClass]
    public class GameHandlerList : List<GameHandler>
    {
        public void CallHandler(string evtName, params object[] parm)
        {
            foreach(GameHandler h in this)
                h.Call(evtName, parm);
        }
        public void Dispose()
        {
            foreach (GameHandler h in this)
                h.Dispose();
            this.Clear();
        }
    }
}
