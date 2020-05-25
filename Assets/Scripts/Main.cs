using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
{
    // 入口 （一生二，二生三，...）
    class Main : MonoBehaviour
    {
        /// <summary>
        /// 游戏模式
        /// </summary>
        public GameManager.GameMode GameMode;
        /// <summary>
        /// 游戏根，所有游戏部件在这里托管
        /// </summary>
        public GameObject GameRoot;
        /// <summary>
        /// 游戏UI根
        /// </summary>
        public GameObject GameCanvas;

        void Start()
        {
            GameLogger.Init();
            GameManager.Init(GameMode, GameRoot, GameCanvas);
        }
        private void OnDestroy()
        {
            GameManager.Destroy();
            GameLogger.Destroy();
        }
    }
}
