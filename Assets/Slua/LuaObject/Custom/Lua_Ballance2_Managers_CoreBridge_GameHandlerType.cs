using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Managers_CoreBridge_GameHandlerType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.Managers.CoreBridge.GameHandlerType");
		addMember(l,0,"CSKernel");
		addMember(l,1,"LuaModul");
		LuaDLL.lua_pop(l, 1);
	}
}
