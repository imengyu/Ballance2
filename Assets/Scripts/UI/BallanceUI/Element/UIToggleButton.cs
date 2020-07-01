using UnityEngine;
using UnityEngine.UI;
using Ballance2.CoreBridge;
using Ballance2.UI.Utils;
using System.Xml;
using Ballance2.Utils;
using Ballance2.Managers;
using Ballance2.Interfaces;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIToggleButton : UIElement
    {
        private const string TAG = "UIBUIToggleButtonutton";

        public UIToggleButton()
        {
            baseName = TAG;       
        }

        public override void SetEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Add(handler);
            if (name == "checkChanged")
                checkChangedEventHandler.Add(handler);
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Remove(handler);
            if (name == "checkChanged")
                checkChangedEventHandler.Remove(handler);
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if (!string.IsNullOrEmpty(xml.InnerXml))
                Text = xml.InnerText;
        }
        protected override void SetProp(string name, string val)
        {
            base.SetProp(name, val);
            switch(name)
            {
                case "text":
                    Text = val;
                    break;
                case "yesText":
                    Text = val;
                    break;
                case "noText":
                    Text = val;
                    break;
                case "checked":
                    bool b = false;
                    if (bool.TryParse(val, out b))
                        Checked = b;
                    break;
                default:
                    base.SetProp(name, val);
                    break;
            }
        }

        public Sprite selectedSprite = null;
        public Sprite normalSprite = null;

        private ISoundManager soundManager = null;

        protected override void OnInitElement()
        {
            soundManager = (ISoundManager)GameManager.GetManager("SoundManager");

            ButtonText = transform.Find("Text").gameObject.GetComponent<Text>();
            UISmallButtonNoText = transform.Find("UISmallButtonNo/Text").gameObject.GetComponent<Text>();
            UISmallButtonYesText = transform.Find("UISmallButtonYes/Text").gameObject.GetComponent<Text>();
            UISmallButtonNo = transform.Find("UISmallButtonNo").gameObject.GetComponent<Button>();
            UISmallButtonYes = transform.Find("UISmallButtonYes").gameObject.GetComponent<Button>();
            UISmallButtonNoImage = transform.Find("UISmallButtonNo").gameObject.GetComponent<Image>();
            UISmallButtonYesImage = transform.Find("UISmallButtonYes").gameObject.GetComponent<Image>();

            UISmallButtonNoImage.sprite = _Checked ? normalSprite : selectedSprite;
            UISmallButtonYesImage.sprite = _Checked ? selectedSprite : normalSprite;

            clickEventHandler = new GameHandlerList();
            checkChangedEventHandler = new GameHandlerList();

            UISmallButtonNo.onClick.AddListener(() =>
            {
                Checked = false;
                OnClick(gameObject);
            });
            UISmallButtonYes.onClick.AddListener(() =>
            {
                Checked = true;
                OnClick(gameObject);
            });

            EventTriggerListener eventTriggerListener = EventTriggerListener.Get(gameObject);
            eventTriggerListener.onClick = OnClick;

            base.OnInitElement();
        }
        protected override void OnDestroyElement()
        {
            if (clickEventHandler != null)
            {
                clickEventHandler.Dispose();
                clickEventHandler = null;
            }
            if (checkChangedEventHandler != null)
            {
                checkChangedEventHandler.Dispose();
                checkChangedEventHandler = null;
            }
            

            base.OnDestroyElement();
        }

        private void OnClick(GameObject g)
        {
            foreach (GameHandler h in clickEventHandler)
                h.CallEventHandler("click", this, Name);

            soundManager.PlayFastVoice("core.assets.sounds:Menu_click.wav", GameSoundType.UI);
        }

        private GameHandlerList clickEventHandler = null;
        private GameHandlerList checkChangedEventHandler = null;

        private Text ButtonText;
        private Text UISmallButtonNoText;
        private Text UISmallButtonYesText;
        private Button UISmallButtonNo;
        private Button UISmallButtonYes;
        private Image UISmallButtonNoImage;
        private Image UISmallButtonYesImage;

        [SerializeField, SetProperty("Checked")]
        private bool _Checked = false;
        [SerializeField, SetProperty("Text")]
        private string _Text = "";
        [SerializeField, SetProperty("YesText")]
        private string _YesText = "";
        [SerializeField, SetProperty("NoText")]
        private string _NoText = "";

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                ButtonText.text = StringUtils.ReplaceBrToLine(value);
            }
        }
        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string YesText
        {
            get { return _YesText; }
            set
            {
                _YesText = value;
                UISmallButtonYesText.text = StringUtils.ReplaceBrToLine(value);
            }
        }
        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string NoText
        {
            get { return _NoText; }
            set
            {
                _NoText = value;
                UISmallButtonYesText.text = StringUtils.ReplaceBrToLine(value);
            }
        }
        /// <summary>
        /// 获取或设置是否选中
        /// </summary>
        public bool Checked
        {
            get { return _Checked; }
            set
            {
                if (_Checked != value)
                {
                    _Checked = value;
                    UISmallButtonNoImage.sprite = _Checked ? normalSprite : selectedSprite;
                    UISmallButtonYesImage.sprite = _Checked ? selectedSprite : normalSprite;
                    checkChangedEventHandler.CallEventHandler("checkChanged", this, value);
                }
            }
        }
    }
}
