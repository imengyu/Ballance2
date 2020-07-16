using Ballance2.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 该类负责在 UnityEditor 中打包 AssetBundle
/// </summary>
public class AssetBundleMaker
{
    [@MenuItem("Tools/AssetBundle/Build From Selection/StandaloneWindows")]
    static void BuildABsPCSel()
    {
        BuildABsSelC(BuildTarget.StandaloneWindows);
    }
    [@MenuItem("Tools/AssetBundle/Build From Selection/StandaloneLinux64")]
    static void BuildABsLinuxSel()
    {
        BuildABsSelC(BuildTarget.StandaloneLinux64);
    }
    [@MenuItem("Tools/AssetBundle/Build From Selection/StandaloneOSX")]
    static void BuildABsMacSel()
    {
        BuildABsSelC(BuildTarget.StandaloneOSX);
    }
    [@MenuItem("Tools/AssetBundle/Build From Selection/Android")]
    static void BuildABsSel()
    {
        BuildABsSelC(BuildTarget.Android);
    }
    [@MenuItem("Tools/AssetBundle/Build From Selection/IOS")]
    static void BuildABsIosSel()
    {
        BuildABsSelC(BuildTarget.iOS);
    }

    static void BuildABsSelC(BuildTarget type)
    {
        string path = "";
        var option = EditorUtility.DisplayDialogComplex(
            "要保存为什么格式?",
            "Please choose one of the following options.",
            "ballance",
            "assetbundle",
            "Cancel");
        switch (option)
        {
            case 0:
                path = EditorUtility.SaveFilePanel("Save Resource", 
                    EditorPrefs.GetString("AssetBundleMakerDefSaveDir", GamePathManager.DEBUG_PATH), 
                    EditorPrefs.GetString("AssetBundleMakerDefFileName", "New Resource"), "ballance");
                break;
            case 1:
                path = EditorUtility.SaveFilePanel("Save Resource", 
                    EditorPrefs.GetString("AssetBundleMakerDefSaveDir", GamePathManager.DEBUG_PATH),
                    EditorPrefs.GetString("AssetBundleMakerDefFileName", "New Resource"), "assetbundle");
                break;
            case 2:
                return;
        }
        if (path.Length != 0)
        {
            if (path != GamePathManager.DEBUG_PATH)
                EditorPrefs.SetString("AssetBundleMakerDefSaveDir", GamePathManager.DEBUG_PATH);
            EditorPrefs.SetString("AssetBundleMakerDefFileName", Path.GetFileNameWithoutExtension(path));

            Object[] selection2 = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            Object[] selection = new Object[selection2.Length + 1];
            for (int i = 0; i < selection2.Length; i++)
                selection[i] = selection2[i];
            selection[selection2.Length] = AssetDatabase.LoadAssetAtPath("Assets/Version.txt", typeof(Object));

            BuildPipeline.BuildAssetBundle(selection[0], selection, path, BuildAssetBundleOptions.CollectDependencies, type);
        }
    }

    [@MenuItem("Tools/AssetBundle/Build All/StandaloneWindows")]
    static void BuildAllStandaloneWindows()
    {
        BuildAll(BuildTarget.StandaloneWindows);
    }
    [@MenuItem("Tools/AssetBundle/Build All/StandaloneLinux64")]
    static void BuildAllStandaloneLinux64()
    {
        BuildAll(BuildTarget.StandaloneLinux64);
    }
    [@MenuItem("Tools/AssetBundle/Build All/StandaloneOSX")]
    static void BuildAllStandaloneOSX()
    {
        BuildAll(BuildTarget.iOS);
    }
    [@MenuItem("Tools/AssetBundle/Build All/Android")]
    static void BuildAllAndroidS()
    {
        BuildAll(BuildTarget.Android);
    }
    [@MenuItem("Tools/AssetBundle/Build All/IOS")]
    static void BuildAllIOS()
    {
        BuildAll(BuildTarget.iOS);
    }

    static void BuildAll(BuildTarget type)
    {
        string path = EditorUtility.SaveFolderPanel("选择输出目录",
                       EditorPrefs.GetString("AssetBundleMakerAllDefSaveDir", GamePathManager.DEBUG_PATH),
                       EditorPrefs.GetString("AssetBundleMakerAllDefName", "New Resource"));
        if (!string.IsNullOrEmpty(path))
        {
            if (path != GamePathManager.DEBUG_PATH)
                EditorPrefs.SetString("AssetBundleMakerAllDefSaveDir", GamePathManager.DEBUG_PATH);
            EditorPrefs.SetString("AssetBundleMakerAllDefName", Path.GetFileNameWithoutExtension(path));

            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, type);
        }
    }
}
