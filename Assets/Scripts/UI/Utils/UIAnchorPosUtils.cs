using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    [SLua.CustomLuaClass]
    /// <summary>
    /// UI 组件锚点位置工具
    /// </summary>
    public static class UIAnchorPosUtils
    {
        /// <summary>
        /// 设置 UI 组件锚点
        /// </summary>
        /// <param name="rectTransform">UI 组件</param>
        /// <param name="x">X 轴锚点</param>
        /// <param name="y">Y 轴锚点</param>
        public static void SetUIAnchor(RectTransform rectTransform, UIAnchor x, UIAnchor y)
        {
            float x1 = 0.5f, x2 = 0.5f, y1 = 0.5f, y2 = 0.5f;

            switch(x)
            {
                case UIAnchor.Left:
                    x1 = 0;
                    x2 = 0;
                    break;
                case UIAnchor.Center:
                    x1 = 0.5f;
                    x2 = 0.5f;
                    break;
                case UIAnchor.Right:
                    x1 = 1;
                    x2 = 1;
                    break;
                case UIAnchor.Stretch:
                    x1 = 0;
                    x2 = 1;
                    break;
            }
            switch (y)
            {
                case UIAnchor.Top:
                    y1 = 1;
                    y2 = 1;
                    break;
                case UIAnchor.Center:
                    y1 = 0.5f;
                    y2 = 0.5f;
                    break;
                case UIAnchor.Bottom:
                    y1 = 0;
                    y2 = 0;
                    break;
                case UIAnchor.Stretch:
                    y1 = 0;
                    y2 = 1;
                    break;
            }

            rectTransform.anchorMin = new Vector2(x1, y1);
            rectTransform.anchorMax = new Vector2(x2, y2);
        }

        /// <summary>
        /// 设置 UI 组件 上 右 坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        public static void SetUIRightTop(RectTransform rectTransform, float right, float top)
        {
            rectTransform.offsetMax = new Vector2(-right, -top);
        }
        /// <summary>
        /// 设置 UI 组件 左 下坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="left"></param>
        /// <param name="bottom"></param>
        public static void SetUILeftBottom(RectTransform rectTransform, float left, float bottom)
        {
            rectTransform.offsetMin = new Vector2(left, bottom);
        }
        /// <summary>
        /// 设置 UI 组件 左 上 右 下坐标
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public static void SetUIPos(RectTransform rectTransform, float left, float top, float right, float bottom)
        {
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        public static float GetUIRight(RectTransform rectTransform)
        {
            return rectTransform.offsetMax.x;
        }
        public static float GetUITop(RectTransform rectTransform)
        {
            return rectTransform.offsetMax.y;
        }
        public static float GetUILeft(RectTransform rectTransform)
        {
            return rectTransform.offsetMin.x;
        }
        public static float GetUIBottom(RectTransform rectTransform)
        {
            return rectTransform.offsetMin.y;
        }
    }

    /// <summary>
    /// UI 组件锚点
    /// </summary>
    public enum UIAnchor
    {
        Top,
        Center,
        Bottom,
        Left,
        Right,
        Stretch,
    }
}
