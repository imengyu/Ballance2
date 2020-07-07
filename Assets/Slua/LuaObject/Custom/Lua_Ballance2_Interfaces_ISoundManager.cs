using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_Interfaces_ISoundManager : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int LoadAudioResource(IntPtr l) {
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
			Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			var ret=self.LoadAudioResource(a1);
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
	static public int RegisterSoundPlayer(IntPtr l) {
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
				Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
				Ballance2.Interfaces.GameSoundType a1;
				a1 = (Ballance2.Interfaces.GameSoundType)LuaDLL.luaL_checkinteger(l, 2);
				UnityEngine.AudioSource a2;
				checkType(l,3,out a2);
				var ret=self.RegisterSoundPlayer(a1,a2);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(Ballance2.Interfaces.GameSoundType),typeof(UnityEngine.AudioClip),typeof(bool),typeof(bool),typeof(string))){
				Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
				Ballance2.Interfaces.GameSoundType a1;
				a1 = (Ballance2.Interfaces.GameSoundType)LuaDLL.luaL_checkinteger(l, 2);
				UnityEngine.AudioClip a2;
				checkType(l,3,out a2);
				System.Boolean a3;
				checkType(l,4,out a3);
				System.Boolean a4;
				checkType(l,5,out a4);
				System.String a5;
				checkType(l,6,out a5);
				var ret=self.RegisterSoundPlayer(a1,a2,a3,a4,a5);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(Ballance2.Interfaces.GameSoundType),typeof(string),typeof(bool),typeof(bool),typeof(string))){
				Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
				Ballance2.Interfaces.GameSoundType a1;
				a1 = (Ballance2.Interfaces.GameSoundType)LuaDLL.luaL_checkinteger(l, 2);
				System.String a2;
				checkType(l,3,out a2);
				System.Boolean a3;
				checkType(l,4,out a3);
				System.Boolean a4;
				checkType(l,5,out a4);
				System.String a5;
				checkType(l,6,out a5);
				var ret=self.RegisterSoundPlayer(a1,a2,a3,a4,a5);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function RegisterSoundPlayer to call");
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
	static public int IsSoundPlayerRegistered(IntPtr l) {
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
			Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
			UnityEngine.AudioSource a1;
			checkType(l,2,out a1);
			var ret=self.IsSoundPlayerRegistered(a1);
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
	static public int DestroySoundPlayer(IntPtr l) {
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
			Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
			UnityEngine.AudioSource a1;
			checkType(l,2,out a1);
			var ret=self.DestroySoundPlayer(a1);
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
	static public int PlayFastVoice(IntPtr l) {
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
			Ballance2.Interfaces.ISoundManager self=(Ballance2.Interfaces.ISoundManager)checkSelf(l);
			System.String a1;
			checkType(l,2,out a1);
			Ballance2.Interfaces.GameSoundType a2;
			a2 = (Ballance2.Interfaces.GameSoundType)LuaDLL.luaL_checkinteger(l, 3);
			var ret=self.PlayFastVoice(a1,a2);
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
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"Ballance2.Interfaces.ISoundManager");
		addMember(l,LoadAudioResource);
		addMember(l,RegisterSoundPlayer);
		addMember(l,IsSoundPlayerRegistered);
		addMember(l,DestroySoundPlayer);
		addMember(l,PlayFastVoice);
		createTypeMetatable(l,null, typeof(Ballance2.Interfaces.ISoundManager));
	}
}
