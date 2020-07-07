
using System;
using System.Collections.Generic;
namespace SLua
{
    public partial class LuaDelegation : LuaObject
    {

        static internal bool Lua_Ballance2_CoreBridge_LuaStoreReturnBoolDelegate(LuaFunction ld ,SLua.LuaTable a1,Ballance2.CoreBridge.Store a2) {
            IntPtr l = ld.L;
            int error = pushTry(l);

			pushValue(l,a1);
			pushValue(l,a2);
			ld.pcall(2, error);
			bool ret;
			checkType(l,error+1,out ret);
			LuaDLL.lua_settop(l, error-1);
			return ret;
		}
	}
}
