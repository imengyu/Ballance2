using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_ModBase_GameModStatus : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModStatus");
		addMember(l,0,"Destroyed");
		addMember(l,1,"NotInitialize");
		addMember(l,2,"InitializeSuccess");
		addMember(l,3,"Loading");
		addMember(l,4,"InitializeFailed");
		addMember(l,5,"BadMod");
		LuaDLL.lua_pop(l, 1);
	}
}
