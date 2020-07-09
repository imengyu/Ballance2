using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ballance2.Editor.Modding
{
    public class ModPackerWindow : EditorWindow
    {
        public ModPackerWindow()
        {
            titleContent = new GUIContent("打包 Ballance 模组包");
            minSize = new Vector2(350, 300);
        }

        private SerializedObject serializedObject;
        private SerializedProperty pmodDefFile = null;
        private SerializedProperty pmodTarget = null;

        private bool isError = false;
        private string errStr = "";
        private bool isResult = false;

        [SerializeField]
        private TextAsset modDefFile = null;
        [SerializeField]
        private BuildTarget modTarget = BuildTarget.NoTarget;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            pmodDefFile = serializedObject.FindProperty("modDefFile");
            pmodTarget = serializedObject.FindProperty("modTarget");
        }
        private void OnGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(pmodDefFile, new GUIContent("请选择 ModDef.xml "));
            if (GUILayout.Button("设置为编辑器中选中的条目",  GUILayout.Width(130)))
                SelectInEditor();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(pmodTarget, new GUIContent("目标平台 "));

            GUILayout.Space(50);

            if (GUILayout.Button("打包"))
                DoPack();

            GUILayout.Space(5);

            if(isError)
                EditorGUILayout.HelpBox(errStr, MessageType.Error);
            if(isResult)
            {
                EditorGUILayout.BeginVertical(GUI.skin.GetStyle("window"));
                GUILayout.Space(3);
                GUILayout.Label("所有资源:");
                foreach(string s in allAssetsPath)
                    GUILayout.Label(s);
                EditorGUILayout.EndVertical();


                if (GUILayout.Button("清空"))
                    isResult = false;
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private List<string> allAssetsPath = new List<string>();

        private void SelectInEditor()
        {
            if(Selection.activeObject == null)
            {
                EditorUtility.DisplayDialog("提示", "请先在编辑器中选择你的 ModDef.xml 哦", "好的");
                return;
            }
            if(Selection.activeObject.GetType() != typeof(TextAsset))
            {
                EditorUtility.DisplayDialog("提示", "你选择的 ModDef.xml 哦文件格式不对，必须是 TextAsset ", "好的");
                return;
            }
            modDefFile = Selection.activeObject as TextAsset;
        }
        private void DoPack()
        {
            isError = false;

            if (modDefFile == null)
            {
                isError = true;
                errStr = "请选择的 ModDef.xml ";
                return;
            }
            if (modTarget == BuildTarget.NoTarget)
            {
                isError = true;
                errStr = "请选择目标平台";
                return;
            }
            if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(modTarget), modTarget))
            {
                isError = true;
                errStr = "你的 Unity 似乎不支持目标平台 "  + modTarget + " 的编译，可能你没有安装对应模块";
                return;
            }

            string path = EditorUtility.SaveFilePanel("保存模组包",
                   EditorPrefs.GetString("ModMakerDefSaveDir", GamePathManager.DEBUG_PATH),
                   EditorPrefs.GetString("ModMakerDefFileName", "New Mod"), "ballance");

            if (!string.IsNullOrEmpty(path))
            {
                if (path != GamePathManager.DEBUG_PATH)
                    EditorPrefs.SetString("ModMakerDefSaveDir", GamePathManager.DEBUG_PATH);
                EditorPrefs.SetString("ModMakerDefFileName", Path.GetFileNameWithoutExtension(path));

                allAssetsPath.Clear();
                string projPath = Directory.GetCurrentDirectory().Replace("\\", "/") + "/";
                string dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(modDefFile));
                if (!string.IsNullOrEmpty(dirPath))
                {
                    if (Directory.Exists(dirPath))
                    {
                        DirectoryInfo direction = new DirectoryInfo(dirPath);
                        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                        for (int i = 0; i < files.Length; i++)
                        {
                            if (files[i].Name.EndsWith(".meta")) continue;
                            allAssetsPath.Add(files[i].FullName.Replace("\\", "/").Replace(projPath, ""));
                        }
                        isResult = true;
                    }

                    return;
                    
                    AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                    assetBundleBuild.assetBundleName = Path.GetFileNameWithoutExtension(path);
                    assetBundleBuild.assetBundleVariant = "Mod";
                    assetBundleBuild.assetNames = allAssetsPath.ToArray();

                    BuildPipeline.BuildAssetBundles(Path.GetDirectoryName(path), new AssetBundleBuild[]{
                        assetBundleBuild
                    }, BuildAssetBundleOptions.None, modTarget);

                    Close();
                }
                else
                {
                    isError = true;
                    errStr = "选择的 ModDef.xml 不在本项目中 ";
                }
            }
            else
            {
                isError = true;
                errStr = "您取消了保存 ";
            }
        }

    }
}
