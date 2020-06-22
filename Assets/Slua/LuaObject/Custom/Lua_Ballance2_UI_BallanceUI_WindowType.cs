using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_WindowType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.BallanceUI.WindowType");
		addMember(l,0,"Normal");
		addMember(l,1,"GlobalAlert");
		addMember(l,2,"Page");
		LuaDLL.lua_pop(l, 1);
	}
}
