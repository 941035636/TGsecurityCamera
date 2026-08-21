using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace ZTools
{
    [RequireComponent(typeof(Image), typeof(Button))]
    public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("按钮下text")]
        [SerializeField]
        private Text tabTxt;
        [Header("给按钮设置标签")]
        public string tabName;
        [SerializeField]
        [Header("是否选中")]
        private bool isOn = false;
        [SerializeField]
        [Header("是否可以多选")]
        private bool multipleChoice = false;
        [Header("改变图片显示状态")]
        [Header("---------------鼠标移入状态设置----------------")]
        [SerializeField]
        private bool isHoverImgActive;
        [SerializeField]
        private GameObject hoverImg;
        [Header("改变图片显示颜色")]
        [SerializeField]
        private bool isHoverImgColor;
        [Header("改变文字显示颜色")]
        [SerializeField]
        private bool isHoverTxtColor;
        [SerializeField]
        [Header("改变图片显示状态")]
        [Header("---------------选中时状态设置----------------")]
        private bool isOnImgActive = false;
        [SerializeField]
        [Header("改变文字显示颜色")]
        private bool isOnTxtColor = false;
        [SerializeField]
        [Header("改变图片显示颜色")]
        private bool isOnImgColor = false;
        [Header("父级控制器，可手动配置")]
        [Header("---------------------------------------------")]
        [SerializeField]
        private TabController tabcontroller = null;
        [SerializeField]
        private Image tabImg;
        [SerializeField]
        private Image tabChangeColorImg;
        [SerializeField]
        private Image hoverImage;
        [SerializeField]
        private Color changeTxtColor;
        [SerializeField]
        private Color hoverTxtColor;
        [SerializeField]
        private Color imgColor;
        [SerializeField]
        private Color hoverImageColor;
        [SerializeField]
        private List<GameObject> showPanel;

        private Button btnTab;
        public TabController tabController
        {
            set
            {
                tabcontroller = value;
                tabController.AddTab(this);
            }
            get { return tabcontroller; }
        }
        private Color origTxtColor;
        private Color origImgColor;
        private Color origHoverImgColor;
        /// <summary>
        /// 点击事件，选中状态再次点击有效
        /// </summary>
        public event Action<Tab> TabClick;
        /// <summary>
        /// 点击事件，选中状态再次点击无效
        /// </summary>
        public event Action<Tab> TabClickFirstEffect;
        private bool isInit = false;
        private int hash;
        public string text
        {
            get
            {
                if (null != tabTxt)
                {
                    return tabTxt.text;
                }
                else
                {
                    Log.Debug("大佬您按钮下可没有text哦，赶快加一个吧/(ㄒoㄒ)/~~");
                    return "大佬您按钮下可没有text哦，赶快加一个吧/(ㄒoㄒ)/~~";
                }
            }
        }
        public bool IsOn
        {
            set
            {
                if (isOn != value && value)
                    if (TabClickFirstEffect != null)
                        TabClickFirstEffect.Invoke(this);
                ChangeType(value);
                if (isOn)
                    if (TabClick != null)
                        TabClick.Invoke(this);
            }
            get { return isOn; }
        }
        public bool IsOnWithOutEvent
        {
            set
            {
                ChangeType(value);
            }
        }

        void Awake()
        {
            btnTab = this.GetComponent<Button>();
            btnTab.transition = Selectable.Transition.None;
            if (null == tabTxt)
            {
                tabTxt = transform.Find("Text").GetComponent<Text>();
            }
            hash = this.GetHashCode();
            if (!isInit)
            {
                InitData();
            }
            if (isOn)
            {
                isOn = false;
                IsOn = true;
            }
            btnTab.onClick.AddListener(() =>
            {
                IsOn = (!multipleChoice) ? true : !IsOn;
            });
        }
        /// <summary>
        /// 保存当前状态
        /// </summary>
        void InitData()
        {
            isInit = true;
            if (isHoverTxtColor || isOnTxtColor)
            {
                if (null != tabTxt)
                {
                    origTxtColor = tabTxt.color;
                }
                else
                {
                    Log.Debug("tabTxt is null");
                }
            }
            if (isOnImgColor)
            {
                if (null != tabChangeColorImg)
                {
                    origImgColor = tabChangeColorImg.color;
                }
                else
                {
                    Log.Debug("tabChangeColorImg is null");
                }
            }
            if (isHoverImgColor)
            {
                if (null != hoverImage)
                {
                    origHoverImgColor = hoverImage.color;
                }
                else
                {
                    Log.Debug("hoverImage is null");
                }
            }
            if (!multipleChoice)
            {
                if (null == tabController)
                {
                    tabController = (this.transform.parent.GetComponent<TabController>() == null) ? this.transform.parent.gameObject.AddComponent<TabController>() : this.transform.parent.GetComponent<TabController>();
                }
                else
                {
                    tabController.AddTab(this);
                }
            }
        }
        /// <summary>
        /// 改变选择的状态
        /// </summary>
        /// <param name="value"></param>
        void ChangeType(bool value)
        {
            if (isOn == value) return;
            isOn = value;
            if (isHoverImgActive)
            {
                hoverImg.gameObject.SetActive(false);
            }
            if (isHoverImgColor)
            {
                hoverImage.color = origHoverImgColor;
            }
            if (isHoverTxtColor)
            {
                tabTxt.color = origTxtColor;
            }
            if (isOnTxtColor)
            {
                tabTxt.color = (value) ? changeTxtColor : origTxtColor;
            }
            if (isOnImgActive)
            {
                tabImg.gameObject.SetActive(value);
            }
            if (isOnImgColor)
            {
                tabChangeColorImg.color = (value) ? imgColor : origImgColor;
            }
            if (showPanel.Count > 0)
            {
                showPanel.ForEach(item =>
                {
                    if (item != null)
                        item.SetActive(value);
                });
            }
            if (value && !multipleChoice)
            {
                tabController.ChoiceTab(hash);
            }
        }
        /// <summary>
        /// 监听改变的状态
        /// </summary>
        /// <param name="txtName"></param>
        public void ChangeTab(int tabHash)
        {
            if (tabHash != hash)
                IsOn = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isHoverImgActive && !isOn)
            {
                hoverImg.SetActive(true);
            }
            if (isHoverTxtColor && !isOn)
            {
                tabTxt.color = hoverTxtColor;
            }
            if (isHoverImgColor && !isOn)
            {
                hoverImage.color = hoverImageColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isHoverImgActive && !isOn)
            {
                hoverImg.SetActive(false);
            }
            if (isHoverTxtColor && !isOn)
            {
                tabTxt.color = origTxtColor;
            }
            if (isHoverImgColor && !isOn)
            {
                hoverImage.color = origHoverImgColor;
            }
        }
        private void OnDestroy()
        {
            if (tabController != null)
            {
                tabController.RemoveTab(this);
            }
        }
    }
}
