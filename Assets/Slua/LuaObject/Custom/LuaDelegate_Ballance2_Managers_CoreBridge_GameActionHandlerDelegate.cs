﻿
using System;
using System.Collections.Generic;
namespace SLua
{
    public partial class LuaDelegation : LuaObject
    {

        static internal Ballance2.Managers.CoreBridge.GameActionCallResult Lua_Ballance2_Managers_CoreBridge_GameActionHandlerDelegate(LuaFunction ld ,object[] a1) {
            IntPtr l = ld.L;
            int error = pushTry(l);

			pushValue(l,a1);
			ld.pcall(1, error);
			Ballance2.Managers.CoreBridge.GameActionCallResult ret;
			checkType(l,error+1,out ret);
			LuaDLL.lua_settop(l, error-1);
			return ret;
		}
	}
}