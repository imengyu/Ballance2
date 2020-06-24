namespace Ballance2.UI.BallanceUI
{
    [SLua.CustomLuaClass]
    public class UIHorizontalLinearLayout : Layout.UILinearLayout
    {
        public UIHorizontalLinearLayout()
        {
            layoutDirection = LayoutAxis.Horizontal;
        }
    }
}
