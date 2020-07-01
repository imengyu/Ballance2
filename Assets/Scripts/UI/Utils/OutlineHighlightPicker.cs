using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.UI.Utils
{
    [SLua.CustomLuaClass]
    public class OutlineHighlightPicker : MonoBehaviour
    {
        public QuickOutline QuickOutline;

        public Color NormalColor = Color.blue;
        public Color EnterColor = Color.red;

        private void Start()
        {
            if (QuickOutline == null)
                QuickOutline = GetComponent<QuickOutline>();
        }

        public Button.ButtonClickedEvent onClick;

        void OnMouseEnter()
        {
            QuickOutline.OutlineColor = EnterColor;
        }
        void OnMouseExit()
        {
            QuickOutline.OutlineColor = NormalColor;
        }
        void OnMouseDown()
        {

        }
        void OnMouseUp()
        {
            if (onClick != null)
                onClick.Invoke();
        }
    }
}
