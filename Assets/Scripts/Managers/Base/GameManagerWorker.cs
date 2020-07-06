
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.Managers.Base
{
    class GameManagerWorker : MonoBehaviour
    {
        private void Update()
        {
            if (nextInitManagerTick > 0)
            {
                nextInitManagerTick--;
                if (nextInitManagerTick == 0)
                    ReqInitManagers();
            }
        }

        public int nextInitManagerTick = 0;
        public List<BaseManager> nextInitManages = null;

        private void ReqInitManagers()
        {
            foreach(BaseManager m in nextInitManages)
            {
                if (!m.PreInitialized)
                    m.PreInitManager();
            }
            foreach (BaseManager m in nextInitManages)
            {
                if (!m.Initialized)
                    m.InitManager();
            }
        }
    }
}
