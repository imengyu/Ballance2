using Ballance2.CoreGame.GamePlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.CoreGame.Interfaces
{
    /// <summary>
    /// 球管理器接口
    /// </summary>
    [SLua.CustomLuaClass]
    public interface IBallManager
    {
        /// <summary>
        /// 所有球类型
        /// </summary>
        List<GameBall> BallTypes { get; }
        /// <summary>
        /// 当前球的名称
        /// </summary>
        string CurrentBallName { get; }
        /// <summary>
        /// 当前球
        /// </summary>
        GameBall CurrentBall { get;}
        /// <summary>
        /// 获取球是否调试
        /// </summary>
        bool IsBallDebug { get; }
        /// <summary>
        /// 获取是否控制反转
        /// </summary>
        bool IsReverseControl { get; }

        /// <summary>
        /// 注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        /// <param name="ball">附加了GameBall组件的球实例</param>
        /// <param name="pieces">球碎片组</param>
        bool RegisterBall(string name, GameBall ball, GameObject pieces);
        /// <summary>
        /// 取消注册球
        /// </summary>
        /// <param name="name">球类型名称</param>
        void UnRegisterBall(string name);
        /// <summary>
        /// 获取已注册的球
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GameBall GetRegisteredBall(string name);

        /// <summary>
        /// 播放球 闪电动画
        /// </summary>
        /// <param name="smallToBig">是否由小变大。</param>
        /// <param name="lightEnd">是否在之后闪一下。</param>
        void PlayLighting(bool smallToBig = false, bool lightEnd = false);

        /// <summary>
        /// 开始控制球
        /// </summary>
        void StartControll();
        /// <summary>
        /// 停止控制球
        /// </summary>
        void EndControll();
        /// <summary>
        /// 获取设置是否可以控制球
        /// </summary>
        bool IsControlling { get; set; }
        /// <summary>
        /// 获取当前球推动方向
        /// </summary>
        BallPushType PushType { get; }

        /// <summary>
        /// 获取当前球是否正在平滑移动
        /// </summary>
        /// <returns></returns>
        bool IsSmoothMove();
        /// <summary>
        /// 指定球速度清零。
        /// </summary>
        /// <param name="ball">指定球</param>
        void RemoveBallSpeed(GameBall ball);
        /// <summary>
        /// 添加球推动方向
        /// </summary>
        /// <param name="t"></param>
        void AddBallPush(BallPushType t);
        /// <summary>
        /// 去除球推动方向
        /// </summary>
        /// <param name="t"></param>
        void RemoveBallPush(BallPushType t);
        /// <summary>
        /// 设置球下次激活的位置。
        /// </summary>
        /// <param name="pos">下次激活的位置</param>
        void RecoverSetPos(Vector3 pos);
        /// <summary>
        /// 重新设置默认球位置并激活
        /// </summary>
        void RecoverBallDef();
        /// <summary>
        /// 重新设置指定球位置并激活
        /// </summary>
        /// <param name="pos">球名字</param>
        void RecoverBall(Vector3 pos);
        /// <summary>
        /// 激活默认球
        /// </summary>
        void ActiveBallDef();
        /// <summary>
        /// 激活指定的球
        /// </summary>
        /// <param name="type">球名字</param>
        void ActiveBall(string type);
        /// <summary>
        /// 清除已激活的球
        /// </summary>
        void ClearBall();
        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="type">球类型</param>
        void ThrowPieces(string type);
        /// <summary>
        /// 抛出指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        void ThrowPieces(GameBall ball);
        /// <summary>
        /// 恢复指定球碎片
        /// </summary>
        /// <param name="ball">球</param>
        void RecoverPieces(GameBall ball);
        /// <summary>
        /// 平滑移动球到指定位置。
        /// </summary>
        /// <param name="pos">指定位置。</param>
        /// <param name="off">动画平滑时间</param>
        void SmoothMoveBallToPos(Vector3 pos, float off = 2f);
        /// <summary>
        /// 根据 GameObject 获取球的类型（通常在lua中调用）
        /// </summary>
        /// <param name="ball"></param>
        /// <returns></returns>
        string GetBallType(GameObject ball);
        /// <summary>
        /// 播放烟雾
        /// </summary>
        void PlaySmoke();
    }
}
