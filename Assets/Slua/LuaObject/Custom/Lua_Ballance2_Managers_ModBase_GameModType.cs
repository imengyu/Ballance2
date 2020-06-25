using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Managers_ModBase_GameModType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.Managers.ModBase.GameModType");
		addMember(l,0,"NotSet");
		addMember(l,1,"AssetPack");
		addMember(l,2,"ModPack");
		addMember(l,3,"Level");
		LuaDLL.lua_pop(l, 1);
	}
}
