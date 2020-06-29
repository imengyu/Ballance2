using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_ModBase_GameModCodeType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.ModBase.GameModCodeType");
		addMember(l,0,"Lua");
		addMember(l,1,"CSharp");
		LuaDLL.lua_pop(l, 1);
	}
}
