using UnityEngine;

namespace Ballance2.Utils
{
    [SLua.CustomLuaClass]
    public static class GameCloneUtils
    {
        public static GameObject CloneNewObject(GameObject prefab, string name)
        {
            GameObject go = Object.Instantiate(prefab);
            go.name = name;
            return go;
        }
        public static GameObject CloneNewObjectWithParent(GameObject prefab, Transform parent,  string name)
        {
            GameObject go = Object.Instantiate(prefab, parent);
            go.name = name;
            return go;
        }
    }
}
