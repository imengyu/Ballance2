﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreBridge_StoreDataAccess : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.CoreBridge.StoreDataAccess");
		addMember(l,0,"Get");
		addMember(l,1,"GetAndSet");
		LuaDLL.lua_pop(l, 1);
	}
}
