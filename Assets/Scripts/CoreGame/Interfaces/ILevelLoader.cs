using Ballance2.GameCore;

namespace Ballance2.CoreGame.Interfaces
{
    /// <summary>
    /// 关卡加载器接口
    /// </summary>
    [SLua.CustomLuaClass]
    public interface ILevelLoader
    {
        /// <summary>
        /// 当前加载状态
        /// </summary>
        LevelLoadStatus LevelLoadStatus { get; }
    }
}
