using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    [SLua.CustomLuaClass]
    [AddComponentMenu("Ballance/UI/Layout/UIVerticalLinearLayout")]
    public class UIVerticalLinearLayout : Layout.UILinearLayout
    {
        public UIVerticalLinearLayout()
        {
            layoutDirection = LayoutAxis.Vertical;
            baseName = "UIVerticalLinearLayout";
        }
    }
}
