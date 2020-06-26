using Ballance2.UI.BallanceUI.Element;
using SLua;
using UnityEngine;
using UnityEngine.Events;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// UI 页
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIPage : MonoBehaviour
    {
        private void Start()
        {
            RectTransform = GetComponent<RectTransform>();
        }
        public void Destroy()
        {
            Destroy(this);
        }

        [SerializeField, SetProperty("PagePath")]
        private string pagePath = null;
        [SerializeField, SetProperty("PageBackground")]
        private GameObject pageBackground = null;
        [SerializeField, SetProperty("ContentRectTransform")]
        private RectTransform contentRectTransform = null;
        [SerializeField, SetProperty("ContentContainer")]
        private UILayout layoutContainer = null;

        /// <summary>
        /// 页路径
        /// </summary>
        public string PagePath { get { return pagePath; } set { pagePath = value; }  }
        /// <summary>
        /// 页的背景
        /// </summary>
        public GameObject PageBackground { get { return pageBackground; } set { pageBackground = value; } }
        /// <summary>
        /// 页的 RectTransform
        /// </summary>
        public RectTransform RectTransform { get; set; }
        /// <summary>
        /// 内容的 RectTransform
        /// </summary>
        public RectTransform ContentRectTransform { get { return contentRectTransform; } set { contentRectTransform = value; } }
        /// <summary>
        /// 内容的容器，可手动指定。
        /// 如果不手动指定，那么必须是 BallanceUI 的页才有此属性。
        /// </summary>
        public UILayout ContentContainer { get { return layoutContainer; } set { layoutContainer = value; } }

        /// <summary>
        /// 通过名字查找元素
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns></returns>
        public UIElement FindElementByName(string name)
        {
            if (ContentContainer == null)
                return null;
            return ContentContainer.FindElementByName(name);
        }

        public PageSwitchEvent onShow;
        public PageSwitchEvent onHide;

        public void OnShow()
        {
            if (onShow != null)
                onShow.Invoke();
        }
        public void OnHide()
        {
            if (onHide != null)
                onHide.Invoke();
        }

        [CustomLuaClass]
        public class PageSwitchEvent : UnityEvent
        {
            public PageSwitchEvent()
            {

            }
        }
    }

}
