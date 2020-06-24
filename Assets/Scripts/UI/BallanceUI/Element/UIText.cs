using Ballance2.Utils;
using System;
using System.Xml;
using UnityEngine.UI;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIText : UIElement
    {
        private const string TAG = "UIText";

        public UIText()
        {
            baseName = TAG;
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if (xml.Name == TAG && xml.Attributes.Count > 0)
            {
                if (!string.IsNullOrEmpty(xml.InnerXml))
                    Text = xml.InnerXml;
            }
        }
        protected override void SetProp(string name, string val)
        {
            base.SetProp(name, val);
            switch (name)
            {
                case "text": Text = val; break;
                case "fontSize":
                    {
                        int i;
                        if (int.TryParse(val, out i))
                            text.fontSize = i;
                        break;
                    }
                case "fontStyle": text.fontStyle = (UnityEngine.FontStyle)Enum.Parse(typeof(UnityEngine.FontStyle), val); break;
                case "color": text.color = StringUtils.StringToColor(val); break;
                case "alignment": text.alignment = (UnityEngine.TextAnchor)Enum.Parse(typeof(UnityEngine.TextAnchor), val); break;
                case "lineSpacing":
                    {
                        float f;
                        if (float.TryParse(val, out f))
                            text.lineSpacing = f;
                        break;
                    }
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
