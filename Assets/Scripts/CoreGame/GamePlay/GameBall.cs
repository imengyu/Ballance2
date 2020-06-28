using Ballance2.Config;
using Ballance2.CoreGame.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{         
    /// <summary>  
    /// 球推动类型
    /// </summary>
    public enum BallPushType
    {
        None,
        /// <summary>
        /// 前
        /// </summary>
        Forward = 2,
        /// <summary>
        /// 后
        /// </summary>
        Back = 4,
        /// <summary>
        /// 左
        /// </summary>
        Left = 8,
        /// <summary>
        /// 右
        /// </summary>
        Right = 16,
        /// <summary>
        /// 上
        /// </summary>
        Up = 32,
        /// <summary>
        /// 下
        /// </summary>
        Down = 64,

        ForwardLeft = Forward | Left,
        ForwardRight = Forward | Right,
        BackLeft = Back | Left,
        BackRight = Back | Right,
    }

    /// <summary>
    /// 球基础类
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameBall : MonoBehaviour
    {
        /// <summary>
        /// 球管理器
        /// </summary>
        public BallManager BallManager { get; private set; }

        public GameObject Pieces;
        public string TypeName;
        public Rigidbody Rigidbody;
        public float PushForce = 3f;
        public bool UseFallForce = false;
        public bool UseAutoCloseFallForce = true;
        public float FallForce = 10f;
        public float FallForceFloor = 10f;
        public bool LimitSpeed = true;

        public float MinSpeedY = 0f;
        public float MaxSpeedY = 0f;
        public float MinSpeedXZ = 0f;
        public float MaxSpeedXZ = 0f;

        public ForceMode FallForceMode = ForceMode.Force;
        public ForceMode ForceMode = ForceMode.Force;

        private bool isOnFloor = false;
        private bool debug = false;
        private Vector3 oldSpeed;

        private void Start()
        {
            BallManager = (BallManager)GameManager.GetManager(BallManager.TAG);
            debug = BallManager.IsBallDebug;
        }
        private void Update()
        {
        }

        private void OnCollisionEnter(Collision collision)
        {
            CurrentColObject = collision.gameObject.name;
            CurrentColObjectLayout = collision.gameObject.layer;
            if (collision.gameObject.layer >= GameLayers.LAYER_PHY_FLOOR && collision.gameObject.layer <= GameLayers.LAYER_PHY_RAIL/*Floor and ail*/)
                isOnFloor = true;
        }
        private void OnCollisionExit(Collision collision)
        {
            if (CurrentColObjectLayout == collision.gameObject.layer)
                CurrentColObjectLayout = 0;
            if (CurrentColObject == collision.gameObject.name)
                CurrentColObject = "";
            if (collision.gameObject.layer >= GameLayers.LAYER_PHY_FLOOR && collision.gameObject.layer <= GameLayers.LAYER_PHY_RAIL/*Floor and ail*/)
                isOnFloor = false;
        }

        //基础操作

        /// <summary>
        /// 重新设置位置
        /// </summary>
        /// <param name="pos">球位置（世界坐标）</param>
        public virtual void Recover(Vector3 pos)
        {
            Deactive();
            gameObject.transform.position = pos;
            gameObject.transform.eulerAngles = Vector3.zero;
        }
        /// <summary>
        /// 冻结球
        /// </summary>
        public virtual void Deactive()
        {
            if (Rigidbody != null)
                Rigidbody.Sleep();
            gameObject.SetActive(false);
        }
        /// <summary>
        /// 在指定位置激活球
        /// </summary>
        /// <param name="posWorld">球位置（世界坐标）</param>
        public void Active(Vector3 posWorld)
        {
            gameObject.transform.position = posWorld;
            if (Rigidbody != null)
            {
                Rigidbody.WakeUp();
                if (Rigidbody.isKinematic)
                    Rigidbody.isKinematic = false;
            }
            gameObject.SetActive(true);
        }
        /// <summary>
        /// 清除速度
        /// </summary>
        public void RemoveSpeed()
        {
            if (Rigidbody != null)
            {
                oldSpeed = Vector3.zero;
                Rigidbody.velocity = oldSpeed;
            }
        }

        /// <summary>
        /// 获取球是否在地面上
        /// </summary>
        public bool IsOnFloor { get { return isOnFloor; } }
        public string CurrentColObject { get; private set; }
        public int CurrentColObjectLayout { get; private set; }
        public float FinalFallForce { get; private set; }
        public float FinalPushForce { get; private set; }

        //控制

        /// <summary>
        /// 开始控制
        /// </summary>
        public void StartControll()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.WakeUp();
        }
        /// <summary>
        /// 结束控制
        /// </summary>
        /// <param name="hide">是否隐藏球</param>
        public void EndControll(bool hide)
        {
            Rigidbody.Sleep();
            Rigidbody.isKinematic = true;
            if (hide) gameObject.SetActive(false);
        }

        //碎片

        /// <summary>
        /// 开始抛掷碎片
        /// </summary>
        public void ThrowPieces()
        {
            if (Pieces != null)
            {

            }
        }
        /// <summary>
        /// 恢复碎片
        /// </summary>
        public void RecoverPieces()
        {
            if (Pieces != null)
            {

            }
        }

        /// <summary>
        /// 推动
        /// </summary>
        public void BallPush()
        {
            if (BallManager.IsControlling)
            {
                //
                //自动压力
                if (UseFallForce)
                {
                    float ySpeed = Mathf.Abs(Rigidbody.velocity.y);
                    if (isOnFloor) FinalFallForce = FallForceFloor;
                    else if (UseAutoCloseFallForce && ySpeed > MinSpeedY)
                    {
                        float refSpeed = ySpeed / MaxSpeedY;
                        FinalFallForce = FallForce * (1 - (refSpeed > 1 ? 1 : refSpeed));
                    }
                    else FinalFallForce = FallForce;
                    if (FinalFallForce > 0)
                        Rigidbody.AddForce(Vector3.down * FinalFallForce, FallForceMode);
                }

                //球速度控制

                //获取 ballsManager 的球推动类型。
                BallPushType currentBallPushType = BallManager.PushType;
                if (currentBallPushType != BallPushType.None)
                {
                    if ((currentBallPushType & BallPushType.Forward) == BallPushType.Forward)
                        Rigidbody.AddForce(BallManager.thisVector3Fornt * PushForce, ForceMode);
                    else if ((currentBallPushType & BallPushType.Back) == BallPushType.Back)
                        Rigidbody.AddForce(BallManager.thisVector3Back * PushForce, ForceMode);
                    if ((currentBallPushType & BallPushType.Left) == BallPushType.Left)
                        Rigidbody.AddForce(BallManager.thisVector3Left * PushForce, ForceMode);
                    else if ((currentBallPushType & BallPushType.Right) == BallPushType.Right)
                        Rigidbody.AddForce(BallManager.thisVector3Right * PushForce, ForceMode);

                    //调试模式可以上下飞行
                    if (debug)
                    {
                        if ((currentBallPushType & BallPushType.Up) == BallPushType.Up) //上
                            Rigidbody.AddForce(Vector3.up * PushForce * 2f, ForceMode);
                        else if ((currentBallPushType & BallPushType.Down) == BallPushType.Down)    //下
                            Rigidbody.AddForce(Vector3.down * PushForce * 0.5f, ForceMode);
                    }
                }
            }
        }
    }
}
