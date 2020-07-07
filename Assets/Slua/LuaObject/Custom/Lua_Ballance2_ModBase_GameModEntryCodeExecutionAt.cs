using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_ModBase_GameModEntryCodeExecutionAt : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModEntryCodeExecutionAt");
		addMember(l,0,"Manual");
		addMember(l,1,"AfterLoaded");
		addMember(l,2,"AtStart");
		addMember(l,3,"AtLevelLoading");
		addMember(l,4,"AfterLevelLoad");
		LuaDLL.lua_pop(l, 1);
	}
}
