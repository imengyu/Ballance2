using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Utils_GameError : LuaObject {
	static public void reg(IntPtr l) {
		getEnumTable(l,"Ballance2.Utils.GameError");
		addMember(l,0,"None");
		addMember(l,1,"BadMode");
		addMember(l,2,"GlobalException");
		addMember(l,3,"GameInitReadFailed");
		addMember(l,4,"GameInitPartLoadFailed");
		addMember(l,5,"HandlerLost");
		addMember(l,6,"AlredayRegistered");
		addMember(l,7,"Unregistered");
		addMember(l,8,"PrefabNotFound");
		addMember(l,9,"InitializationFailed");
		addMember(l,10,"BadAssetBundle");
		addMember(l,11,"FileNotFound");
		addMember(l,12,"ModConflict");
		addMember(l,13,"ModDependenciesLoadFailed");
		addMember(l,14,"ModCanNotRun");
		addMember(l,15,"BadMod");
		addMember(l,16,"NetworkError");
		addMember(l,17,"NotInitialize");
		addMember(l,18,"MustBeContainer");
		LuaDLL.lua_pop(l, 1);
	}
}
