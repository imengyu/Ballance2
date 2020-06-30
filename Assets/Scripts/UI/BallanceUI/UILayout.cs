using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
using SubjectNerd.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// 布局
    /// </summary>
    [SLua.CustomLuaClass]
    public class UILayout : UIElement
    {
        /// <summary>
        /// 子元素
        /// </summary>
        public List<UIElement> Elements { get { return elements; } }
        /// <summary>
        /// 内容边距
        /// </summary>
        public LayoutGravity Gravity
        {
            get { return gravity; }
            set
            {
                gravity = value;
                OnGravityChanged();
                PostDoLayout();
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public virtual UIElement AddElement(UIElement element, bool doLayout = true)
        {
            if (element != null)
            {
                RectTransform rectTransform = element.RectTransform;
                rectTransform.SetParent(RectTransform);
                if (doLayout)
                    PostDoLayout();
                elements.Add(element);
                element.Parent = this;
            }
            return element;
        }
        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="destroy">是否释放元素，否则隐藏元素</param>
        /// <param name="doLayout">是否立即重新布局</param>
        public virtual void RemoveElement(UIElement element, bool destroy = true, bool doLayout = true)
        {
            GameObject go = null;
            for (int i = transform.childCount; i >= 0; i--)
            {
                go = transform.GetChild(i).gameObject;
                if (go == element.gameObject)
                {
                    if (destroy) Destroy(go); else go.SetActive(false);
                    if (doLayout) PostDoLayout();
                    break;
                }
            }
            if (!destroy)
                element.Parent = null;
            elements.Remove(element);
        }
        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="index">插入的索引</param>
        /// <returns></returns>
        public virtual UIElement InsertElement(UIElement element, int index, bool doLayout = true)
        {
            RectTransform rectTransform = element.RectTransform;
            rectTransform.SetParent(RectTransform);
            if (doLayout)
                PostDoLayout();
            element.Parent = this;
            elements.Insert(index, element);
            return element;
        }
        /// <summary>
        /// 通过名字查找元素
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns></returns>
        public UIElement FindElementByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach(UIElement u in elements)
            {
                if (u.Name == name) return u;
                else if(u is UILayout)
                {
                    UIElement rs = (u as UILayout).FindElementByName(name);
                    if (rs != null) return rs;
                }
            }
            return null;
        }
        /// <summary>
        /// 通过名字查找布局内部的元素
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns></returns>
        public UIElement FindElementInLayoutByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach (UIElement u in elements)
            {
                if (u.Name == name) return u;
            }
            return null;
        }

        public UILayout()
        {
            isLayout = true;
            elements = new List<UIElement>();
        }

        [SerializeField, SetProperty("Elements")]
        private List<UIElement> elements = null;
        [SerializeField, SetProperty("Gravity")]
        private LayoutGravity gravity;

        private int loopUpdate = 6;
        private int layoutDelyCount = 0;
        [SerializeField, SetProperty("LayoutLock")]
        private bool layoutLock = false;

        protected override void OnUpdateElement()
        {
            if (loopUpdate > 0 && !layoutLock)
            {
                loopUpdate--;
                if (loopUpdate == 0)
                    DoLayout();
            }
            if (layoutDelyCount > 0 && !layoutLock)
                layoutDelyCount--;
            base.OnUpdateElement();
        }
        protected override void OnDestroyElement()
        {
            if (elements != null)
            {
                elements.Clear();
                elements = null;
            }
            base.OnDestroyElement();
        }
        protected override void OnInitElement()
        {
            base.OnInitElement();
            loopUpdate = 6;
        }

        /// <summary>
        /// 解锁布局
        /// </summary>
        public void LayoutUnLock() { layoutLock = false; }
        /// <summary>
        /// 锁定布局
        /// </summary>
        public void LayoutLock() { layoutLock = true;  }
        /// <summary>
        /// 发送布局消息（延迟布局）
        /// </summary>
        public void PostDoLayout()
        {
            loopUpdate = 10;
            /*
            layoutPendCount++;
            loopUpdate = 5 - (int)(5 * (layoutPendCount / 5.0f));
            if (loopUpdate < 2) loopUpdate = 2;
            if (loopUpdate > 15) loopUpdate = 15;
            */
        }
        /// <summary>
        /// 强制布局
        /// </summary>
        /// <param name="startChildIndex"></param>
        public void DoLayout()
        {
            if (!layoutLock)
            {
                layoutLock = true;
                OnLayout();
                layoutLock = false;
            }
            /*
            if (!layoutLock)
            {
                if (layoutDelyCount == 0)
                {
                    layoutDelyCount = 2;
                    layoutPendCount = 0;
                    layoutLock = true;
                   
                    OnLayout();

                    layoutLock = false;
                }
                else loopUpdate = 2;
            }
            */
        }
        /// <summary>
        /// 元素布局属性初始化应用
        /// </summary>
        /// <param name="element">目标元素</param>
        public virtual void ReInitElement(UIElement element) { }



        protected virtual void OnGravityChanged() { }
        protected virtual void OnLayout() { }

        protected override void SetProp(string name, string val)
        {
            switch (name)
            {
                case "gravity":
                    System.Enum.TryParse(val, out gravity);
                    OnGravityChanged();
                    PostDoLayout();
                    break;
                default:
                    base.SetProp(name, val);
                    break;
            }
        }
    }

    [SLua.CustomLuaClass] 
    public static class UILayoutUtils
    {
        public static UIAnchor GravityToAnchor(LayoutGravity gravity, RectTransform.Axis axis)
        {
            if (axis == RectTransform.Axis.Vertical)
            {
                switch (gravity)
                {
                    case LayoutGravity.Top: return UIAnchor.Top;
                    case LayoutGravity.Bottom: return UIAnchor.Bottom;
                    case LayoutGravity.CenterVertical:
                    case LayoutGravity.Center: return UIAnchor.Center;
                }
            }
            else
            {
                switch (gravity)
                {
                    case LayoutGravity.Start: return UIAnchor.Left;
                    case LayoutGravity.End: return UIAnchor.Right;
                    case LayoutGravity.CenterHorizontal:
                    case LayoutGravity.Center: return UIAnchor.Center;
                }
            }
            return UIAnchor.None;
        }
        public static UIPivot AnchorToPivot(UIAnchor anchor, RectTransform.Axis axis)
        {
            if (axis == RectTransform.Axis.Vertical)
            {
                switch (anchor)
                {
                    case UIAnchor.Top: return UIPivot.Top;
                    case UIAnchor.Bottom: return UIPivot.Bottom;
                    case UIAnchor.Center: return UIPivot.Center;
                }
            }
            else
            {
                switch (anchor)
                {
                    case UIAnchor.Left: return UIPivot.Left;
                    case UIAnchor.Right: return UIPivot.Right;
                    case UIAnchor.Center: return UIPivot.Center;
                }
            }
            return UIPivot.None;
        }
    }

    [SLua.CustomLuaClass]
    /// <summary>
    /// 布局轴
    /// </summary>
    public enum LayoutAxis
    {
        /// <summary>
        /// 垂直
        /// </summary>
        Vertical,
        /// <summary>
        /// 水平
        /// </summary>
        Horizontal,
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 布局方式
    /// </summary>
    public enum LayoutType
    {
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
    [SLua.CustomLuaClass]
    /// <summary>
    /// 布局对其
    /// </summary>
    public enum LayoutGravity
    {
        /// <summary>
        /// 未知
        /// </summary>
        None,
        Top = 0x1,
        Bottom = 0x2,
        Start = 0x4,
        End = 0x8,
        CenterHorizontal = 0x10,
        CenterVertical = 0x20,
        Center = CenterHorizontal | CenterVertical,
    }
}
