using Ballance2.CoreBridge;
using SLua;
using System;
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
        private LuaReturnBoolDelegate fnInit = null;
        private LuaVoidDelegate fnBallPush = null;
        private LuaVoidDelegate fnDestroy = null;
        private LuaVoidDelegate fnDeactive = null;
        private LuaBoolDelegate fnEndControll = null;
        private LuaVector3Delegate fnRecover = null;
        private LuaVoidDelegate fnRemoveSpeed = null;
        private LuaVoidDelegate fnStartControll = null;

        private GameLuaObjectHost GameLuaObjectHost;

        private void Start()
        {
            GameLuaObjectHost = GetComponent<GameLuaObjectHost>();
            if (GameLuaObjectHost == null)
                throw new Exception("GameBallLuaWapper can only use when GameLuaObjectHost is bind ! ");

            InitLuaFun();
        }
        private void InitLuaFun()
        {
            self = GameLuaObjectHost.LuaSelf;
            LuaFunction fn = self["Active"] as LuaFunction;
            if (fn != null) fnActive = fn.cast<LuaVector3Delegate>();
            fn = self["Deactive"] as LuaFunction;
            if (fn != null) fnDeactive = fn.cast<LuaVoidDelegate>();
            fn = self["Init"] as LuaFunction;
            if (fn != null) fnInit = fn.cast<LuaReturnBoolDelegate>();
            fn = self["Destroy"] as LuaFunction;
            if (fn != null) fnDestroy = fn.cast<LuaVoidDelegate>();
            fn = self["BallPush"] as LuaFunction;
            if (fn != null) fnBallPush = fn.cast<LuaVoidDelegate>();
            fn = self["EndControll"] as LuaFunction;
            if (fn != null) fnEndControll = fn.cast<LuaBoolDelegate>();
            fn = self["Recover"] as LuaFunction;
            if (fn != null) fnRecover = fn.cast<LuaVector3Delegate>();
            fn = self["RemoveSpeed"] as LuaFunction;
            if (fn != null) fnRemoveSpeed = fn.cast<LuaVoidDelegate>();
            fn = self["StartControll"] as LuaFunction;
            if (fn != null) fnStartControll = fn.cast<LuaVoidDelegate>();
        }

        [DoNotToLua]
        public override void Active(Vector3 posWorld)
        {
            if (fnActive != null) fnActive(self, posWorld);
            else base.Active(posWorld);
        }
        [DoNotToLua]
        public override void BallPush()
        {
            if (fnBallPush != null) fnBallPush(self);
            else base.BallPush();
        }
        [DoNotToLua]
        public override void Deactive()
        {
            if (fnDeactive != null) fnDeactive(self);
            else base.Deactive();
        }
        [DoNotToLua]
        public override void EndControll(bool hide)
        {
            if (fnEndControll != null) fnEndControll(self, hide);
            else base.EndControll(hide);
        }
        [DoNotToLua]
        public override void Recover(Vector3 pos)
        {
            if (fnRecover != null) fnRecover(self, pos);
            else base.Recover(pos);
        }
        [DoNotToLua]
        public override void RemoveSpeed()
        {
            if (fnRemoveSpeed != null) fnRemoveSpeed(self);
            else base.RemoveSpeed();
        }
        [DoNotToLua]
        public override void StartControll()
        {
            if (fnStartControll != null) fnStartControll(self);
            else base.StartControll();
        }
        [DoNotToLua]
        public override bool Init()
        {
            if (fnInit != null) return fnInit(self);
            return base.Init();
        }
        [DoNotToLua]
        public override void Destroy()
        {
            if (fnDestroy != null) fnDestroy(self);
            else base.Destroy();
        }
    }
}
