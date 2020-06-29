using SLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    class GameLuaWapper
    {
    }

    [CustomLuaClass]
    public delegate void LuaStartDelegate(LuaTable self, GameObject gameObject);
    [CustomLuaClass]
    public delegate void LuaVoidDelegate(LuaTable self);
    [CustomLuaClass]
    public delegate void LuaCollisionDelegate(LuaTable self, Collision collision);
    [CustomLuaClass]
    public delegate void LuaVector3Delegate(LuaTable self, Vector3 vector3);
    [CustomLuaClass]
    public delegate void LuaBoolDelegate(LuaTable self, bool b);

}
