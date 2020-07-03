using Ballance2.CoreGame.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.CoreGame.GamePlay
{
    /// <summary>
    /// 球碎片自定义处理类
    /// </summary>
    [SLua.CustomLuaClass]
    public class GameBallPiecesControl : MonoBehaviour
    {
        /// <summary>
        /// 获取球
        /// </summary>
        public GameBall Ball { get; set; }

        /// <summary>
        /// 当恢复碎片时会调用此方法
        /// </summary>
        /// <returns>如果返回true，可以屏蔽球管理器的默认恢复碎片方法</returns>
        [SLua.DoNotToLua]
        public virtual bool RecoverPieces()
        {
            return false;
        }
        /// <summary>
        /// 当抛出碎片时会调用此方法
        /// </summary>
        /// <returns>如果返回true，可以屏蔽球管理器的默认抛出碎片方法</returns>
        [SLua.DoNotToLua]
        public virtual bool ThrowPieces()
        {
            return false;
        }
    }
}
