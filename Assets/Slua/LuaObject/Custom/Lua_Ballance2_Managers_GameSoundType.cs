using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Managers_GameSoundType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.Managers.GameSoundType");
		addMember(l,0,"Normal");
		addMember(l,1,"BallEffect");
		addMember(l,2,"ModulEffect");
		addMember(l,3,"UI");
		addMember(l,4,"Background");
		LuaDLL.lua_pop(l, 1);
	}
}
