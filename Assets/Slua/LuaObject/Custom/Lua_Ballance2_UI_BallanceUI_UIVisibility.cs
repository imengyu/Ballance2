using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_UIVisibility : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.BallanceUI.UIVisibility");
		addMember(l,0,"Visible");
		addMember(l,1,"Gone");
		addMember(l,2,"InVisible");
		LuaDLL.lua_pop(l, 1);
	}
}
