using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.CoreGame.Interfaces;
using Ballance2.Managers;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 球管理器
    /// </summary>
    public class BallManagerLuaWapper : BallManager
    {
        public BallManagerLuaWapper()
        {
            IsLuaModul = true;
        }

        /// <summary>
        /// 基类
        /// </summary>
        public BallManager Base { get { return this; } }

        protected override void InitLuaFuns()
        {
            base.InitLuaFuns();


        }

        public override bool RegisterBall(string name, GameBall ball, GameObject pieces)
        {
            return false;
        }
        public override void UnRegisterBall(string name)
        {

        }
        public override GameBall GetRegisteredBall(string name)
        {
            return null;
        }

        public override void StartControll()
        {
        }
        public override void EndControll()
        {
        }

        public override void PlayLighting(bool smallToBig = false, bool lightEnd = false)
        {
            
        }

        public override bool IsControlling
        {
            get
            {
                return base.IsControlling;
            }
            set
            {
               
            }
        }

        public override void RemoveBallSpeed(GameBall ball)
        {
        }
        public override void AddBallPush(BallPushType t)
        {
        }
        public override void RemoveBallPush(BallPushType t)
        {
        }
        public override void RecoverSetPos(Vector3 pos)
        {
        }
        public override void RecoverBallDef()
        {
        }
        public override void RecoverBall(Vector3 pos)
        {
        }
        public override void ActiveBallDef()
        {
        }
        public override void ActiveBall(string type)
        {
        }
        public override void ClearBall()
        {
        }
        public override void ThrowPieces(string type)
        {
        }
        public override void ThrowPieces(GameBall ball)
        {
        }
        public override void RecoverPieces(GameBall ball)
        {
        }
        public override void SmoothMoveBallToPos(Vector3 pos, float off = 2f)
        {
        }
        public override void PlaySmoke()
        {

        }

    }
}
