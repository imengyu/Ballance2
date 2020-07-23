using SubjectNerd.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


namespace Ballance2.Main
{
    // 主入口
    class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }

        /// <summary>
        /// 游戏模式
        /// </summary>
        public GameMode GameMode = GameMode.None;
        /// <summary>
        /// 游戏根，所有游戏部件在这里托管
        /// </summary>
        public GameObject GameRoot = null;
        /// <summary>
        /// 游戏UI根
        /// </summary>
        public GameObject GameCanvas = null;
        /// <summary>
        /// 游戏UI根
        /// </summary>
        public GameObject UIRoot = null;
        /// <summary>
        /// 启动时暂停游戏，在控制台中继续（通常用于调试）
        /// </summary>
        public bool BreakAtStart = false;

        /// <summary>
        /// 目标帧率
        /// </summary>
        public int TargetFrameRate = 60;
        /// <summary>
        /// 是否设置帧率
        /// </summary>
        public bool SetFrameRate = true;
        /// <summary>
        /// 调试目标关卡路径
        /// </summary>
        public string LevelDebugTarget = "";
        /// <summary>
        /// 内核调试基础元件
        /// </summary>
        public GameObject CoreDebugBase = null;

        /// <summary>
        /// 静态 Prefab 资源引入
        /// </summary>
        [Reorderable("GamePrefab", true, "Name")]
        public List<GameManager.GameObjectInfo> GamePrefab = null;
        /// <summary>
        /// 静态资源引入
        /// </summary>
        [Reorderable("GameAssets", true, "Name")]
        public List<GameManager.GameAssetsInfo> GameAssets = null;

        public GameObject GlobalGamePermissionTipDialog;
        public GameObject GlobalGameUserAgreementTipDialog;

        private bool GlobalGamePermissionTipDialogClosed;
        private bool GlobalGameUserAgreementTipDialogClosed;

        void Start()
        {
            if (SetFrameRate) Application.targetFrameRate = TargetFrameRate;

            Instance = this;
            InitCommandLine();

            StartCoroutine(InitMain());
        }
        private void OnDestroy()
        {
            GameManager.Destroy();
            GameLogger.DestroyLogger();
        }

        private bool ShowUserArgeement()
        {
            if (PlayerPrefs.GetInt("UserAgreementAgreed", 0) == 0)
            {
                GlobalGameUserAgreementTipDialog.SetActive(true);
                return true;
            }
            return false;
        }
        private bool TestAndroidPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                return true;
#endif
            return false;
        }

        public void ArgeedUserArgeement()
        {
            PlayerPrefs.SetInt("UserAgreementAgreed", 1);
            GlobalGameUserAgreementTipDialogClosed = true;
            GlobalGameUserAgreementTipDialog.SetActive(false);
        }
        public void RequestAndroidPermission()
        {
#if UNITY_ANDROID
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#endif
            GlobalGamePermissionTipDialogClosed = true;
            GlobalGamePermissionTipDialog.SetActive(false);
        }
        public void QuitGame()
        {
            StopAllCoroutines();
            Application.Quit();
        }

        private void InitCommandLine()
        {
            string[] CommandLineArgs = Environment.GetCommandLineArgs();
            int len = CommandLineArgs.Length;
            if (len > 1)
            {
                for (int i = 0; i < len; i++)
                {
                    if (CommandLineArgs[i] == "-mode" && i + 1 < len)
                        Enum.TryParse(CommandLineArgs[i + 1], out GameMode);
                    if (CommandLineArgs[i] == "-level" && i + 1 < len)
                        LevelDebugTarget = CommandLineArgs[i + 1];
                    if (CommandLineArgs[i] == "-debug")
                        PlayerPrefs.SetString("core.debug", "true");
                }
            }
        }

        private IEnumerator InitMain()
        {
            if (TestAndroidPermission())
            {
                GlobalGamePermissionTipDialog.SetActive(true);

                yield return new WaitUntil(() => GlobalGamePermissionTipDialogClosed);
            }

            if (ShowUserArgeement())
            {
                yield return new WaitUntil(() => GlobalGameUserAgreementTipDialogClosed);
            }

            GameLogger.InitLogger();
            GameManager.Mode = GameMode;
            GameManager.GameRoot = GameRoot;
            GameManager.GameCanvas = GameCanvas;
            GameManager.GamePrefab = GamePrefab;
            GameManager.GameAssets = GameAssets;
            GameManager.BreakAtStart = BreakAtStart;
            GameManager.UIRoot = UIRoot;

            StartCoroutine(GameManager.Init());
        }
    }
}
