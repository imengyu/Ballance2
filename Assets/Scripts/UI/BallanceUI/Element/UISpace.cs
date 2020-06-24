using UnityEngine;
using Ballance2.UI.BallanceUI.Layout;

namespace Ballance2.UI.BallanceUI.Element
{
    [SLua.CustomLuaClass]
    public class UISpace : UIElement
    {
        private const string TAG = "UISpace";

        public UISpace()
        {
            baseName = TAG;
        }

        protected override void SetProp(string name, string value)
        {
            base.SetProp(name, value);
            if (name.ToLower() == "height")
            {
                float val = 0;
                if (float.TryParse(value, out val))
                    Height = val;
            }
        }

        [SerializeField, SetProperty("Height")]
        private float height = 50;

        /// <summary>
        /// 获取或设置空白的高度
        /// </summary>
        public float Height
        {
            get { return height; }
            set {
                height = value;
                if (Parent != null && Parent is UILinearLayout)
                {
                    switch ((Parent as UILinearLayout).LayoutDirection)
                    {
                        case LayoutAxis.Vertical:
                            RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, height);
                            break;
                        case LayoutAxis.Horizontal:
                            RectTransform.sizeDelta = new Vector2(height, RectTransform.sizeDelta.y);
                            break;
                    }
                }
                else
                {
                    RectTransform.sizeDelta = new Vector2(height, height);
                }
            }
        }
    }
}
