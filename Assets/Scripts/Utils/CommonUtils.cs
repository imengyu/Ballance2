using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballance2.Utils
{
    [SLua.CustomLuaClass]
    public class CommonUtils
    {
        private static Random random = new Random();
        private static int idPool = 1000;
        private static int idPoolSq = 0;

        /// <summary>
        /// 生成自增长ID
        /// </summary>
        /// <returns></returns>
        public static int GenAutoIncrementID()
        {
            return idPoolSq++;
        }
        /// <summary>
        /// 生成不重复ID
        /// </summary>
        /// <returns></returns>
        public static int GenNonDuplicateID()
        {
            return idPool++;
        }
        /// <summary>
        /// 检查数组是否为空
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static bool IsArrayNullOrEmpty(object [] arr)
        {
            return (arr == null || arr.Length == 0); 
        }
    }
}
