using System;
using System.Collections.Generic;
using SLua;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    /// <summary>
    /// lua 引入 var 信息
    /// </summary>
    [CustomLuaClass]
    [Serializable]
    public class LuaVarObjectInfo
    {
        /// <summary>
        /// 值类型
        /// </summary>
        [Tooltip("值类型")]
        [SerializeField]
        public LuaVarObjectType Type;
        /// <summary>
        /// 值名称
        /// </summary>
        [Tooltip("值名称")]
        [SerializeField]
        public string Name;

        public LuaVarObjectInfo()
        {
            Type = LuaVarObjectType.None;
            Name = "";
        }

        public override string ToString() { return Name; }

        [HideInInspector, SerializeField, DoNotToLua]
        public Vector2 vector2;
        [HideInInspector, SerializeField, DoNotToLua]
        public Vector2Int vector2Int;
        [HideInInspector, SerializeField, DoNotToLua]
        public Vector3 vector3;
        [HideInInspector, SerializeField, DoNotToLua]
        public Vector3Int vector3Int;
        [HideInInspector,SerializeField,DoNotToLua]
        public Vector4 vector4;
        [HideInInspector, SerializeField, DoNotToLua]
        public Rect rect;
        [HideInInspector, SerializeField, DoNotToLua]
        public RectInt rectInt;
        [HideInInspector, SerializeField, DoNotToLua]
        public Gradient gradient;
        [HideInInspector, SerializeField, DoNotToLua]
        public int layer;
        [HideInInspector, SerializeField, DoNotToLua]
        public AnimationCurve curve;
        [HideInInspector, SerializeField, DoNotToLua]
        public Color color;
        [HideInInspector, SerializeField, DoNotToLua]
        public BoundsInt boundsInt;
        [HideInInspector, SerializeField, DoNotToLua]
        public Bounds bounds;
        [HideInInspector, SerializeField, DoNotToLua]
        public UnityEngine.Object objectVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public GameObject gameObjectVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public long longVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public string stringVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public int intVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public double doubleVal;
        [HideInInspector, SerializeField, DoNotToLua]
        public double boolVal;

        public Vector2 Vector2() { return vector2; }
        public Vector2Int Vector2Int() { return vector2Int; }
        public Vector3 Vector3() { return vector3; }
        public Vector3Int Vector3Int() { return vector3Int; }
        public Vector4 Vector4() { return vector4; }
        public Rect Rect() { return rect; }
        public RectInt RectInt() { return rectInt; }
        public Gradient Gradient() { return gradient; }
        public int Layer() { return layer; }
        public AnimationCurve Curve() { return curve; }
        public Color Color() { return color; }
        public BoundsInt BoundsInt() { return boundsInt; }
        public Bounds Bounds() { return bounds; }
        public UnityEngine.Object Object() { return objectVal; }
        public UnityEngine.Object GameObject() { return gameObjectVal; }
        public long Long() { return longVal; }
        public string String() { return stringVal ; }
        public int Int() { return intVal; }
        public double Double() { return doubleVal; }
        public double Bool() { return boolVal; }
    }

    /// <summary>
    /// 指定引入数据的类型
    /// </summary>
    [Serializable]
    [CustomLuaClass]
    public enum LuaVarObjectType
    {
        None = 0,
        Vector2,
        Vector2Int,
        Vector3,
        Vector3Int,
        Vector4,
        Rect,
        RectInt,
        Gradient,
        Layer,
        Curve,
        Color,
        BoundsInt,
        Bounds,
        Object,
        GameObject,

        Long,
        Int,
        String,
        Double,
        Bool,
    }


}
