using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.Managers
{
    /// <summary>
    /// 管理器接口
    /// </summary>
    public interface IManager
    {
        /// <summary>
        /// 当管理器第一次初始化时（场景进入）
        /// </summary>
        /// <returns></returns>
        bool InitManager();
        /// <summary>
        /// 当管理器卸载时（场景卸载）
        /// </summary>
        /// <returns></returns>
        bool ReleaseManager();
        /// <summary>
        /// 当管理器需要重载数据时（场景重载）
        /// </summary>
        void ReloadData();

        string GetName();
        string GetSubName();
        bool GetIsSingleton();
    }

    /// <summary>
    /// 管理器基类（单例）
    /// </summary>
    public class BaseManagerSingleton : IManager
    {
        /// <summary>
        /// 创建管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        public BaseManagerSingleton(string name)
        {
            this.name = name;
        }

        private string name = "";

        public bool GetIsSingleton() { return true; }
        public string GetName()
        {
            return name;
        }
        public string GetSubName()
        {
            return "Singleton";
        }

        public virtual bool InitManager()
        {
            return false;
        }
        public virtual bool ReleaseManager()
        {
            return false;
        }
        public virtual void ReloadData()
        {
        }
    }

    /// <summary>
    /// 管理器基类（可绑定）
    /// </summary>
    public class BaseManagerBindable : MonoBehaviour, IManager
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

        public bool GetIsSingleton() { return isSingleton; }
        public string GetName()
        {
            return name;
        }
        public string GetSubName()
        {
            return subName;
        }

        public virtual bool InitManager()
        {
            return false;
        }
        public virtual bool ReleaseManager()
        {
            return false;
        }
        public virtual void ReloadData()
        {
        }
    }
}
