﻿using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.Managers;
using SLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 球基础类 Lua 包装
    /// </summary>
    [CustomLuaClass]
    public class GameBallLuaWapper : GameBall
    {
        /// <summary>
        /// 获取球基类
        /// </summary>
        public GameBall Base { get { return this; } }

        private LuaTable self = null;
        private LuaVector3Delegate fnActive = null;
        private LuaVoidDelegate fnBallPush = null;
        private LuaVoidDelegate fnDeactive = null;
        private LuaBoolDelegate fnEndControll = null;
        private LuaVector3Delegate fnRecover = null;
        private LuaVoidDelegate fnRecoverPieces = null;
        private LuaVoidDelegate fnRemoveSpeed = null;
        private LuaVoidDelegate fnStartControll = null;
        private LuaVoidDelegate fnThrowPieces = null;

        private GameLuaObjectHost GameLuaObjectHost;

        private void Start()
        {
            GameLuaObjectHost = GetComponent<GameLuaObjectHost>();
            if (GameLuaObjectHost == null)
                throw new Exception("GameBallLuaWapper can oly use in GameLuaObjectHost ! ");

            InitLuaFun();
        }
        private void InitLuaFun()
        {
            self = GameLuaObjectHost.LuaSelf;
            LuaFunction fn = self["Active"] as LuaFunction;
            if (fn != null) fnActive = fn.cast<LuaVector3Delegate>();
            fn = self["Deactive"] as LuaFunction;
            if (fn != null) fnDeactive = fn.cast<LuaVoidDelegate>();
            fn = self["BallPush"] as LuaFunction;
            if (fn != null) fnBallPush = fn.cast<LuaVoidDelegate>();
            fn = self["EndControll"] as LuaFunction;
            if (fn != null) fnEndControll = fn.cast<LuaBoolDelegate>();
            fn = self["Recover"] as LuaFunction;
            if (fn != null) fnRecover = fn.cast<LuaVector3Delegate>();
            fn = self["RecoverPieces"] as LuaFunction;
            if (fn != null) fnRecoverPieces = fn.cast<LuaVoidDelegate>();
            fn = self["RemoveSpeed"] as LuaFunction;
            if (fn != null) fnRemoveSpeed = fn.cast<LuaVoidDelegate>();
            fn = self["StartControll"] as LuaFunction;
            if (fn != null) fnStartControll = fn.cast<LuaVoidDelegate>();
            fn = self["ThrowPieces"] as LuaFunction;
            if (fn != null) fnThrowPieces = fn.cast<LuaVoidDelegate>();
        }

        [DoNotToLua]
        public override void Active(Vector3 posWorld)
        {
            if (fnActive != null) fnActive(self, posWorld);
        }
        [DoNotToLua]
        public override void BallPush()
        {
            if (fnBallPush != null) fnBallPush(self);
        }
        [DoNotToLua]
        public override void Deactive()
        {
            if (fnDeactive != null) fnDeactive(self);
        }
        [DoNotToLua]
        public override void EndControll(bool hide)
        {
            if (fnEndControll != null) fnEndControll(self, hide);
        }
        [DoNotToLua]
        public override void Recover(Vector3 pos)
        {
            if (fnRecover != null) fnRecover(self, pos);
        }
        [DoNotToLua]
        public override void RecoverPieces()
        {
            if (fnRecoverPieces != null) fnRecoverPieces(self);
        }
        [DoNotToLua]
        public override void RemoveSpeed()
        {
            if (fnRemoveSpeed != null) fnRemoveSpeed(self);
        }
        [DoNotToLua]
        public override void StartControll()
        {
            if (fnStartControll != null) fnStartControll(self);
        }
        [DoNotToLua]
        public override void ThrowPieces()
        {
            if (fnThrowPieces != null) fnThrowPieces(self);
        }
    }
}