using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreGame_Interfaces_ILevelLoader : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_LevelLoadStatus(IntPtr l) {
		try {
			#if DEBUG
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			string methodName = GetMethodName(method);
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.BeginSample(methodName);
			#else
			Profiler.BeginSample(methodName);
			#endif
			#endif
			Ballance2.CoreGame.Interfaces.ILevelLoader self=(Ballance2.CoreGame.Interfaces.ILevelLoader)checkSelf(l);
			pushValue(l,true);
			pushEnum(l,(int)self.LevelLoadStatus);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
		#if DEBUG
		finally {
			#if UNITY_5_5_OR_NEWER
			UnityEngine.Profiling.Profiler.EndSample();
			#else
			Profiler.EndSample();
			#endif
		}
		#endif
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"Ballance2.CoreGame.Interfaces.ILevelLoader");
		addMember(l,"LevelLoadStatus",get_LevelLoadStatus,null,true);
		createTypeMetatable(l,null, typeof(Ballance2.CoreGame.Interfaces.ILevelLoader));
	}
}
