using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIElement), true)]
public class UIElementEditor : Editor
{
    private string setPropName = "";
    private string setPropValue = "";
    private bool setPropEmpty = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIElement myScript = (UIElement)target;

        GUILayout.Label("PropName");
        setPropName = EditorGUILayout.TextField(setPropName);
        GUILayout.Label("PropValue");
        setPropValue = EditorGUILayout.TextField(setPropValue);

        if(setPropEmpty)
        {
            GUILayout.Label("Please Enter Prop name and value !");
        }

        if (GUILayout.Button("SetProp"))
        {
            if (string.IsNullOrEmpty(setPropName) || string.IsNullOrEmpty(setPropValue))
                setPropEmpty = true;
            else myScript.SetProperty(setPropName, setPropValue);
        }
    }
}
