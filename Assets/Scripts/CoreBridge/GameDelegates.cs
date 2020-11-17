using Ballance2.CoreGame.GamePlay;
using Ballance2.GameCore;
using Ballance2.ModBase;
using SLua;
using System.Xml;
using UnityEngine;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameDelegates.cs
 * 用途：
 * 提供一些委托定义，用于向lua导出
 * 
 * 作者：
 * mengyu
 * 
 * 更改历史：
 * 2020-1-1 创建
 *
 */

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
