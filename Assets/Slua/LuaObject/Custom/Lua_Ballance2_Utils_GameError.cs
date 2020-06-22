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
		addMember(l,8,"ParamNotProvide");
		addMember(l,9,"PrefabNotFound");
		addMember(l,10,"InitializationFailed");
		addMember(l,11,"BadAssetBundle");
		addMember(l,12,"FileNotFound");
		addMember(l,13,"ModConflict");
		addMember(l,14,"ModDependenciesLoadFailed");
		addMember(l,15,"ModCanNotRun");
		addMember(l,16,"BadMod");
		addMember(l,17,"NetworkError");
		addMember(l,18,"NotInitialize");
		addMember(l,19,"MustBeContainer");
		LuaDLL.lua_pop(l, 1);
	}
}
