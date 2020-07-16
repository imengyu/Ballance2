using Ballance2.ModBase;
using Ballance2.Utils;
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
    [Serializable]
    public class StoreData
    {
        /// <summary>
        /// 空
        /// </summary>
        public static StoreData Empty { get; } = new StoreData("Empty", StoreDataAccess.GetAndSet, StoreDataType.NotSet);

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public object DataRaw
        {
            get
            {
                if (StoreDataProvider != null)
                {
                   object data = StoreDataProvider();
                    if(data == null)
                    {
                        _DataRaw = null;
                        return _DataRaw;
                    }
                    //类型检查
                    string typeName = data.GetType().Name;
                    if (_DataType != StoreDataType.Custom && typeName != _DataType.ToString())
                        return _DataRaw;

                    _DataRaw = data;
                }
                return _DataRaw;
            }
        }
        /// <summary>
        /// 数据类型
        /// </summary>
        public StoreDataType DataType
        {
            get { return _DataType; }
        }
        /// <summary>
        /// 数组类型
        /// </summary>
        public List<StoreData> DataArray
        {
            get { return _DataArray; }
        }
        
        private int currentHolderContext = 0;
        private object _DataRaw = null;
        private StoreDataType _DataType = StoreDataType.Custom;
        [NonSerialized]
        private List<StoreData> _DataArray = new List<StoreData>();
        private StoreDataAccess _StoreDataAccess = StoreDataAccess.Get;

        internal StoreData(string name, StoreDataAccess access, StoreDataType dataType)
        {
            Name = name;
            _StoreDataAccess = access;
            _DataType = dataType;
        }

        /// <summary>
        /// 判断是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            if (DataType == StoreDataType.NotSet)
                return true;
            if (DataType == StoreDataType.GameObject)
                return ((GameObject)DataRaw) == null;
            if (DataType == StoreDataType.Object)
                return ((UnityEngine.Object)DataRaw) == null;
            return DataRaw == null;
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Destroy()
        {
            _DataRaw = null;
            _DataType = StoreDataType.NotSet;
            if(DataArray != null)
            {
                foreach (var v in DataArray)
                    v.Destroy();
                _DataArray.Clear();
                _DataArray = null;
            }
        }

        // 观察者和数据提供者
        // ====================================

        private StoreOnDataChanged DataObserver;

        public void RegisterDataObserver(StoreOnDataChanged observer)
        {
            DataObserver += observer;
        }
        public void UnRegisterDataObserver(StoreOnDataChanged observer)
        {
            DataObserver -= observer;
        }
        public void NotificationDataObserver(object oldV, object newV)
        {
            if (DataObserver != null)
                DataObserver(this, oldV, newV);
        }

        private StoreDataProvider StoreDataProvider = null;

        public int SetDataProvider(int context, StoreDataProvider provider)
        {
            if(StoreDataProvider == null)
            {
                StoreDataProvider = provider;
                currentHolderContext = context;
                return context;
            }
            else if(context != currentHolderContext)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ContextMismatch, "StoreData", 
                    "上下文 {0} 没有操作此数据的权限", context);
                return 0;
            }
            else
            {
                StoreDataProvider = provider;
                return context;
            }
        }
        public void RemoveRegisterProvider(int context)
        {
            if (StoreDataProvider != null)
            {
                if (context != currentHolderContext)
                {
                    GameErrorManager.SetLastErrorAndLog(GameError.ContextMismatch, "StoreData",
                        "上下文 {0} 没有操作此数据的权限", context);
                    return;
                }

                StoreDataProvider = null;
            }
        }

        // 设置数据
        // ====================================

        public bool SetData(int context, object data)
        {
            if (_StoreDataAccess != StoreDataAccess.GetAndSet && context != currentHolderContext)
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ContextMismatch, "StoreData",
                    "上下文 {0} 没有操作此数据的权限", context);
                return false;
            }

            //类型检查
            if (_DataType != StoreDataType.Custom && data.GetType().Name != _DataType.ToString())
            {
                GameErrorManager.SetLastErrorAndLog(GameError.ContextMismatch, "StoreData",
                  "输入 {0} 与设置的类型不符 {1}", data, _DataType.ToString());
                return false;
            }

            if (_DataRaw != data)
            {
                object old = _DataRaw;
                _DataRaw = data;
                NotificationDataObserver(old, data);
            }
            return true;
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
        public Material MaterialData() { return DataRaw == null ? null : (Material)DataRaw; }
        public Texture TextureData() { return DataRaw == null ? null : (Texture)DataRaw; }
        public Texture2D Texture2DData() { return DataRaw == null ? null : (Texture2D)DataRaw; }
        public Vector2 Vector2Data() { return (Vector2)DataRaw; }
        public Vector3 Vector3Data() { return (Vector3)DataRaw; }
        public Vector4 Vector4Data() { return (Vector4)DataRaw; }
        public Quaternion QuaternionData() { return (Quaternion)DataRaw; }
        public Sprite SpriteData() { return DataRaw == null ? null : (Sprite)DataRaw; }
        public Rigidbody RigidbodyData() { return DataRaw == null ? null : (Rigidbody)DataRaw; }
        public Rigidbody2D Rigidbody2DData() { return DataRaw == null ? null : (Rigidbody2D)DataRaw; }
        public RectTransform RectTransformData() { return DataRaw == null ? null : (RectTransform)DataRaw; }
        public Transform TransformData() { return DataRaw == null ? null : (Transform)DataRaw; }
        public Camera CameraData() { return DataRaw == null ? null : (Camera)DataRaw; }
        public GameObject GameObjectData() { return DataRaw == null ? null : (GameObject)DataRaw; }
        public UnityEngine.Object ObjectData() { return DataRaw == null ? null : (UnityEngine.Object)DataRaw; }
        public AudioClip AudioClipData() { return DataRaw == null ? null : (AudioClip)DataRaw; }
        public AudioSource AudioSourceData() { return DataRaw == null ? null : (AudioSource)DataRaw; }
        public MonoBehaviour MonoBehaviourData() { return DataRaw == null ? null : (MonoBehaviour)DataRaw; }
        public GameMod GameModData() { return DataRaw == null ? null : (GameMod)DataRaw; }

        public override string ToString()
        {
            if (DataType == StoreDataType.NotSet)
                return " [StoreData NotSet]";
            if (DataType == StoreDataType.Array)
                return DataArray.Count + " [StoreData Array]";
            if (DataRaw == null)
                return "[StoreData Null]";
            return DataRaw.ToString() + " [StoreData " + DataType + "]";
        }
    }

    [CustomLuaClass]
    public delegate void StoreOnDataChanged(StoreData data, object oldV, object newV);
    [CustomLuaClass]
    public delegate object StoreDataProvider();

    /// <summary>
    /// 数据类型
    /// </summary>
    [CustomLuaClass]
    public enum StoreDataType
    {
        NotSet,
        /// <summary>
        /// object 类型
        /// </summary>
        Custom,
        Array,
        Integer,
        Long,
        Float,
        String,
        Boolean,
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
    /// 数据访问
    /// </summary>
    [CustomLuaClass]
    public enum StoreDataAccess
    {
        /// <summary>
        /// 仅获取
        /// </summary>
        Get,
        /// <summary>
        /// 获取和设置
        /// </summary>
        GetAndSet,
    }

    /// <summary>
    /// 全局数据共享存储池类
    /// </summary>
    [CustomLuaClass]
    [Serializable]
    public class Store
    {
        [SerializeField, SetProperty("PoolName")]
        private string _PoolName;
        [SerializeField, SetProperty("PoolDatas")]
        private Dictionary<string, StoreData> _PoolDatas;

        /// <summary>
        /// 池的名称
        /// </summary>
        public string PoolName { get { return _PoolName; }  }
        /// <summary>
        /// 池中的数据
        /// </summary>
        public Dictionary<string, StoreData> PoolDatas { get { return _PoolDatas; } }

        internal Store(string name)
        {
            _PoolName = name;
            _PoolDatas = new Dictionary<string, StoreData>();
        }

        public void Destroy()
        {
            _PoolName = null;
            if(_PoolDatas != null)
            {
                foreach (var v in _PoolDatas)
                    v.Value.Destroy();
                _PoolDatas.Clear();
                _PoolDatas = null;
            }
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <returns>添加成功，则返回参数，如果参数已经存在，则返回存在的实例</returns>
        public StoreData AddParameter(string name, StoreDataAccess access, StoreDataType storeDataType)
        {
            StoreData old;
            if (PoolDatas.TryGetValue(name, out old))
                return old;

            old = new StoreData(name, access, storeDataType);
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
