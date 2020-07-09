using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    [SLua.CustomLuaClass]
    [AddComponentMenu("Ballance/UI/Layout/UIHorizontalLinearLayout")]
    public class UIHorizontalLinearLayout : Layout.UILinearLayout
    {
        public UIHorizontalLinearLayout()
        {
            layoutDirection = LayoutAxis.Horizontal;
            baseName = "UIHorizontalLinearLayout";
        }
    }
}
