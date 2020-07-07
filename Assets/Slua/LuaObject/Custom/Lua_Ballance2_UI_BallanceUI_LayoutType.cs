using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_LayoutType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.BallanceUI.LayoutType");
		addMember(l,0,"Start");
		addMember(l,1,"End");
		addMember(l,2,"Center");
		LuaDLL.lua_pop(l, 1);
	}
}
