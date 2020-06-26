using Knife.PostProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.UI.Utils
{
    [SLua.CustomLuaClass]
    public class OutlineHighlightPicker : MonoBehaviour
    {
        public OutlineRegister OutlineRegister;

        public Color NormalColor = Color.blue;
        public Color EnterColor = Color.red;

        private void Start()
        {
            OutlineRegister = GetComponent<OutlineRegister>();
        }

        public Button.ButtonClickedEvent onClick;

        void OnMouseEnter()
        {
            OutlineRegister.OutlineTint = NormalColor;
            OutlineRegister.setupPropertyBlock();
        }
        void OnMouseExit()
        {
            OutlineRegister.OutlineTint = EnterColor;
            OutlineRegister.setupPropertyBlock();
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
