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
        public AnimationCurve animationCurveCameraY;
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

        private bool isFollowCam = false;
        private bool isLookingBall = false;
        private bool isCameraSpaced;
        private bool isCameraRoteing;
        private bool isCameraRoteingX;
        private bool isCameraRoteingY;
        private bool isCameraMovingY;
        private bool isCameraMovingYDown;
        private bool isCameraMovingZ;
        private bool isCameraMovingZFar;

        public int cameraRoteValue;
        public float cameraLeaveFarMaxOffest = 15f;
        public float cameraMaxRoteingOffest = 5f;
        public float cameraCurRoteingOffestX = 1f;
        public float cameraCurRoteingOffestY = 1f;
        public float cameraSpaceMaxOffest = 80f;

        public float cameraSpaceOffest = 10f;

        //摄像机方向控制
        //============================

        public float camFollowSpeed = 0.1f;
        public float camFollowSpeed2 = 0.05f;
        public float camMoveSpeedZ = 1f;

        public GameObject ballCamMoveY;
        public Camera ballCamera;
        public GameObject ballCamFollowHost;
        public GameObject ballCamFollowTarget;

        private Vector3 camVelocityTarget2 = new Vector3();
        private Transform camFollowTarget;
        private Vector3 camVelocityTarget = new Vector3();

        private float cameraNeedRoteingValueX;
        private float cameraRoteingRealValueX;
        private float cameraMovingZOld;

        /// <summary>
        /// 将摄像机开启
        /// </summary>
        public void CamStart() 
        {
            ballCamera.transform.position = new Vector3(0, cameraLeaveFarMaxOffest, -cameraLeaveFarMaxOffest);
            ballCamera.transform.eulerAngles = new Vector3(45, 0, 0);
            ballCamera.gameObject.SetActive(true);
            cameraMovingZOld = ballCamera.transform.localPosition.z; 
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
            if (!isCameraRoteing)
            {
                if (cameraRoteValue < 3)
                    cameraRoteValue++;
                else
                    cameraRoteValue = 0;
                cameraNeedRoteingValueX = 90f;
                isCameraRoteing = true;
                isCameraRoteingX = true;
                CamRote2();
            }
        }
        /// <summary>
        /// 摄像机向右旋转
        /// </summary>
        public void CamRoteRight()
        {
            if (!isCameraRoteing)
            {
                if (cameraRoteValue > 0)
                {
                    cameraRoteValue--;
                }
                else
                {
                    cameraRoteValue = 3;
                }
                cameraNeedRoteingValueX = -90f;
                isCameraRoteing = true;
                isCameraRoteingX = true;
                CamRote2();
            }
        }
        //摄像机面对向量重置
        private void CamRote2()
        {
            //根据摄像机朝向重置几个球推动的方向向量
            //    这4个方向向量用于球
            switch (cameraRoteValue)
            {
                case 0:
                    _thisVector3Right = Vector3.right;
                    _thisVector3Left = Vector3.left;
                    _thisVector3Fornt = Vector3.forward;
                    _thisVector3Back = Vector3.back;
                    break;
                case 1:
                    _thisVector3Right = Vector3.back;
                    _thisVector3Left = Vector3.forward;
                    _thisVector3Fornt = Vector3.right;
                    _thisVector3Back = Vector3.left;
                    break;
                case 2:
                    _thisVector3Right = Vector3.left;
                    _thisVector3Left = Vector3.right;
                    _thisVector3Fornt = Vector3.back;
                    _thisVector3Back = Vector3.forward;
                    break;
                case 3:
                    _thisVector3Right = Vector3.forward;
                    _thisVector3Left = Vector3.back;
                    _thisVector3Fornt = Vector3.left;
                    _thisVector3Back = Vector3.right;
                    break;
            }
        }
        //摄像机旋转偏移重置
        private void CamRote3()
        {
            cameraMovingZOld = ballCamera.transform.localPosition.z;
            switch (cameraRoteValue)
            {
                case 0:
                case 2:
                    if (ballCamera.transform.localPosition.x != 0) ballCamera.transform.localPosition = new Vector3(0, ballCamera.transform.localPosition.y, ballCamera.transform.localPosition.z);
                    break;
                case 1:
                case 3:
                    if (ballCamera.transform.localPosition.y != 0) ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, 0);
                    break;
            }
        }

        /// <summary>
        /// 摄像机 按住 空格键 上升
        /// </summary>
        public void CamRoteSpace()
        {
            if (!isCameraSpaced)
            {
                isCameraSpaced = true;
                isCameraRoteingY = true;
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
                isCameraRoteingY = true;
                isCameraMovingY = true;
                isCameraMovingYDown = true;
                isCameraMovingZ = true;
                isCameraMovingZFar = true;
                
            }
            
        }

        //几个动画计算曲线
        private float CamRoteSpeedFun(float cameraRoteingRealValue)
        {
            return animationCurveCamera.Evaluate(Mathf.Abs(cameraRoteingRealValue / 90)) * cameraMaxRoteingOffest;
        }
        private float CamMoveSpeedFunZ(float cameraRoteingRealValue)
        {
            return animationCurveCameraY.Evaluate(Mathf.Abs(cameraRoteingRealValue / cameraLeaveFarMaxOffest)) * camMoveSpeedZ;
        }
        private float CamMoveSpeedFunY(float cameraRoteingRealValue)
        {
            return animationCurveCameraMoveY.Evaluate(Mathf.Abs(cameraRoteingRealValue / cameraSpaceMaxOffest)) * cameraSpaceOffest;
        }
        private float CamMoveSpeedFunYDown(float cameraRoteingRealValue)
        {
            return animationCurveCameraMoveYDown.Evaluate(Mathf.Abs((cameraRoteingRealValue) / cameraSpaceMaxOffest)) * cameraSpaceOffest;
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
            if (isCameraRoteing)
            {
                //水平旋转
                if (isCameraRoteingX)
                {
                    if (cameraNeedRoteingValueX > 0.00001f)
                    {
                        if (cameraRoteingRealValueX < cameraNeedRoteingValueX)
                        {
                            cameraCurRoteingOffestX = CamRoteSpeedFun(cameraRoteingRealValueX);
                            cameraRoteingRealValueX += cameraCurRoteingOffestX;
                            ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, cameraCurRoteingOffestX);
                        }
                        else
                        {
                            float f = cameraNeedRoteingValueX - cameraRoteingRealValueX;
                            if (f > 0) ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -f);
                            CamRote3();
                            cameraRoteingRealValueX = 0f;
                            isCameraRoteingX = false;
                            isCameraRoteing = false;
                        }
                    }
                    else
                    {
                        if (cameraRoteingRealValueX > cameraNeedRoteingValueX)
                        {
                            cameraCurRoteingOffestX = CamRoteSpeedFun(cameraRoteingRealValueX);
                            cameraRoteingRealValueX -= cameraCurRoteingOffestX;
                            ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -cameraCurRoteingOffestX);
                        }
                        else
                        {
                            float f = cameraNeedRoteingValueX - cameraRoteingRealValueX;
                            if (f < 0) ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -f);
                            CamRote3();
                            cameraRoteingRealValueX = 0f;
                            isCameraRoteingX = false;
                            isCameraRoteing = false;
                        }
                    }
                }
            }

            //空格键 垂直上升
            if (isCameraMovingY)
            {
                if (isCameraMovingYDown)
                {
                    if (ballCamMoveY.transform.localPosition.y > 0)
                        ballCamMoveY.transform.localPosition = new Vector3(0, (ballCamMoveY.transform.localPosition.y - CamMoveSpeedFunYDown(ballCamMoveY.transform.localPosition.y)), 0);
                    else
                    {
                        ballCamMoveY.transform.localPosition = new Vector3(0, 0, 0);
                        isCameraMovingY = false;
                        if (!isCameraRoteingY)
                            isCameraRoteing = false;
                    }
                }
                else
                {
                    if (ballCamMoveY.transform.localPosition.y < cameraSpaceMaxOffest)
                        ballCamMoveY.transform.localPosition = new Vector3(0, ballCamMoveY.transform.localPosition.y + CamMoveSpeedFunY(ballCamMoveY.transform.localPosition.y), 0);
                    else
                    {
                        ballCamMoveY.transform.localPosition = new Vector3(0, cameraSpaceMaxOffest, 0);
                        isCameraMovingY = false;
                        if (!isCameraRoteingY)
                            isCameraRoteing = false;
                    }
                }
            }
            //空格键 靠近球
            if (isCameraMovingZ)
            {
                if (isCameraMovingZFar)
                {
                    float abs = Mathf.Abs(ballCamera.transform.localPosition.z - cameraMovingZOld);
                    if (abs > 1f)
                    {
                        if (ballCamera.transform.localPosition.z < cameraMovingZOld)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z + camMoveSpeedZ));
                        else if (ballCamera.transform.localPosition.z > cameraMovingZOld)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z - camMoveSpeedZ));
                    }
                    else
                    {
                        ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, cameraMovingZOld);
                        isCameraMovingZ = false;
                    }
                }
                else
                {
                    float end = cameraMovingZOld > 0 ? 5f : -5f;
                    float abs = Mathf.Abs(ballCamera.transform.localPosition.z - end);
                    if (abs > 0.2f)
                    {
                        if (ballCamera.transform.localPosition.z < end)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z + camMoveSpeedZ));
                        else if (ballCamera.transform.localPosition.z > end)
                            ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, (ballCamera.transform.localPosition.z - camMoveSpeedZ));
                    }
                    else
                    {
                        ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, ballCamera.transform.localPosition.y, end);
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
