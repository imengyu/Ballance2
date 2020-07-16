using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Ballance2.Editor.Modding
{
    class ModMaker
    {
        [@MenuItem("Ballance/帮助", false, 100)]
        static void ShowHelp()
        {

        }
        [@MenuItem("Ballance/模组开发/帮助", false, 100)]
        static void ShowModHelp()
        {

        }

        [@MenuItem("Ballance/模组开发/创建模组包")]
        static void MakeModFile()
        {
            EditorWindow.GetWindowWithRect(typeof(ModMakerWindow), new Rect(200, 150, 450, 550));
        }

        [@MenuItem("Ballance/模组开发/打包模组包")]
        static void PackModFile()
        {
            EditorWindow.GetWindowWithRect(typeof(ModPackerWindow), new Rect(200, 150, 450, 550));
        }
    }
}
