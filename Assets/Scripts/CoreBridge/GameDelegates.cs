using Ballance2.CoreGame.GamePlay;
using Ballance2.GameCore;
using Ballance2.ModBase;
using SLua;
using System.Xml;
using UnityEngine;

namespace Ballance2.CoreBridge
{
    [CustomLuaClass]
    public delegate void VoidDelegate();
    [CustomLuaClass]
    public delegate bool BooleanDelegate();

    [CustomLuaClass]
    public delegate bool ModDefCustomPropSolveDelegate(XmlNode xmlNode, GameMod mod);
    [CustomLuaClass]
    public delegate bool LevelDefCustomPropSolveDelegate(XmlNode xmlNode, GameLevel mod);

    [CustomLuaClass]
    public delegate bool LevelFloorSolveDelegate(GameObject @object, LevelFloorGroup group);
    [CustomLuaClass]
    public delegate bool LevelModulSolveDelegate(GameObject @object, LevelModulGroup group);


}
