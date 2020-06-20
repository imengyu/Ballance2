using Ballance2.Managers.CoreBridge;
using System.Xml;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// 元素接口类
    /// </summary>
    interface IElement
    {
        void Init(string name, string xml);
        void Init(string name, XmlNode xml);
        void SetEventHandler(string name, GameHandler handler);
        void RemoveEventHandler(string name, GameHandler handler);

        ILayoutContainer Parent { get; set; }
    }
}
