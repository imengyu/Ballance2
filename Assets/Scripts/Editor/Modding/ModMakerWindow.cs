using Ballance2.Config;
using Ballance2.ModBase;
using Ballance2.Utils;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Ballance2.Editor.Modding
{
    public class ModMakerWindow : EditorWindow
    {
        public ModMakerWindow()
        {
            titleContent = new GUIContent("创建 Ballance 模组包");
        }

        private string modPackageName = "com.yourname.modname";
        private string modName = "";
        private string modAuthor = "";
        private string modIntroduction = "";
        private string modVersion = "1.0";
        private bool GenEntryCodeTemplate = true;
        private GameModType ModType = GameModType.NotSet;
        private string EntryCode = "Entry.lua.txt";

        private SerializedObject serializedObject;

        private GUIStyle groupBox = null;
        private bool error = false;

        private TextAsset template_ModDef;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);

            template_ModDef = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Mods/template_ModDef.xml");
        }

        private void OnGUI()
        {
            serializedObject.Update();

            if (groupBox == null)
                groupBox = GUI.skin.FindStyle("GroupBox");

            error = false;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(groupBox);

            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(new GUIContent("使用这个工具来快速生成一个模组包模板，生成文件会输出到 Assets/Mods 下。"), true);
            EditorGUILayout.Space(15);

            modPackageName = EditorGUILayout.TextField("模组包名", modPackageName);
            if (StringUtils.isNullOrEmpty(modPackageName))
            {
                EditorGUILayout.HelpBox("必须填写模组包名", MessageType.Error);
                error = true;
            }
            if (!StringUtils.IsPackageName(modPackageName))
            {
                EditorGUILayout.HelpBox("包名必须是 com.xxx.xxx 格式", MessageType.Error);
                error = true;
            }
            
            modName = EditorGUILayout.TextField("模组名称", modName);
            modAuthor = EditorGUILayout.TextField("模组作者名字", modAuthor);
            modIntroduction = EditorGUILayout.TextField("模组简介文字", modIntroduction, GUILayout.Height(60));
            modVersion = EditorGUILayout.TextField("模组版本（默认1.0）", modVersion);

            ModType = (GameModType)EditorGUILayout.EnumPopup("模组类型", ModType);
            if (ModType == GameModType.NotSet)
            {
                EditorGUILayout.HelpBox("请选择模组类型", MessageType.Error);
                error = true;
            }
            if (ModType == GameModType.Level)
            {
                EditorGUILayout.HelpBox("这个选项已经弃用，如果要生成关卡，请在“关卡制作”中创建", MessageType.Warning);
                error = true;
            }
            if (ModType == GameModType.ModulePack)
            {
                GenEntryCodeTemplate = EditorGUILayout.Toggle("生成模组入口代码模板", GenEntryCodeTemplate);
                if (GenEntryCodeTemplate)
                {
                    EntryCode = EditorGUILayout.TextField("入口代码名称", EntryCode);
                    if (StringUtils.isNullOrEmpty(EntryCode))
                    {
                        EditorGUILayout.HelpBox("必须填写入口代码名称， xxx.lula.txt ", MessageType.Error);
                        error = true;
                    }
                }
            }

            EditorGUILayout.Space(20);

            if(GUILayout.Button("生成") && !error)
            {
                Make();
            }

            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void Make()
        {
            string folderPath = GamePathManager.DEBUG_MOD_FOLDER + "/" + modPackageName;
            if (Directory.Exists(folderPath))
            {
                if (!EditorUtility.DisplayDialog("提示", "指定包名模组 " + modPackageName + " 已经在： \n" +
                    folderPath + "\n存在了，是否要替换？", "替换", "取消"))
                    return;
            }

            DirectoryInfo directoryInfo = Directory.CreateDirectory(folderPath);
            if(directoryInfo == null)
            {
                EditorUtility.DisplayDialog("错误", "创建文件夹失败：\n" + folderPath, "确定");
                return;
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(template_ModDef.text);

            XmlNode BaseInfo = xml.SelectSingleNode("Mod/BaseInfo");
            XmlNode Name = BaseInfo.SelectSingleNode("Name");
            XmlNode Author = BaseInfo.SelectSingleNode("Author");
            XmlNode Introduction = BaseInfo.SelectSingleNode("Introduction");
            XmlNode Version = BaseInfo.SelectSingleNode("Version");

            BaseInfo.Attributes["packageName"].InnerText = modPackageName;
            Name.InnerText = modName;
            Author.InnerText = modAuthor;
            Introduction.InnerText = modIntroduction;
            Version.InnerText = modVersion;

            XmlNode MinVersion = xml.SelectSingleNode("Mod/Compatibility/MinVersion");
            XmlNode TargetVersion = xml.SelectSingleNode("Mod/Compatibility/TargetVersion");

            MinVersion.InnerText = GameConst.GameBulidVersion.ToString();
            TargetVersion.InnerText = GameConst.GameBulidVersion.ToString();

            XmlNode EntryCode = xml.SelectSingleNode("Mod/EntryCode");
            XmlNode ModType = xml.SelectSingleNode("Mod/ModType");

            if (!this.EntryCode.Contains("."))
                this.EntryCode += ".lua.txt";
            if (!this.EntryCode.EndsWith(".txt"))
                this.EntryCode += ".txt";

            EntryCode.InnerText = this.EntryCode;
            ModType.InnerText = this.ModType.ToString();

            xml.Save(folderPath + "/ModDef.xml");

            if(GenEntryCodeTemplate)
                File.Copy("Assets/Mods/template_Entry.lua.txt", folderPath + "/" + this.EntryCode);

            File.Copy("Assets/Mods/template_ModLogo.png", folderPath + "/ModLogo.png");

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("提示", "生成模板成功！", "好的");

            Close();
        }
    }
}
