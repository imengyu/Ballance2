using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    if (a.Name == "text")
                        Text = a.InnerText;
                    if (a.Name == "fontSize")
                    {
                        int i;
                        if (int.TryParse(a.Value, out i))
                            text.fontSize = i;
                    }
                    if (a.Name == "fontSize")
                        text.fontStyle = (UnityEngine.FontStyle)Enum.Parse(typeof(UnityEngine.FontStyle), a.Value);
                    if (a.Name == "color")
                        text.color = StringUtils.StringToColor(a.Value);
                    if (a.Name == "alignment")
                        text.alignment = (UnityEngine.TextAnchor)Enum.Parse(typeof(UnityEngine.TextAnchor), a.Value);
                    if (a.Name == "lineSpacing")
                    {
                        float f;
                        if (float.TryParse(a.Value, out f))
                            text.lineSpacing = f;
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
