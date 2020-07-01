using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.CoreGame.Interfaces
{
    /// <summary>
    /// 游戏摄像机管理器
    /// </summary>
    [SLua.CustomLuaClass]
    public interface ICamManager
    {


        /// <summary>
        /// 摄像机面对的右方向量
        /// </summary>
        Vector3 thisVector3Right { get; }
        /// <summary>
        /// 摄像机面对的左方向量
        /// </summary>
        Vector3 thisVector3Left { get; }
        /// <summary>
        /// 摄像机面对的前方向量
        /// </summary>
        Vector3 thisVector3Fornt { get; }
        /// <summary>
        /// 摄像机面对的后方向量
        /// </summary>
        Vector3 thisVector3Back { get; }

        /// <summary>
        /// 获取摄像机需要跟踪的对象
        /// </summary>
        Transform CamFollowTarget { get; set; }
        /// <summary>
        /// 获取是否正在跟随球
        /// </summary>
        bool IsFollowCam { get; set; }
        /// <summary>
        /// 获取是否正在看着球
        /// </summary>
        bool IsLookingBall { get; set; }

        /// <summary>
        /// 将摄像机开启
        /// </summary>
        void CamStart();
        /// <summary>
        /// 将摄像机关闭
        /// </summary>
        void CamClose();
        /// <summary>
        /// 让摄像机不看着球
        /// </summary>
        void CamSetNoLookAtBall();
        /// <summary>
        /// 让摄像机看着球
        /// </summary>
        void CamSetLookAtBall();
        /// <summary>
        /// 让摄像机只看着球
        /// </summary>
        void CamSetJustLookAtBall();
        /// <summary>
        /// 摄像机向左旋转
        /// </summary>
        void CamRoteLeft();
        /// <summary>
        /// 摄像机向右旋转
        /// </summary>
        void CamRoteRight();
        /// <summary>
        /// 摄像机 按住 空格键 上升
        /// </summary>
        void CamRoteSpace();
        /// <summary>
        /// 摄像机 放开 空格键 下降
        /// </summary>
        void CamRoteSpaceBack();


    }
}
