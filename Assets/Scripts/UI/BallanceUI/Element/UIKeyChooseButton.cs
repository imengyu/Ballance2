using UnityEngine;
using UnityEngine.UI;
using Ballance2.CoreBridge;
using Ballance2.UI.Utils;
using System.Xml;
using Ballance2.Utils;
using Ballance2.Managers;
using UnityEngine.EventSystems;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIKeyChooseButton : UIElement
    {
        private const string TAG = "UIKeyChooseButton";

        public UIKeyChooseButton()
        {
            baseName = TAG;       
        }

        public override void SetEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Add(handler);
            if (name == "keyChanged")
                keyChangedEventHandler.Add(handler);
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Remove(handler);
            if (name == "keyChanged")
                keyChangedEventHandler.Remove(handler);
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
                case "key":
                    if (System.Enum.TryParse(val, out key))
                        Key = key;
                    break;
                default:
                    base.SetProp(name, val);
                    break;
            }
        }

        private SoundManager soundManager = null;

        protected override void OnInitElement()
        {
            soundManager = (SoundManager)GameManager.GetManager(SoundManager.TAG);

            LeftText = transform.Find("LeftText").gameObject.GetComponent<Text>();
            RightText = transform.Find("RightText").gameObject.GetComponent<Text>();
            Button = GetComponent<Button>();

            clickEventHandler = new GameHandlerList();
            keyChangedEventHandler = new GameHandlerList();

            EventTriggerListener eventTriggerListener = EventTriggerListener.Get(gameObject);
            eventTriggerListener.onClick = (g) =>
            {
                foreach (GameHandler h in clickEventHandler)
                    h.CallEventHandler("click", this, Name);
                soundManager.PlayFastVoice("core.assets.sounds:Menu_click.wav", GameSoundType.UI);
            };
            base.OnInitElement();
        }
        protected override void OnDestroyElement()
        {
            if (clickEventHandler != null)
            {
                clickEventHandler.Dispose();
                clickEventHandler = null;
            }
            if (keyChangedEventHandler != null)
            {
                keyChangedEventHandler.Dispose();
                keyChangedEventHandler = null;
            }
            base.OnDestroyElement();
        }
        protected override void OnUpdateElement()
        {
            base.OnUpdateElement();

            if(Input.anyKeyDown && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                Key = Event.current.keyCode;
                soundManager.PlayFastVoice("core.assets.sounds:Menu_dong.wav", GameSoundType.UI);
            }
        }

        private void OnClick(GameObject g)
        {
            foreach (GameHandler h in clickEventHandler)
                h.CallEventHandler("click", this, Name);

            soundManager.PlayFastVoice("core.assets.sounds:Menu_click.wav", GameSoundType.UI);
        }

        private GameHandlerList clickEventHandler = null;
        private GameHandlerList keyChangedEventHandler = null;

        [SerializeField, SetProperty("Text")]
        private string text;
        private Text LeftText;
        private Text RightText;
        private Button Button;

        [SerializeField, SetProperty("Key")]
        private KeyCode key = KeyCode.None;

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                if(LeftText != null)
                    LeftText.text = StringUtils.ReplaceBrToLine(value);
            }
        }
        /// <summary>
        /// 获取或设置按钮选中的键
        /// </summary>
        public KeyCode Key
        {
            get { return key; }
            set
            {
                key = value;
                if (LeftText != null)
                    RightText.text = key.ToString();
                if (keyChangedEventHandler != null)
                    keyChangedEventHandler.CallEventHandler("keyChanged", this, value.ToString());
            }
        }
    }
}
