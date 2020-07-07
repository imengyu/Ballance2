using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Interfaces_IICManager : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int BackupIC(IntPtr l) {
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
			if(argc==2){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				var ret=self.BackupIC(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				Ballance2.Interfaces.ICBackType a2;
				a2 = (Ballance2.Interfaces.ICBackType)LuaDLL.luaL_checkinteger(l, 3);
				var ret=self.BackupIC(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function BackupIC to call");
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
	static public int RemoveIC(IntPtr l) {
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
			if(argc==2){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				var ret=self.RemoveIC(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				Ballance2.Interfaces.ICBackType a2;
				a2 = (Ballance2.Interfaces.ICBackType)LuaDLL.luaL_checkinteger(l, 3);
				var ret=self.RemoveIC(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function RemoveIC to call");
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
	static public int ResetIC(IntPtr l) {
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
			if(argc==2){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				var ret=self.ResetIC(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==3){
				Ballance2.Interfaces.IICManager self=(Ballance2.Interfaces.IICManager)checkSelf(l);
				UnityEngine.GameObject a1;
				checkType(l,2,out a1);
				Ballance2.Interfaces.ICBackType a2;
				a2 = (Ballance2.Interfaces.ICBackType)LuaDLL.luaL_checkinteger(l, 3);
				var ret=self.ResetIC(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function ResetIC to call");
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
		getTypeTable(l,"Ballance2.Interfaces.IICManager");
		addMember(l,BackupIC);
		addMember(l,RemoveIC);
		addMember(l,ResetIC);
		createTypeMetatable(l,null, typeof(Ballance2.Interfaces.IICManager));
	}
}
