using UnityEngine;
using UnityEngine.UI;
using Ballance2.CoreBridge;
using System.Xml;
using Ballance2.Utils;
using Ballance2.Managers;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UISlider : UIElement
    {
        private const string TAG = "UISilder";

        public UISlider()
        {
            baseName = TAG;       
        }

        public override void SetEventHandler(string name, GameHandler handler)
        {
            if (name == "valueChanged")
                valueChangedEventHandler.Add(handler);
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            if (name == "valueChanged")
                valueChangedEventHandler.Remove(handler);
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if (!string.IsNullOrEmpty(xml.InnerXml))
                Text = xml.InnerText;
        }
        protected override void SetProp(string name, string val)
        {
            switch(name)
            {
                case "text":Text = val; break;
                case "minValue":
                    {
                        float v = 0;
                        if (float.TryParse(val, out v))
                            Slider.minValue = v;
                        break;
                    }
                case "maxValue":
                    {
                        float v = 0;
                        if (float.TryParse(val, out v))
                            Slider.maxValue = v;
                        break;
                    }
                case "value":
                    {
                        float v = 0;
                        if (float.TryParse(val, out v))
                            Slider.value = v;
                        break;
                    }
                case "valFormatText":
                    valFormatText = val;
                    break;
                case "direction":
                    {
                        Slider.Direction v = 0;
                        if (System.Enum.TryParse(val, out v))
                            Slider.direction = v;
                        break;
                    }
                case "wholeNumbers":
                    {
                        bool v = false;
                        if (bool.TryParse(val, out v))
                            Slider.wholeNumbers = v;
                        break;
                    }
                default: base.SetProp(name, val); break;
            }
        }

        private SoundManager soundManager = null;

        protected override void OnInitElement()
        {
            soundManager = (SoundManager)GameManager.GetManager(SoundManager.TAG);

            text = transform.Find("Text").gameObject.GetComponent<Text>();
            ValueText = transform.Find("ValueText").gameObject.GetComponent<Text>();
            Slider = transform.Find("Slider").gameObject.GetComponent<Slider>();
            Slider.onValueChanged.AddListener((float v) =>
            {
                ValueText.text = v.ToString(valFormatText);
                valueChangedEventHandler.CallEventHandler("valueChanged", this, v, ValueText);
            });

            valueChangedEventHandler = new GameHandlerList();
            base.OnInitElement();
        }
        protected override void OnDestroyElement()
        {
            if (valueChangedEventHandler != null)
            {
                valueChangedEventHandler.Dispose();
                valueChangedEventHandler = null;
            }

            base.OnDestroyElement();
        }

        private GameHandlerList valueChangedEventHandler = null;

        [SerializeField, SetProperty("ValFormatText")]
        private string valFormatText = "0";
        [SerializeField, SetProperty("Text")]
        private string textVal;
        private Text text;
        private Text ValueText;

        /// <summary>
        /// 获取 Slider
        /// </summary>
        public Slider Slider;

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return textVal; }
            set
            {
                textVal = value;
                if (text != null)
                    text.text = StringUtils.ReplaceBrToLine(value);
            }
        }
        /// <summary>
        /// 数值格式化字符串
        /// </summary>
        public string ValFormatText
        {
            get { return valFormatText; }
            set
            {
                valFormatText = value;
                if (ValueText != null && Slider != null)
                    ValueText.text = Slider.value.ToString(valFormatText);
            }
        }
    }
}
