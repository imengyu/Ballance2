using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using Assets.Scripts.Managers.CoreBridge;
using System.Xml;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class Button : UIElement
    {
        private const string TAG = "Button";

        public Button()
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

            if (xml.Name == TAG && xml.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in xml.Attributes)
                {
                    if (a.Name == "text")
                        Text = a.InnerText;
                }
            }
        }

        private GameHandlerList clickEventHandler = null;
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
        }
        private void OnDestroy()
        {
            if (clickEventHandler != null)
            {
                clickEventHandler.Dispose();
                clickEventHandler = null;
            }
        }
    }
}
