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

        public static int GenNonDuplicateID()
        {
            return idPool++;
        }
    }
}
