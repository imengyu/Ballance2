﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using Ballance2.Config;
using Ballance2.Utils;

namespace Ballance2.Editor.Modding
{
    class ModMaker
    {
        [@MenuItem("Ballance/复制Debug文件夹到目录", false, 103)]
        static void CopyDebugFolder()
        {
            string debugFolder = DebugSettings.Instance.DebugFolder;
            string folder = EditorUtility.OpenFolderPanel("选择输出目录", EditorPrefs.GetString("CopyDebugFolderDefSaveDir", GamePathManager.DEBUG_PATH), "");
            if (string.IsNullOrEmpty(folder))
                return;
            if (Directory.Exists(folder) && Directory.Exists(folder))
            {
                if (folder != GamePathManager.DEBUG_PATH)
                    EditorPrefs.SetString("CopyDebugFolderDefSaveDir", GamePathManager.DEBUG_PATH);

                CopyDebugFolder("Core", debugFolder, folder);
                CopyDebugFolder("Mods", debugFolder, folder);
                CopyDebugFolder("Levels", debugFolder, folder);

                EditorUtility.DisplayDialog("提示", "复制成功", "确定");
            }
            else EditorUtility.DisplayDialog("错误", "文件夹不存在：\n" + debugFolder + "\n▼\n" + folder, "确定"); 
        }

        private static void CopyDebugFolder(string name, string debugFolder, string folder)
        {
            string folderCoreSrc = debugFolder + "/" + name;
            string folderCoreTarget = folder + "/" + name;
            if (Directory.Exists(folderCoreSrc))
            {
                if (!Directory.Exists(folderCoreTarget))
                    Directory.CreateDirectory(folderCoreTarget);

                DirectoryInfo direction = new DirectoryInfo(folderCoreSrc);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".ballance") || files[i].Name == "core.gameinit.txt")
                    {
                        File.Copy(folderCoreSrc + "/" + files[i].Name, folderCoreTarget + "/" + files[i].Name, true);
                    }
                }
            }
        }
    

        [@MenuItem("Ballance/模组开发/帮助", false, 100)]
        static void ShowModHelp()
        {

        }

        [@MenuItem("Ballance/模组开发/生成模组包模板", false, 100)]
        static void MakeModFile()
        {
            EditorWindow.GetWindowWithRect(typeof(ModMakerWindow), new Rect(200, 150, 450, 400));
        }

        [@MenuItem("Ballance/模组开发/打包模组包", false, 100)]
        static void PackModFile()
        {
            EditorWindow.GetWindowWithRect(typeof(ModPackerWindow), new Rect(200, 150, 450, 550));
        }
    }
}
