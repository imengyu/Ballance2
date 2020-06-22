using UnityEngine;
using System.Xml;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UISpace : UIElement
    {
        private const string TAG = "Space";
        internal LayoutType layoutType = LayoutType.None;

        public UISpace()
        {
            baseName = TAG;
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if(xml.Name == TAG && xml.Attributes.Count > 0)
            {
                foreach(XmlAttribute a in xml.Attributes)
                {
                    if(a.Name.ToLower() == "height")
                    {
                        float val = 0;
                        if (float.TryParse(a.InnerText, out val))
                            Height = val;
                    }
                }
            }
        }

        private float height = 50;

        /// <summary>
        /// 获取或设置空白的高度
        /// </summary>
        public float Height
        {
            get { return height; }
            set {
                height = value;
                switch (layoutType)
                {
                    case LayoutType.None:
                        RectTransform.sizeDelta = new Vector2(height, height);
                        break;
                    case LayoutType.Vertical:
                        RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
                        break;
                    case LayoutType.Horizontal:
                        RectTransform.sizeDelta = new Vector2(height, RectTransform.sizeDelta.y);
                        break;
                }
            }
        }
    }
}
