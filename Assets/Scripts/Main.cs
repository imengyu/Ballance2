using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
{
    // 主入口
    class Main : MonoBehaviour
    {
        /// <summary>
        /// 游戏模式
        /// </summary>
        public GameManager.GameMode GameMode = GameManager.GameMode.None;
        /// <summary>
        /// 游戏根，所有游戏部件在这里托管
        /// </summary>
        public GameObject GameRoot = null;
        /// <summary>
        /// 游戏UI根
        /// </summary>
        public GameObject GameCanvas = null;
        /// <summary>
        /// 启动时暂停游戏，在控制台中继续
        /// </summary>
        public bool BreakAtStart = false;
        /// <summary>
        /// 静态 Prefab 资源引入
        /// </summary>
        public List<GameManager.GameObjectInfo> GamePrefab = null;
        /// <summary>
        /// 静态资源引入
        /// </summary>
        public List<GameManager.GameAssetsInfo> GameAssets = null;

        void Start()
        {
            GameLogger.InitLogger();
            GameManager.Init(GameMode, GameRoot, GameCanvas, GamePrefab, GameAssets, BreakAtStart);
        }
        private void OnDestroy()
        {
            GameManager.Destroy();
            GameLogger.DestroyLogger();
        }
    }
}
