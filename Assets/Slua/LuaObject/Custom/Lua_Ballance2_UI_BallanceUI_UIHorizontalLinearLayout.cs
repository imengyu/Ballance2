﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_UIHorizontalLinearLayout : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"Ballance2.UI.BallanceUI.UIHorizontalLinearLayout");
		createTypeMetatable(l,null, typeof(Ballance2.UI.BallanceUI.UIHorizontalLinearLayout),typeof(Ballance2.UI.BallanceUI.UILinearLayout));
	}
}
