using Ballance2.CoreBridge;
using System.Collections.Generic;

namespace Ballance2.CoreBridge
{
    [SLua.CustomLuaClass]
    public class GameHandlerList : List<GameHandler>
    {
        public void CallEventHandler(string evtName, params object[] parm)
        {
            foreach(GameHandler h in this)
                h.CallEventHandler(evtName, parm);
        }
        public void Dispose()
        {
            foreach (GameHandler h in this)
                h.Dispose();
            this.Clear();
        }
    }
}
