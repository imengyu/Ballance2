using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreGame_Managers_DirectionType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.CoreGame.Managers.DirectionType");
		addMember(l,0,"Forward");
		addMember(l,1,"Left");
		addMember(l,2,"Back");
		addMember(l,3,"Right");
		LuaDLL.lua_pop(l, 1);
	}
}
