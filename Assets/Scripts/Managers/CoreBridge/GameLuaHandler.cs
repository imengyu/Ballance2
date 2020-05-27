using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.Managers.CoreBridge
{
    public class GameLuaHandler
    {
        public GameLuaHandler(string luaModulHandlerString)
        {

        }



        public bool RunCommandHandler(string keyword, int argCount, string fullCmd, string[] args)
        {
            return false;
        }
        public bool RunEventHandler(string evtName, params object[] pararms)
        {
            return false;
        }


    }
}
