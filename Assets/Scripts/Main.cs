using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
{
    // 入口
    class Main : MonoBehaviour
    {
        void Start()
        {
            GameManager.Init();
        }
        private void OnDestroy()
        {

            GameManager.Destroy();
        }
    }
}
