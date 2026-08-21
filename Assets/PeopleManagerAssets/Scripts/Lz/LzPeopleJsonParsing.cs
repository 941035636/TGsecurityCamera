using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
public class LzPeopleJsonParsing :MonoSingleton<LzPeopleJsonParsing> 
{

    string RequestInfoUrl = @"http://"+GameStart.IP+"/api/personnel/tg/user/client?";

    public GameObject UserContent;
    public GameObject PagingLoad;
    string jsonPath = string.Empty;
    //UserGroup userGroup = new UserGroup();
    [SerializeField]//必须要加
    public List<object> ShowTenPeople = new List<object>();
    public int PeopleTotalCount;

 

    void Start()
    {
    
    }
 
 


    /// <summary>
    /// 分页请求信息
    /// </summary>
    /// <param name="idNum">传值的就是单个请求，不传代表请求所有，分页请求不用传   </param>
    /// <param name="PageNum"> 请求第几页</param>
    /// <param name="PageSize">一页多少条数据</param>
    public void HttpPageGetData(string idNum, int PageNum, int PageSize, int Type,int issued, Action<List<object>, int> callBak)
    {
        string param = "idNum=" + idNum + "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&type=" + Type.ToString()+ "&issued="+ issued.ToString();

        Log.Error("请求人员信息:"+ RequestInfoUrl + param);
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestInfoUrl + param, PagehttpCallback, callBak, false, false, true);
        PageLoadingController.IsSearch = false;
    }


    /// <summary>
    /// http查询人员信息----------------------------------------------------------------------------------------------------------------
    /// </summary>
    public void HttpPageSearchGetData(string idNum, int pageNum, int PageSize, int type, Action<List<object>, int> callback, string Username = null)
    {
        string param = "pageNum=" + pageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&userName=" + Username + "&idNum=" + idNum + "&type=" + type;
        HttpNetManagerPeople.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/user/client/info?" + param, PagehttpCallback, callback, false, false, true);
        Log.Debug("请求查询的人员信息：" + "http://" + GameStart.IP + "/api/personnel/tg/user/client/info?" + param);

    }


    //分页Http请求get回调
    public void PagehttpCallback(HttpCallBackArgsPeople args)
    {

        print("Http收到服务器信息:" + args.Value);
        ShowTenPeople.Clear();

        if (!string.IsNullOrEmpty(args.Value))
        {
            HttpPageDeserialization(args.Value);
            List<object> itemList = LzPeopleJsonParsing.Instance.ShowTenPeople;
            args.callBak(itemList, LzPeopleJsonParsing.Instance.PeopleTotalCount);
            print(LzPeopleJsonParsing.Instance.PeopleTotalCount);
            EventCenter.BroadCast(Eventdefine.OnDataUpdate);
        }

    }

    /// <summary>
    /// 反序列化分页请求返回的Json
    /// </summary>
    public void HttpPageDeserialization(string ReceiveStr)
    {
    

        try
        {

            UserInfoArrPeople userInfoarr = JsonUtility.FromJson<UserInfoArrPeople>(ReceiveStr);
            PeopleController.Ins.NumPage.text = userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString();
            PeopleTotalCount = userInfoarr.total;
            for (int i = 0; i < userInfoarr.records.Count; i++)
            {

                ShowTenPeople.Add(userInfoarr.records[i]);
                //for (int j = 0; j < userInfoarr.records[i].auth.Count; j++)
                //{

                //    switch (userInfoarr.records[i].auth[j].type)
                //    {
                //        case (int)Usertype.employee:
                //            userGroup.employeelist.Add(userInfoarr.records[i]);
                //            break;
                //        case (int)Usertype.visitor:
                //            userGroup.visitorlist.Add(userInfoarr.records[i]);

                //            break;
                //        case (int)Usertype.family_mem:
                //            userGroup.family_memlist.Add(userInfoarr.records[i]);

                //            break;
                //        case (int)Usertype.retirees:
                //            userGroup.retireeslist.Add(userInfoarr.records[i]);

                //            break;
                //        case (int)Usertype.outside_construction_workers:
                //            userGroup.construction_workerslist.Add(userInfoarr.records[i]);

                //            break;
                //        case (int)Usertype.outside_contractor_pers:
                //            userGroup.contractor_perslist.Add(userInfoarr.records[i]);

                //            break;
                //        case (int)Usertype.spec_approved_pers:
                //            userGroup.spec_approved_perslist.Add(userInfoarr.records[i]);

                //            break;
                //        default:
                //            break;
                //    }

                //}

            }
        }
        catch (Exception e)
        {
            Log.Debug(e.Message);
        }


    }
}
