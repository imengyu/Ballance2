using Ballance2.UI.BallanceUI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILayout), true)]
public class UILayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UILayout myScript = (UILayout)target;
        if (GUILayout.Button("DoLayout"))
        {
            myScript.DoLayout();
        }
    }
}
