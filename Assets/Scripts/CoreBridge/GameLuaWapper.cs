﻿using Ballance2.Managers;
using SLua;
using UnityEngine;

/*
 * Copyright (c) 2020  mengyu
 * 
 * 模块名：     
 * GameLuaWapper.cs
 * 用途：
 * Lua包装所需的委托。
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
    public delegate void LuaManagerRedayDelegate(LuaTable self, 
        Store store, GameActionStore actionStore, BaseManager baseManager);

    [CustomLuaClass]
    public delegate void LuaStartDelegate(LuaTable self, GameObject gameObject);
    [CustomLuaClass]
    public delegate void LuaCollisionDelegate(LuaTable self, Collision collision);
    [CustomLuaClass]
    public delegate void LuaColliderDelegate(LuaTable self, Collider collider);

    [CustomLuaClass]
    public delegate void LuaCollision2DDelegate(LuaTable self, Collision2D collision);
    [CustomLuaClass]
    public delegate void LuaCollider2DDelegate(LuaTable self, Collider2D collider);

    [CustomLuaClass]
    public delegate void LuaVector3Delegate(LuaTable self, Vector3 vector3);
    [CustomLuaClass]
    public delegate void LuaBoolDelegate(LuaTable self, bool b);
    [CustomLuaClass]
    public delegate void LuaIntDelegate(LuaTable self, int v);
    [CustomLuaClass]
    public delegate void LuaGameObjectDelegate(LuaTable self, GameObject gameObject);
    [CustomLuaClass]
    public delegate void LuaVoidDelegate(LuaTable self);

    [CustomLuaClass]
    public delegate bool LuaReturnBoolDelegate(LuaTable self);
    [CustomLuaClass]
    public delegate bool LuaStoreReturnBoolDelegate(LuaTable self, Store store);
    [CustomLuaClass]
    public delegate bool LuaActionStoreReturnBoolDelegate(LuaTable self, GameActionStore store);


}
