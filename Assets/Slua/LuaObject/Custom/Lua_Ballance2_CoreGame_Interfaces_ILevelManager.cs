using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreGame_Interfaces_ILevelManager : LuaObject {
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"Ballance2.CoreGame.Interfaces.ILevelManager");
		createTypeMetatable(l,null, typeof(Ballance2.CoreGame.Interfaces.ILevelManager));
	}
}
