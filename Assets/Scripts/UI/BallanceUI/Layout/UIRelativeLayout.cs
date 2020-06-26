using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI.Layout
{
    /// <summary>
    /// 相对布局
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIRelativeLayout : UILayout
    {
        public UIRelativeLayout()
        {
            baseName = "UIRelativeLayout";
        }

        protected override void OnGravityChanged()
        {
            base.OnGravityChanged();
            PostDoLayout();
        }
        protected override void OnLayout()
        {
            //调整最小大小
            DoResize();
            magEles.Clear();
            //构建依赖
            BuildSortChilds();
            //布局
            DoLayoutInternal();

            if (Parent != null)
                Parent.DoLayout();
            base.OnLayout();
        }
        protected override void SetProp(string name, string val)
        {
            base.SetProp(name, val);
        }

        private List<ElementNode> sortedHorizontalChildren = new List<ElementNode>();
        private List<ElementNode> sortedVerticalChildren = new List<ElementNode>();
        private Dictionary<string, ElementNode> magEles = new Dictionary<string, ElementNode>();
        private class ElementNode
        {
            public UIElement element = null;
            public int dependenciesLevelHorizontal = 0;
            public int dependenciesLevelVertical = 0;

            public ElementNode() { }
            public ElementNode(UIElement e, int dv, int dh)
            {
                element = e;
                dependenciesLevelHorizontal = dh;
                dependenciesLevelVertical = dv;
            }
        }

        private int GetEleRefLevel(string name, bool isV)
        {
            ElementNode parent = null;
            if (magEles.TryGetValue(name, out parent))
                return isV ? parent.dependenciesLevelVertical : parent.dependenciesLevelHorizontal;
            else
            {
                UIElement parentEle = FindElementInLayoutByName(name);
                if (parentEle != null && parentEle.Visibility != UIVisibility.Gone)
                {
                    parent = BuildChildRefInfo(parentEle);
                    return isV ? parent.dependenciesLevelVertical : parent.dependenciesLevelHorizontal;
                }
            }
            return 0;
        }
        private ElementNode BuildChildRefInfo(UIElement e)
        {
            int levelV = 0, levelH = 0, v = 0;
            ElementNode thisNode = (ElementNode)e.GetCustomData("UIRelativeLayout_NodeData");
            if (thisNode == null)
            {
                thisNode = new ElementNode();
                thisNode.element = e;

                e.SetCustomData("UIRelativeLayout_NodeData", thisNode);
            }

            if (e.Layout_above != null)
            {
                v = GetEleRefLevel(e.Layout_above, true);
                if (levelV < v) levelV = v;
            }
            if (e.Layout_alignBottom != null)
            {
                v = GetEleRefLevel(e.Layout_alignBottom, true);
                if (levelV < v) levelV = v;
            }
            if (e.Layout_alignTop != null)
            {
                v = GetEleRefLevel(e.Layout_alignTop, true);
                if (levelV < v) levelV = v;
            }
            if (e.Layout_below != null)
            {
                v = GetEleRefLevel(e.Layout_below, true);
                if (levelV < v) levelV = v;
            }

            if (e.Layout_alignLeft != null)
            {
                v = GetEleRefLevel(e.Layout_alignLeft, false);
                if (levelH < v) levelH = v;
            }
            if (e.Layout_alignRight != null)
            {
                v = GetEleRefLevel(e.Layout_alignRight, false);
                if (levelH < v) levelH = v;
            }
            if (e.Layout_toLeftOf != null)
            {
                v = GetEleRefLevel(e.Layout_toLeftOf, false);
                if (levelH < v) levelH = v;
            }
            if (e.Layout_toRightOf != null)
            {
                v = GetEleRefLevel(e.Layout_toRightOf, false);
                if (levelH < v) levelH = v;
            }

            thisNode.dependenciesLevelHorizontal = levelH + 1;
            thisNode.dependenciesLevelVertical = levelV + 1;
            magEles.Add(e.Name, thisNode);

            return thisNode;
        }
        private void BuildSortChilds()
        {
            sortedHorizontalChildren.Clear();
            sortedVerticalChildren.Clear();

            UIElement e = null;
            //根元素
            for (int i = 0; i < Elements.Count; i++)
            {
                e = Elements[i];
                if (e.Visibility != UIVisibility.Gone)
                {
                    if (e.IsLayout) (e as UILayout).DoLayout(); //如果是子布局，需要布局
                    if (e.Layout_above == null && e.Layout_alignBottom == null && e.Layout_alignLeft == null
                        && e.Layout_alignRight == null && e.Layout_alignTop == null && e.Layout_below == null
                        && e.Layout_toLeftOf == null && e.Layout_toRightOf == null)
                        magEles.Add(e.Name, new ElementNode(e, 0, 0));
                }
            }
            //引用
            for (int i = 0; i < Elements.Count; i++)
            {
                e = Elements[i];
                if (e.Visibility != UIVisibility.Gone)
                    BuildChildRefInfo(e);
            }
            //生成排序列表
            foreach (var item in magEles)
            {
                sortedHorizontalChildren.Add(item.Value);
                sortedVerticalChildren.Add(item.Value);
            }
            //排序
            sortedHorizontalChildren.Sort((x, y) => x.dependenciesLevelHorizontal.CompareTo(y.dependenciesLevelHorizontal));//升序
            sortedVerticalChildren.Sort((x, y) => x.dependenciesLevelVertical.CompareTo(y.dependenciesLevelVertical));//升序
        }
        private void DoLayoutInternal()
        {
            for (int i = 0, c = sortedHorizontalChildren.Count; i < c; i++)
                DoLayoutElementHorizontalInternal(sortedHorizontalChildren[i].element);
            for (int i = 0, c = sortedVerticalChildren.Count; i < c; i++)
                DoLayoutElementVerticalInternal(sortedVerticalChildren[i].element);
        }
        private void DoLayoutElementHorizontalInternal(UIElement e)
        {
            UIAnchor a = UIAnchor.None;

            if (e.AnchorX != UIAnchor.Stretch)
            {

                if (!string.IsNullOrEmpty(e.Layout_alignLeft) || !string.IsNullOrEmpty(e.Layout_toLeftOf)
                    || e.Layout_alignParentLeft) a = UIAnchor.Left;
                else if (!string.IsNullOrEmpty(e.Layout_alignRight) || !string.IsNullOrEmpty(e.Layout_toRightOf)
                    || e.Layout_alignParentRight) a = UIAnchor.Right;
                else if (e.Layout_centerHorizontal || e.Layout_centerInParent) a = UIAnchor.Center;
                else a = UILayoutUtils.GravityToAnchor(Gravity, RectTransform.Axis.Horizontal);

                UIAnchorPosUtils.SetUIAnchor(e.RectTransform, a,
                    UIAnchorPosUtils.GetUIAnchor(e.RectTransform, RectTransform.Axis.Vertical));
                UIAnchorPosUtils.SetUIPivot(e.RectTransform,
                    UILayoutUtils.AnchorToPivot(a, RectTransform.Axis.Horizontal), RectTransform.Axis.Horizontal);

                float finalValue = 0;
                UIElement eRef = null;
                if (!string.IsNullOrEmpty(e.Layout_toLeftOf))
                {
                    eRef = FindElementInLayoutByName(e.Layout_toLeftOf);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.x + eRef.RectTransform.rect.width
                            + e.Layout_marginLeft;
                }
                else if (!string.IsNullOrEmpty(e.Layout_toRightOf))
                {
                    eRef = FindElementInLayoutByName(e.Layout_toRightOf);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.x - eRef.RectTransform.rect.width
                            - e.RectTransform.rect.width
                            - e.Layout_marginRight;
                }
                else if (!string.IsNullOrEmpty(e.Layout_alignLeft))
                {
                    eRef = FindElementInLayoutByName(e.Layout_alignLeft);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.x + e.Layout_marginLeft;
                }
                else if (!string.IsNullOrEmpty(e.Layout_alignRight))
                {
                    eRef = FindElementInLayoutByName(e.Layout_alignRight);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.x + eRef.RectTransform.rect.width
                            - e.RectTransform.rect.width - e.Layout_marginRight;
                }
                else if (a == UIAnchor.Center) finalValue = 0;
                else if (a == UIAnchor.Right) finalValue = -e.Layout_marginRight;
                else if (a == UIAnchor.Left) finalValue = e.Layout_marginLeft;

                e.RectTransform.anchoredPosition = new Vector2(
                    finalValue,
                    e.RectTransform.anchoredPosition.y);

            }
        }
        private void DoLayoutElementVerticalInternal(UIElement e)
        {
            UIAnchor a = UIAnchor.None;
            
            if (e.AnchorY != UIAnchor.Stretch)
            {
                if (!string.IsNullOrEmpty(e.Layout_alignTop) || !string.IsNullOrEmpty(e.Layout_below)
                    || e.Layout_alignParentTop) a = UIAnchor.Top;
                else if (!string.IsNullOrEmpty(e.Layout_alignBottom) || !string.IsNullOrEmpty(e.Layout_above)
                    || e.Layout_alignParentBottom) a = UIAnchor.Bottom;
                else if (e.Layout_centerVertical || e.Layout_centerInParent) a = UIAnchor.Center;
                else a = UILayoutUtils.GravityToAnchor(Gravity, RectTransform.Axis.Vertical);

                UIAnchorPosUtils.SetUIAnchor(e.RectTransform, 
                    UIAnchorPosUtils.GetUIAnchor(e.RectTransform, RectTransform.Axis.Horizontal), a);
                UIAnchorPosUtils.SetUIPivot(e.RectTransform,
                    UILayoutUtils.AnchorToPivot(a, RectTransform.Axis.Vertical), RectTransform.Axis.Vertical);

                float finalValue = 0;
                UIElement eRef = null;
                if (!string.IsNullOrEmpty(e.Layout_below))
                {
                    eRef = FindElementInLayoutByName(e.Layout_below);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.y - eRef.RectTransform.rect.height
                            - e.Layout_marginTop;
                }
                else if (!string.IsNullOrEmpty(e.Layout_above))
                {
                    eRef = FindElementInLayoutByName(e.Layout_above);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.x + e.RectTransform.rect.height
                            + e.Layout_marginBottom;
                }
                else if (!string.IsNullOrEmpty(e.Layout_alignTop))
                {
                    eRef = FindElementInLayoutByName(e.Layout_alignTop);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.y - e.Layout_marginTop;
                }
                else if (!string.IsNullOrEmpty(e.Layout_alignBottom))
                {
                    eRef = FindElementInLayoutByName(e.Layout_alignBottom);
                    if (eRef != null)
                        finalValue = eRef.RectTransform.anchoredPosition.y - eRef.RectTransform.rect.height
                            + e.RectTransform.rect.height + e.Layout_marginBottom;
                }
                else if (a == UIAnchor.Center) finalValue = 0;
                else if (a == UIAnchor.Top) finalValue = -e.Layout_marginTop;
                else if (a == UIAnchor.Bottom) finalValue = e.Layout_marginBottom;

                
                e.RectTransform.anchoredPosition = new Vector2(
                    e.RectTransform.anchoredPosition.x,
                    finalValue
                );
            }
        }
    }
}
