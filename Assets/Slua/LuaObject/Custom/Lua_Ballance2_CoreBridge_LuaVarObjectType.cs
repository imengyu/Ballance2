﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreBridge_LuaVarObjectType : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.CoreBridge.LuaVarObjectType");
		addMember(l,0,"None");
		addMember(l,1,"Vector2");
		addMember(l,2,"Vector2Int");
		addMember(l,3,"Vector3");
		addMember(l,4,"Vector3Int");
		addMember(l,5,"Vector4");
		addMember(l,6,"Rect");
		addMember(l,7,"RectInt");
		addMember(l,8,"Gradient");
		addMember(l,9,"Layer");
		addMember(l,10,"Curve");
		addMember(l,11,"Color");
		addMember(l,12,"BoundsInt");
		addMember(l,13,"Bounds");
		addMember(l,14,"Object");
		addMember(l,15,"GameObject");
		addMember(l,16,"Long");
		addMember(l,17,"Int");
		addMember(l,18,"String");
		addMember(l,19,"Double");
		addMember(l,20,"Bool");
		LuaDLL.lua_pop(l, 1);
	}
}
