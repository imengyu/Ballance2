using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 纸球碎片控制
    /// </summary>
    public class GameBallPaperPiecesControl : GameBallPiecesControl
    {
        private bool paperPeicesThowed = false;
        private int ticker = 0;

        public int pushTick= 5;
        public float windForce = 1.0f;
        public Vector3 windDirection = new Vector3(1.4f, 0.0f, 0.6f);

        public override bool RecoverPieces()
        {
            paperPeicesThowed = false;
            ticker = 0;
            return base.RecoverPieces();
        }
        public override bool ThrowPieces()
        {
            paperPeicesThowed = true;
            ticker = pushTick;
            return base.ThrowPieces();
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
            if (Ball.PiecesRigidbody != null)
                foreach (Rigidbody r in Ball.PiecesRigidbody)
                    r.AddForce(windDirection * windForce, ForceMode.Force);
        }
    }
}
