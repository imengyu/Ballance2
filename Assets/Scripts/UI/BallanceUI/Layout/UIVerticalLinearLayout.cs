namespace Ballance2.UI.BallanceUI
{
    [SLua.CustomLuaClass]
    public class UIVerticalLinearLayout : Layout.UILinearLayout
    {
        public UIVerticalLinearLayout()
        {
            layoutDirection = LayoutAxis.Vertical;
        }
    }
}
