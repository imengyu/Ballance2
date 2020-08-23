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
		addMember(l,7,"NotFinished");
		addMember(l,8,"NotRegister");
		addMember(l,9,"ParamNotProvide");
		addMember(l,10,"Empty");
		addMember(l,11,"PrefabNotFound");
		addMember(l,12,"AssetsNotFound");
		addMember(l,13,"InitializationFailed");
		addMember(l,14,"BadAssetBundle");
		addMember(l,15,"BadFileType");
		addMember(l,16,"FileNotFound");
		addMember(l,17,"ModConflict");
		addMember(l,18,"ModDependenciesLoadFailed");
		addMember(l,19,"ModExecutionCodeRunFailed");
		addMember(l,20,"ModCanNotRun");
		addMember(l,21,"FunctionNotFound");
		addMember(l,22,"ClassNotFound");
		addMember(l,23,"NotReturn");
		addMember(l,24,"BadMod");
		addMember(l,25,"BrokenMod");
		addMember(l,26,"NetworkError");
		addMember(l,27,"NotInitialize");
		addMember(l,28,"NotLoad");
		addMember(l,29,"InProgress");
		addMember(l,30,"AlredayLoaded");
		addMember(l,31,"MustBeContainer");
		addMember(l,32,"LayoutBuildFailed");
		addMember(l,33,"ContextMismatch");
		addMember(l,34,"BaseModuleCannotReplace");
		LuaDLL.lua_pop(l, 1);
	}
}
