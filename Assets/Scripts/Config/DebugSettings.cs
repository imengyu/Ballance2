#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
using UnityEngine;

namespace Ballance2.Config
{
	/// <summary>
	/// ��������
	/// </summary>
	public class DebugSettings : ScriptableObject
	{
		[Tooltip("���� Ballance ������� ����ģ����ļ���·��")]
		public string DebugFolder = "";
		[Tooltip("(��Editorģʽ��Ч)�����Ƿ�Ĭ���� Editor ��Ŀ�м���ģ����������Ŀ����ģ����Ļ���")]
		public bool ModLoadInEditor = false;
		[Tooltip("(��Editorģʽ��Ч)�����Ƿ�Ĭ���� Editor ��Ŀ�м��� core.gameinit.txt")]
		public bool GameInitLoadInEditor = false;

		private static DebugSettings _instance = null;
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
		[MenuItem("Ballance/��������/Debug Settings", priority = 298)]
		public static void Open()
		{
			Selection.activeObject = Instance;
		}
#endif

	}
}