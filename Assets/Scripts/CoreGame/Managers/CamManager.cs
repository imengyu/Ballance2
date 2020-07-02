using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.CoreGame.Interfaces;
using Ballance2.Managers;
using Ballance2.UI.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    /// <summary>
    /// 游戏摄像机管理器
    /// </summary>
    public class CamManager : BaseManager, ICamManager
    {
        public const string TAG = "CamManager";

        public CamManager() : base(TAG, "Singleton")
        {

        }

        private IBallManager BallManager;

        public override bool InitManager()
        {
            BallManager = (IBallManager)GameManager.GetManager("BallManager");
            ballCamera.gameObject.SetActive(false);
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private void FixedUpdate()
        {
            if (BallManager != null && BallManager.IsControlling)
                CamUpdate();
            if (isFollowCam)
                CamFollow();
        }

        [SerializeField, SetProperty("thisVector3Right")]
        public Vector3 _thisVector3Right = Vector3.right;
        [SerializeField, SetProperty("thisVector3Left")]
        public Vector3 _thisVector3Left  = Vector3.left;
        [SerializeField, SetProperty("thisVector3Fornt")]
        public Vector3 _thisVector3Fornt = Vector3.forward;
        [SerializeField, SetProperty("thisVector3Back")]
        public Vector3 _thisVector3Back = Vector3.back;

        /// <summary>
        /// 摄像机面对的右方向量
        /// </summary>
        public Vector3 thisVector3Right { get { return _thisVector3Right; } }
        /// <summary>
        /// 摄像机面对的左方向量
        /// </summary>
        public Vector3 thisVector3Left { get { return _thisVector3Left; } } 
        /// <summary>
        /// 摄像机面对的前方向量
        /// </summary>
        public Vector3 thisVector3Fornt { get { return _thisVector3Fornt; } } 
        /// <summary>
        /// 摄像机面对的后方向量
        /// </summary>
        public Vector3 thisVector3Back { get { return _thisVector3Back; } } 

        //一些摄像机的旋转动画曲线
        public AnimationCurve animationCurveCamera;
        public AnimationCurve animationCurveCameraZ;
        public AnimationCurve animationCurveCameraMoveY;
        public AnimationCurve animationCurveCameraMoveYDown;

        public Transform CamFollowTarget
        {
            get { return camFollowTarget; }
            set { camFollowTarget = value; }
        }
        /// <summary>
        /// 获取是否正在跟随球
        /// </summary>
        public bool IsFollowCam
        {
            get { return isFollowCam; }
            set { isFollowCam = value; }
        }
        /// <summary>
        /// 获取是否正在看着球
        /// </summary>
        public bool IsLookingBall
        {
            get { return isLookingBall; }
            set { isLookingBall = value; }
        }
        /// <summary>
        /// 获取或设置摄像机朝向
        /// </summary>
        public DirectionType CurrentDirection
        {
            get { return cameraRoteValue; }
            set
            {
                if(cameraRoteValue != value)
                {
                    cameraRoteValue = value;

                    CamRoteResetTarget(false);
                    CamRoteResetVector();

                    isCameraRoteingX = true;
                }
            }
        }

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
        private Transform camFollowTarget;
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
        public void CamStart() 
        {
            ballCamera.transform.position = new Vector3(0, cameraHeight, -cameraLeaveFarMaxOffest);
            ballCamera.transform.localEulerAngles = new Vector3(45, 0, 0);
            ballCamera.gameObject.SetActive(true);
            cameraRoteXVal = 0;
        }
        /// <summary>
        /// 将摄像机关闭
        /// </summary>
        public void CamClose()
        {
            ballCamera.gameObject.SetActive(false);
        }
        /// <summary>
        /// 让摄像机不看着球
        /// </summary>
        public void CamSetNoLookAtBall()
        {
            isLookingBall = false;
        }
        /// <summary>
        /// 让摄像机看着球
        /// </summary>
        public void CamSetLookAtBall()
        {
            if (BallManager.CurrentBall != null)
                isLookingBall = true;
        }
        /// <summary>
        /// 让摄像机只看着球
        /// </summary>
        public void CamSetJustLookAtBall()
        {
            if (BallManager.CurrentBall != null)
            {
                isFollowCam = false;
                isLookingBall = true;
            }
        }
        /// <summary>
        /// 摄像机向左旋转
        /// </summary>
        public void CamRoteLeft()
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
        public void CamRoteRight()
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
            _thisVector3Fornt = Quaternion.AngleAxis(-y, Vector3.up) * Vector3.forward;
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
                if (BallManager.CurrentBall != null)
                {
                    ballCamFollowTarget.transform.position = Vector3.SmoothDamp(ballCamFollowTarget.transform.position, BallManager.CurrentBall.transform.position, ref camVelocityTarget2, camFollowSpeed2);
                    //ballCamFollowHost.transform.position = Vector3.SmoothDamp(ballCamFollowHost.transform.position, ballCamFollowTarget.transform.position, ref camVelocityTarget, camFollowSpeed);
                    ballCamFollowHost.transform.position = Vector3.SmoothDamp(ballCamFollowHost.transform.position, BallManager.CurrentBall.transform.position, ref camVelocityTarget, camFollowSpeed);
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
                if (BallManager.CurrentBall != null)
                    ballCamera.transform.LookAt(BallManager.CurrentBall.transform);
            }
        }

    }

}
