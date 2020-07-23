﻿using Ballance2.UI.BallanceUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ballance2.UI.Utils
{
    public class UISizeDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler,
        IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public UISizeDrag()
        {
        }

        public UIWindow DragWindow = null;
        public RectTransform DragWindowRectTransform = null;
        public Image DragImage = null;

        /// <summary>
        /// 是否正在拖动
        /// </summary>
        public bool isDrag
        {
            get; private set;
        }
        public bool isDraged
        {
            get
            {
                bool rs = _isDraged;
                _isDraged = false;
                return rs;
            }
        }

        private bool _isDraged = false;
        private Vector2 mouseDownWindowPos;

        public void OnPointerDown(PointerEventData eventData)
        {
            isDrag = true;
            DragImage.color = new Color(DragImage.color.r, DragImage.color.g, DragImage.color.b, 0.7f);
            if (DragWindowRectTransform != null)
            {
                mouseDownWindowPos = new Vector2(eventData.position.x - DragWindowRectTransform.rect.width,
                   -eventData.position.y - DragWindowRectTransform.rect.height);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            _isDraged = true;

            if (DragWindowRectTransform != null)
            {
                Vector2 dragPos = eventData.position;
                if (dragPos.x > Screen.width - 80) dragPos.x = Screen.width - 80;
                if (dragPos.y < 30) dragPos.y = 30;

                Vector2 minSize = DragWindow.GetMinSize();
                Vector2 v =  new Vector2(
                    (dragPos.x - mouseDownWindowPos.x),
                    ((-dragPos.y - mouseDownWindowPos.y))
                );
                if (minSize.x > 0 && v.x < minSize.x) v.x = minSize.x;
                if (minSize.y > 0 && v.y < minSize.y) v.y = minSize.y;
                DragWindowRectTransform.sizeDelta = v;
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            isDrag = false;
            DragImage.color = new Color(DragImage.color.r, DragImage.color.g, DragImage.color.b, 1f);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            DragImage.color = new Color(1, 0.5869601f, 0);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            DragImage.color = new Color(1, 0.783546f, 0);
        }
    }
}
