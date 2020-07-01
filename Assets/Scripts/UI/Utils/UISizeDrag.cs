using Ballance2.UI.BallanceUI;
using UnityEngine;
using UnityEngine.EventSystems;

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
            if (DragWindowRectTransform != null)
            {
                mouseDownWindowPos = DragWindowRectTransform.position;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 mouseDrag = eventData.position;
            _isDraged = true;

            if (DragWindowRectTransform != null)
            {
                Vector2 minSize = DragWindow.GetMinSize();
                Vector2 v =  new Vector2(
                    (mouseDrag.x - mouseDownWindowPos.x) * 2,
                    -((mouseDrag.y - mouseDownWindowPos.y) * 2)
                );
                if (minSize.x > 0 && v.x < minSize.x) v.x = minSize.x;
                if (minSize.y > 0 && v.y < minSize.y) v.y = minSize.y;
                DragWindowRectTransform.sizeDelta = v;
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            isDrag = false;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
        }
        public void OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
