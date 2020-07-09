using Ballance2.CoreBridge;
using UnityEngine;

namespace Ballance2.CoreBridge.GameLuaWapperEvents
{
    public class GameLuaObjectEventCaller : MonoBehaviour
    {
        public virtual string[] GetSupportEvents() { return null; }
        public virtual void OnInitLua(GameLuaObjectHost host) { }
        public virtual GameLuaObjectEventWarps GetEventType() { return GameLuaObjectEventWarps.Unknow; }
    }

    public enum GameLuaObjectEventWarps
    {
        Unknow,
        Physics,
        Physics2D,
        Mouse,
        Animator,
        Particle,
        Other,
    }
}
