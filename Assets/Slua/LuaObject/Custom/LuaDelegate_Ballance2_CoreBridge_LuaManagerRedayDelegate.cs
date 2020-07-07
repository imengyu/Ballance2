
using System;
using System.Collections.Generic;
namespace SLua
{
    public partial class LuaDelegation : LuaObject
    {

        static internal void Lua_Ballance2_CoreBridge_LuaManagerRedayDelegate(LuaFunction ld ,SLua.LuaTable a1,Ballance2.CoreBridge.Store a2,Ballance2.Managers.BaseManager a3) {
            IntPtr l = ld.L;
            int error = pushTry(l);

			pushValue(l,a1);
			pushValue(l,a2);
			pushValue(l,a3);
			ld.pcall(3, error);
			LuaDLL.lua_settop(l, error-1);
		}
	}
}
