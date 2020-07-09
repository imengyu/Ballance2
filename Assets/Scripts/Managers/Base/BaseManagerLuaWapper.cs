using Ballance2.CoreBridge;
using Ballance2.Utils;
using SLua;
using UnityEngine;

namespace Ballance2.Managers
{
    [CustomLuaClass]
    [AddComponentMenu("Ballance/Lua/BaseManagerLuaWapper")]
    [RequireComponent(typeof(GameLuaObjectHost))]
    /// <summary>
    /// 管理器 Lua 接口
    /// </summary>
    public class BaseManagerLuaWapper : BaseManager
    {
        public BaseManagerLuaWapper() : base()
        {
            IsLuaModul = true;
        }
    }
}
