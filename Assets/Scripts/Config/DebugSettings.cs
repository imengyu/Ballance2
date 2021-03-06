#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

/*
* Copyright (c) 2020  mengyu
* 
* 模块名： 
* DebugSettings.cs
* 
* 用途：
* 获取调试设置。
* 你可以在Unity编辑器中点击 Ballance/开发设置/Debug Settings 菜单来配置自己的开发设置。
* 
* 作者：
* mengyu
* 
* 更改历史：
* 2020-6-12 创建
* 
*/

namespace Ballance2.Config
{
	/// <summary>
	/// 调试设置
	/// </summary>
	public class DebugSettings : ScriptableObject
	{
		[Tooltip("设置 Ballance 调试输出 组与模块的文件夹路径")]
		public string DebugFolder = "";
		[Tooltip("(仅Editor模式有效)设置是否默认在 Editor 项目中加载模组包（如果项目内有模组包的话）")]
		public bool ModLoadInEditor = false;
		[Tooltip("(仅Editor模式有效)设置是否默认在 Editor 项目中加载 core.gameinit.txt")]
		public bool GameInitLoadInEditor = false;

		private static DebugSettings _instance = null;

		/// <summary>
		/// 获取调试设置实例
		/// </summary>
		public static DebugSettings Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<DebugSettings>("DebugSettings");
#if UNITY_EDITOR
					if (_instance == null)
					{
						_instance = CreateInstance<DebugSettings>();
						try
						{
							AssetDatabase.CreateAsset(_instance, "Assets/Resources/DebugSettings.asset");
						}
						catch(Exception e)
						{
							Debug.LogError("CreateInstance DebugSetting.asset failed!" + e.Message + "\n\n" + e.ToString());
						}
					}
#endif
				}
				return _instance;
			}
		}

#if UNITY_EDITOR 
		[MenuItem("Ballance/开发设置/Debug Settings", priority = 298)]
		public static void Open()
		{
			Selection.activeObject = Instance;
		}
#endif

	}
}