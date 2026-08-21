using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using static ManManagentUi;
using zFramework.Media;
//using WSX.HS;

namespace SuperTreeView
{

    public class ItemScript : MonoBehaviour
    {
        private TreeViewItem treeViewItem;
        [System.NonSerialized]
        public string id;
        [System.NonSerialized]
        public string level;
        [System.NonSerialized]
        public string parentId;
        public Button expandBtn;
        public Image icon;
        public Image selectImg;
        public Button clickBtn;
        public InputField labelText;
        string mData = "";
        public int type;//新添加判断传入的是监控还是门禁
        public List<GetAreaGroupDevs> children = new List<GetAreaGroupDevs>();  //特意加
        public List<NVRInformation> devList = new List<NVRInformation>();  //特意加
        public NVRInformation nvr;
        public string Data
        {
            get
            {
                return mData;
            }
            set
            {
                mData = value;
            }
        }
        public Toggle choiceTog;

        private void Awake()
        {
            treeViewItem = GetComponent<TreeViewItem>();
        }

        void Start()
        {
            expandBtn.onClick.AddListener(OnExpandBtnClicked);
            clickBtn.onClick.AddListener(OnItemClicked);
        }

        public void Init()
        {
            SetExpandBtnVisible(false);
            SetExpandStatus(true);
            IsSelected = false;
        }

        void OnExpandBtnClicked()
        {
            treeViewItem.DoExpandOrCollapse();
        }

        public void SetItemInfo(string iconSpriteName, string labelTxt, string data = "")
        {
            Init();
            //mIcon.sprite = ResManager.Instance.GetSpriteByName(iconSpriteName);
            labelText.text = labelTxt;
            mData = data;

        }

        public void SetItem(ItemScript item)
        {
            Init();
            //mIcon.sprite = ResManager.Instance.GetSpriteByName(iconSpriteName);
            labelText.text = item.labelText.text ;

        }


        void OnItemClicked()
        {
            treeViewItem.RaiseCustomEvent(CustomEvent.ItemClicked, null);
             Log.Debug("TreeViewItem Clicked " + Data);
            
        }
        

        public void SetExpandBtnVisible(bool visible)
        {
            if (expandBtn.gameObject.activeSelf != visible)
            {
                expandBtn.gameObject.SetActive(visible);
            }
        }

        public bool IsSelected
        {
            get
            {
                return selectImg.gameObject.activeSelf;
            }
            set
            {
                if (selectImg.gameObject.activeSelf != value)
                {
                    selectImg.gameObject.SetActive(value);
                }
            }
        }

        public void ResetForPool(Sprite defaultIcon)
        {
            if (choiceTog != null)
            {
                choiceTog.onValueChanged.RemoveAllListeners();
                choiceTog.isOn = false;
            }
            id = null;
            level = null;
            parentId = null;
            type = 0;
            children = null;
            devList = null;
            nvr = default(NVRInformation);
            mData = string.Empty;
            labelText.text = string.Empty;
            icon.sprite = defaultIcon;
            IsSelected = false;
        }
        public void SetExpandStatus(bool expand)
        {
            if (expand)
            {
                expandBtn.transform.localEulerAngles = new Vector3(0, 0, -90);
            }
            else
            {
                expandBtn.transform.localEulerAngles = new Vector3(0, 0, 0);

            }
        }


    }

}
