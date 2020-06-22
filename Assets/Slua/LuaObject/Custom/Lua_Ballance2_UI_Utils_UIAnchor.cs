using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_Utils_UIAnchor : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.UI.Utils.UIAnchor");
		addMember(l,0,"Top");
		addMember(l,1,"Center");
		addMember(l,2,"Bottom");
		addMember(l,3,"Left");
		addMember(l,4,"Right");
		addMember(l,5,"Stretch");
		LuaDLL.lua_pop(l, 1);
	}
}
