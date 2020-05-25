using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// 获取名字
        /// </summary>
        /// <returns></returns>
        string GetName();
    }

    /// <summary>
    /// 管理器基类
    /// </summary>
    public class BaseManager : IManager
    {
        /// <summary>
        /// 创建管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        public BaseManager(string name)
        {
            this.name = name;
        }

        private string name = "";

        public string GetName()
        {
            return name;
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
