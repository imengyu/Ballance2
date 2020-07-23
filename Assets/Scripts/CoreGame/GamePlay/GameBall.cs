using Ballance2.CoreBridge;
using Ballance2.Interfaces;
using System.Collections.Generic;
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
        Forward = 0x2,
        /// <summary>
        /// 后
        /// </summary>
        Back = 0x4,
        /// <summary>
        /// 左
        /// </summary>
        Left = 0x8,
        /// <summary>
        /// 右
        /// </summary>
        Right = 0x10,
        /// <summary>
        /// 上
        /// </summary>
        Up = 0x20,
        /// <summary>
        /// 下
        /// </summary>
        Down = 0x40,

        ForwardLeft = Forward | Left,
        ForwardRight = Forward | Right,
        BackLeft = Back | Left,
        BackRight = Back | Right,
    }

    /// <summary>
    /// 球基础类
    /// </summary>
    [SLua.CustomLuaClass]
    [AddComponentMenu("Ballance/Game/GameBall")]
    [RequireComponent(typeof(Rigidbody))]
    public class GameBall : MonoBehaviour
    {
        /// <summary>
        /// 碎片控制器
        /// </summary>
        public GameBallPiecesControl BallPiecesControl { get; set; }

        [Tooltip("球的碎片")]
        public GameObject Pieces;
        [Tooltip("球类型名字")]
        public string TypeName;
        [Tooltip("刚体")]
        public Rigidbody Rigidbody;

        [Tooltip("球的推力类型")]
        public ForceMode ForceMode = ForceMode.Force;

        [Tooltip("球的推力大小")]
        public float PushForce = 3f;
        [Tooltip("球的推力位置")]
        public Vector3 PushForcePosition = new Vector3(0, 0, 0);
        [Tooltip("球的推力下偏角度(0-45)")]
        [Range(0.0f, 45f)]
        public float PushForceDownAngle = 0;
        [Tooltip("球的向上推力大小")]
        public float PushUpForce = 13f;
        [Tooltip("球的自动下落力")]
        public float FallForce = 0;

        [Tooltip("球的XZ轴移动最大速度（这个用来模拟移动空气阻力）")]
        public float MaxSpeedXZ = 0;
        public float MaxSpeedXZCurrctRatio = 1;
        public float MaxSpeedXZForceMax = 1;

        [Tooltip("球的Y轴移动最大速度（这个用来模拟下落空气阻力）")]
        public float MaxSpeedY = 0;
        public float MaxSpeedYCurrctRatio = 1;
        public float MaxSpeedYForceMax = 1;

        [Tooltip("抛出碎片的力大小")]
        public float ThrowPiecesForce = 5f;
        [Tooltip("自动回收碎片时间（秒）")]
        public float CollectPiecesSec = 15f;

        internal float CollectPiecesSecTick = 5f;

        public List<Rigidbody> PiecesRigidbody { get { return piecesRigidbody; } }
        public List<MeshRenderer> PiecesMaterial { get { return piecesMaterial; } }

        private bool isOnFloor = false;
        private Vector3 oldSpeed;

        protected void OnCollisionEnter(Collision collision)
        {
            CurrentColObject = collision.gameObject.name;
            CurrentColObjectLayout = collision.gameObject.layer;
            if (collision.gameObject.layer == GameLayers.LAYER_PHY_FLOOR
                || collision.gameObject.layer == GameLayers.LAYER_PHY_RAIL/*Floor and ail*/)
                isOnFloor = true;
        }
        protected void OnCollisionExit(Collision collision)
        {
            if (CurrentColObjectLayout == collision.gameObject.layer)
                CurrentColObjectLayout = 0;
            if (CurrentColObject == collision.gameObject.name)
            {
                CurrentColObject = "";
                isOnFloor = false;
            }
            if (collision.gameObject.layer == GameLayers.LAYER_PHY_FLOOR 
                || collision.gameObject.layer == GameLayers.LAYER_PHY_RAIL/*Floor and rail*/)
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
        public virtual void Active(Vector3 posWorld)
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
        public virtual void RemoveSpeed()
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
        /// <summary>
        /// 当前球触碰的物体
        /// </summary>
        public string CurrentColObject { get; private set; }
        /// <summary>
        /// 当前球触碰的物体所在层
        /// </summary>
        public int CurrentColObjectLayout { get; private set; }
        /// <summary>
        /// 获取球最终的推力
        /// </summary>
        public float FinalPushForce { get; private set; }
        /// <summary>
        /// 获取当前球最终的推力方向
        /// </summary>
        public Vector3 FinalPushForceVectorFB = Vector3.zero;
        /// <summary>
        /// 获取当前球最终的推力方向
        /// </summary>
        public Vector3 FinalPushForceVectorLR = Vector3.zero;

        //控制

        /// <summary>
        /// 开始控制
        /// </summary>
        public virtual void StartControll()
        {
            Rigidbody.isKinematic = false;
            Rigidbody.WakeUp();
        }
        /// <summary>
        /// 结束控制
        /// </summary>
        /// <param name="hide">是否隐藏球</param>
        public virtual void EndControll(bool hide)
        {
            Rigidbody.Sleep();
            Rigidbody.isKinematic = true;
            if (hide) gameObject.SetActive(false);
        }

        private IICManager ICManager = null;

        #region  其他模块全局共享数据

        private Store storeBallmgr = null;
        private StoreData IsControlling = null;
        private StoreData PushType = null;
        private StoreData IsBallDebug = null;
        private Store storeCammgr = null;
        private StoreData thisVector3Right = null;
        private StoreData thisVector3Left = null;
        private StoreData thisVector3Forward = null;
        private StoreData thisVector3Back = null;

        private void InitShareData()
        {
            storeBallmgr = GameManager.GameMediator.GetGlobalDataStore("core.ballmgr");
            storeCammgr = GameManager.GameMediator.GetGlobalDataStore("core.cammgr");
            IsControlling = storeBallmgr.GetParameter("IsControlling");
            PushType = storeBallmgr.GetParameter("PushType");
            IsBallDebug = storeBallmgr.GetParameter("IsBallDebug");
            thisVector3Right = storeCammgr.GetParameter("thisVector3Right");
            thisVector3Left = storeCammgr.GetParameter("thisVector3Left");
            thisVector3Forward = storeCammgr.GetParameter("thisVector3Forward");
            thisVector3Back = storeCammgr.GetParameter("thisVector3Back");
        }

        #endregion

        public virtual bool Init()
        {
            ICManager = (IICManager)GameManager.GetManager("ICManager");
            InitShareData();
            BuildPiecesData();
            return true;
        }
        public virtual void Destroy()
        {
            if (piecesRigidbody != null)
            {
                piecesRigidbody.Clear();
                piecesRigidbody = null;
            }
            if (piecesMaterial != null)
            {
                piecesMaterial.Clear();
                piecesMaterial = null;
            }
        }

        //碎片

        private List<Rigidbody> piecesRigidbody = null;
        private List<MeshRenderer> piecesMaterial = null;

        private void BuildPiecesData()
        {
            if (piecesRigidbody == null)
            {
                piecesRigidbody = new List<Rigidbody>();
                Rigidbody r = null;
                for (int i = 0; i < Pieces.transform.childCount; i++)
                {
                    r = Pieces.transform.GetChild(i).GetComponent<Rigidbody>();
                    //保存IC
                    ICManager.BackupIC(Pieces.transform.GetChild(i).gameObject);

                    if (r != null) piecesRigidbody.Add(r);
                }
            }
            if (piecesMaterial == null)
            {
                piecesMaterial = new List<MeshRenderer>();
                MeshRenderer r = null;
                for (int i = 0; i < Pieces.transform.childCount; i++)
                {
                    r = Pieces.transform.GetChild(i).GetComponent<MeshRenderer>();
                    if (r != null && r.material != null) piecesMaterial.Add(r);
                }
            }
        }

        private Vector3 pushVectorReverse = new Vector3();
        private Vector3 pushPositionLocal = new Vector3();

        /// <summary>
        /// 推动
        /// </summary>
        public virtual void BallPush()
        {
            if (IsControlling.BoolData())
            {
                FinalPushForceVectorFB = Vector3.zero;
                FinalPushForceVectorLR = Vector3.zero;
                pushPositionLocal.x = transform.position.x + PushForcePosition.x;
                pushPositionLocal.y = transform.position.y + PushForcePosition.y;
                pushPositionLocal.z = transform.position.z + PushForcePosition.z;

                //获取 ballsManager 的球推动类型。
                BallPushType currentBallPushType = PushType.Data<BallPushType>();
                if (currentBallPushType != BallPushType.None)
                {
                    if ((currentBallPushType & BallPushType.Forward) == BallPushType.Forward)
                    {
                        if (PushForceDownAngle > 0)
                            FinalPushForceVectorFB = Quaternion.AngleAxis(-PushForceDownAngle, thisVector3Left.Vector3Data()) *
                                thisVector3Forward.Vector3Data() * PushForce;
                        else FinalPushForceVectorFB = thisVector3Forward.Vector3Data() * PushForce;
                    }
                    else if ((currentBallPushType & BallPushType.Back) == BallPushType.Back)
                    {
                        if (PushForceDownAngle > 0)
                            FinalPushForceVectorFB = Quaternion.AngleAxis(PushForceDownAngle, thisVector3Left.Vector3Data()) *
                                thisVector3Back.Vector3Data() * PushForce;
                        else FinalPushForceVectorFB = thisVector3Back.Vector3Data() * PushForce;
                    }
                    if ((currentBallPushType & BallPushType.Left) == BallPushType.Left)
                    {
                        if (PushForceDownAngle > 0)
                            FinalPushForceVectorLR = Quaternion.AngleAxis(PushForceDownAngle, thisVector3Forward.Vector3Data()) *
                                thisVector3Left.Vector3Data() * PushForce;
                        else FinalPushForceVectorLR = thisVector3Left.Vector3Data() * PushForce;
                    }
                    else if ((currentBallPushType & BallPushType.Right) == BallPushType.Right)
                    {
                        if (PushForceDownAngle > 0)
                            FinalPushForceVectorLR = Quaternion.AngleAxis(-PushForceDownAngle, thisVector3Forward.Vector3Data()) *
                                thisVector3Right.Vector3Data() * PushForce;
                        else FinalPushForceVectorLR = thisVector3Right.Vector3Data() * PushForce;
                    }

                    if (FinalPushForceVectorFB != Vector3.zero) Rigidbody.AddForceAtPosition(FinalPushForceVectorFB, pushPositionLocal, ForceMode); 
                    if (FinalPushForceVectorLR != Vector3.zero) Rigidbody.AddForceAtPosition(FinalPushForceVectorLR, pushPositionLocal, ForceMode);

                    //调试模式可以上下飞行
                    if (IsBallDebug.BoolData())
                    {
                        if ((currentBallPushType & BallPushType.Up) == BallPushType.Up) //上
                            Rigidbody.AddForce(Vector3.up * PushUpForce, ForceMode);
                        else if ((currentBallPushType & BallPushType.Down) == BallPushType.Down)    //下
                            Rigidbody.AddForce(Vector3.down * PushForce * 0.5f, ForceMode);
                    }
                }

                if (FallForce > 0 && (currentBallPushType & BallPushType.Up) != BallPushType.Up)
                    Rigidbody.AddForce(Vector3.down * FallForce, ForceMode);

                //Y轴移动最大速度（这个用来模拟下落空气阻力）
                if (MaxSpeedY > 0)
                {
                    float speedOut = Mathf.Abs(Rigidbody.velocity.y) - Mathf.Abs(MaxSpeedY);
                    if (speedOut > 0)
                    {
                        float force = (speedOut / MaxSpeedYCurrctRatio) * MaxSpeedYForceMax;
                        Rigidbody.AddForce((Rigidbody.velocity.y < 0 ? (Vector3.up) : Vector3.down) *
                            (force + (Rigidbody.velocity.y < 0 ? FallForce : 0)),
                            ForceMode);
                    }
                }

                //XZ轴移动最大速度（这个用来模拟移动空气阻力）
                if (MaxSpeedXZ > 0)
                {
                    float speedOutXZ = Mathf.Sqrt(Mathf.Pow(Rigidbody.velocity.x, 2) +
                        Mathf.Pow(Rigidbody.velocity.z, 2)) - Mathf.Abs(MaxSpeedXZ);
                    if (speedOutXZ > 0)
                    {
                        float force = (speedOutXZ / MaxSpeedXZCurrctRatio) * MaxSpeedXZForceMax;
                        pushVectorReverse.x = -Rigidbody.velocity.x;
                        pushVectorReverse.z = -Rigidbody.velocity.z;
                        Rigidbody.AddForce(pushVectorReverse * force, ForceMode);
                    }
                }

            }
        }
    }
}
