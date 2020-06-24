using System;
using System.Collections.Generic;
namespace SLua {
	[LuaBinder(3)]
	public class BindCustom {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_CustomData.reg,
				Lua_Ballance2_GameLogger.reg,
				Lua_Ballance2_GameManager.reg,
				Lua_Ballance2_Utils_CommonUtils.reg,
				Lua_Ballance2_Utils_GameCloneUtils.reg,
				Lua_Ballance2_Utils_GameErrorManager.reg,
				Lua_Ballance2_Utils_GameError.reg,
				Lua_Ballance2_Utils_GamePathManager.reg,
				Lua_Ballance2_Utils_SkyBoxUtils.reg,
				Lua_Ballance2_Utils_StringSpliter.reg,
				Lua_Ballance2_Utils_StringUtils.reg,
				Lua_Ballance2_UI_Utils_EventTriggerListener.reg,
				Lua_Ballance2_UI_Utils_UIAnchorPosUtils.reg,
				Lua_Ballance2_UI_Utils_UIPivot.reg,
				Lua_Ballance2_UI_Utils_UIAnchor.reg,
				Lua_Ballance2_UI_Utils_UIContentSizeUtils.reg,
				Lua_Ballance2_UI_Utils_KeyListener.reg,
				Lua_Ballance2_UI_BallanceUI_UIElement.reg,
				Lua_Ballance2_UI_BallanceUI_UILayout.reg,
				Lua_Ballance2_UI_BallanceUI_Layout_UILinearLayout.reg,
				Lua_Ballance2_UI_BallanceUI_UIHorizontalLinearLayout.reg,
				Lua_Ballance2_UI_BallanceUI_UIVerticalLinearLayout.reg,
				Lua_Ballance2_UI_BallanceUI_UIVisibility.reg,
				Lua_Ballance2_UI_BallanceUI_UILayoutUtils.reg,
				Lua_Ballance2_UI_BallanceUI_UIPage.reg,
				Lua_Ballance2_UI_BallanceUI_UIWindow.reg,
				Lua_Ballance2_UI_BallanceUI_WindowType.reg,
				Lua_Ballance2_UI_BallanceUI_IWindow.reg,
				Lua_Ballance2_UI_BallanceUI_Layout_UIRelativeLayout.reg,
				Lua_Ballance2_UI_BallanceUI_Element_UIButton.reg,
				Lua_Ballance2_UI_BallanceUI_Element_UISpace.reg,
				Lua_Ballance2_UI_BallanceUI_Element_UIText.reg,
				Lua_Ballance2_Managers_BaseManager.reg,
				Lua_Ballance2_Managers_DebugManager.reg,
				Lua_Ballance2_Managers_GameMediator.reg,
				Lua_Ballance2_Managers_ModManager.reg,
				Lua_Ballance2_Managers_SoundManager.reg,
				Lua_Ballance2_Managers_UIManager.reg,
				Lua_Ballance2_Managers_CoreBridge_GameEvent.reg,
				Lua_Ballance2_Managers_CoreBridge_GameEventNames.reg,
				Lua_Ballance2_Managers_CoreBridge_GameHandler.reg,
				Lua_Ballance2_Managers_CoreBridge_GameHandlerType.reg,
				Lua_Ballance2_Managers_CoreBridge_GameHandlerList.reg,
				Lua_Ballance2_Managers_CoreBridge_GameLuaObjectHost.reg,
				Lua_Ballance2_Managers_CoreBridge_GameMod.reg,
				Lua_Ballance2_Config_GameConst.reg,
				Lua_Ballance2_GameLogger_LogType.reg,
				Lua_Ballance2_GameManager_GameMode.reg,
				Lua_System_Collections_Generic_List_1_int.reg,
				Lua_System_String.reg,
			};
			return list;
		}
	}
}
