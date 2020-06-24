using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using System.Xml;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UIButton : UIElement
    {
        private const string TAG = "UIButton";

        public UIButton()
        {
            baseName = TAG;       
        }

        public override void SetEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Add(handler);
        }
        public override void RemoveEventHandler(string name, GameHandler handler)
        {
            if (name == "click")
                clickEventHandler.Remove(handler);
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
            if (name.ToLower() == "text")
                Text = val;
        }

        private GameHandlerList clickEventHandler = null;
        private Text text;

        /// <summary>
        /// 获取或设置按钮文字
        /// </summary>
        public string Text
        {
            get { return text.text; }
            set { text.text = value.Replace("<br>", "\n").Replace("<br/>", "\n"); }
        }

        private new void Start()
        {
            clickEventHandler = new GameHandlerList();
            text = transform.Find("Text").gameObject.GetComponent<Text>();

            EventTriggerListener eventTriggerListener = EventTriggerListener.Get(gameObject);
            eventTriggerListener.onClick = (g) =>
            {
                foreach(GameHandler h in clickEventHandler)
                    h.Call("click", Name);
            };
            eventTriggerListener.onEnter = (g) =>
            {

            };

            base.Start();
        }
        private new void OnDestroy()
        {
            if (clickEventHandler != null)
            {
                clickEventHandler.Dispose();
                clickEventHandler = null;
            }

            base.OnDestroy();
        }
    }
}
