using UnityEngine;

namespace Ballance2.Managers
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 管理器接口
    /// </summary>
    public class BaseManager : MonoBehaviour
    {
        /// <summary>
        /// 当管理器第一次初始化时（场景进入）
        /// </summary>
        /// <returns></returns>
        public virtual  bool InitManager()
        {
            return false;
        }
        /// <summary>
        /// 当管理器卸载时（场景卸载）
        /// </summary>
        /// <returns></returns>
        public virtual bool ReleaseManager()
        {
            return false;
        }
        /// <summary>
        /// 当管理器需要重载数据时（场景重载）
        /// </summary>
        public virtual void ReloadData()
        {

        }

        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetSubName()
        {
            return "";
        }
        public virtual bool GetIsSingleton()
        {
            return false;
        }
    }

    /// <summary>
    /// 管理器基类（单例）
    /// </summary>
    public class BaseManagerSingleton : BaseManager
    {
        /// <summary>
        /// 创建管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        public BaseManagerSingleton(string name)
        {
            this.name = name;
        }

        private new string name = "";

        public override bool GetIsSingleton() { return true; }
        public override string GetName()
        {
            return name;
        }
        public override string GetSubName()
        {
            return "Singleton";
        }
    }

    /// <summary>
    /// 管理器基类（可绑定）
    /// </summary>
    public class BaseManagerBindable : BaseManager
    {
        /// <summary>
        /// 创建管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="subName">二级名称，用于区分多个管理器</param>
        public BaseManagerBindable(string name, string subName)
        {
            this.name = name;
            this.subName = subName;
            this.isSingleton = subName == "Singleton";
        }

        private new string name = "";
        private string subName = "";
        private bool isSingleton = false;

        /// <summary>
        /// 二级名称，用于区分多个管理器
        /// </summary>
        public string SubName {
            get  { return subName; }
            set { subName = value; }
        }

        public override bool GetIsSingleton() { return isSingleton; }
        public override string GetName()
        {
            return name;
        }
        public override string GetSubName()
        {
            return subName;
        }
    }
}
