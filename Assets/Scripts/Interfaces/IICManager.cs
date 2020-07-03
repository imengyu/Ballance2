using UnityEngine;

namespace Ballance2.Interfaces
{
    /// <summary>
    /// IC 管理器（类似Virtools的IC）。
    /// 此管理器仅备份 3d物体的 位置、旋转
    /// </summary>
    [SLua.CustomLuaClass]
    public interface IICManager
    {
        /// <summary>
        /// 保存物体的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <returns></returns>
        bool BackupIC(GameObject g);
        /// <summary>
        /// 移除物体保存的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <returns></returns>
        bool RemoveIC(GameObject g);
        /// <summary>
        /// 保存物体的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <param name="iCBackType">IC 备份方式</param>
        /// <returns></returns>
        bool BackupIC(GameObject g, ICBackType iCBackType);
        /// <summary>
        /// 移除物体保存的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <param name="iCBackType">IC 备份方式</param>
        /// <returns></returns>
        bool RemoveIC(GameObject g, ICBackType iCBackType);
        /// <summary>
        /// 恢复物体保存的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <returns></returns>
        bool ResetIC(GameObject g);
        /// <summary>
        /// 恢复物体保存的状态
        /// </summary>
        /// <param name="g">物体</param>
        /// <param name="iCBackType">IC 备份方式</param>
        /// <returns></returns>
        bool ResetIC(GameObject g, ICBackType iCBackType);
    }

    /// <summary>
    /// IC 备份方式
    /// </summary>
    public enum ICBackType
    {
        /// <summary>
        /// 备份本体
        /// </summary>
        BackupThisObject,
        /// <summary>
        /// 备份本体和所有子
        /// </summary>
        BackupThisAndChild,
    }
}
