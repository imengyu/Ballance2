using SubjectNerd.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        /// 启动时暂停游戏，在控制台中继续（通常用于调试）
        /// </summary>
        public bool BreakAtStart = false;
        /// <summary>
        /// 资源优先选择 Editor 中的资源（仅 Editor 有效）
        /// </summary>
        public bool AssetsPreferEditor = true;

        /// <summary>
        /// 目标帧率
        /// </summary>
        public int TargetFrameRate = 60;
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

        void Start()
        {
            Instance = this;
            InitCommandLine();
            Application.targetFrameRate = TargetFrameRate;
            GameLogger.InitLogger();
            GameManager.AssetsPreferEditor = AssetsPreferEditor;
            GameManager.BreakAtStart = BreakAtStart;
            StartCoroutine(GameManager.Init(GameMode, GameRoot, GameCanvas, GamePrefab, GameAssets));
        }
        private void OnDestroy()
        {
            GameManager.Destroy();
            GameLogger.DestroyLogger();
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
    }
}
