using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_GameCore_LevelLoadStatus : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.GameCore.LevelLoadStatus");
		addMember(l,0,"NotLoad");
		addMember(l,1,"Loading");
		addMember(l,2,"LoadFailed");
		addMember(l,3,"Loaded");
		addMember(l,4,"UnLoading");
		LuaDLL.lua_pop(l, 1);
	}
}
