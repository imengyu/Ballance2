using Ballance2.Config;
using Ballance2.CoreBridge;
using Ballance2.CoreGame.GamePlay;
using Ballance2.Managers;
using Ballance2.UI.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.Managers
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// 游戏摄像机管理器
    /// </summary>
    public class CamManager : BaseManagerBindable
    {
        public const string TAG = "CamManager";

        public CamManager() : base(TAG, "Singleton")
        {

        }

        private BallManager BallManager;

        public override bool InitManager()
        {
            BallManager = (BallManager)GameManager.GetManager(BallManager.TAG);
            return true;
        }
        public override bool ReleaseManager()
        {
            return true;
        }

        private void FixedUpdate()
        {
            if (BallManager.IsControlling)
                CamUpdate();
            if (isFollowCam)
                CamFollow();
        }

        /// <summary>
        /// 摄像机面对的右方向量
        /// </summary>
        public Vector3 thisVector3Right { get; private set; }
        /// <summary>
        /// 摄像机面对的左方向量
        /// </summary>
        public Vector3 thisVector3Left { get; private set; }
        /// <summary>
        /// 摄像机面对的前方向量
        /// </summary>
        public Vector3 thisVector3Fornt { get; private set; }
        /// <summary>
        /// 摄像机面对的后方向量
        /// </summary>
        public Vector3 thisVector3Back { get; private set; }

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

        public int cameraRoteValue;
        public float cameraMaxRoteingOffest = 5f;
        public float cameraCurRoteingOffest = 1f;
        public float cameraSpaceMaxOffest = 80f;
        public float cameraSpaceOffest = 10f;

        //摄像机方向控制
        //============================

        public float camFollowSpeed = 0.1f;
        public float camFollowSpeed2 = 0.05f;

        public GameObject ballCamMoveY;
        public Camera ballCamera;
        public GameObject ballCamFollowHost;
        public GameObject ballCamFollowTarget;


        private Vector3 camVelocityTarget2 = new Vector3();
        private Transform camFollowTarget;
        private Vector3 camVelocityTarget = new Vector3();
        private float cameraNeedRoteingValue;
        private float cameraRoteingRealValue;

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
                cameraNeedRoteingValue = 90f;
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
                cameraNeedRoteingValue = -90f;
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
                    thisVector3Right = Vector3.right;
                    thisVector3Left = Vector3.left;
                    thisVector3Fornt = Vector3.forward;
                    thisVector3Back = Vector3.back;
                    break;
                case 1:
                    thisVector3Right = Vector3.back;
                    thisVector3Left = Vector3.forward;
                    thisVector3Fornt = Vector3.right;
                    thisVector3Back = Vector3.left;
                    break;
                case 2:
                    thisVector3Right = Vector3.left;
                    thisVector3Left = Vector3.right;
                    thisVector3Fornt = Vector3.back;
                    thisVector3Back = Vector3.forward;
                    break;
                case 3:
                    thisVector3Right = Vector3.forward;
                    thisVector3Left = Vector3.back;
                    thisVector3Fornt = Vector3.left;
                    thisVector3Back = Vector3.right;
                    break;
            }
        }
        //摄像机旋转偏移重置
        private void CamRote3()
        {
            switch (cameraRoteValue)
            {
                case 0:
                case 2:
                    if (ballCamera.transform.localPosition.x != 0) ballCamera.transform.localPosition = new Vector3(0, ballCamera.transform.localPosition.y, ballCamera.transform.localPosition.z);
                    break;
                case 1:
                case 3:
                    if (ballCamera.transform.localPosition.y != 0) ballCamera.transform.localPosition = new Vector3(ballCamera.transform.localPosition.x, 0, ballCamera.transform.localPosition.z);
                    break;
            }
        }

        /// <summary>
        /// 摄像机 按住 空格键 上升
        /// </summary>
        public void CamRoteSpace()
        {
            if (!isCameraRoteing && !isCameraSpaced)
            {
                cameraNeedRoteingValue = -27f;
                isCameraRoteing = true;
                isCameraRoteingY = true;
                isCameraMovingY = true;
                isCameraMovingYDown = false;
            }
        }
        /// <summary>
        /// 摄像机 放开 空格键 下降
        /// </summary>
        public void CamRoteSpaceBack()
        {
            if (isCameraSpaced)
            {
                cameraNeedRoteingValue = 27f;
                isCameraRoteing = true;
                isCameraRoteingY = true;
                isCameraMovingY = true;
                isCameraMovingYDown = true;
            }
        }

        //几个动画计算曲线
        private float CamRoteSpeedFun(float cameraRoteingRealValue)
        {
            return animationCurveCamera.Evaluate(Mathf.Abs(cameraRoteingRealValue / 90)) * cameraMaxRoteingOffest;
        }
        private float CamRoteSpeedFunY(float cameraRoteingRealValue)
        {
            return animationCurveCameraY.Evaluate(Mathf.Abs(cameraRoteingRealValue / 27)) * cameraMaxRoteingOffest;
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
                    if (cameraNeedRoteingValue > 0.00001f)
                    {
                        if (cameraRoteingRealValue < cameraNeedRoteingValue)
                        {
                            cameraCurRoteingOffest = CamRoteSpeedFun(cameraRoteingRealValue);
                            cameraRoteingRealValue += cameraCurRoteingOffest;
                            ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, cameraCurRoteingOffest);
                        }
                        else
                        {
                            float f = cameraNeedRoteingValue - cameraRoteingRealValue;
                            if (f > 0) ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -f);
                            CamRote3();
                            cameraRoteingRealValue = 0f;
                            isCameraRoteingX = false;
                            isCameraRoteing = false;
                        }
                    }
                    else
                    {
                        if (cameraRoteingRealValue > cameraNeedRoteingValue)
                        {
                            cameraCurRoteingOffest = CamRoteSpeedFun(cameraRoteingRealValue);
                            cameraRoteingRealValue -= cameraCurRoteingOffest;
                            ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -cameraCurRoteingOffest);
                        }
                        else
                        {
                            float f = cameraNeedRoteingValue - cameraRoteingRealValue;
                            if (f < 0) ballCamera.transform.RotateAround(ballCamFollowHost.transform.position, Vector3.up, -f);
                            CamRote3();
                            cameraRoteingRealValue = 0f;
                            isCameraRoteingX = false;
                            isCameraRoteing = false;
                        }
                    }
                }
                //垂直旋转
                if (isCameraRoteingY)
                {
                    if (cameraNeedRoteingValue > 0f)
                    {
                        if (cameraRoteingRealValue < cameraNeedRoteingValue)
                        {
                            cameraCurRoteingOffest = CamRoteSpeedFunY(cameraRoteingRealValue);
                            cameraRoteingRealValue += cameraCurRoteingOffest;
                            ballCamera.transform.RotateAround(ballCamMoveY.transform.position, thisVector3Left, cameraCurRoteingOffest);
                        }
                        else
                        {
                            isCameraSpaced = false;
                            cameraRoteingRealValue = 0f;
                            isCameraRoteingY = false;
                            if (!isCameraMovingY)
                                isCameraRoteing = false;
                        }
                    }
                    else if (cameraNeedRoteingValue < 0f)
                    {
                        if (cameraRoteingRealValue > cameraNeedRoteingValue)
                        {
                            cameraCurRoteingOffest = CamRoteSpeedFunY(cameraRoteingRealValue);
                            cameraRoteingRealValue -= cameraCurRoteingOffest;
                            ballCamera.transform.RotateAround(ballCamMoveY.transform.position, thisVector3Left, -cameraCurRoteingOffest);
                        }
                        else
                        {
                            isCameraSpaced = true;
                            cameraRoteingRealValue = 0f;
                            isCameraRoteingY = false;
                            if (!isCameraMovingY)
                                isCameraRoteing = false;
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
