using Ballance2.ModBase;
using SLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Ballance2.CoreBridge
{
    /// <summary>
    /// 全局数据共享存储类
    /// </summary>
    [CustomLuaClass]
    public class StoreData
    {
        /// <summary>
        /// 原始数据
        /// </summary>
        public object DataRaw;
        /// <summary>
        /// 数据类型
        /// </summary>
        public StoreDataType DataType;
        /// <summary>
        /// 数组类型
        /// </summary>
        public List<StoreData> DataArray = new List<StoreData>();

        public StoreData()
        {
            DataRaw = null;
            DataType = StoreDataType.Null;
        }
        public StoreData(int data) {
            DataRaw = data;
            DataType = StoreDataType.Int;
        }
        public StoreData(long data)
        {
            DataRaw = data;
            DataType = StoreDataType.Long;
        }
        public StoreData(float data)
        {
            DataRaw = data;
            DataType = StoreDataType.Float;
        }
        public StoreData(string data)
        {
            DataRaw = data;
            DataType = StoreDataType.String;
        }
        public StoreData(bool data)
        {
            DataRaw = data;
            DataType = StoreDataType.Bool;
        }
        public StoreData(double data)
        {
            DataRaw = data;
            DataType = StoreDataType.Double;
        }
        public StoreData(object data)
        {
            DataRaw = data;
            DataType = StoreDataType.Raw;
        }
        public StoreData(StoreData[] data)
        {
            DataRaw = null;
            DataArray.AddRange(data);
            DataType = StoreDataType.Array;
        }

        /// <summary>
        /// 判断是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            if (DataType == StoreDataType.GameObject)
                return ((GameObject)DataRaw) == null;
            if (DataType == StoreDataType.Object)
                return ((UnityEngine.Object)DataRaw) == null;
            return DataRaw == null || DataType == StoreDataType.Null;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Destroy()
        {
            DataRaw = null;
            DataType = StoreDataType.Null;
            if(DataArray != null)
            {
                foreach (var v in DataArray)
                    v.Destroy();
                DataArray.Clear();
                DataArray = null;
            }
        }

        // 观察者
        // ====================================

        public StoreOnDataChanged OnDataChanged;

        public void RegisterDataObserver(StoreOnDataChanged observer)
        {
            OnDataChanged += observer;
        }
        public void UnRegisterDataObserver(StoreOnDataChanged observer)
        {
            OnDataChanged -= observer;
        }

        // 设置数据
        // ====================================

        public void SetData(object data)
        {
            DataRaw = data;

            //获得类型
            string typeName = data.GetType().Name;
            StoreDataType type;
            if (typeName == "UnityEngine.Object") type = StoreDataType.Object;
            else if(Enum.TryParse(typeName, out type)) DataType = type;
            else DataType = StoreDataType.Raw;
        }
        public void SetRawData(object data)
        {
            DataRaw = data;
            DataType = StoreDataType.Raw;
        }
        public void SetRawData<T>(T data, StoreDataType type)
        {
            DataRaw = data;
            DataType = type;
        }

        // 数组操作
        // ====================================

        public bool DataArrayAdd(StoreData data)
        {
            if (!DataArray.Contains(data))
            {
                DataArray.Add(data);
                return true;
            }
            return false;
        }
        public void DataArrayRemove(StoreData data)
        {
            DataArray.Remove(data);
        }
        public void DataArrayRemoveAt(int index)
        {
            DataArray.RemoveAt(index);
        }
        public void DataArrayRemoveAt(int index, int count)
        {
            DataArray.RemoveRange(index, count);
        }
        public void DataArrayInsert(int index, StoreData data)
        {
            DataArray.Insert(index, data);
        }
        public int DataArrayGetCount()
        {
            return DataArray.Count;
        }
        public StoreData DataArrayGet(int index)
        {
            return DataArray[index];
        }
        public void DataArrayClear()
        {
            DataArray.Clear();
        }

        // 获取数据
        // ====================================

        public int IntData() { return (int)DataRaw; }
        public long LongData() { return (long)DataRaw; }
        public float FloatData() { return (float)DataRaw; }
        public string StringData() { return (string)DataRaw; }
        public bool BoolData() { return (bool)DataRaw; }
        public double DoubleData() { return (double)DataRaw; }
        public object Data() { return DataRaw; }
        public T Data<T>() { return (T)DataRaw; }
        public StoreData[] ArrayData() { return DataArray.ToArray(); }
        public List<StoreData> ListArrayData() { return DataArray; }
        public Color ColorData() { return (Color)DataRaw; }
        public Material MaterialData() { return (Material)DataRaw; }
        public Texture TextureData() { return (Texture)DataRaw; }
        public Texture2D Texture2DData() { return (Texture2D)DataRaw; }
        public Vector2 Vector2Data() { return (Vector2)DataRaw; }
        public Vector3 Vector3Data() { return (Vector3)DataRaw; }
        public Vector4 Vector4Data() { return (Vector4)DataRaw; }
        public Quaternion QuaternionData() { return (Quaternion)DataRaw; }
        public Sprite SpriteData() { return (Sprite)DataRaw; }
        public Rigidbody RigidbodyData() { return (Rigidbody)DataRaw; }
        public Rigidbody2D Rigidbody2DData() { return (Rigidbody2D)DataRaw; }
        public RectTransform RectTransformData() { return (RectTransform)DataRaw; }
        public Transform TransformData() { return (Transform)DataRaw; }
        public Camera CameraData() { return (Camera)DataRaw; }
        public GameObject GameObjectData() { return (GameObject)DataRaw; }
        public UnityEngine.Object ObjectData() { return (UnityEngine.Object)DataRaw; }
        public AudioClip AudioClipData() { return (AudioClip)DataRaw; }
        public AudioSource AudioSourceData() { return (AudioSource)DataRaw; }
        public MonoBehaviour MonoBehaviourData() { return (MonoBehaviour)DataRaw; }
        public GameMod GameModData() { return (GameMod)DataRaw; }

        public override string ToString()
        {
            if(DataType == StoreDataType.Array)
                return DataArray.Count + " [StoreData Array]";
            if (DataType == StoreDataType.Null)
                return "[StoreData Null]";
            return DataRaw.ToString() + " [StoreData " + DataType + "]";
        }
    }

    public delegate void StoreOnDataChanged(StoreData data, object oldV, object newV);

    /// <summary>
    /// 数据类型
    /// </summary>
    [CustomLuaClass]
    public enum StoreDataType
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Null,
        /// <summary>
        /// object 类型
        /// </summary>
        Raw,
        Array,
        Int,
        Long,
        Float,
        String,
        Bool,
        Double,
        Color,
        Material,
        Texture,
        Texture2D,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
        Sprite,
        Rigidbody,
        RectTransform,
        Transform,
        Camera,
        GameObject,
        /// <summary>
        /// UnityEngine.Object
        /// </summary>
        Object,
        AudioClip,
        AudioSource,
        MonoBehaviour,
    }

    /// <summary>
    /// 全局数据共享存储池类
    /// </summary>
    [CustomLuaClass]
    public class Store
    {
        /// <summary>
        /// 池的名称
        /// </summary>
        public string PoolName { get; private set; }
        /// <summary>
        /// 池中的数据
        /// </summary>
        public Dictionary<string, StoreData> PoolDatas { get; private set; }

        public Store(string name)
        {
            PoolName = name;
            PoolDatas = new Dictionary<string, StoreData>();
        }

        public void Destroy()
        {
            PoolName = null;
            if(PoolDatas != null)
            {
                foreach (var v in PoolDatas)
                    v.Value.Destroy();
                PoolDatas.Clear();
                PoolDatas = null;
            }
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>添加成功，则返回参数，如果参数已经存在，则返回存在的实例</returns>
        public StoreData AddParameter(string name)
        {
            StoreData old;
            if (PoolDatas.TryGetValue(name, out old))
                return old;

            old = new StoreData();
            PoolDatas.Add(name, old);
            return old;
        }
        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>如果移除成功，返回true，如果参数不存在，返回false</returns>
        public bool RemoveAddParameter(string name)
        {
            if (PoolDatas.ContainsKey(name))
            {
                PoolDatas.Remove(name);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取池中的参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>返回参数实例</returns>
        public StoreData GetParameter(string name)
        {
            StoreData old;
            PoolDatas.TryGetValue(name, out old);
            return old;
        }
    }
}
