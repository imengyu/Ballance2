using Ballance2.CoreBridge;
using Ballance2.UI.Utils;
using Ballance2.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Ballance2.UI
{
    /// <summary>
    /// 一个简单的列表组件
    /// </summary>
    [SLua.CustomLuaClass]
    public class UICommonList : MonoBehaviour
    {
        private void Start()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            List.CollectionChanged += List_CollectionChanged;
        }
        private void OnDestroy()
        {
            List.CollectionChanged -= List_CollectionChanged;
        }
        private void Update()
        {
            if(needRelayout >= 0)
            {
                needRelayout--;
                if (needRelayout == 0) Relayout();
            }
        }

        [Tooltip("设置列表对应的 RectTransform，可不写，默认使用当前 GameObject 的 RectTransform")]
        public RectTransform rectTransform;

        /// <summary>
        /// 列表的数据条目结构
        /// </summary>
        [Serializable, SLua.CustomLuaClass]
        public class CommonListItem
        {
            public GameObject itemObject;
            public RectTransform itemRectTransform;
            public int id;
            public bool selected;
            public bool visible;
            public object data;
            public Image backgroundImage;
            public Button backgroundImageButton;
        }
        /// <summary>
        /// 列表的数据集合
        /// </summary>
        [Serializable, SLua.CustomLuaClass]
        public class CommonList : ObservableCollection<CommonListItem>
        {
        }

        private CommonList list = new CommonList();
        [SerializeField, SetProperty("ItemPrefab")]
        [Tooltip("设置列表条目的模板 Prefab")]
        private GameObject itemPrefab = null;
        [SerializeField, SetProperty("ItemCanMultiSelect")]
        [Tooltip("设置列表是否可多选")]
        private bool itemCanMultiSelect = true;

        [Tooltip("设置列表条目背景")]
        public Sprite itemBackgroundNormal;
        [Tooltip("设置列表条目选中的背景")]
        public Sprite itemBackgroundSelected;
        [Tooltip("设置列表条目鼠标悬浮时的背景")]
        public Sprite itemBackgroundHover;
        [Tooltip("设置列表条目选中按下的背景")]
        public Sprite itemBackgroundPressed;

        /// <summary>
        /// 获取或设置列表条目的模板 Prefab
        /// </summary>
        public GameObject ItemPrefab
        {
            get
            {
                return itemPrefab;
            }
            set
            {
                itemPrefab = value;
            }
        }
        /// <summary>
        /// 获取或设置列表是否可多选
        /// </summary>
        public bool ItemCanMultiSelect
        {
            get
            {
                return itemCanMultiSelect;
            }
            set
            {
                itemCanMultiSelect = value;
            }
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (CommonListItem li in e.OldItems)
                    {
                        Destroy(li.itemObject);
                    }
                    needRelayout = 10;
                    break;
            }
        }
        private void OnItemClicked(CommonListItem item)
        {
            SetItemSelected(item, !item.selected);
        }


        /// <summary>
        /// 设置条目选中状态
        /// </summary>
        /// <param name="item">条目</param>
        /// <param name="selected">是否选中</param>
        public void SetItemSelected(CommonListItem item, bool selected)
        {
            item.selected = selected;
            if (item.selected)
            {
                if (itemCanMultiSelect)
                {
                    if (!currentSelectedItems.Contains(item))
                        currentSelectedItems.Add(item);
                }
                else
                {
                    currentSelectedItems.Clear();
                    currentSelectedItems.Add(item);
                }

                item.backgroundImage.sprite = itemBackgroundSelected;
            }
            else
            {
                currentSelectedItems.Remove(item);
                item.backgroundImage.sprite = itemBackgroundNormal;
            }
        }

        private List<CommonListItem> currentSelectedItems = new List<CommonListItem>();

        /// <summary>
        /// 获取当前选中的条目
        /// </summary>
        public List<CommonListItem> CurrentSelectedItems { get { return currentSelectedItems; } }
        /// <summary>
        /// 条目更新回调
        /// </summary>
        public GameHandler ItemUpdateHandler { get; set; }
        /// <summary>
        /// 条目列表list
        /// </summary>
        public CommonList List
        {
            get
            {
                return list;
            }
        }

        /// <summary>
        /// 添加条目
        /// </summary>
        /// <returns></returns>
        public CommonListItem AddItem()
        {
            CommonListItem newItem = new CommonListItem();
            newItem.itemObject = GameCloneUtils.CloneNewObjectWithParent(ItemPrefab, rectTransform.transform);
            newItem.itemRectTransform = newItem.itemObject.GetComponent<RectTransform>();
            newItem.backgroundImage = newItem.itemObject.AddComponent<Image>();
            newItem.backgroundImage.type = Image.Type.Sliced;
            newItem.backgroundImageButton = newItem.itemObject.AddComponent<Button>();
            newItem.backgroundImageButton.transition = Selectable.Transition.SpriteSwap;
            newItem.backgroundImage.sprite = itemBackgroundNormal;
            newItem.visible = true;
            newItem.selected = false;
            SpriteState spriteState = new SpriteState();
            spriteState.highlightedSprite = itemBackgroundHover;
            spriteState.selectedSprite = itemBackgroundSelected;
            spriteState.pressedSprite = itemBackgroundPressed;
            spriteState.disabledSprite = itemBackgroundPressed;

            newItem.backgroundImageButton.spriteState = spriteState;

            EventTriggerListener.Get(newItem.itemObject).onClick = (g) => OnItemClicked(newItem);
            List.Add(newItem);
            needRelayout = 10;
            return newItem;
        }
        /// <summary>
        /// 通过条目位置获取条目实例
        /// </summary>
        /// <param name="index">条目位置索引</param>
        /// <returns></returns>
        public CommonListItem GetItem(int index)
        {
            return List[index];
        }
        /// <summary>
        /// 通过ID获取条目实例
        /// </summary>
        /// <param name="id">条目ID</param>
        /// <returns></returns>
        public CommonListItem GetItemById(int id)
        {
            foreach(CommonListItem li in list)
                if (li.id == id) return li;
            return null;
        }
        /// <summary>
        /// 移除条目
        /// </summary>
        /// <param name="index">条目位置索引</param>
        public void RemoveItem(int index)
        {
            List.RemoveAt(index);
        }
        /// <summary>
        /// 移除条目
        /// </summary>
        /// <param name="item">条目实例</param>
        public void RemoveItem(CommonListItem item)
        {
            List.Remove(item);
        }

        private int needRelayout = 0;

        /// <summary>
        /// 强制重新布局
        /// </summary>
        public void Relayout()
        {
            float allHeight = 0;

            CommonListItem li = null;
            float startY = 0;
            for (int i = 0; i < List.Count; i++)
            {
                li = GetItem(i);
                if (li.visible)
                {
                    li.itemRectTransform.anchoredPosition = new Vector2(li.itemRectTransform.anchoredPosition.x, -startY);
                    startY += li.itemRectTransform.sizeDelta.y;
                    allHeight += li.itemRectTransform.sizeDelta.y;
                    if (!li.itemObject.activeSelf)
                        li.itemObject.SetActive(true);
                }
                else if(li.itemObject.activeSelf)
                    li.itemObject.SetActive(false);
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, allHeight);
        }

    }
}
