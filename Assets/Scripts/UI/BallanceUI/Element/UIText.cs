using Ballance2.Utils;
using System;
using System.Xml;
using UnityEngine.UI;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIText : UIElement
    {
        private const string TAG = "Text";

        public UIText()
        {
            baseName = TAG;
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if (xml.Name == TAG && xml.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in xml.Attributes)
                {
                    switch (a.Name.ToLower())
                    {
                        case "text": Text = a.InnerText;  break;
                        case "fontSize":
                            {
                                int i;
                                if (int.TryParse(a.Value, out i))
                                    text.fontSize = i;
                                break;
                            }
                        case "fontsize": text.fontStyle = (UnityEngine.FontStyle)Enum.Parse(typeof(UnityEngine.FontStyle), a.Value); break;
                        case "color": text.color = StringUtils.StringToColor(a.Value); break;
                        case "alignment": text.alignment = (UnityEngine.TextAnchor)Enum.Parse(typeof(UnityEngine.TextAnchor), a.Value); break;
                        case "lineSpacing":
                            {
                                float f;
                                if (float.TryParse(a.Value, out f))
                                    text.lineSpacing = f;
                                break;
                            }
                    }
                }
                if (!string.IsNullOrEmpty(xml.InnerXml))
                    Text = xml.InnerXml;
            }
        }

        private Text text;

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return text.text; }
            set { text.text = value; }
        }

        private void Start()
        {
            text = GetComponent<Text>();
        }
    }
}
