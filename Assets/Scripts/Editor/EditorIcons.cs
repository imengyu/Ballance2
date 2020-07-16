using System;
using UnityEditor;
using UnityEngine;

namespace Ballance2.Editor
{
    class EditorIcons : EditorWindow
    {
        static string[] text;

        public EditorIcons()
        {
            titleContent = new GUIContent("Unity 系统内置图标查看");
        }

        [MenuItem("Tools/GUI/Editor Icon")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(EditorIcons));
        }

        public Vector2 scrollPosition;

        private GUIStyle window = null;

        private void OnEnable()
        {
            
        }
        private void OnGUI()
        {
            if(window == null)
                window = GUI.skin.FindStyle("window");

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(window, GUILayout.Width(120));//鼠标放在按钮上的样式
            GUILayout.Label("鼠标放在按钮上的样式");

            foreach (MouseCursor item in Enum.GetValues(typeof(MouseCursor)))
            {
                GUILayout.Button(Enum.GetName(typeof(MouseCursor), item));
                EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), item);
                GUILayout.Space(10);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(window);   //内置图标
            GUILayout.Label("内置图标");

            for (int i = 0; i < text.Length; i += 8)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 8; j++)
                {
                    int index = i + j;
                    if (index < text.Length)
                        GUILayout.Button(EditorGUIUtility.IconContent(text[index]), GUILayout.Width(50), GUILayout.Height(30));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }
    }
}
