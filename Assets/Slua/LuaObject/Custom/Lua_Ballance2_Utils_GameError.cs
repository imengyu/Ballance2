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
		addMember(l,7,"NotRegister");
		addMember(l,8,"ParamNotProvide");
		addMember(l,9,"Empty");
		addMember(l,10,"PrefabNotFound");
		addMember(l,11,"AssetsNotFound");
		addMember(l,12,"InitializationFailed");
		addMember(l,13,"BadAssetBundle");
		addMember(l,14,"BadFileType");
		addMember(l,15,"FileNotFound");
		addMember(l,16,"ModConflict");
		addMember(l,17,"ModDependenciesLoadFailed");
		addMember(l,18,"ModExecutionCodeRunFailed");
		addMember(l,19,"ModCanNotRun");
		addMember(l,20,"FunctionNotFound");
		addMember(l,21,"NotReturn");
		addMember(l,22,"BadMod");
		addMember(l,23,"NetworkError");
		addMember(l,24,"NotInitialize");
		addMember(l,25,"NotLoad");
		addMember(l,26,"InProgress");
		addMember(l,27,"AlredayLoaded");
		addMember(l,28,"MustBeContainer");
		addMember(l,29,"LayoutBuildFailed");
		addMember(l,30,"ContextMismatch");
		addMember(l,31,"BaseModuleCannotReplace");
		LuaDLL.lua_pop(l, 1);
	}
}
