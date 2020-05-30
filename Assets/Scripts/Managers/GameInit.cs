using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using Ballance2.Managers.CoreBridge;
using System.Collections;
using UnityEngine.Networking;
using Ballance2.Utils;

namespace Ballance2.Managers
{
    public class GameInit : BaseManagerBindable
    {
        public const string TAG = "GameInit";

        public GameInit() : base(TAG, "Singleton")
        {
        }

        public override bool InitManager()
        {
            GameManager.GameMediator.RegisterEventKernalHandler(
                GameEventNames.EVENT_GAME_INIT_ENTRY, TAG, (e, p) =>  
                { StartCoroutine(GameInitModuls()); return false; });
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        /*
         * GameInit.txt 格式
         * 
         * 格式：REQUIRED/OPTIONAL:CORE/MOD:资源路径
         * 必须/可选模组       内核(游戏内置)/外部模组    文件路径 可使用 [Platform] 来代表平台，其会替换为
         * 指定平台字符串：win/android/ios 等等
         *
         * eg : REQUIRED:CORE:assets/musics_[Platform].unity3d
         *          REQUIRED:CORE:scenses/menulevel.unity3d
         * 
         */

        //加载 GameInit.txt 中的模块
        private IEnumerator GameInitModuls()
        {
            ModManager ModManager = (ModManager)GameManager.GetManager(ModManager.TAG);
            GameManager.GameMediator.RegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);

            string gameinit_txt_path = GamePathManager.GetResRealPath("gameinit", "");
            UnityWebRequest request = UnityWebRequest.Get(gameinit_txt_path);
            yield return request.SendWebRequest();
            if (request.isDone && string.IsNullOrEmpty(request.error))
            {
                string GameInitTable = request.downloadHandler.text;
                StringSpliter sp = new StringSpliter(GameInitTable, '\n');
                if (sp.Count >= 1)
                {
                    int loadedCount = 0;
                    foreach (string ar in sp.Result)
                    {
                        bool required = false;
                        string fullPath = ar;
                        if (ar.StartsWith(":")) continue;
                        else if (ar.StartsWith("REQUIRED:"))
                        {
                            fullPath = GamePathManager.GetResRealPath("", fullPath.Substring(9));
                            required = true;
                        }
                        else if (ar.StartsWith("OPTIONAL:"))
                            fullPath = GamePathManager.GetResRealPath("", fullPath.Substring(10));

                        //状态
                        loadedCount++;
                        //GameBulider.GameUIManager.GameBuliderUI.SetProgress((int)(100 * (loadedCount / (float)sp.Count)));
                        //GameBulider.GameUIManager.GameBuliderUI.SetProgressText("Loading " + fullPath);

                        //加载
                        int modUid = ModManager.LoadGameMod(fullPath, false);
                        GameMod mod = ModManager.FindGameMod(modUid);
                        //等待加载
                        yield return StartCoroutine(mod.LoadInternal());

                        if (mod.LoadStatus == GameModStatus.InitializeSuccess)
                            continue;
                        else if (required)
                           GameErrorManager.ThrowGameError(GameError.GameInitPartLoadFailed, "加载模块  " + fullPath + " 时发生错误");
                        else GameLogger.Warning(TAG, "加载模块  {0} 时发生错误", fullPath);
                    }
                }
            }
            else GameErrorManager.ThrowGameError(GameError.GameInitReadFailed, "加载 GameInit.txt  " + gameinit_txt_path + " 时发生错误：" + request.error);

            //GameBulider.GameUIManager.GameBuliderUI.SetProgressText("Loading");

            GameManager.GameMediator.DispatchGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH, "*");
            GameManager.GameMediator.UnRegisterGlobalEvent(GameEventNames.EVENT_GAME_INIT_FINISH);
        }
    }
}
