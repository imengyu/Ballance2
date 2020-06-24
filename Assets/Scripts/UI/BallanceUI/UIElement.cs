using Ballance2.Managers.CoreBridge;
using Ballance2.UI.Utils;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Ballance2.UI.BallanceUI
{
    /// <summary>
    /// UI 元素 基类
    /// </summary>
    [SLua.CustomLuaClass]
    public class UIElement : MonoBehaviour
    {
        private XmlNode lateInitXml = null;
        private Dictionary<string, GameHandler> lateInitHandlers = null;

        public void LateInitHandlers(Dictionary<string, GameHandler> gameHandlers)
        {
            lateInitHandlers = gameHandlers;
            lateInitDeaily = 30;
        }
        public void LateInit(XmlNode xml)
        {
            if (startClled)
                SolveXml(xml);
            else
            {
                lateInitXml = xml;
                lateInitDeaily = 30;
            }
        }

        /// <summary>
        /// 设置事件接收器
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="handler">接收器</param>
        public virtual void SetEventHandler(string name, GameHandler handler) {
            GameLogger.Log(Name, "SetEventHandler {0} {1}", name, handler.Name);
        }
        /// <summary>
        /// 移除事件接收器
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="handler">接收器</param>
        public virtual void RemoveEventHandler(string name, GameHandler handler) {
            GameLogger.Log(Name, "RemoveEventHandler {0} {1}", name, handler.Name);
        }

        [SerializeField, SetProperty("AnchorX")]
        private UIAnchor anchorX;
        [SerializeField, SetProperty("AnchorY")]
        private UIAnchor anchorY;
        [SerializeField, SetProperty("MinSize")]
        private Vector2 minSize = new Vector2(0, 0);
        [SerializeField, SetProperty("MaxSize")]
        private Vector2 maxSize = new Vector2(0, 0);
        [SerializeField, SetProperty("Name")]
        private new string name = "";
        [SerializeField, SetProperty("Layout_marginLeft")]
        private float layout_marginLeft = 0;
        [SerializeField, SetProperty("Layout_marginTop")]
        private float layout_marginTop = 0;
        [SerializeField, SetProperty("Layout_marginRight")]
        private float layout_marginRight = 0;
        [SerializeField, SetProperty("Layout_marginBottom")]
        private float layout_marginBottom = 0;

        internal UILayout rootContainer;

        /// <summary>
        /// 当前元素所属根布局
        /// </summary>
        public UILayout RootContainer { get { return rootContainer; } }
        /// <summary>
        /// 边距
        /// </summary>
        public float Layout_marginLeft { get { return layout_marginLeft; } set { layout_marginLeft = value; DoPostLayout(); } }
        /// <summary>
        /// 边距
        /// </summary>
        public float Layout_marginTop { get { return layout_marginTop; } set { layout_marginTop = value; DoPostLayout(); } }
        /// <summary>
        /// 边距
        /// </summary>
        public float Layout_marginRight { get { return layout_marginRight; } set { layout_marginRight = value; DoPostLayout(); } }
        /// <summary>
        /// 边距
        /// </summary>
        public float Layout_marginBottom { get { return layout_marginBottom; } set { layout_marginBottom = value; DoPostLayout(); } }
        /// <summary>
        /// 最小大小
        /// </summary>
        public Vector2 MinSize { get { return minSize; } set  { minSize = value; DoPostLayout(); } }
        /// <summary>
        /// 最大大小
        /// </summary>
        public Vector2 MaxSize { get { return maxSize; } set { maxSize = value; DoPostLayout(); } }
        /// <summary>
        /// X轴锚点
        /// </summary>
        public UIAnchor AnchorX
        {
            get { return anchorX; }
            set
            {
                anchorX = value;
                if (Parent != null) Parent.PostDoLayout();
            }
        }
        /// <summary>
        /// Y轴锚点
        /// </summary>
        public UIAnchor AnchorY
        {
            get { return anchorY; }
            set
            {
                anchorY = value;
                if (Parent != null) Parent.PostDoLayout();
            }
        }

        /// <summary>
        /// 创建时的名字
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                base.name = value;
            }
        }
        /// <summary>
        /// 当前 RectTransform
        /// </summary>
        public RectTransform RectTransform { get; set; }
        /// <summary>
        /// 父级
        /// </summary>
        public UILayout Parent { get; set; }

        [SerializeField, SetProperty("Visibility")]
        private UIVisibility visibility = UIVisibility.Visible;
        [SerializeField, SetProperty("Layout_centerHorizontal")]
        private bool layout_centerHorizontal = false;
        [SerializeField, SetProperty("Layout_centerVertical")]
        private bool layout_centerVertical = false;
        [SerializeField, SetProperty("Layout_centerInParent")]
        private bool layout_centerInParent = false;
        [SerializeField, SetProperty("Layout_alignParentTop")]
        private bool layout_alignParentTop = false;
        [SerializeField, SetProperty("Layout_alignParentBottom")]
        private bool layout_alignParentBottom = false;
        [SerializeField, SetProperty("Layout_alignParentLeft")]
        private bool layout_alignParentLeft = false;
        [SerializeField, SetProperty("Layout_alignParentRight")]
        private bool layout_alignParentRight = false;
        [SerializeField, SetProperty("Layout_above")]
        private string layout_above = null;
        [SerializeField, SetProperty("Layout_below")]
        private string layout_below = null;
        [SerializeField, SetProperty("Layout_toRightOf")]
        private string layout_toRightOf = null;
        [SerializeField, SetProperty("Layout_toLeftOf")]
        private string layout_toLeftOf = null;
        [SerializeField, SetProperty("Layout_alignTop")]
        private string layout_alignTop = null;
        [SerializeField, SetProperty("Layout_alignBottom")]
        private string layout_alignBottom = null;
        [SerializeField, SetProperty("Layout_alignLeft")]
        private string layout_alignLeft = null;
        [SerializeField, SetProperty("Layout_alignRight")]
        private string layout_alignRight = null;

        public UIVisibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                gameObject.SetActive(visibility == UIVisibility.Visible);
                DoPostLayout();
            }
        }
        public bool Layout_centerHorizontal { get { return layout_centerHorizontal; } set { layout_centerHorizontal = value; DoPostLayout(); } }
        public bool Layout_centerVertical { get { return layout_centerVertical; } set { layout_centerVertical = value; DoPostLayout(); } }
        public bool Layout_centerInParent { get { return layout_centerInParent; } set { layout_centerInParent = value; DoPostLayout(); } }
        public bool Layout_alignParentTop { get { return layout_alignParentTop; } set { layout_alignParentTop = value; DoPostLayout(); } }
        public bool Layout_alignParentBottom { get { return layout_alignParentBottom; } set { layout_alignParentBottom = value; DoPostLayout(); } }
        public bool Layout_alignParentLeft { get { return layout_alignParentLeft; } set { layout_alignParentLeft = value; DoPostLayout(); } }
        public bool Layout_alignParentRight { get { return layout_alignParentRight; } set { layout_alignParentRight = value; DoPostLayout(); } }
        public string Layout_above { get { return layout_above; } set { layout_above = value; DoPostLayout(); } }
        public string Layout_below { get { return layout_below; } set { layout_below = value; DoPostLayout(); } }
        public string Layout_toRightOf { get { return layout_toRightOf; } set { layout_toRightOf = value; DoPostLayout(); } }
        public string Layout_toLeftOf { get { return layout_toLeftOf; } set { layout_toLeftOf = value; DoPostLayout(); } }
        public string Layout_alignTop { get { return layout_alignTop; } set { layout_alignTop = value; DoPostLayout(); } }
        public string Layout_alignBottom { get { return layout_alignBottom; } set { layout_alignBottom = value; DoPostLayout(); } }
        public string Layout_alignLeft { get { return layout_alignLeft; } set { layout_alignLeft = value; DoPostLayout(); } }
        public string Layout_alignRight { get { return layout_alignRight; } set { layout_alignRight = value; DoPostLayout(); } }

        private Dictionary<string, object> data = new Dictionary<string, object>(); 

        public object SetCustomData(string name, object data)
        {
            this.data.Add(name, data);
            return data;
        }
        public object GetCustomData(string name)
        {
            object o = null;
            data.TryGetValue(name, out o);
            return o;
        }

        protected string baseName = "UIElement";
        protected virtual void SolveXml(XmlNode xml)
        {
            foreach (XmlAttribute a in xml.Attributes)
                SetProp(a.Name, a.Value);
        }
        protected virtual void SetProp(string name, string val)
        {
            switch (name)
            {
                case "name":
                    {
                        Name = val;
                        break;
                    }
                case "minSize":
                    {
                        string[] av = val.Split(',');
                        float x, y;
                        if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y)) { 
                            minSize = new Vector2(x, y);
                            DoPostLayout();
                        }
                        break;
                    }
                case "width":
                    {
                        if (val.ToLower() == "match_parent")
                        {
                            anchorX = UIAnchor.Stretch;
                            UIAnchorPosUtils.SetUIAnchor(RectTransform, anchorX, anchorY);
                            DoPostLayout();
                        }
                        else
                        {
                            float x;
                            if (float.TryParse(val, out x)) { 
                                RectTransform.sizeDelta = new Vector2(x, RectTransform.sizeDelta.y);
                                DoPostLayout();
                            }
                        }
                        break;
                    }
                case "height":
                    {
                        if (val.ToLower() == "match_parent")
                        {
                            anchorY = UIAnchor.Stretch;
                            UIAnchorPosUtils.SetUIAnchor(RectTransform, anchorX, anchorY);
                            DoPostLayout();
                        }
                        else
                        {
                            float y;
                            if (float.TryParse(val, out y)) { 
                                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, y);
                                DoPostLayout();
                            }
                        }
                        break;
                    }
                case "x":
                    {
                        float v;
                        if (float.TryParse(val, out v)) { 
                            RectTransform.anchoredPosition = new Vector2(v, RectTransform.anchoredPosition.y);
                            DoPostLayout();
                        }
                        break;
                    }
                case "y":
                    {
                        float v;
                        if (float.TryParse(val, out v)) { 
                            RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, v);
                            DoPostLayout();
                        }
                        break;
                    }
                case "position":
                    {
                        string[] av = val.Split(',');
                        float x, y;
                        if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y)) { 
                            RectTransform.anchoredPosition = new Vector2(x, y);
                            DoPostLayout();
                        }
                        break;
                    }
                case "left":
                    {
                        float v;
                        if (float.TryParse(val, out v)) 
                            UIAnchorPosUtils.SetUILeftBottom(RectTransform, v, UIAnchorPosUtils.GetUIBottom(RectTransform));
                           break;
                    }
                case "bottom":
                    {
                        float v;
                        if (float.TryParse(val, out v))
                            UIAnchorPosUtils.SetUILeftBottom(RectTransform, UIAnchorPosUtils.GetUILeft(RectTransform), v);
                        break;
                    }
                case "leftBottom":
                    {
                        string[] av = val.Split(',');
                        float x, y;
                        if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                            UIAnchorPosUtils.SetUILeftBottom(RectTransform, x, y);
                        break;
                    }
                case "right":
                    {
                        float v;
                        if (float.TryParse(val, out v))
                            UIAnchorPosUtils.SetUIRightTop(RectTransform, v, UIAnchorPosUtils.GetUITop(RectTransform));
                        break;
                    }
                case "top":
                    {
                        float v;
                        if (float.TryParse(val, out v))
                            UIAnchorPosUtils.SetUIRightTop(RectTransform, UIAnchorPosUtils.GetUIRight(RectTransform), v);
                        break;
                    }
                case "rightTop":
                    {
                        string[] av = val.Split(',');
                        float x, y;
                        if (av.Length >= 2 && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                            UIAnchorPosUtils.SetUIRightTop(RectTransform, x, y);
                        break;
                    }
                case "anchor":
                    {
                        string[] av = val.Split(',');
                        if (av.Length >= 2)
                        {
                            System.Enum.TryParse(av[0], out anchorX);
                            System.Enum.TryParse(av[1], out anchorY);
                            DoPostLayout();
                        }
                        break;
                    }
                case "anchorX":
                    {
                        System.Enum.TryParse(val, out anchorX);
                        DoPostLayout();
                        break;
                    }
                case "visibility":
                    {
                        UIVisibility v;
                        System.Enum.TryParse(val, out v);
                        Visibility = v;
                        break;
                    }
                case "anchorY":
                    {
                        System.Enum.TryParse(val, out anchorY);
                        DoPostLayout();
                        break;
                    }    
                case "layout_margin":
                    {
                        string[] av = val.Split(',');
                        float x, y, z, w;
                        if (av.Length >= 4
                            && float.TryParse(av[0], out x) && float.TryParse(av[1], out y)
                            && float.TryParse(av[1], out z) && float.TryParse(av[2], out w))
                        {
                            Layout_marginLeft = x;
                            Layout_marginTop = y;
                            Layout_marginRight = z;
                            Layout_marginBottom = w;
                        }

                        if (av.Length >= 2
                            && float.TryParse(av[0], out x) && float.TryParse(av[1], out y))
                        {
                            Layout_marginLeft = x;
                            Layout_marginTop = y;
                            Layout_marginRight = x;
                            Layout_marginBottom = y;
                        }
                        if (av.Length >= 1
                           && float.TryParse(av[0], out x))
                        {
                            Layout_marginLeft = x;
                            Layout_marginTop = x;
                            Layout_marginRight = x;
                            Layout_marginBottom = x;
                        }
                        DoPostLayout();
                        break;
                    }
                case "layout_marginLeft":
                    {
                        float v;
                        if (float.TryParse(val, out v))
                        {
                            Layout_marginLeft = v;
                            DoPostLayout();
                        }
                        break;
                    }
                case "layout_marginTop":
                    {
                        float v;
                        if (float.TryParse(val, out v)) {
                            Layout_marginTop = v;
                            DoPostLayout();
                        }
                        break;
                    }
                case "layout_marginRight":
                    {
                        float v;
                        if (float.TryParse(val, out v)) {
                            Layout_marginRight = v;
                            DoPostLayout();
                        }
                        break;
                    }
                case "layout_marginBottom":
                    {
                        float v;
                        if (float.TryParse(val, out v)) {
                            Layout_marginBottom = v;
                            DoPostLayout();
                        }
                        break;
                    }
                case "layout_centerHorizontal":
                    {
                        if (bool.TryParse(val, out layout_centerHorizontal))
                            DoPostLayout();
                        break;
                    }
                case "layout_centerVertical":
                    {
                        if (bool.TryParse(val, out layout_centerVertical))
                            DoPostLayout();
                        break;
                    }
                case "layout_centerInParent":
                    {
                        if (bool.TryParse(val, out layout_centerInParent))
                            DoPostLayout();
                        break;
                    }
                case "layout_alignParentTop":
                    {
                        if (bool.TryParse(val, out layout_alignParentTop))
                            DoPostLayout();
                        break;
                    }
                case "layout_alignParentBottom":
                    {
                        if (bool.TryParse(val, out layout_alignParentBottom))
                            DoPostLayout();
                        break;
                    }
                case "layout_alignParentLeft":
                    {
                        if (bool.TryParse(val, out layout_alignParentLeft))
                            DoPostLayout();
                        break;
                    }
                case "layout_alignParentRight":
                    {
                        if (bool.TryParse(val, out layout_alignParentRight))
                            DoPostLayout();
                        break;
                    }
                case "layout_above":
                    {
                        layout_toLeftOf = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_below":
                    {
                        layout_toLeftOf = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_toRightOf":
                    {
                        layout_toLeftOf = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_toLeftOf":
                    {
                        layout_toLeftOf = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_alignTop":
                    {
                        layout_alignTop = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_alignBottom":
                    {
                        layout_alignBottom = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_alignLeft":
                    {
                        layout_alignLeft = val;
                        DoPostLayout();
                        break;
                    }
                case "layout_alignRight":
                    {
                        layout_alignRight = val;
                        DoPostLayout();
                        break;
                    }
            }
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="name">属性名字</param>
        /// <param name="val">属性值</param>
        public void SetProperty(string name, string val)
        {
            SetProp(name, val);
        }

        private bool startClled = false;
        private int lateInitDeaily = 0;

        protected void Start()
        {
            if(string.IsNullOrEmpty(name))
                Name = baseName;
            RectTransform = GetComponent<RectTransform>();
            startClled = true;
        }
        protected void Update()
        {
            if(lateInitDeaily > 0)
            {
                lateInitDeaily--;
                if (lateInitDeaily == 0) DoLateInit();
            }
        }
        protected void OnDestroy()
        {
            if (data != null)
            {
                data.Clear();
                data = null;
            }
        }
        private void DoLateInit()
        {
            if (lateInitXml != null)
            {
                SolveXml(lateInitXml);
            }
            if (lateInitHandlers != null)
            {
                foreach (string key in lateInitHandlers.Keys)
                    SetEventHandler(key, lateInitHandlers[key]);
            }
        }
        protected void DoPostLayout()
        {
            if (Parent != null)
            {
                Parent.ReInitElement(this);
                Parent.PostDoLayout();
            }
            if (this is UILayout) (this as UILayout).PostDoLayout();
            else DoResize();
        }

        /// <summary>
        /// 强制重新适应 MinSize MaxSize
        /// </summary>
        public void DoResize()
        {
            UIAnchor[] thisAnchor = UIAnchorPosUtils.GetUIAnchor(RectTransform);

            if (RectTransform.rect.width < MinSize.x && thisAnchor[0] != UIAnchor.Stretch)
                RectTransform.sizeDelta = new Vector2(MinSize.x, RectTransform.sizeDelta.y);
            if (RectTransform.rect.height < MinSize.y && thisAnchor[1] != UIAnchor.Stretch)
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, MinSize.y);

            if (MaxSize.x > 0 && RectTransform.rect.width > MaxSize.x && thisAnchor[0] != UIAnchor.Stretch)
                RectTransform.sizeDelta = new Vector2(MaxSize.x, RectTransform.sizeDelta.y);
            if (MaxSize.y > 0 && RectTransform.rect.height > MaxSize.y && thisAnchor[1] != UIAnchor.Stretch)
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, MaxSize.y);

            if (thisAnchor[0] == UIAnchor.Stretch)
            {
                UIAnchorPosUtils.SetUILeftBottom(RectTransform, layout_marginLeft, UIAnchorPosUtils.GetUIBottom(RectTransform));
                UIAnchorPosUtils.SetUIRightTop(RectTransform, layout_marginRight, UIAnchorPosUtils.GetUITop(RectTransform));
            }
            if (thisAnchor[1] == UIAnchor.Stretch)
            {
                UIAnchorPosUtils.SetUILeftBottom(RectTransform, UIAnchorPosUtils.GetUILeft(RectTransform), layout_marginBottom);
                UIAnchorPosUtils.SetUIRightTop(RectTransform, UIAnchorPosUtils.GetUIRight(RectTransform), layout_marginTop);
            }
        }
    }
    [SLua.CustomLuaClass]
    /// <summary>
    /// 可见性
    /// </summary>
    public enum UIVisibility
    {
        Visible,
        Gone,
        InVisible,
    }
}
