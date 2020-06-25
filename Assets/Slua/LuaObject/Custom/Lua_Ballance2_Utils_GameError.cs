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
		addMember(l,10,"AssetsNotFound");
		addMember(l,11,"InitializationFailed");
		addMember(l,12,"BadAssetBundle");
		addMember(l,13,"FileNotFound");
		addMember(l,14,"ModConflict");
		addMember(l,15,"ModDependenciesLoadFailed");
		addMember(l,16,"ModExecutionCodeRunFailed");
		addMember(l,17,"ModCanNotRun");
		addMember(l,18,"FunctionNotFound");
		addMember(l,19,"BadMod");
		addMember(l,20,"NetworkError");
		addMember(l,21,"NotInitialize");
		addMember(l,22,"MustBeContainer");
		addMember(l,23,"LayoutBuildFailed");
		LuaDLL.lua_pop(l, 1);
	}
}
