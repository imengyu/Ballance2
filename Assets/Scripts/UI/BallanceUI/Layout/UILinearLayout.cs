using Ballance2.UI.Utils;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Layout
{
    /// <summary>
    /// 线性布局
    /// </summary>
    [SLua.CustomLuaClass]
    public class UILinearLayout : UILayout
    {
        public UILinearLayout()
        {
            baseName = "UILinearLayout";
        }

        [SerializeField, SetProperty("LayoutDirection")]
        protected LayoutAxis layoutDirection = LayoutAxis.Vertical;
        [SerializeField, SetProperty("LayoutChildSpacing")]
        private float layoutChildSpacing = 0;
        [SerializeField, SetProperty("LayoutReverse")]
        private bool layoutReverse = false;

        /// <summary>
        /// 布局方向
        /// </summary>
        public LayoutAxis LayoutDirection {
            get { return layoutDirection; }
            set { layoutDirection = value; PostDoLayout(); }
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
        /// 是否反向布局
        /// </summary>
        public bool LayoutReverse
        {
            get { return layoutReverse; }
            set { layoutReverse = value; PostDoLayout(); }
        }

        protected override void OnGravityChanged()
        {
            foreach (UIElement e in Elements)
                ReInitElement(e);
        }
        protected override void OnLayout()
        {
            UIElement e = null;
            RectTransform rectTransform = null;
            float startVal = 0;
            UIAnchor[] thisAnchor = UIAnchorPosUtils.GetUIAnchor(RectTransform);

            //计算所有元素布局占用高度
            float allLayoutHeight = (Elements.Count - 1) * layoutChildSpacing;
            for (int i = 0; i < Elements.Count; i++)
            {
                e = Elements[i];
                if (e.Visibility != UIVisibility.Gone)
                {
                    rectTransform = e.RectTransform;
                    allLayoutHeight += (layoutDirection == LayoutAxis.Vertical ? rectTransform.rect.height : rectTransform.rect.width);
                    //计算控件边距
                    allLayoutHeight += (layoutDirection == LayoutAxis.Vertical ?
                        (e.Layout_marginTop + e.Layout_marginBottom) : (e.Layout_marginLeft + e.Layout_marginRight));

                    if (layoutDirection == LayoutAxis.Vertical)
                    {
                        if (e.AnchorX == UIAnchor.Stretch)
                        {
                            UIAnchorPosUtils.SetUIAnchor(e.RectTransform, UIAnchor.Stretch,
                                 layoutReverse ? UIAnchor.Bottom : UIAnchor.Top);
                            UIAnchorPosUtils.SetUILeftBottom(e.RectTransform, e.Layout_marginLeft,
                                UIAnchorPosUtils.GetUIBottom(e.RectTransform));
                            UIAnchorPosUtils.SetUIRightTop(e.RectTransform, e.Layout_marginRight,
                                UIAnchorPosUtils.GetUITop(e.RectTransform));
                        }
                        else
                        {
                            UIAnchorPosUtils.SetUIAnchor(e.RectTransform,
                                UILayoutUtils.GravityToAnchor(Gravity, RectTransform.Axis.Horizontal),
                                layoutReverse ? UIAnchor.Bottom : UIAnchor.Top);
                            e.RectTransform.anchoredPosition = Vector2.zero;
                        }
                    }
                    else
                    {
                        if (e.AnchorY == UIAnchor.Stretch)
                        {
                            UIAnchorPosUtils.SetUIAnchor(e.RectTransform,
                                 layoutReverse ? UIAnchor.Right : UIAnchor.Left, 
                                UIAnchor.Stretch);
                            UIAnchorPosUtils.SetUILeftBottom(e.RectTransform,
                                UIAnchorPosUtils.GetUILeft(e.RectTransform), e.Layout_marginBottom);
                            UIAnchorPosUtils.SetUIRightTop(e.RectTransform,
                                UIAnchorPosUtils.GetUIRight(e.RectTransform), e.Layout_marginTop);
                        }
                        else
                        {
                            UIAnchorPosUtils.SetUIAnchor(e.RectTransform,
                                    layoutReverse ? UIAnchor.Right : UIAnchor.Left,
                                    UILayoutUtils.GravityToAnchor(Gravity, RectTransform.Axis.Vertical));
                            e.RectTransform.anchoredPosition = Vector2.zero;
                        }
                    }

                    e.DoResize();
                }
            }

            //自动将本容器撑大
            if (layoutDirection == LayoutAxis.Vertical)
            {
                if (allLayoutHeight > MinSize.y)
                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, allLayoutHeight);
                else if (thisAnchor[1] != UIAnchor.Stretch)
                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, MinSize.y);
            }
            else if (layoutDirection == LayoutAxis.Horizontal)
            {
                if (allLayoutHeight > MinSize.x)
                    RectTransform.sizeDelta = new Vector2(allLayoutHeight, RectTransform.sizeDelta.y);
                else if (thisAnchor[0] != UIAnchor.Stretch)
                    RectTransform.sizeDelta = new Vector2(MinSize.x, RectTransform.sizeDelta.y);
            }

            DoResize();

            //如果内容空间小于容器大小，那么居中内容
            if (allLayoutHeight < (layoutDirection == LayoutAxis.Vertical ? RectTransform.rect.height : RectTransform.rect.width))
            {
                if (layoutDirection == LayoutAxis.Vertical)
                {
                    if ((Gravity & LayoutGravity.CenterVertical) == LayoutGravity.CenterVertical)
                        startVal -= RectTransform.rect.height / 2 - allLayoutHeight / 2;
                    if ((Gravity & LayoutGravity.Bottom) == LayoutGravity.Bottom)
                        startVal -= RectTransform.rect.height - allLayoutHeight;
                }
                else
                {
                    if ((Gravity & LayoutGravity.CenterHorizontal) == LayoutGravity.CenterVertical)
                        startVal -= RectTransform.rect.width / 2 - allLayoutHeight / 2;
                    if ((Gravity & LayoutGravity.End) == LayoutGravity.Bottom)
                        startVal -= RectTransform.rect.width - allLayoutHeight;
                }
            }

            //布局
            if (!layoutReverse)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    e = Elements[i];
                    if (e.Visibility != UIVisibility.Gone) //跳过隐藏元素
                    {
                        rectTransform = e.RectTransform;
                        if (layoutDirection == LayoutAxis.Vertical)
                        {
                            startVal -= e.Layout_marginTop;
                            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 
                                UIAnchorPosUtils.GetUIPivotLocationOffest(rectTransform, startVal, RectTransform.Axis.Vertical));
                            startVal -= e.Layout_marginBottom;
                            
                        }
                        else
                        {
                            startVal -= e.Layout_marginLeft;
                            rectTransform.anchoredPosition = new Vector2(
                                UIAnchorPosUtils.GetUIPivotLocationOffest(rectTransform, startVal, RectTransform.Axis.Horizontal), 
                                rectTransform.anchoredPosition.y);
                            startVal -= e.Layout_marginRight;
                        }

                        startVal = startVal -
                            (layoutDirection == LayoutAxis.Vertical ? (rectTransform.rect.height) : (rectTransform.rect.width))
                            - layoutChildSpacing;
                    }
                }
            }
            else
            {
                //反向布局
                for (int i = Elements.Count; i >= 0; i--)
                {
                    e = Elements[i];
                    if (e.Visibility != UIVisibility.Gone)//跳过隐藏元素
                    {
                        rectTransform = e.RectTransform;

                        if (layoutDirection == LayoutAxis.Vertical)
                        {
                            startVal -= e.Layout_marginTop;
                            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 
                                UIAnchorPosUtils.GetUIPivotLocationOffest(rectTransform, startVal, RectTransform.Axis.Vertical));
                            startVal -= e.Layout_marginBottom;
                        }
                        else
                        {
                            startVal -= e.Layout_marginLeft;
                            rectTransform.anchoredPosition = new Vector2(
                                UIAnchorPosUtils.GetUIPivotLocationOffest(rectTransform, startVal, RectTransform.Axis.Horizontal), rectTransform.anchoredPosition.y);
                            startVal -= e.Layout_marginRight;
                        }
                        startVal = startVal - (layoutDirection == LayoutAxis.Vertical ? rectTransform.rect.height : rectTransform.rect.width) - layoutChildSpacing;
                    }
                }
            }

            if (Parent != null)
            {
                if (layoutDirection == LayoutAxis.Vertical
                    && Parent.RectTransform.rect.width > MinSize.x
                    && thisAnchor[0] != UIAnchor.Stretch)
                    RectTransform.sizeDelta = new Vector2(Parent.RectTransform.rect.width, RectTransform.sizeDelta.y);
                if (layoutDirection == LayoutAxis.Horizontal
                    && Parent.RectTransform.rect.height > MinSize.y
                    && thisAnchor[1] != UIAnchor.Stretch)
                    RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, Parent.RectTransform.rect.height);

                DoResize();
                Parent.DoLayout();
            }


            base.OnLayout();
        }


        protected override void SetProp(string name, string val)
        {
            switch (name)
            {
                case "layoutDirection":
                    System.Enum.TryParse(val, out layoutDirection);
                    PostDoLayout();
                    break;
                case "layoutReverse":
                    bool.TryParse(val, out layoutReverse);
                    PostDoLayout();
                    break;
                case "layoutChildSpacing":
                    float.TryParse(val, out layoutChildSpacing);
                    PostDoLayout();
                    break;
                default:
                    base.SetProp(name, val);
                    break;
            }
        }
    }

}
