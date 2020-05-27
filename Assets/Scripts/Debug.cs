using Ballance2.Managers.CoreBridge;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
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

        IEnumerator Run()
        {
            yield return new WaitForSeconds(0.2f);

              GameLogger.Log(TAG, "Run Start");

            yield return new WaitUntil(GameManager.IsGameBaseInitFinished);

            GameManager.GameMediator.RegisterEventKernalHandler(GameEventNames.EVENT_GLOBAL_ALERT_CLOSE,
                "Debug", (evtName, param) =>
                {
                    GameLogger.Log(TAG, "{0} Alert closed : {1} => {2}", evtName, param[0], param[1]);
                    return false;
                });

            GameLogger.Log(TAG, "IsGameBaseInitFinished");
            GameLogger.Warning(TAG, "Test warning log");
            GameLogger.Error(TAG, "Test Error log");
            GameLogger.Error(TAG, "Test Error log2");
            //GameManager.UIManager.GlobalAlert("测试对话框内容", "测试对话框");

            GameLogger.Log(TAG, "Run End");
        }


    }
}
