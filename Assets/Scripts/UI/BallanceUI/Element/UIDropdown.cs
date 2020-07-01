using UnityEngine;
using UnityEngine.UI;
using Ballance2.CoreBridge;
using System.Xml;
using Ballance2.Utils;
using Ballance2.Managers;
using Ballance2.Interfaces;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIDropdown : UIElement
    {
        private const string TAG = "UIDropdown";

        public UIDropdown()
        {
            baseName = TAG;       
        }

        public override void SetEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Add(handler);
            if (name == "valueChanged")
                valueChangedEventHandler.Add(handler);
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            if (name == "valueChanged")
                valueChangedEventHandler.Remove(handler);
            if (name == "click")
                clickEventHandler.Remove(handler);
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if(xml.ChildNodes.Count > 0)
            {
                foreach (XmlNode n in xml.ChildNodes)
                    Dropdown.options.Add(new Dropdown.OptionData(n.InnerText));
            }
            else if (!string.IsNullOrEmpty(xml.InnerXml))
                Text = xml.InnerText;
        }
        protected override void SetProp(string name, string val)
        {
            base.SetProp(name, val);
            if (name.ToLower() == "text")
                Text = val;
            if (name.ToLower() == "value")
            {
                int v;
                if (int.TryParse(val, out v))
                    Dropdown.value = v;
            }
        }

        private ISoundManager soundManager = null;

        protected override void OnInitElement()
        {
            soundManager = (ISoundManager)GameManager.GetManager("SoundManager");
            text = transform.Find("Text").gameObject.GetComponent<Text>();
            Dropdown = GetComponent<Dropdown>();
            Dropdown.onValueChanged.AddListener((int i) =>
            {
                valueChangedEventHandler.CallEventHandler("valueChanged", this, i);
                OnClick(gameObject);
            });

            clickEventHandler = new GameHandlerList();
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
            if (clickEventHandler != null)
            {
                clickEventHandler.Dispose();
                clickEventHandler = null;
            }
            base.OnDestroyElement();
        }

        public void AddOption(string s)
        {
            Dropdown.options.Add(new Dropdown.OptionData(s));
        }

        private GameHandlerList valueChangedEventHandler = null;
        private GameHandlerList clickEventHandler = null;

        private void OnClick(GameObject g)
        {
            foreach (GameHandler h in clickEventHandler)
                h.CallEventHandler("click", this, Name);

            soundManager.PlayFastVoice("core.assets.sounds:Menu_click.wav", GameSoundType.UI);
        }

        [SerializeField, SetProperty("Text")]
        private string textVal;
        private Text text;

        /// <summary>
        /// 获取 Dropdown
        /// </summary>
        public Dropdown Dropdown;

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

    }
}
