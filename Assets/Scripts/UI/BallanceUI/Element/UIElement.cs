using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Element
{
    /// <summary>
    /// UI 元素 基类
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIElement : MonoBehaviour
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

        private XmlNode lateInitXml = null;
        private Dictionary<string, GameHandler> lateInitHandlers = null;

        public void LateInitHandlers(Dictionary<string, GameHandler> gameHandlers)
        {
            lateInitHandlers = gameHandlers;
        }
        public void LateInit(string name, string xml)
        {
            Name = name;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            lateInitXml = doc.DocumentElement;
        }
        public void LateInit(string name, XmlNode xml)
        {
            Name = name;
            lateInitXml = xml;
        }

        /// <summary>
        /// 处理元素XML样式
        /// </summary>
        /// <param name="xml">元素XML</param>
        public void ReadAndSolveXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            SolveXml(doc.DocumentElement);
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
        private Vector2 minSize = new Vector2(0, 0);

        /// <summary>
        /// 最小大小
        /// </summary>
        public Vector2 MinSize { get { return minSize; } set  { minSize = value; DoResize(); } }
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
        public RectTransform RectTransform { get; set; }
        /// <summary>
        /// 父级
        /// </summary>
        public UILayout Parent { get; set; }

        protected string baseName = "UIElement";
        protected virtual void SolveXml(XmlNode xml)
        {
            if (xml.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in xml.Attributes)
                {
                    switch (a.Name.ToLower())
                    {
                        case "name": Name = a.Value; break;
                        case "size":
                            {
                                string[] av = a.Value.Split(',');
                                float x, y;
                                if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                                    RectTransform.sizeDelta = new Vector2(x, y);
                                break;
                            }
                        case "minsize":
                            {
                                string[] av = a.Value.Split(',');
                                float x, y;
                                if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                                    minSize = new Vector2(x, y);
                                break;
                            }
                        case "width":
                            {
                                float x;
                                if (float.TryParse(a.Value, out x))
                                    RectTransform.sizeDelta = new Vector2(x, RectTransform.sizeDelta.y);
                                break;
                            }
                        case "height":
                            {
                                float y;
                                if (float.TryParse(a.Value, out y))
                                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, y);
                                break;
                            }
                        case "x":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    RectTransform.anchoredPosition = new Vector2(v, RectTransform.anchoredPosition.y);
                                break;
                            }
                        case "y":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, v);
                                break;
                            }
                        case "position":
                            {
                                string[] av = a.Value.Split(',');
                                float x, y;
                                if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                                    RectTransform.anchoredPosition = new Vector2(x, y);
                                break;
                            }
                        case "left":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    UIAnchorPosUtils.SetUILeftBottom(RectTransform, v, UIAnchorPosUtils.GetUIBottom(RectTransform));
                                break;
                            }
                        case "bottom":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    UIAnchorPosUtils.SetUILeftBottom(RectTransform, UIAnchorPosUtils.GetUILeft(RectTransform), v);
                                break;
                            }
                        case "leftbottom":
                            {
                                string[] av = a.Value.Split(',');
                                float x, y;
                                if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                                    UIAnchorPosUtils.SetUILeftBottom(RectTransform, x, y);
                                break;
                            }
                        case "right":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    UIAnchorPosUtils.SetUIRightTop(RectTransform, v, UIAnchorPosUtils.GetUITop(RectTransform));
                                break;
                            }
                        case "top":
                            {
                                float v;
                                if (float.TryParse(a.Value, out v))
                                    UIAnchorPosUtils.SetUIRightTop(RectTransform, UIAnchorPosUtils.GetUIRight(RectTransform), v);
                                break;
                            }
                        case "righttop":
                            {
                                string[] av = a.Value.Split(',');
                                float x, y;
                                if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                                    UIAnchorPosUtils.SetUIRightTop(RectTransform, x, y);
                                break;
                            }
                        case "anchor":
                            {
                                string[] av = a.Value.Split(',');
                                if (av.Length >= 2)
                                {
                                    System.Enum.TryParse(av[0], out anchorX);
                                    System.Enum.TryParse(av[1], out anchorY);
                                }
                                break;
                            }
                        case "anchorX":
                            {
                                System.Enum.TryParse(a.Value, out anchorX);
                                break;
                            }
                        case "anchorY":
                            {
                                System.Enum.TryParse(a.Value, out anchorY);
                                break;
                            }
                    }
                }
            }
        }

        private void Start()
        {
            RectTransform = GetComponent<RectTransform>();
            name = Name;
            if (lateInitXml != null || lateInitHandlers != null)
                Invoke("DoLateInit", 0.3f);
        }

        private void DoResize()
        {
            bool changed = false;
            Vector2 newSize = RectTransform.sizeDelta;
            if (RectTransform.sizeDelta.x > minSize.x)
            {
                newSize.x = minSize.x;
                changed = true;
            }
            if (RectTransform.sizeDelta.y > minSize.y)
            {
                newSize.y = minSize.y;
                changed = true;
            }
            if (changed)
                RectTransform.sizeDelta = newSize;
        }
        private void DoLateInit()
        {
            if (lateInitXml != null)
            {
                Init(Name, lateInitXml);
                lateInitXml = null;
            }
            if(lateInitHandlers != null)
            {
                foreach(string key in lateInitHandlers.Keys)
                    SetEventHandler(key, lateInitHandlers[key]);
                
                lateInitHandlers = null;
            }
            DoResize();
        }
    }
}
