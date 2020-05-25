using Ballance2.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2
{
    [SLua.CustomLuaClass]
    public static class GameManager
    {
        //============================
        // 管理器控制
        //============================

        /// <summary>
        /// 全局管理器单例数组
        /// </summary>
        private static List<IManager> managers = new List<IManager>();

        /// <summary>
        /// 获取管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static IManager GetManager(string name)
        {
            foreach (IManager m in managers)
                if (m.GetName() == name)
                    return m;
            return null;
        }
        /// <summary>
        /// 注册自定义管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <param name="classInstance">实例</param>
        /// <returns></returns>
        public static IManager RegisterManager(string name, IManager classInstance)
        {
            if(GetManager(name) != null)
            {
                
                return null;
            }
            return null;
        }
        /// <summary>
        /// 手动释放管理器
        /// </summary>
        /// <param name="name">标识符名称</param>
        /// <returns></returns>
        public static bool DestroyManager(string name)
        {
            foreach (IManager m in managers)
                if (m.GetName() == name)
                {
                    m.ReleaseManager();
                    managers.Remove(m);
                    return true;
                }
            Debug.LogWarning("" + name + "");
            return false;
        }

        //============================
        // 游戏总体初始化例程
        //============================

        internal static bool Init()
        {
            bool result = InitAllManagers();

            return result;
        }
        internal static bool Destroy()
        {
            bool result = DestryAllManagers();

            return result;
        }

        private static bool InitAllManagers()
        {


            return false;
        }
        private static bool DestryAllManagers()
        {
            return false;
        }
    }
}
