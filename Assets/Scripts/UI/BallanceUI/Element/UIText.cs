using Ballance2.Utils;
using System;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    [AddComponentMenu("Ballance/UI/UIText")]
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

            if (!string.IsNullOrEmpty(xml.InnerXml))
                Text = xml.InnerXml;
        }
        protected override void SetProp(string name, string val)
        {
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
                case "width":
                    if (val.ToLower() == "warp_content")
                        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    else
                    {
                        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        base.SetProp(name, val);
                    }
                    DoPostLayout();
                    break;
                case "height":
                    if (val.ToLower() == "warp_content")
                        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    else
                    {
                        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                        base.SetProp(name, val);
                    }
                    DoPostLayout();
                    break;
                default:
                    base.SetProp(name, val);
                    break;

            }
        }

        [SerializeField, SetProperty("Text")]
        private string textVal;
        private Text text;
        private ContentSizeFitter contentSizeFitter;

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return textVal; }
            set
            {
                textVal = value;
                text.text = StringUtils.ReplaceBrToLine(value);
            }
        }

        protected override void OnInitElement()
        {
            text = GetComponent<Text>();
            contentSizeFitter = GetComponent<ContentSizeFitter>();
            base.OnInitElement();
        }
    }
}
