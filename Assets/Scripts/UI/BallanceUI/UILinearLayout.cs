using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// 线性布局
    /// </summary>
    [SLua.CustomLuaClass]
    public class UILinearLayout : UILayout
    {
        protected LayoutType layoutType = LayoutType.None;
        [SetProperty("LayoutDirection")]
        private LayoutType layoutDirection = LayoutType.Start;
        private bool layoutAutoCenter = true;
        private float layoutChildSpacing = 0;

        /// <summary>
        /// 布局类型
        /// </summary>
        public LayoutType LayoutType { get { return layoutType; } }
        /// <summary>
        /// 布局方向
        /// </summary>
        public LayoutType LayoutDirection {
            get { return layoutDirection; }
            set { layoutDirection = value; PostDoLayout(); }
        }
        /// <summary>
        /// 布局是否自动居中
        /// </summary>
        public bool LayoutAutoCenter
        {
            get { return layoutAutoCenter; }
            set { layoutAutoCenter = value; PostDoLayout(); }
        }
        /// <summary>
        /// 布局字元素间距
        /// </summary>
        public float LayoutChildSpacing
        {
            get { return layoutChildSpacing; }
            set { layoutChildSpacing = value; PostDoLayout(); }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public override UIElement AddElement(UIElement element, bool doLayout = true)
        {
            ReinitElement(element);
            return base.AddElement(element, doLayout);
        }
        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="index">插入的索引</param>
        /// <returns></returns>
        public override UIElement InsertElement(UIElement element, int index, bool doLayout = true)
        {
            ReinitElement(element);
            return base.InsertElement(element, index, doLayout);
        }

        private void ReinitElement(UIElement element)
        {
            RectTransform rectTransform = element.RectTransform;
            switch (layoutType)
            {
                case LayoutType.Vertical:
                    UIAnchorPosUtils.SetUIAnchor(rectTransform, element.AnchorX,
                        layoutDirection == LayoutType.Start ? UIAnchor.Top : UIAnchor.Bottom);
                    break;
                case LayoutType.Horizontal:
                    UIAnchorPosUtils.SetUIAnchor(rectTransform, element.AnchorY,
                        layoutDirection == LayoutType.Start ? UIAnchor.Left : UIAnchor.Right);
                    break;
            }
        }

        /// <summary>
        /// 强制重新布局
        /// </summary>
        /// <param name="startChildIndex">开始的子元素索引，为 0 时强制布局全部</param>
        public override void DoLayout(int startChildIndex)
        {
            UIElement e = null;
            RectTransform rect = null;
            float startVal = 0;

            float allLayoutHeight = (Elements.Count - 1) * layoutChildSpacing;
            for (int i = 0; i < Elements.Count; i++)
            {
                e = Elements[i];
                rect = e.RectTransform;
                allLayoutHeight += (layoutType == LayoutType.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x);
            }

            startVal = layoutAutoCenter ? (
                (layoutType == LayoutType.Vertical ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x) / 2 - allLayoutHeight / 2
                ) : 0;

            if (layoutDirection == LayoutType.Start)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    e = Elements[i];
                    rect = e.RectTransform;

                    if (layoutType == LayoutType.Vertical)
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startVal);
                    else if (layoutType == LayoutType.Horizontal)
                        rect.anchoredPosition = new Vector2(startVal, rect.anchoredPosition.y);

                    startVal += (layoutType == LayoutType.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x);
                }
            }
            else if(layoutDirection == LayoutType.End)
            {
                for (int i = Elements.Count; i >= 0; i--)
                {
                    e = Elements[i];
                    rect = e.RectTransform;

                    if (layoutType == LayoutType.Vertical)
                        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, startVal);
                    else if (layoutType == LayoutType.Horizontal)
                        rect.anchoredPosition = new Vector2(startVal, rect.anchoredPosition.y);

                    startVal += (layoutType == LayoutType.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x);
                }
            }

            if(!layoutAutoCenter && allLayoutHeight > (layoutType == LayoutType.Vertical ? MinSize.y : MinSize.x))
            {
                if (layoutType == LayoutType.Vertical && allLayoutHeight > MinSize.y)
                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, allLayoutHeight);
                if (layoutType == LayoutType.Horizontal && allLayoutHeight > MinSize.x)
                    RectTransform.sizeDelta = new Vector2(allLayoutHeight, RectTransform.sizeDelta.y);
            }

            if (Parent != null)
            {



                Parent.DoLayout(0);
            }
        }

        protected override void SolveXml(XmlNode xml)
        {
            base.SolveXml(xml);

            if (xml.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in xml.Attributes)
                {
                    switch(a.Name.ToLower())
                    {
                        case "layoutdirection":
                            System.Enum.TryParse(a.Value, out layoutDirection);
                            break;
                        case "layoutautocenter":
                            bool.TryParse(a.Value, out layoutAutoCenter);
                            break;
                        case "layoutchildspacing":
                            float.TryParse(a.Value, out layoutChildSpacing);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 布局方式
    /// </summary>
    public enum LayoutType
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        /// <summary>
        /// 垂直
        /// </summary>
        Vertical,
        /// <summary>
        /// 水平
        /// </summary>
        Horizontal,

        /// <summary>
        /// 正向布局
        /// </summary>
        Start,
        /// <summary>
        /// 反向布局
        /// </summary>
        End,
        /// <summary>
        /// 中心布局
        /// </summary>
        Center,
    }
}
