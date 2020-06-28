using Ballance2.Managers;
using Ballance2.CoreBridge;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.Main
{
    //调试承载
    public class Debug : MonoBehaviour
    {
        private const string TAG = "DebugMain";

        void Start()
        {
            StartCoroutine(Run());
        }
        void Update()
        {

        }

        private int dialogQuitId = 0;

        IEnumerator Run()
        {
            yield return new WaitForSeconds(1f);

            if(GameManager.Mode != GameManager.GameMode.MinimumLoad)
            {
                yield break;
            }

            GameLogger.Log(TAG, "Run Start");

            yield return new WaitUntil(GameManager.IsGameBaseInitFinished);

            GameManager.CloseGameManagerAlert();
            GameManager.GameMediator.RegisterEventHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE,
               "Debug", (evtName, param) =>
               {
                   if((int)param[0] == dialogQuitId)
                   {
                       if ((bool)param[1])
                           GameManager.QuitGame();
                   }
                   return false;
               });

            DebugLinearLayout();
            //DebugPageGlobal();

            GameManager.UIManager.MaskBlackSet(false);
            GameLogger.Log(TAG, "Run End");
        }

        public TextAsset PageMain;
        public TextAsset PageAbout;

        private void DebugLinearLayout()
        {
            GameManager.UIManager.RegisterBallanceUIPage("main", PageMain.text,
                new string[] { "btn.start:click" , "btn.quit:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        GameManager.UIManager.GotoUIPage("main.about");
                        return false;
                    },
                    (evtName, param) => {
                        dialogQuitId  = GameManager.UIManager.GlobalConfirm("真的要退出游戏吗", "提示", "确定","取消");
                        return false;
                    }
                },
                "Default");
            GameManager.UIManager.RegisterBallanceUIPage("main.about", PageAbout.text,
                new string[] { "btn.back:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        GameManager.UIManager.BackUIPage();
                        return false;
                    }
                },
                "Default");
            GameManager.UIManager.GotoUIPage("main");
        }

        public TextAsset PageGlobalAlert;
        public TextAsset PageGlobalConfirm;

        private void DebugPageGlobal()
        {
            GameManager.UIManager.RegisterBallanceUIPage("global.confirm", PageGlobalConfirm.text,
                new string[] { "btn.ok:click", "btn.cancel:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        GameManager.UIManager.CloseUIPage("global.alert");
                        return false;
                    },
                    (evtName, param) => {
                        GameManager.UIManager.CloseUIPage("global.alert");
                        return false;
                    }
                },
                "Default");
            GameManager.UIManager.RegisterBallanceUIPage("global.alert", PageGlobalAlert.text,
                new string[] { "btn.ok:click" },
                new GameEventHandlerDelegate[] {
                    (evtName, param) => {
                        GameManager.UIManager.CloseUIPage("global.alert");
                        return false;
                    }
                },
                "Default");
            GameManager.UIManager.GotoUIPage("global.confirm");
        }
    }
}
