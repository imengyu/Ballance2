using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_GameMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.GameMode");
		addMember(l,0,"None");
		addMember(l,1,"Game");
		addMember(l,2,"Level");
		addMember(l,3,"LevelEditor");
		addMember(l,4,"MinimumDebug");
		addMember(l,5,"LoaderDebug");
		addMember(l,6,"CoreDebug");
		LuaDLL.lua_pop(l, 1);
	}
}
