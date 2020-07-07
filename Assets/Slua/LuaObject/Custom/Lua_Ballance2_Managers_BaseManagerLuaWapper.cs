using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Managers_BaseManagerLuaWapper : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"Ballance2.Managers.BaseManagerLuaWapper");
		createTypeMetatable(l,null, typeof(Ballance2.Managers.BaseManagerLuaWapper),typeof(Ballance2.Managers.BaseManager));
	}
}
