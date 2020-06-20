using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Element
{
    /// <summary>
    /// UI 元素 基类
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIElement : MonoBehaviour, IElement
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">名字</param>
        /// <param name="xml">元素XML</param>
        public virtual void Init(string name, string xml)
        {
            Name = name;
            this.name = baseName + ":" + name;
            ReadAndSolveXml(xml);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">名字</param>
        /// <param name="xml">元素XML</param>
        public virtual void Init(string name, XmlNode xml)
        {
            Name = name;
            this.name = baseName + ":" + name;
            ReadAndSolveXml(xml);
        }

        /// <summary>
        /// 处理元素XML样式
        /// </summary>
        /// <param name="xml">元素XML</param>
        public void ReadAndSolveXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            SolveXml(doc);
        }
        /// <summary>
        /// 处理元素XML样式
        /// </summary>
        /// <param name="xml">元素XML</param>
        public void ReadAndSolveXml(XmlNode xml)
        {
            SolveXml(xml);
        }

        /// <summary>
        /// 设置事件接收器
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="handler">接收器</param>
        public virtual void SetEventHandler(string name, GameHandler handler) { }
        /// <summary>
        /// 移除事件接收器
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="handler">接收器</param>
        public virtual void RemoveEventHandler(string name, GameHandler handler) { }

        private UIAnchor anchorX;
        private UIAnchor anchorY;

        /// <summary>
        /// X轴锚点
        /// </summary>
        public UIAnchor AnchorX
        {
            get { return anchorX; }
            set
            {
                anchorX = value;
                if (Parent != null) Parent.PostDoLayout();
            }
        }
        /// <summary>
        /// Y轴锚点
        /// </summary>
        public UIAnchor AnchorY
        {
            get { return anchorY; }
            set
            {
                anchorY = value;
                if (Parent != null) Parent.PostDoLayout();
            }
        }
        /// <summary>
        /// 创建时的名字
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 当前 RectTransform
        /// </summary>
        public RectTransform RectTransform { get; private set; }
        /// <summary>
        /// 父级
        /// </summary>
        public ILayoutContainer Parent { get; set; }

        protected string baseName = "UIElement";
        protected virtual void SolveXml(XmlNode xml)
        {
            if (xml.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in xml.Attributes)
                {
                    if (a.Name == "anchorX")
                        System.Enum.TryParse(a.Value, out anchorX);
                    if (a.Name == "anchorY")
                        System.Enum.TryParse(a.Value, out anchorY);
                }
            }
        }

        private void Start()
        {
            RectTransform = GetComponent<RectTransform>();
        }
    }
}
