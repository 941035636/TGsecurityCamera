using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace ZTools
{
    public class PageLoadingController : SingletonManager<PageLoadingController>
    {
        public PageLoadingModel pageLoadingModel;
        public PageLoading pageLoadingView;
        /// <summary>
        /// 记录每一页的信息
        /// </summary>
        private Dictionary<int, List<object>> pageItemDic = new Dictionary<int, List<object>>();
        /// <summary>
        /// 存放当前按钮列表
        /// </summary>
        private List<PageLoadingPageBtn> pageBtnList = new List<PageLoadingPageBtn>();
        private int startPage;
        private int indexPage;
        private int allPage;
        private bool isLoading = false;
        private int allItemNum = 0;
        public bool isInit = false;
        int maxStartPageNum = 0;

        public static bool IsSearch = false;


        /// <summary>
        /// 初始化数据
        /// </summary>
        public void Init()
        {

            startPage = 1;
            indexPage = 1;
            allItemNum = 0;
            maxStartPageNum = 0;
            isLoading = false;
            isInit = false;
            pageItemDic.Clear();
            pageBtnList.Clear();
            pageLoadingModel.DestroyContentChild();
            pageLoadingModel.DestroyBtnContentChild();
            allPage = 0;
            if (IsSearch)
                updateSearchData();
            else
                UpdateData();
        }
        /// <summary>
        /// 显示列表，每次点击都需要调用
        /// </summary>
        void UpdateData()
        {
            GameStart.Instance.ShowSearchTxt("正在加载，请稍后...");
            Log.Error("正在加载，请稍后...");
            if (isLoading)
            {

                return;
            }

            if (!pageItemDic.ContainsKey(indexPage) || pageLoadingModel.everyPageRequest)
            {
                if (pageItemDic.ContainsKey(indexPage))
                {
                    pageItemDic[indexPage].Clear();
                    pageItemDic.Remove(indexPage);
                }
                isLoading = true;
                pageLoadingView.GetData(indexPage, pageLoadingModel.pageItemNum, (List<object> dataList, int count) =>
                {
                    AddDataList(indexPage, dataList, count);
                });
            }
            else
            {
                AddDataList(indexPage, pageItemDic[indexPage], allItemNum);
            }
        }


        /// <summary>
        /// 点击查询时显示列表
        /// </summary>
        void updateSearchData()
        {

            Log.Debug("开始查询:" + Time.realtimeSinceStartup);
            if (isLoading)
            {

                GameStart.Instance.ShowSearchTxt("正在查询，请稍后...");
                return;
            }

            if (!pageItemDic.ContainsKey(indexPage) || pageLoadingModel.everyPageRequest)
            {
                if (pageItemDic.ContainsKey(indexPage))
                {
                    pageItemDic[indexPage].Clear();
                    pageItemDic.Remove(indexPage);
                }
                isLoading = true;
                pageLoadingView.GetSearchData(indexPage, pageLoadingModel.pageItemNum, (List<object> dataList, int count) =>
                {
                    AddDataList(indexPage, dataList, count);
                });
            }
            else
            {
                AddDataList(indexPage, pageItemDic[indexPage], allItemNum);
            }


        }


        /// <summary>
        /// 更新页面内容
        /// </summary>
        /// <param name="iPage">当前页面</param>
        /// <param name="pageDataList">该页显示内容列表</param>
        /// <param name="totalList">总条数</param>
        void AddDataList(int iPage, List<object> pageDataList, int totalList)
        {
            Log.Error("更新页面内容:" + isInit + "大小：" + totalList);
            if (totalList == 0)
            {


                GameStart.Instance.ShowSearchTxt("信息不存在!");
                allPage = 0;
                UnityTimer.Timer.Register(1, () =>
                {

                    GameStart.Instance.DestorySearchTxt();
                });

            }
            else
            {
                GameStart.Instance.DestorySearchTxt();
            }
            //if (IsSearch&&pageDataList.Count == 13&&totalList>13)
            //{
            //    //注释
            //    IsSearch = false;
            //}


            if (!pageItemDic.ContainsKey(iPage))
            {
                Log.Error("pageDataList.count:" + pageDataList.Count);
                pageItemDic.Add(iPage, pageDataList);
            }
            // 初始化数据
            if (!isInit)
            {
                allItemNum = totalList;
                AddPageBtn();
                InitEvent();
                isInit = true;
            }
            UpdatePageBtnShowType();
            UpdatePageBtnChoiceType();
            UpdatePageItemList();
            // 加载结束
            isLoading = false;
            Debug.Log("查询结束：" + Time.realtimeSinceStartup);
        }
        /// <summary>
        /// 初始化底部按钮状态
        /// </summary>
        void AddPageBtn()
        {
            if (pageLoadingModel.pageItemNum != 0 && pageLoadingModel.pageBtnNum != 0)
            {
                Log.Error("总数:" + allItemNum + " 每页数量：" + pageLoadingModel.pageItemNum);
                if (allItemNum % pageLoadingModel.pageItemNum == 0) 
                {
                    allPage = allItemNum / pageLoadingModel.pageItemNum;
                }
                else
                allPage = (int)Mathf.Ceil(allItemNum / pageLoadingModel.pageItemNum) + 1;
                allPage = allPage == 0 ? 1 : allPage;
                Log.Debug("Allpage:" + allPage);
                maxStartPageNum = allPage < pageLoadingModel.pageBtnNum ? 1 : allPage - pageLoadingModel.pageBtnNum + 1;
                pageLoadingModel.AllPageTxt = allPage.ToString();
                float instAllPage = (allPage > pageLoadingModel.pageBtnNum) ? pageLoadingModel.pageBtnNum : allPage;
                for (int i = 0; i < instAllPage; i++)
                {
                    PageLoadingPageBtn pageBtn = pageLoadingModel.AddBtnObj();
                    pageBtn.prtObj = this;
                    pageBtnList.Add(pageBtn);
                }
            }
        }


        //门禁事件搜索按钮查询监听
        public void SearchClick()
        {
            if (!isLoading)
            {
                indexPage = 1;
                startPage = 1;
                IsSearch = true;
                Init();

                //updateSearchData();
            }

        }

        //查询监控事件
        public void SearchCameraClick()
        {
            if (!isLoading)
            {
                indexPage = 1;
                startPage = 1;
                IsSearch = true;
                Init();

                //updateSearchData();
            }

        }


        //查询人
        public void SearchPeople()
        {
            if (!isLoading)
            {
                indexPage = 1;
                startPage = 1;
                IsSearch = true;
                Init();

                //updateSearchData();

            }
            Log.Debug("查询人员信息");
        }


        /// <summary>
        /// 初始化点击绑定事件
        /// </summary>
        void InitEvent()
        {

            if (pageLoadingModel.btnLeft != null)
            {
                pageLoadingModel.btnLeft.onClick.RemoveAllListeners();
                pageLoadingModel.btnLeft.onClick.AddListener(LeftClick);
                Log.Debug("左翻页添加监听:");
            }

            if (pageLoadingModel.btnRight != null)
            {
                pageLoadingModel.btnRight.onClick.RemoveAllListeners();
                pageLoadingModel.btnRight.onClick.AddListener(RightClick);

            }


            if (pageLoadingModel.btnPageLeft != null)
                pageLoadingModel.btnPageLeft.onClick.RemoveAllListeners();
            pageLoadingModel.btnPageLeft.onClick.AddListener(BtnPageLeft);
            if (pageLoadingModel.btnPageRight != null)
            {
                pageLoadingModel.btnPageRight.onClick.RemoveAllListeners();
                pageLoadingModel.btnPageRight.onClick.AddListener(BtnPageRight);

            }

            if (pageLoadingModel.btnAllPage != null)
                pageLoadingModel.btnAllPage.onClick.RemoveAllListeners();
            pageLoadingModel.btnAllPage.onClick.AddListener(AllPage);
            if (pageLoadingModel.btnFirstPage != null)
                pageLoadingModel.btnFirstPage.onClick.RemoveAllListeners();
            pageLoadingModel.btnFirstPage.onClick.AddListener(FirstPage);



            if (pageLoadingModel.SearchDoorEventBtn != null)
            {
                pageLoadingModel.SearchDoorEventBtn.onClick.RemoveAllListeners();
                pageLoadingModel.SearchDoorEventBtn.onClick.AddListener(() =>
                {
                    if (LoginManager.Ins.IsDoorEvent)
                    {
                        if (!string.IsNullOrEmpty(DoorEventJsonParse.Doorusername) || !string.IsNullOrEmpty(DoorEventJsonParse.DoorcardNum) || !string.IsNullOrEmpty(DoorEventJsonParse.Dooreventype) || !string.IsNullOrEmpty(DoorEventJsonParse.DoorName)||!string.IsNullOrEmpty(ZCalendarEvent.EventSearchStartTime)|| !string.IsNullOrEmpty(ZCalendarEvent.EventSearchEndTime))
                            SearchClick();
                        else
                            GameStart.Instance.ShowTip("请输入查询条件!");
                    }
                    else
                    {
                        GameStart.Instance.ShowTip("当前用户没有门禁事件查询权限！");
                    }

                });
            }



            if (pageLoadingModel.SearchCameraEventBtn != null)
            {
                pageLoadingModel.SearchCameraEventBtn.onClick.RemoveAllListeners();
                pageLoadingModel.SearchCameraEventBtn.onClick.AddListener(() =>
                {

                    if (LoginManager.Ins.IsCameraEvent)
                    {
                        if (!string.IsNullOrEmpty(CameraEventJsonParse.Camerausername) || !string.IsNullOrEmpty(CameraEventJsonParse.CameracardNum) || !string.IsNullOrEmpty(CameraEventJsonParse.Cameraeventype) || !string.IsNullOrEmpty(CameraEventJsonParse.CameraName))
                            SearchCameraClick();
                        else
                            GameStart.Instance.ShowTip("请输入查询条件!");
                    }
                    else
                    {
                        GameStart.Instance.ShowTip("当前用户没有监控事件查询权限！");
                    }


                });
            }






            if (pageLoadingModel.SearchPeopleBtn != null)
            {
                pageLoadingModel.SearchPeopleBtn.onClick.RemoveAllListeners();
                pageLoadingModel.SearchPeopleBtn.onClick.AddListener(() =>
                {
                    if (!string.IsNullOrEmpty(PeopleController.cardNum) || !string.IsNullOrEmpty(PeopleController.userName))
                        SearchPeople();
                    else
                        GameStart.Instance.ShowTip("请输入查询条件!");
                });
            }
           


            if (pageLoadingModel.SearchVisitorBtn != null)
            {
                pageLoadingModel.SearchVisitorBtn.onClick.RemoveAllListeners();
                pageLoadingModel.SearchVisitorBtn.onClick.AddListener(() =>
                {
                    Log.Error("visitor search");
                    if (!string.IsNullOrEmpty(VistorController.cardNum) || !string.IsNullOrEmpty(VistorController.userName))
                        SearchPeople();
                    else
                        GameStart.Instance.ShowTip("请输入查询条件!");
                });
            }
 
            if (pageLoadingModel.SearchSXnormalBtn != null)
            {
                pageLoadingModel.SearchSXnormalBtn.onClick.RemoveAllListeners();
                pageLoadingModel.SearchSXnormalBtn.onClick.AddListener(() =>
                {
                    Log.Error("SXnormal search");
                    if (!string.IsNullOrEmpty(SXNormalJsonParsing.cardNum) || !string.IsNullOrEmpty(SXNormalJsonParsing.userName) || SXNormalJsonParsing.devId!=0)
                        SearchPeople();
                    else
                        GameStart.Instance.ShowTip("请输入查询条件!");
                });
            }

        }
        /// <summary>
        /// 点击向左翻页按钮
        /// </summary>
        public void LeftClick()
        {
            Log.Debug("向左翻页:" + !isLoading);
            if (!isLoading)
            {
                Log.Debug("indexPage:" + indexPage + "startPage:" + startPage);
                indexPage--;
                if (indexPage < startPage)
                {
                    startPage--;
                }
                Log.Debug("indexPage:" + indexPage + "startPage:" + startPage);
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 点击向右翻页按钮
        /// </summary>
        public void RightClick()
        {
            Log.Debug("又翻页");
            if (!isLoading)
            {
                indexPage++;
                if (indexPage > pageLoadingModel.pageBtnNum && startPage + pageLoadingModel.pageBtnNum - 1 < allPage)
                {
                    startPage++;
                }
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 点击向左大翻页按钮
        /// </summary>
        public void BtnPageLeft()
        {
            Log.Debug("向左大翻页");
            if (!isLoading)
            {
                startPage -= pageLoadingModel.pageBtnNum;
                indexPage -= pageLoadingModel.pageBtnNum;
                if (startPage < 1)
                {
                    indexPage += (1 - startPage);
                    startPage = 1;
                }
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 点击向右大翻页按钮
        /// </summary>
        public void BtnPageRight()
        {
            Log.Debug("向右大翻页");
            if (!isLoading)
            {
                startPage += pageLoadingModel.pageBtnNum;
                indexPage += pageLoadingModel.pageBtnNum;
                int lastPage = allPage - pageLoadingModel.pageBtnNum + 1;
                if (startPage > lastPage)
                {
                    indexPage -= (startPage - lastPage);
                    startPage = lastPage;
                }
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 点击首页
        /// </summary>
        public void FirstPage()
        {
            if (!isLoading)
            {
                indexPage = 1;
                startPage = 1;
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 点击尾页
        /// </summary>
        public void AllPage()
        {
            if (!isLoading)
            {
                indexPage = allPage;
                startPage = (allPage > pageLoadingModel.pageBtnNum) ? allPage - pageLoadingModel.pageBtnNum + 1 : 1;
                if (IsSearch)
                    updateSearchData();
                else
                    UpdateData();
            }
        }
        /// <summary>
        /// 翻页时，清空当前显示内容
        /// </summary>
        void UpdatePageItemList()
        {

            pageLoadingModel.DestroyContentChild();
            Log.Error("index:" + indexPage + " pageItemDic[indexPage].Count:" + pageItemDic[indexPage].Count);
            for (int i = 0; i < pageItemDic[indexPage].Count; i++)
            {
                Transform item = pageLoadingModel.AddItemObj();
                pageLoadingView.SetItem(item, pageItemDic[indexPage][i], i + 1);

            }
        }
        /// <summary>
        /// 更新按钮显示状态及内容
        /// </summary>
        void UpdatePageBtnShowType()
        {
            pageLoadingView.ChangePageNum(indexPage);
            if (pageBtnList[0].PageIndex != startPage)
            {
                for (int i = 0; i < pageBtnList.Count; i++)
                {
                    pageBtnList[i].PageIndex = startPage + i;
                }
            }
            if (allPage > pageLoadingModel.pageBtnNum)
            {
                pageLoadingModel.btnPageLeft.interactable = pageBtnList[0].PageIndex != 1;
                pageLoadingModel.btnPageRight.interactable = pageBtnList[pageBtnList.Count - 1].PageIndex != allPage;
            }
            else
            {
                pageLoadingModel.btnPageLeft.interactable = false;
                pageLoadingModel.btnPageRight.interactable = false;
            }
            pageLoadingModel.btnRight.interactable = (indexPage != allPage);
            pageLoadingModel.btnLeft.interactable = (indexPage != 1);
        }
        /// <summary>
        /// 更新页码按钮显示状态
        /// </summary>
        void UpdatePageBtnChoiceType()
        {
            if (pageLoadingModel.pageBtnNum > 0)
            {
                for (int i = 0; i < pageBtnList.Count; i++)
                {
                    pageBtnList[i].ChangeChoiceType(indexPage);
                }
            }
            else
            {
                Log.Debug("最少显示页数不能为0");
            }
        }
        /// <summary>
        /// 点击翻页按钮后，对应跳到相应页面
        /// </summary>
        /// <param name="page"></param>
        public void Click2Page(int page)
        {
            if (page < 1 || page > allPage) return;
            if (allPage > pageLoadingModel.pageBtnNum && (page > startPage + pageLoadingModel.pageBtnNum - 1 || page < startPage))
            {
                startPage = GetStartPage(page);
            }
            indexPage = page;
            if (IsSearch)
                updateSearchData();
            else
                UpdateData();
        }
        /// <summary>
        /// 设置起始页
        /// </summary>
        int GetStartPage(int num)
        {
            return num > maxStartPageNum ? maxStartPageNum : num;
        }
    }
}
