using Ballance2.Managers.CoreBridge;
using Ballance2.Utils;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Ballance2.UI
{
    [SLua.CustomLuaClass]
    public class UICommonList : MonoBehaviour
    {
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            List.CollectionChanged += List_CollectionChanged;
        }
        private void OnDestroy()
        {
            List.CollectionChanged -= List_CollectionChanged;
        }
        private void Update()
        {
            if(needRelayout > 0)
            {
                needRelayout--;
                if (needRelayout == 0) Relayout();
            }
        }

        public RectTransform rectTransform;

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

        [Serializable, SLua.CustomLuaClass]
        public class CommonListItem
        {
            public GameObject itemObject;
            public RectTransform itemRectTransform;
            public int id;
            public object data;
        }
        [Serializable, SLua.CustomLuaClass]
        public class CommonList : ObservableCollection<CommonListItem>
        {
        }

        private CommonList list = new CommonList();
        [SerializeField, SetProperty("ItemPrefab")]
        private GameObject itemPrefab = null;

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

        public GameHandler ItemUpdateHandler { get; set; }
        public CommonList List
        {
            get
            {
                return list;
            }
        }

        public CommonListItem AddItem()
        {
            CommonListItem newItem = new CommonListItem();
            newItem.itemObject = GameCloneUtils.CloneNewObjectWithParent(ItemPrefab, rectTransform.transform);
            newItem.itemRectTransform = newItem.itemObject.GetComponent<RectTransform>();
            List.Add(newItem);
            needRelayout = 10;
            return newItem;
        }
        public CommonListItem GetItem(int index)
        {
            return List[index];
        }
        public CommonListItem GetItemById(int id)
        {
            foreach(CommonListItem li in list)
                if (li.id == id) return li;
            return null;
        }
        public void RemoveItem(int index)
        {
            List.RemoveAt(index);
        }
        public void RemoveItem(CommonListItem item)
        {
            List.Remove(item);
        }

        private int needRelayout = 0;

        public void Relayout()
        {
            float allHeight = 0;

            CommonListItem li = null;
            float startY = 0;
            for (int i = 0; i < List.Count; i++)
            {
                li = GetItem(i);
                li.itemRectTransform.anchoredPosition = new Vector2(li.itemRectTransform.anchoredPosition.x, -startY);
                startY += li.itemRectTransform.sizeDelta.y;
                allHeight += li.itemRectTransform.sizeDelta.y;
            }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, allHeight);
        }

    }
}
