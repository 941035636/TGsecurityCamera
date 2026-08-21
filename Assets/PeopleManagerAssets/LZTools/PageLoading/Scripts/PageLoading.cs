using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTools
{
    [RequireComponent(typeof(PageLoadingModel))]
    public abstract class PageLoading : MonoBehaviour
    {
        /// <summary>
        /// 手动获取当前选择那页
        /// </summary>
        public int CrtPageNum { get; set; }
        public PageLoadingController pageLoadingController;
        [HideInInspector]
        public PageLoadingModel pageLoadingModel;
        public virtual void Start()
        {
            pageLoadingModel = this.GetComponent<PageLoadingModel>();
            //pageLoadingController = new PageLoadingController
            //{
            //    pageLoadingModel = this.pageLoadingModel,
            //    pageLoadingView = this
            //};
            pageLoadingModel.pageLoadingController = pageLoadingController;
            if (pageLoadingModel.awake2Init)
            {
                pageLoadingController.Init();
            }
            pageLoadingModel.Init();
         

        }



        /// <summary>
        /// 初始化分页加载
        /// </summary>
        public void Init()
        {
            pageLoadingController.Init();
        }
        /// <summary>
        /// 跳到对应的页面
        /// </summary>
        public void Jump2Page(int page)
        {
            pageLoadingController.Click2Page(page);
        }
        /// <summary>
        /// 通过网络请求获取当前页面数据
        /// </summary>
        /// <param name="indexPage">当前页码</param>
        /// <param name="pageNum">当前页请求数量</param>
        /// <param name="callBak">回调函数：（当前页数据对象list，总共有多少条数据）</param>
        public abstract void GetPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak);
        //查询获取当前页面数据
        public abstract void GetSearchPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak);
        /// <summary>
        /// 设置显示每一条数据
        /// </summary>
        /// <param name="item">要显示的内容对象</param>
        /// <param name="itemData">显示数据</param>
        /// <param name="pageNum">当前是第几条内容</param>
        public abstract void SetPageItemEvent(Transform item, object itemData, int itemIndex);
        /// <summary>
        /// 调用接口获取要更新的数据
        /// </summary>
        /// <param name="indexPage">当前页码</param>
        /// <param name="pageNum">当前页请求数量</param>
        /// <param name="fun">回调函数（当前页数据对象list，总共有多少条数据）</param>
        public void GetData(int indexPage, int pageNum, Action<List<object>, int> fun)
        {
            GetPageDataEvent(indexPage, pageNum, fun);
        }

        /// <summary>
        /// 调用查询接口获取数据
        /// </summary>
        public void GetSearchData(int indexPage, int pageNum, Action<List<object>, int> fun)
        {
            GetSearchPageDataEvent(indexPage, pageNum, fun);

        }



        /// <summary>
        /// 根据获得的对象设置应该显示的内容
        /// </summary>
        /// <param name="item"></param>
        /// <param name="data"></param>
        /// <param name="i"></param>
        public void SetItem(Transform item, object data, int i)
        {
            SetPageItemEvent(item, data, i);
        }
        /// <summary>
        /// 改变选择的页码
        /// </summary>
        public void ChangePageNum(int page)
        {
            CrtPageNum = page;
        }
        public virtual void OnDestroy()
        {
            pageLoadingController = null;
            GC.Collect();
        }
    }
}

