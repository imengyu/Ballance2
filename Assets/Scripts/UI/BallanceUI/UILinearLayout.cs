using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
using System.Collections.Generic;
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

        /// <summary>
        /// 布局类型
        /// </summary>
        public LayoutType LayoutType { get { return layoutType; } }
        /// <summary>
        /// 布局方向
        /// </summary>
        public LayoutType LayoutDirection {
            get { return layoutDirection; }
            set { layoutDirection = value; DoLayout(0); }
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

            if (layoutDirection == LayoutType.Start)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    e = Elements[i];
                    rect = e.RectTransform;

                    if (layoutType == LayoutType.Vertical)
                        UIAnchorPosUtils.SetUIRightTop(rect, UIAnchorPosUtils.GetUIRight(rect), startVal);
                    else if (layoutType == LayoutType.Horizontal)
                        UIAnchorPosUtils.SetUILeftBottom(rect, startVal, UIAnchorPosUtils.GetUIBottom(rect));

                    startVal += (layoutType == LayoutType.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x);
                }
            }
            if (layoutDirection == LayoutType.End)
                for (int i = Elements.Count; i >= 0; i--)
                {
                    e = Elements[i];
                    rect = e.RectTransform;

                    if (layoutType == LayoutType.Vertical)
                        UIAnchorPosUtils.SetUILeftBottom(rect, UIAnchorPosUtils.GetUILeft(rect), startVal);
                    else if (layoutType == LayoutType.Horizontal)
                        UIAnchorPosUtils.SetUIRightTop(rect, startVal, UIAnchorPosUtils.GetUITop(rect));

                    startVal += (layoutType == LayoutType.Vertical ? rect.sizeDelta.y : rect.sizeDelta.x);
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
    }
}
