using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Utils_GameErrorManager : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetLastErrorAndLog_s(IntPtr l) {
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
			int argc = LuaDLL.lua_gettop(l);
			if(argc==3){
				Ballance2.Utils.GameError a1;
				a1 = (Ballance2.Utils.GameError)LuaDLL.luaL_checkinteger(l, 1);
				System.String a2;
				checkType(l,2,out a2);
				System.String a3;
				checkType(l,3,out a3);
				Ballance2.Utils.GameErrorManager.SetLastErrorAndLog(a1,a2,a3);
				pushValue(l,true);
				return 1;
			}
			else if(argc==4){
				Ballance2.Utils.GameError a1;
				a1 = (Ballance2.Utils.GameError)LuaDLL.luaL_checkinteger(l, 1);
				System.String a2;
				checkType(l,2,out a2);
				System.String a3;
				checkType(l,3,out a3);
				System.Object[] a4;
				checkParams(l,4,out a4);
				Ballance2.Utils.GameErrorManager.SetLastErrorAndLog(a1,a2,a3,a4);
				pushValue(l,true);
				return 1;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function SetLastErrorAndLog to call");
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
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int ThrowGameError_s(IntPtr l) {
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
			Ballance2.Utils.GameError a1;
			a1 = (Ballance2.Utils.GameError)LuaDLL.luaL_checkinteger(l, 1);
			System.String a2;
			checkType(l,2,out a2);
			Ballance2.Utils.GameErrorManager.ThrowGameError(a1,a2);
			pushValue(l,true);
			return 1;
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
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_LastError(IntPtr l) {
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
			pushValue(l,true);
			pushEnum(l,(int)Ballance2.Utils.GameErrorManager.LastError);
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
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_LastError(IntPtr l) {
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
			Ballance2.Utils.GameError v;
			v = (Ballance2.Utils.GameError)LuaDLL.luaL_checkinteger(l, 2);
			Ballance2.Utils.GameErrorManager.LastError=v;
			pushValue(l,true);
			return 1;
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
		getTypeTable(l,"Ballance2.Utils.GameErrorManager");
		addMember(l,SetLastErrorAndLog_s);
		addMember(l,ThrowGameError_s);
		addMember(l,"LastError",get_LastError,set_LastError,false);
		createTypeMetatable(l,null, typeof(Ballance2.Utils.GameErrorManager));
	}
}
