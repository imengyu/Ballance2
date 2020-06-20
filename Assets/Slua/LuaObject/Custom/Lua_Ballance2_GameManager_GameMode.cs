using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_GameManager_GameMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.GameManager.GameMode");
		addMember(l,0,"None");
		addMember(l,1,"Game");
		addMember(l,2,"MinimumLoad");
		addMember(l,3,"Level");
		addMember(l,4,"LevelEditor");
		LuaDLL.lua_pop(l, 1);
	}
}
