using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ZTools
{
    public class PageLoadingModel : MonoBehaviour
    {
        [Header("是否自动初始化")]
        public bool awake2Init = true;
        [Header("是否每一页都需要重新请求")]
        public bool everyPageRequest = false;
        [Header("是否需要输入跳转")]
        public bool input2Page = false;
        [Header("一页多少条内容")]
        public int pageItemNum;
        [Header("一次显示几页")]
        public int pageBtnNum;
        [Header("需要加载的item")]
        public Transform pageItemObj;
        [Header("翻页按钮预制体")]
        public GameObject pageBtnObj;
        [Header("-----------------------------------------")]
        public Button btnFirstPage;
        public Button btnAllPage;
        public Button btnLeft;
        public Button btnRight;
        public Button btnPageLeft;
        public Button btnPageRight;
        public Transform content;
        public Transform pageContent;
        public InputField inputObj;
        public Button btnSearch;
        public Text allPageTxt;
        public Button SearchDoorEventBtn;
        public Button SearchCameraEventBtn;
        public Button SearchPeopleBtn;
        public Button SearchVisitorBtn;
        public Button SearchSXnormalBtn;
        public string AllPageTxt
        {
            set
            {
                allPageTxt.text = value;
            }
        }
        [HideInInspector]
        public PageLoadingController pageLoadingController;
        /// <summary>
        /// model初始化
        /// </summary>
        public void Init()
        {
            if (input2Page)
            {
                ShowSearchObj();
            }
        }
        /// <summary>
        /// 删除内容列表
        /// </summary>
        public void DestroyContentChild()
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
        /// <summary>
        /// 删除翻页按钮列表
        /// </summary>
        public void DestroyBtnContentChild()
        {
            for (int i = 0; i < pageContent.childCount; i++)
            {
                // Log.Debug(pageContent.GetChild(i).gameObject.name);
                Destroy(pageContent.GetChild(i).gameObject);
            }
        }
        /// <summary>
        /// 添加一个按钮对象
        /// </summary>
        /// <returns></returns>
        public PageLoadingPageBtn AddBtnObj()
        {
            return Instantiate(pageBtnObj, pageContent).GetComponent<PageLoadingPageBtn>();
        }
        /// <summary>
        /// 添加一个显示对象
        /// </summary>
        /// <returns></returns>
        public Transform AddItemObj()
        {
            return Instantiate(pageItemObj, content);
        }
        /// <summary>
        /// 显示可搜索定位功能
        /// </summary>
        public void ShowSearchObj()
        {
            inputObj.gameObject.SetActive(true);
            btnSearch.gameObject.SetActive(true);
            btnSearch.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(inputObj.text)) return;
                pageLoadingController.Click2Page(int.Parse(inputObj.text));
                inputObj.text = string.Empty;
            });
        }
    }
}
