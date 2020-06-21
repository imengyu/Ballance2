using UnityEngine;

namespace Ballance2.Utils
{
    [SLua.CustomLuaClass]
    public static class GameCloneUtils
    {
        public static GameObject CloneNewObject(GameObject prefab, string name)
        {
            GameObject go = Object.Instantiate(prefab, GameManager.GameManagerObject.transform);
            go.name = name;
            return go;
        }
        public static GameObject CloneNewObjectWithParent(GameObject prefab, Transform parent,  string name)
        {
            GameObject go = Object.Instantiate(prefab, parent);
            go.name = name;
            return go;
        }
        public static GameObject CloneNewObjectWithParent(GameObject prefab, Transform parent)
        {
            return Object.Instantiate(prefab, parent);
        }
        public static GameObject CloneNewObjectWithParent(GameObject prefab, Transform parent, string name, bool active)
        {
            GameObject go = Object.Instantiate(prefab, parent);
            go.name = name;
            go.SetActive(active);
            return go;
        }

        public static GameObject CreateEmptyObject(string name)
        {
            GameObject go = Object.Instantiate(GameManager.PrefabEmpty, GameManager.GameManagerObject.transform);
            go.name = name;
            return go;
        }
        public static GameObject CreateEmptyObjectWithParent(Transform parent, string name)
        {
            GameObject go = Object.Instantiate(GameManager.PrefabEmpty, parent);
            go.name = name;
            return go;
        }
        public static GameObject CreateEmptyObjectWithParent(Transform parent)
        {
            return Object.Instantiate(GameManager.PrefabEmpty, parent);
        }

        public static GameObject CreateEmptyUIObject(string name)
        {
            GameObject go = Object.Instantiate(GameManager.PrefabUIEmpty, GameManager.GameCanvas.transform);
            go.name = name;
            return go;
        }
        public static GameObject CreateEmptyUIObjectWithParent(Transform parent, string name)
        {
            GameObject go = Object.Instantiate(GameManager.PrefabUIEmpty, parent);
            go.name = name;
            return go;
        }
        public static GameObject CreateEmptyUIObjectWithParent(Transform parent)
        {
            return Object.Instantiate(GameManager.PrefabUIEmpty, parent);
        }
    }
}
