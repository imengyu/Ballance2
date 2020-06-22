using Ballance2.UI.BallanceUI;
using Ballance2.UI.BallanceUI.Element;
using Ballance2.UI.Utils;
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
        /// 添加元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        public virtual UIElement AddElement(UIElement element, bool doLayout = true)
        {
            RectTransform rectTransform = element.RectTransform;
            rectTransform.SetParent(RectTransform);
            if (doLayout)
                DoLayout(transform.childCount - 1);
            elements.Add(element);
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
                    if (doLayout) DoLayout(i);
                    break;
                }
            }

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
                DoLayout(index);
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

        public UILayout()
        {
            elements = new List<UIElement>();
        }

        private List<UIElement> elements = null;

        int loopUpdate = 0; 

        private void OnDestroy()
        {
            if (elements != null)
            {
                elements.Clear();
                elements = null;
            }
        }
        private void Update()
        {
            if (loopUpdate > 0) {
                loopUpdate--;
                if (loopUpdate == 0)
                    DoLayout(0);
            }
            
        }

        public virtual void PostDoLayout()
        {
            loopUpdate = 30;
        }
        public virtual void DoLayout(int startChildIndex)
        {
        }


    }
}
