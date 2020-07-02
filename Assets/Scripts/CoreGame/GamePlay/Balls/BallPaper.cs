using Ballance2.CoreGame.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 纸球
    /// </summary>
    [SLua.CustomLuaClass]
    public class BallPaper : GameBall
    {
        private bool paperPeicesThowed = false;
        private int ticker = 0;

        public int pushTick= 5;
        public float windForce = 3f;
        public Vector3 windDirection = new Vector3(1.4f, 0.2f, 0.6f);

        public override void RecoverPieces()
        {
            base.RecoverPieces();
            paperPeicesThowed = true;
            ticker = pushTick;
        }
        public override void ThrowPieces()
        {
            base.ThrowPieces();
            paperPeicesThowed = false;
            ticker = 0;
        }

        private void Update()
        {
            if (paperPeicesThowed)
            {
                if (ticker > 0)
                {
                    ticker--;
                    if (ticker <= 0) {
                        PushPieces();
                        ticker = pushTick;
                    }
                }
            }
        }

        //这里添加纸球碎片被风吹的力
        private void PushPieces()
        {
            if (PiecesRigidbody != null)
                foreach (Rigidbody r in PiecesRigidbody)
                    r.AddForce(windDirection * windForce, ForceMode.Force);
        }
    }
}
