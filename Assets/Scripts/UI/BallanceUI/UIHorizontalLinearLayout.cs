using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    [SLua.CustomLuaClass]
    public class UIHorizontalLinearLayout : UILinearLayout
    {
        public UIHorizontalLinearLayout()
        {
            layoutType = LayoutType.Horizontal;
        }
    }
}
