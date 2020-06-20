using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ballance2.Utils
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    public static class StringUtils
    {
        public static bool IsUrl(string text)
        {
            return Regex.IsMatch(text, "(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
        }
        public static bool IsPackageName(string text)
        {
            return Regex.IsMatch(text, "^([a-zA-Z]+[.][a-zA-Z]+)[.]*.*");
        }
        public static int CompareTwoVersion(string version1, string version2)
        {
            if (version1 == version2) return 0;
            long ver1 = 0, ver2 = 0;

            string[] vf = version1.Split('.');
            int v = 0;
            for (int i = 0; i < vf.Length; i++)
            {
                if(int.TryParse(vf[i], out v))
                    ver1 += v * (int)Math.Pow(10, i);
            }
            vf = version2.Split('.');
            for (int i = 0; i < vf.Length; i++)
            {
                if (int.TryParse(vf[i], out v))
                    ver2 += v * (int)Math.Pow(10, i);
            }

            if (ver1 == ver2) return 0;
            return ver1 < ver2 ? -1 : 1;
        }
    }
}
