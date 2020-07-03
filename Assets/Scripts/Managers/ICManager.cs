using Ballance2.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.Managers
{
    /// <summary>
    /// IC 管理器（类似Virtools的IC）
    /// </summary>
    class ICManager : BaseManager
    {
        private const string TAG = "ICManager";

        public ICManager() : base(TAG)
        {
        }

        private class ICInfo { 
            public GameObject obj;
            public Vector3 pos;
            public Vector3 rote;
        }
        private Dictionary<GameObject, ICInfo> icPools = null;

        public override bool InitManager()
        {
            icPools = new Dictionary<GameObject, ICInfo>();
            return true;
        }
        public override bool ReleaseManager()
        {
            if (icPools != null)
            {
                icPools.Clear();
                icPools = null;
            }
            return true;
        }

        private ICInfo TryGetIC(GameObject g)
        {
            ICInfo i = null;
            icPools.TryGetValue(g,out i);
            return i;
        }

        public bool BackupIC(GameObject g)
        {
            ICInfo info = TryGetIC(g);
            if (info == null)
            {
                info = new ICInfo();
                icPools.Add(g, info);
            }

            info.obj = g;
            info.pos = g.transform.position;
            info.rote = g.transform.eulerAngles;

            return true;
        }
        public bool RemoveIC(GameObject g)
        {
            if (icPools.ContainsKey(g))
            {
                icPools.Remove(g);
                return true;
            }
            return false;
        }
        public bool BackupIC(GameObject g, ICBackType iCBackType)
        {
            if (iCBackType == ICBackType.BackupThisObject)
                return BackupIC(g);

            foreach (Transform t in g.transform)
                BackupIC(t.gameObject, iCBackType);

            return BackupIC(g);
        }
        public bool RemoveIC(GameObject g, ICBackType iCBackType)
        {
            if (iCBackType == ICBackType.BackupThisObject)
                return RemoveIC(g);

            foreach (Transform t in g.transform)
                RemoveIC(t.gameObject, iCBackType);

            return RemoveIC(g);
        }
        public bool ResetIC(GameObject g)
        {
            if (icPools.ContainsKey(g))
            {
                ICInfo info = TryGetIC(g);
                g.transform.position = info.pos;
                g.transform.eulerAngles = info.rote;
                return true;
            }
            return false;
        }
        public bool ResetIC(GameObject g, ICBackType iCBackType)
        {
            if (iCBackType == ICBackType.BackupThisObject)
                return ResetIC(g);

            foreach (Transform t in g.transform)
                ResetIC(t.gameObject, iCBackType);

            return ResetIC(g);
        }
    }
}
