using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_LayoutGravity : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.BallanceUI.LayoutGravity");
		addMember(l,0,"None");
		addMember(l,1,"Top");
		addMember(l,2,"Bottom");
		addMember(l,4,"Start");
		addMember(l,8,"End");
		addMember(l,16,"CenterHorizontal");
		addMember(l,32,"CenterVertical");
		addMember(l,48,"Center");
		LuaDLL.lua_pop(l, 1);
	}
}
