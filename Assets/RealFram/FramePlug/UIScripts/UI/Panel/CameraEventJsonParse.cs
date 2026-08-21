using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;

public class CameraEventJsonParse : MonoSingleton<CameraEventJsonParse>
{
    string RequestAlthriomUrl = @"http://"+GameStart.IP+"/api/alarm/info?";

    public GameObject UserContent;
    public GameObject PagingLoad;
    string jsonPath = string.Empty;
    UserGroup userGroup = new UserGroup();
    [SerializeField]//必须要加
    public List<object> ShowTenPeople = new List<object>();
    public int PeopleTotalCount;
    public Text PagesTxt;
    private static CameraEventJsonParse Instanc;
    public static CameraEventJsonParse GetInstance()
    {
        if (Instanc==null)
        {
            Instanc = new CameraEventJsonParse();
        }
        return Instanc;

    }


    public EventCenterPanel eventCenterPanel;
    public static string Camerausername;
    public static string CameracardNum;
    public static string Cameraeventype;
    public static string CameraName;

    private void OnEnable()
    {
        eventCenterPanel._CameratypeDrop.onValueChanged.AddListener((value) => {



            if (value != 0)
            {
                string eventname = TypeDropdownItemChanged(value);
                switch (eventname)
                {
                    case "人脸识别":
                        Cameraeventype = "face_rec";
                        break;
                    case "车牌识别":
                        Cameraeventype = "car_plate";
                        break;
                    case "人员闯入":
                        Cameraeventype = "person_brust";
                        break;
                    case "人员聚集":
                        Cameraeventype = "person_group";
                        break;
                    case "人员摔倒":
                        Cameraeventype = "person_fall";
                        break;
                    case "火灾检测":
                        Cameraeventype = "fire_smoke";
                        break;
                    default:
                        break;
                }


            }
            else
            {
                Cameraeventype = "";
            }


        });
        eventCenterPanel._CameratypeDrop.onValueChanged.Invoke(0);
        eventCenterPanel._CameratypeDrop.value = 0;

    }


    void Start()
    {
        Init();
        EventCenter.addlistener<string>(Eventdefine.pagetxt, pageshowcallback);
        eventCenterPanel._CameranameInput.onEndEdit.AddListener((name) => { 
            Camerausername = name;
          
        });
        eventCenterPanel._CameraCardnumInput.onEndEdit.AddListener((num) => { 
            CameracardNum = num;
  
        });
       
       

    }
    //查询监控事件http回调
    void SearchCameraEventCallBack(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("服务器返回查询结果:"+args.Value);
        }
    }


    private string TypeDropdownItemChanged(int index)
    {
        // 下拉框项目索引值
         Log.Debug(index);
        // 获取下拉框项目文本值
        string type = "";
        if(index!=0)
         type = eventCenterPanel._CameratypeDrop.options[index].text;
        return type;
    }
    private string AreanameDropdownItemChanged(int index)
    {
        // 下拉框项目索引值
         Log.Debug(index);
        // 获取下拉框项目文本值
        var text = eventCenterPanel._CameratypeDrop.options[index].text;
        return text;
    }

    private void OnDestroy()
    {
        EventCenter.RemoveListener<string>(Eventdefine.pagetxt, pageshowcallback);
    }
    void Init()
    {

        userGroup = new UserGroup();

    }

    void pageshowcallback( string pages) 
    {
        PagesTxt.text = pages;
    }



    /// <summary>
    /// 分页请求信息
    /// </summary>
    /// <param name="idNum">传值的就是单个请求，不传代表请求所有，分页请求不用传   </param>
    /// <param name="PageNum"> 请求第几页</param>
    /// <param name="PageSize">一页多少条数据</param>
    public void HttpPageGetData(string idNum, int PageNum, int PageSize, int Type, int issued, Action<List<object>, int> callBak)
    {
        string param = "idNum=" + idNum + "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString() ;
        Log.Debug("请求的信息："+RequestAlthriomUrl + param);
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestAlthriomUrl + param, PagehttpCallback, callBak, false, false, true);
        PageLoadingController.IsSearch = false;
        //GameStart.Instance.ShowTip("请求监控事件展示成功，正在加载，请稍后！");
        GameStart.instance.ShowSearchTxt("正在加载，请稍后！");
    }

    /// <summary>
    /// 监控事件查询分页请求信息----------------------------------------------------------------------------------------------------------------
    /// </summary>
    public void HttpPageSearchGetData(string idNum, int pageNum, int PageSize, int type, int issued, Action<List<object>, int> callback, string Doorusername = null, string doorcardNum = null, string Doorevntype = null, string DoorareaName = null)
    {
        string param = "idNum=" + CameracardNum + "&pageNum=" + pageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&day=" + "&eventType=" + Cameraeventype + "&userName=" + Camerausername + "&camName=" + CameraName;
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestAlthriomUrl + param, PagehttpCallback, callback, false, false, true);
        Log.Debug("请求查询的监控信息：" + RequestAlthriomUrl + param);
        Log.Debug("请求查询的人员姓名：" + Camerausername);

        //GameStart.Instance.ShowTip("请求查询监控事件成功，正在加载，请稍后！");
        GameStart.instance.ShowSearchTxt("正在加载，请稍后！");
    }



    //分页Http请求get回调
    public void PagehttpCallback(HttpCallBackArgsPeople args)
    {
        if (args.Value.Contains("code:401") || args.Value.Contains("code:400"))
        {
            Log.Error("Http收到服务器信息:" + args.Value);
            GameStart.Instance.ShowTip("服务器未响应！");
        }
        else 
        {
            print("Http收到服务器信息:" + args.Value);
            ShowTenPeople.Clear();

            if (!string.IsNullOrEmpty(args.Value))
            {
                HttpPageDeserialization(args.Value);
                List<object> itemList = ShowTenPeople;
                args.callBak(itemList, PeopleTotalCount);
                print(PeopleTotalCount);
                //EventCenter.BroadCast(Eventdefine.OnDataUpdate);
            }
        }
    

    }

    /// <summary>
    /// 反序列化分页请求返回的Json
    /// </summary>
    public void HttpPageDeserialization(string ReceiveStr)
    {
        Init();

        try
        {
 
            Althorims userInfoarr = JsonUtility.FromJson<Althorims>(ReceiveStr);
            Log.Error(userInfoarr.records[0].camName);
            //PeopleController.Ins.NumPage.text = userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString();
            //PageLoadingCameraEvent
            Log.Debug(userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString());
            EventCenter.BroadCast(Eventdefine.pagetxt, userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString());
            //PagingLoad.transform.Find("PapeBox/Text").GetComponent<Text>().text = userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString();

            PeopleTotalCount = userInfoarr.total;
            Log.Debug("人员总数："+PeopleTotalCount);
            for (int i = 0; i < userInfoarr.records.Count; i++)
            {

                ShowTenPeople.Add(userInfoarr.records[i]);
              

            }
        }
        catch (Exception e)
        {
            Log.Debug(e.Message);
        }


    }
}
