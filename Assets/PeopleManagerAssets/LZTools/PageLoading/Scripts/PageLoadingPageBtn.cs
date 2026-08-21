using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ZTools
{
    public class PageLoadingPageBtn : MonoBehaviour
    {
        [HideInInspector]
        public PageLoadingController prtObj;
        // 从1开始
        public int pageIndex;
        public Text txt;
        public int PageIndex
        {
            set
            {
                txt.text = value.ToString();
                pageIndex = value;
            }
            get
            {
                return pageIndex;
            }
        }

        private Tab tab;
        private Button btnPage;

        private void Start()
        {
            btnPage = this.GetComponent<Button>();
            tab = this.GetComponent<Tab>();
            btnPage.onClick.AddListener(() =>
            { 
                prtObj.Click2Page(pageIndex);
            });
        }
        /// <summary>
        /// 改变当前按钮的选择状态
        /// </summary>
        /// <param name="indexPage"></param>
        public void ChangeChoiceType(int indexPage)
        {
            if (tab == null)
            {
                tab = this.GetComponent<Tab>();
            }
            tab.IsOn = (pageIndex == indexPage);
        }
    }
}
