using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_Ballance2_UI_BallanceUI_UILayoutBuilder : LuaObject {
	[SLua.MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
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
			Ballance2.UI.BallanceUI.UILayoutBuilder o;
			Ballance2.Managers.UIManager a1;
			checkType(l,2,out a1);
			o=new Ballance2.UI.BallanceUI.UILayoutBuilder(a1);
			pushValue(l,true);
			pushValue(l,o);
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
	static public int BuildLayoutByTemplate(IntPtr l) {
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
			if(argc==5){
				Ballance2.UI.BallanceUI.UILayoutBuilder self=(Ballance2.UI.BallanceUI.UILayoutBuilder)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.String a2;
				checkType(l,3,out a2);
				System.Collections.Generic.Dictionary<System.String,Ballance2.CoreBridge.GameHandler> a3;
				checkType(l,4,out a3);
				System.String[] a4;
				checkArray(l,5,out a4);
				var ret=self.BuildLayoutByTemplate(a1,a2,a3,a4);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(string),typeof(System.String[]),typeof(System.String[]),typeof(System.String[]))){
				Ballance2.UI.BallanceUI.UILayoutBuilder self=(Ballance2.UI.BallanceUI.UILayoutBuilder)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.String a2;
				checkType(l,3,out a2);
				System.String[] a3;
				checkArray(l,4,out a3);
				System.String[] a4;
				checkArray(l,5,out a4);
				System.String[] a5;
				checkArray(l,6,out a5);
				var ret=self.BuildLayoutByTemplate(a1,a2,a3,a4,a5);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(string),typeof(string),typeof(System.String[]),typeof(Ballance2.CoreBridge.GameEventHandlerDelegate[]),typeof(System.String[]))){
				Ballance2.UI.BallanceUI.UILayoutBuilder self=(Ballance2.UI.BallanceUI.UILayoutBuilder)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.String a2;
				checkType(l,3,out a2);
				System.String[] a3;
				checkArray(l,4,out a3);
				Ballance2.CoreBridge.GameEventHandlerDelegate[] a4;
				checkArray(l,5,out a4);
				System.String[] a5;
				checkArray(l,6,out a5);
				var ret=self.BuildLayoutByTemplate(a1,a2,a3,a4,a5);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(argc==7){
				Ballance2.UI.BallanceUI.UILayoutBuilder self=(Ballance2.UI.BallanceUI.UILayoutBuilder)checkSelf(l);
				System.String a1;
				checkType(l,2,out a1);
				System.String a2;
				checkType(l,3,out a2);
				System.String[] a3;
				checkArray(l,4,out a3);
				SLua.LuaFunction[] a4;
				checkArray(l,5,out a4);
				SLua.LuaTable a5;
				checkType(l,6,out a5);
				System.String[] a6;
				checkArray(l,7,out a6);
				var ret=self.BuildLayoutByTemplate(a1,a2,a3,a4,a5,a6);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function BuildLayoutByTemplate to call");
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
		getTypeTable(l,"Ballance2.UI.BallanceUI.UILayoutBuilder");
		addMember(l,BuildLayoutByTemplate);
		createTypeMetatable(l,constructor, typeof(Ballance2.UI.BallanceUI.UILayoutBuilder));
	}
}
