using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
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

        public string PagePath { get { return pagePath; } set { pagePath = value; }  }
        public GameObject PageBackground { get { return pageBackground; } set { pageBackground = value; } }
        public RectTransform RectTransform { get; private set; }
        public RectTransform ContentRectTransform { get { return contentRectTransform; } set { contentRectTransform = value; } }

    }

}
