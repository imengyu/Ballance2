using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreGame_GamePlay_GameBallLuaWapper : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_Base(IntPtr l) {
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
			Ballance2.CoreGame.GamePlay.GameBallLuaWapper self=(Ballance2.CoreGame.GamePlay.GameBallLuaWapper)checkSelf(l);
			pushValue(l,true);
			pushValue(l,self.Base);
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
		getTypeTable(l,"Ballance2.CoreGame.GamePlay.GameBallLuaWapper");
		addMember(l,"Base",get_Base,null,true);
		createTypeMetatable(l,null, typeof(Ballance2.CoreGame.GamePlay.GameBallLuaWapper),typeof(Ballance2.CoreGame.GamePlay.GameBall));
	}
}
