using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_GameLogger_LogType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.GameLogger.LogType");
		addMember(l,0,"Error");
		addMember(l,1,"Assert");
		addMember(l,2,"Warning");
		addMember(l,3,"Text");
		addMember(l,5,"Info");
		addMember(l,6,"Max");
		LuaDLL.lua_pop(l, 1);
	}
}
