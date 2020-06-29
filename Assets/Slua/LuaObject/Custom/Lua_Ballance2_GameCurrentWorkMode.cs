using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_GameCurrentWorkMode : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.GameCurrentWorkMode");
		addMember(l,0,"None");
		addMember(l,1,"Intro");
		addMember(l,2,"Level");
		addMember(l,3,"LevelLoader");
		addMember(l,4,"LevelEditor");
		addMember(l,5,"LevelViewer");
		addMember(l,6,"MenuLevel");
		LuaDLL.lua_pop(l, 1);
	}
}
