using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

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
            EditorWindow.GetWindow(typeof(ModMakerWindow));
        }

        [@MenuItem("Ballance/模组开发/打包模组包")]
        static void PackModFile()
        {
            EditorWindow.GetWindow(typeof(ModPackerWindow)); 
        }
    }
}
