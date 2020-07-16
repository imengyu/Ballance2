using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_ModBase_GameModType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModType");
		addMember(l,0,"NotSet");
		addMember(l,1,"AssetPack");
		addMember(l,2,"ModulePack");
		addMember(l,3,"Level");
		LuaDLL.lua_pop(l, 1);
	}
}
