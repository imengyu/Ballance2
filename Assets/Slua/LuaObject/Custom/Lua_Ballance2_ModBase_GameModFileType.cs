using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_ModBase_GameModFileType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModFileType");
		addMember(l,0,"NotSet");
		addMember(l,1,"AssetBundle");
		addMember(l,2,"BallanceZipPack");
		LuaDLL.lua_pop(l, 1);
	}
}
