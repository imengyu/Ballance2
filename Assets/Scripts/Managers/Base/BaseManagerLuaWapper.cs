using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using UnityEngine;

namespace Ballance2.Managers
{
    [CustomLuaClass]
    /// <summary>
    /// 管理器 Lua 接口
    /// </summary>
    public class BaseManagerLuaWapper : BaseManager
    {
        public BaseManagerLuaWapper() : base()
        {
            IsLuaModul = true;
        }

        private void Start()
        {
            if (IsLuaModul)
            {
                if (!InitLua())
                    GameLogger.Error(GetFullName(), "LuaModul can oly use when GameLuaObjectHost is bind ! ");
            }
        }
    }
}
