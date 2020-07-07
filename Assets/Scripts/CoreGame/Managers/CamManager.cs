using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.Managers;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    /// <summary>
    /// 游戏摄像机管理器
    /// </summary>
    class CamManager : BaseManager
    {
        public const string TAG = "CamManager";

        public CamManager() : base(GamePartName.CamManager, TAG, "Singleton")
        {

        }

        #region 全局数据共享

        //私有控制数据
        private StoreData thisVector3Right = null;
        private StoreData thisVector3Left = null;
        private StoreData thisVector3Forward = null;
        private StoreData thisVector3Back = null;
        private StoreData CamFollowTarget = null;//[Transform] 获取或设置跟随对象
        private StoreData IsFollowCam = null;//[bool] 获取或设置是否正在跟随球
        private StoreData IsLookingBall = null;//[bool] 获取或设置是否正在看着球
        private StoreData CurrentDirection = null;//[DirectionType] 获取或设置摄像机朝向
         
        //其他模块全局共享数据
        private StoreData CurrentBall = StoreData.Empty;
        private StoreData IsControlling = StoreData.Empty;

        private void InitGlobaShareAndStore(Store store)
        {
            //初始化数据桥
            thisVector3Right = store.AddParameter("thisVector3Right", StoreDataAccess.Get, StoreDataType.Vector3);
            thisVector3Left = store.AddParameter("thisVector3Left", StoreDataAccess.Get, StoreDataType.Vector3);
            thisVector3Forward = store.AddParameter("thisVector3Forward", StoreDataAccess.Get, StoreDataType.Vector3);
            thisVector3Back = store.AddParameter("thisVector3Back", StoreDataAccess.Get, StoreDataType.Vector3);
            IsFollowCam = store.AddParameter("IsFollowCam", StoreDataAccess.GetAndSet,  StoreDataType.Boolean);
            IsLookingBall = store.AddParameter("IsLookingBall", StoreDataAccess.GetAndSet,  StoreDataType.Boolean);
            CurrentDirection = store.AddParameter("CurrentDirection", StoreDataAccess.GetAndSet, StoreDataType.Custom);
            CamFollowTarget = store.AddParameter("CamFollowTarget", StoreDataAccess.GetAndSet, StoreDataType.Transform);

            //Get
            thisVector3Right.SetDataProvider(currentContext, () => _thisVector3Right);
            thisVector3Left.SetDataProvider(currentContext, () => _thisVector3Left);
            thisVector3Forward.SetDataProvider(currentContext, () => _thisVector3Forward);
            thisVector3Back.SetDataProvider(currentContext, () => _thisVector3Back);
            IsFollowCam.SetDataProvider(currentContext, () => isFollowCam);
            IsLookingBall.SetDataProvider(currentContext, () => isLookingBall);
            CurrentDirection.SetDataProvider(currentContext, () => cameraRoteValue);
            CamFollowTarget.SetDataProvider(currentContext, () => camFollowTarget);

            //Set
            CurrentDirection.RegisterDataObserver((storeV, oldV, newV) =>
            {
                DirectionType value = (DirectionType)newV;
                if (cameraRoteValue != value)
                {
                    cameraRoteValue = value;

                    CamRoteResetTarget(false);
                    CamRoteResetVector();

                    isCameraRoteingX = true;
                }
            });
            CamFollowTarget.RegisterDataObserver((storeV, oldV, newV) => camFollowTarget = (Transform)newV);
            IsLookingBall.RegisterDataObserver((storeV, oldV, newV) => isLookingBall = (bool)newV);
            IsFollowCam.RegisterDataObserver((storeV, oldV, newV) => isFollowCam = (bool)newV);

            
        }
        private void InitShareGlobalStore()
        {
            GameManager.RegisterManagerRedayCallback("BallManager", (self, store, manager) =>
            {
                CurrentBall = store.GetParameter("CurrentBall");
                IsControlling = store.GetParameter("IsControlling");
            });
        }
        private void InitActions()
        {
            //注册操作
            GameManager.GameMediator.RegisterActions(
                GameActionNames.CamManager,
                TAG,
                new GameActionHandlerDelegate[]
                {
                    (param) => {
                        CamStart();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamClose();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamSetNoLookAtBall();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamSetLookAtBall();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamSetJustLookAtBall();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamRoteLeft();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamRoteRight();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamRoteSpace();
                        return GameActionCallResult.SuccessResult;
                    },
                    (param) => {
                        CamRoteSpaceBack();
                        return GameActionCallResult.SuccessResult;
                    },
                },
                null
             );
        }
        private void UnInitActions()
        {
            GameManager.GameMediator.UnRegisterActions(GameActionNames.CamManager);
        }

        #endregion

        protected override void InitPre()
        {
            InitActions();
            InitShareGlobalStore();
            base.InitPre();
        }
        protected override bool InitStore(Store store)
        {
            InitGlobaShareAndStore(store);
            return base.InitStore(store);
        }

        public override bool InitManager()
        {
            ballCamera.gameObject.SetActive(false);
            return true;
        }
        public override bool ReleaseManager()
        {
            CamClose();
            UnInitActions();
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsControlling.IsNull() && IsControlling.BoolData())
                CamUpdate();
            if (isFollowCam)
                CamFollow();
        }

        [SerializeField, SetProperty("thisVector3Right")]
        public Vector3 _thisVector3Right = Vector3.right;
        [SerializeField, SetProperty("thisVector3Left")]
        public Vector3 _thisVector3Left  = Vector3.left;
        [SerializeField, SetProperty("thisVector3Fornt")]
        public Vector3 _thisVector3Forward = Vector3.forward;
        [SerializeField, SetProperty("thisVector3Back")]
        public Vector3 _thisVector3Back = Vector3.back;

        //一些摄像机的旋转动画曲线
        public AnimationCurve animationCurveCamera;
        public AnimationCurve animationCurveCameraZ;
        public AnimationCurve animationCurveCameraMoveY;
        public AnimationCurve animationCurveCameraMoveYDown;

        private bool isFollowCam = false;
        private bool isLookingBall = false;
        private bool isCameraSpaced;
        private bool isCameraRoteingX;
        private bool isCameraMovingY;
        private bool isCameraMovingYDown;
        private bool isCameraMovingZ;
        private bool isCameraMovingZFar;

        public DirectionType cameraRoteValue = DirectionType.Forward;
        public float cameraHeight = 15f;
        public float cameraLeaveFarMaxOffest = 15f;
        public float cameraLeaveNearMaxOffestZ = -5f;
        public float cameraLeaveFarMaxOffestZ = -15f;
        public float cameraMaxRoteingOffest = 5f;
        public float cameraSpaceMaxOffest = 80f;
        public float cameraSpaceOffest = 10f;

        //摄像机方向控制
        //============================

        public float camFollowSpeed = 0.1f;
        public float camFollowSpeed2 = 0.05f;
        public float camMoveSpeedZ = 1f;

        public GameObject ballCamMoveHost;
        public Camera ballCamera;
        public GameObject ballCamFollowHost;
        public GameObject ballCamFollowTarget;

        private Vector3 camVelocityTarget2 = new Vector3();
        private Transform camFollowTarget = null;
        private Vector3 camVelocityTarget = new Vector3();

        private float cameraRoteXVal = 0;
        private float cameraRoteXTarget = 0;
        private float cameraRoteXAll = 0;

        private Rect line = new Rect(10, 135, 300, 20);

        void OnGUI()
        {
            /*if (isFollowCam)
            {
                line.y = 185;
                GUI.Label(line, "cameraRoteXVal : " + cameraRoteXVal + "  y: " + ballCamMoveY.transform.localEulerAngles.y); line.y += 16;
                GUI.Label(line, "cameraRoteXTarget : " + cameraRoteXTarget); line.y += 16;
                GUI.Label(line, "cameraRoteXAll : " + cameraRoteXAll); line.y += 16;
            }*/
        }

        /// <summary>
        /// 将摄像机开启
        /// </summary>
        private void CamStart() 
        {
            ballCamera.transform.position = new Vector3(0, cameraHeight, -cameraLeaveFarMaxOffest);
            ballCamera.transform.localEulerAngles = new Vector3(45, 0, 0);
            ballCamera.gameObject.SetActive(true);
            cameraRoteXVal = 0;
        }
        /// <summary>
        /// 将摄像机关闭
        /// </summary>
        private void CamClose()
        {
            ballCamera.gameObject.SetActive(false);
        }
        /// <summary>
        /// 让摄像机不看着球
        /// </summary>
        private void CamSetNoLookAtBall()
        {
            isLookingBall = false;
        }
        /// <summary>
        /// 让摄像机看着球
        /// </summary>
        private void CamSetLookAtBall()
        {
            if (!CurrentBall.IsNull())
                isLookingBall = true;
        }
        /// <summary>
        /// 让摄像机只看着球（不跟随）
        /// </summary>
        private void CamSetJustLookAtBall()
        {
            if (!CurrentBall.IsNull())
            {
                isFollowCam = false;
                isLookingBall = true;
            }
        }
        /// <summary>
        /// 摄像机向左旋转
        /// </summary>
        private void CamRoteLeft()
        {
            if (cameraRoteValue < DirectionType.Right) cameraRoteValue++;
            else cameraRoteValue = DirectionType.Forward;

            CamRoteResetTarget(true);
            CamRoteResetVector();

            isCameraRoteingX = true;
        }
        /// <summary>
        /// 摄像机向右旋转
        /// </summary>
        private void CamRoteRight()
        {
            if (cameraRoteValue > DirectionType.Forward) cameraRoteValue--;
            else cameraRoteValue = DirectionType.Right;

            CamRoteResetTarget(false);
            CamRoteResetVector();

            isCameraRoteingX = true;
        }
        //摄像机面对向量重置
        private void CamRoteResetVector()
        {
            //根据摄像机朝向重置几个球推动的方向向量
            //    这4个方向向量用于球
            float y = -ballCamMoveHost.transform.localEulerAngles.y;
            _thisVector3Right = Quaternion.AngleAxis(-y, Vector3.up) * Vector3.right;
            _thisVector3Left = Quaternion.AngleAxis(-y, Vector3.up) * Vector3.left;
            _thisVector3Forward = Quaternion.AngleAxis(-y, Vector3.up) * Vector3.forward;
            _thisVector3Back = Quaternion.AngleAxis(-y, Vector3.up) * Vector3.back;
        }
        //摄像机旋转目标
        private void CamRoteResetTarget(bool left)
        { 
            cameraRoteXTarget += (left ? 90 : -90);
            cameraRoteXAll = Mathf.Abs(cameraRoteXTarget - cameraRoteXVal);
        }
        //摄像机旋转偏移重置
        private void CamRoteResetTargetOffest()
        {
            if (ballCamMoveHost.transform.localEulerAngles.y != cameraRoteXTarget)
                ballCamMoveHost.transform.localEulerAngles =
                    new Vector3(0, cameraRoteXTarget, 0);
        }

        /// <summary>
        /// 摄像机 按住 空格键 上升
        /// </summary>
        public void CamRoteSpace()
        {
            if (!isCameraSpaced)
            {
                isCameraSpaced = true;
                isCameraMovingY = true;
                isCameraMovingYDown = false;
                isCameraMovingZ = true;
                isCameraMovingZFar = false;
            }
           
        }
        /// <summary>
        /// 摄像机 放开 空格键 下降
        /// </summary>
        public void CamRoteSpaceBack()
        {
            if (isCameraSpaced)
            {
                isCameraSpaced = false;
                isCameraMovingY = true;
                isCameraMovingYDown = true;
                isCameraMovingZ = true;
                isCameraMovingZFar = true;
                
            }
            
        }

        //几个动画计算曲线
        private float CamRoteSpeedFun(float v)
        {
            return animationCurveCamera.Evaluate((
                Mathf.Abs(cameraRoteXTarget -  v) / cameraRoteXAll)
                ) * cameraMaxRoteingOffest;
        }
        private float CamMoveSpeedFunZ(float v)
        {
            return animationCurveCameraZ.Evaluate(Mathf.Abs(v / cameraLeaveFarMaxOffest)) * camMoveSpeedZ;
        }
        private float CamMoveSpeedFunY(float v)
        {
            return animationCurveCameraMoveY.Evaluate(Mathf.Abs(v / cameraSpaceMaxOffest)) * cameraSpaceOffest;
        }
        private float CamMoveSpeedFunYDown(float v)
        {
            return animationCurveCameraMoveYDown.Evaluate(Mathf.Abs((v) / cameraSpaceMaxOffest)) * cameraSpaceOffest;
        }

        //摄像机跟随 每帧
        private void CamFollow()
        {
            if (camFollowTarget == null)
            {
                isFollowCam = false;
                return;
            }
            if (isFollowCam)
            {
                if (!CurrentBall.IsNull())
                {
                    ballCamFollowTarget.transform.position = Vector3.SmoothDamp(ballCamFollowTarget.transform.position, ((GameBall)CurrentBall.Data()).transform.position, ref camVelocityTarget2, camFollowSpeed2);
                    ballCamFollowHost.transform.position = Vector3.SmoothDamp(ballCamFollowHost.transform.position, ((GameBall)CurrentBall.Data()).transform.position, ref camVelocityTarget, camFollowSpeed);
                }
            }
        }
        private void CamUpdate()
        {
            //水平旋转
            if (isCameraRoteingX)
            {
                float abs = Mathf.Abs(cameraRoteXVal - cameraRoteXTarget);
                float off = CamRoteSpeedFun(cameraRoteXVal);
                if (abs > 0.8f)
                {
                    if (off > abs) off = abs - 0.1f;
                    if (cameraRoteXVal < cameraRoteXTarget)
                        cameraRoteXVal += off;
                    else if (cameraRoteXVal > cameraRoteXTarget)
                        cameraRoteXVal -= off;

                    ballCamMoveHost.transform.localEulerAngles = new Vector3(0,
                            cameraRoteXVal,
                            0);
                }
                else
                {
                    isCameraRoteingX = false;
                    CamRoteResetTargetOffest();
                }

                CamRoteResetVector();
            }

            //空格键 垂直上升
            if (isCameraMovingY)
            {
                if (isCameraMovingYDown)
                {
                    if (ballCamMoveHost.transform.localPosition.y > 0)
                        ballCamMoveHost.transform.localPosition = new Vector3(0, (ballCamMoveHost.transform.localPosition.y - CamMoveSpeedFunYDown(ballCamMoveHost.transform.localPosition.y)), 0);
                    else
                    {
                        ballCamMoveHost.transform.localPosition = new Vector3(0, 0, 0);
                        isCameraMovingY = false;
                    }
                }
                else
                {
                    if (ballCamMoveHost.transform.localPosition.y < cameraSpaceMaxOffest)
                        ballCamMoveHost.transform.localPosition = new Vector3(0, ballCamMoveHost.transform.localPosition.y + CamMoveSpeedFunY(ballCamMoveHost.transform.localPosition.y), 0);
                    else
                    {
                        ballCamMoveHost.transform.localPosition = new Vector3(0, cameraSpaceMaxOffest, 0);
                        isCameraMovingY = false;
                    }
                }
            }
            //空格键 靠近球
            if (isCameraMovingZ)
            {
                if (isCameraMovingZFar)
                {
                    float abs = Mathf.Abs(ballCamera.transform.localPosition.z - cameraLeaveFarMaxOffestZ);
                    float off = CamMoveSpeedFunZ(ballCamera.transform.localPosition.z);
                    if (abs > 1f)
                    {
                        if (off > abs) off = abs - 0.1f;
                        if (ballCamera.transform.localPosition.z < cameraLeaveFarMaxOffestZ)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z + off));
                        else if (ballCamera.transform.localPosition.z > cameraLeaveFarMaxOffestZ)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z - off));
                    }
                    else
                    {
                        ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, cameraLeaveFarMaxOffestZ);
                        isCameraMovingZ = false;
                    }
                }
                else
                {
                    float abs = Mathf.Abs(ballCamera.transform.localPosition.z - cameraLeaveNearMaxOffestZ);
                    float off = CamMoveSpeedFunZ(ballCamera.transform.localPosition.z);
                    if (abs > 0.2f)
                    {
                        if (off > abs) off = abs - 0.1f;
                        if (ballCamera.transform.localPosition.z < cameraLeaveNearMaxOffestZ)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z + off));
                        else if (ballCamera.transform.localPosition.z > cameraLeaveNearMaxOffestZ)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z - off));
                    }
                    else
                    {
                        ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, cameraLeaveNearMaxOffestZ);
                        isCameraMovingZ = false;
                    }
                }
            }

            //看着球
            if (isLookingBall && isFollowCam)
            {
                ballCamera.transform.LookAt(ballCamFollowTarget.transform);
            }
            else if (isLookingBall && !isFollowCam)
            {
                if (!CurrentBall.IsNull())
                    ballCamera.transform.LookAt(((GameBall)CurrentBall.Data()).transform);
            }
        }

    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 方向朝向
    /// </summary>
    public enum DirectionType
    {
        Forward,
        Left,
        Back,
        Right,
    }
}
