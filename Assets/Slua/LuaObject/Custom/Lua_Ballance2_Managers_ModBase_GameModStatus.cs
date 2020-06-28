using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Managers_ModBase_GameModStatus : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModStatus");
		addMember(l,0,"NotInitialize");
		addMember(l,1,"InitializeSuccess");
		addMember(l,2,"Loading");
		addMember(l,3,"InitializeFailed");
		addMember(l,4,"BadMod");
		LuaDLL.lua_pop(l, 1);
	}
}
