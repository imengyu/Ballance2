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
		addMember(l,21,"ClassNotFound");
		addMember(l,22,"NotReturn");
		addMember(l,23,"BadMod");
		addMember(l,24,"BrokenMod");
		addMember(l,25,"NetworkError");
		addMember(l,26,"NotInitialize");
		addMember(l,27,"NotLoad");
		addMember(l,28,"InProgress");
		addMember(l,29,"AlredayLoaded");
		addMember(l,30,"MustBeContainer");
		addMember(l,31,"LayoutBuildFailed");
		addMember(l,32,"ContextMismatch");
		addMember(l,33,"BaseModuleCannotReplace");
		LuaDLL.lua_pop(l, 1);
	}
}
