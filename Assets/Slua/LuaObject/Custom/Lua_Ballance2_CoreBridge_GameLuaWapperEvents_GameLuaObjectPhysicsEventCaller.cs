﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_CoreBridge_GameLuaWapperEvents_GameLuaObjectPhysicsEventCaller : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetEventType(IntPtr l) {
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
			Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller self=(Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller)checkSelf(l);
			var ret=self.GetEventType();
			pushValue(l,true);
			pushEnum(l,(int)ret);
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
	static public int GetSupportEvents(IntPtr l) {
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
			Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller self=(Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller)checkSelf(l);
			var ret=self.GetSupportEvents();
			pushValue(l,true);
			pushValue(l,ret);
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
	static public int OnInitLua(IntPtr l) {
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
			Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller self=(Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller)checkSelf(l);
			Ballance2.CoreBridge.GameLuaObjectHost a1;
			checkType(l,2,out a1);
			self.OnInitLua(a1);
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
		getTypeTable(l,"Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller");
		addMember(l,GetEventType);
		addMember(l,GetSupportEvents);
		addMember(l,OnInitLua);
		createTypeMetatable(l,null, typeof(Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectPhysicsEventCaller),typeof(Ballance2.CoreBridge.GameLuaWapperEvents.GameLuaObjectEventCaller));
	}
}
