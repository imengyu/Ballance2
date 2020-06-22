using Ballance2.UI.BallanceUI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// 布局容器接口
    /// </summary>
    public  interface ILayoutContainer
    {
        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns></returns>
        UIElement AddElement(UIElement element, bool doLayout = true);
        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="destroy">是否释放元素，否则隐藏元素</param>
        /// <param name="doLayout">是否立即重新布局</param>
        void RemoveElement(UIElement element, bool destroy = true, bool doLayout = true);
        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="index">插入的索引</param>
        /// <returns></returns>
        UIElement InsertElement(UIElement element, int index, bool doLayout = true);
        /// <summary>
        /// 通过元素名字查找
        /// </summary>
        /// <param name="name">元素名字</param>
        /// <returns></returns>
        UIElement FindElementByName(string name);

        /// <summary>
        /// 强制重新布局
        /// </summary>
        /// <param name="startChildIndex">开始的子元素索引，为 0 时强制布局全部</param>
        void DoLayout(int startChildIndex);
        /// <summary>
        /// 发送更新消息
        /// </summary>
        void PostDoLayout();

        /// <summary>
        /// 获取元素
        /// </summary>
        List<UIElement> Elements { get; }
        /// <summary>
        /// 获取 RectTransform
        /// </summary>
        RectTransform RectTransform { get; set; }
    }
}
