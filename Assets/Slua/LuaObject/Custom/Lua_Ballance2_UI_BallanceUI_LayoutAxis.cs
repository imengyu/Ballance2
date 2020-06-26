using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_LayoutAxis : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.BallanceUI.LayoutAxis");
		addMember(l,0,"Vertical");
		addMember(l,1,"Horizontal");
		LuaDLL.lua_pop(l, 1);
	}
}
