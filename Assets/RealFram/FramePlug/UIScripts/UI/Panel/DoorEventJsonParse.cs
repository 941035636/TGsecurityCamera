using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using ZTools;

public class DoorEventJsonParse : MonoBehaviour
{
   

    string RequestDoorEventUrl = @"http://"+GameStart.IP+"/api/door/tg/door/alarm?";

    public GameObject UserContent;
    public GameObject PagingLoad;
    string jsonPath = string.Empty;
    UserGroup userGroup = new UserGroup();
    [SerializeField]//必须要加
    public List<object> ShowTenPeople = new List<object>();
    public int PeopleTotalCount;
    public Text PagesTxt;
    private static DoorEventJsonParse Instanc;
    public static DoorEventJsonParse GetInstance()
    {
        if (Instanc == null)
        {
            Instanc = new DoorEventJsonParse();
        }
        return Instanc;

    }

    //门禁
    public EventCenterPanel eventCenterPanel;
    public static string Doorusername;
    public static string DoorcardNum;
    public static string Dooreventype;
    public static string DoorName;

    List<string> dooreventnamelist = new List<string>();

    private void OnEnable()
    {       //初始化门禁名称下拉列表数据信息

        List<NVRInformation> alldoors = GameStart.AllDoorEquips.records;
        UpdateDropdownView(eventCenterPanel._DoorArenameDrop, alldoors);
        eventCenterPanel._DoorArenameDrop.onValueChanged.AddListener((value) => {
       
            if (value == 0)
                DoorName = "";
            else
                DoorName = DoorAreanameDropdownItemChanged(value);
            Log.Debug("选择门禁:" + DoorName);

        });
        eventCenterPanel._DoorArenameDrop.onValueChanged.Invoke(10);
        //初始化门禁事件类型下拉列表
        dooreventnamelist.Clear();
        dooreventnamelist.Add("请选择事件类型");
        dooreventnamelist.Add("人脸认证通过");
        dooreventnamelist.Add("人脸认证失败");
        dooreventnamelist.Add("黑名单人员");
        UpdateDropdownView(eventCenterPanel._DoortypeDrop, dooreventnamelist);

        eventCenterPanel._DoortypeDrop.onValueChanged.AddListener((value) => {

            if (value == 0)
                Dooreventype = "";
            else
                Dooreventype = DoorTypeDropdownItemChanged(value);
            Log.Debug("选择类型:" + Dooreventype);
        });

        eventCenterPanel._DoorArenameDrop.onValueChanged.Invoke(0);
        eventCenterPanel._DoorArenameDrop.value = 0;
        eventCenterPanel._DoortypeDrop.onValueChanged.Invoke(0);
        eventCenterPanel._DoortypeDrop.value = 0;
        eventCenterPanel.DateChoiceBtn.onClick.RemoveAllListeners();
        eventCenterPanel.DateChoiceBtn.onClick.AddListener(() => {

            eventCenterPanel._zcalendar.GetComponent<ZCalendar>().Show();
            
        
        });

    }

    void Start()
    {
        Init();
        EventCenter.addlistener<string>(Eventdefine.pagetxt, pageshowcallback);
        EventCenter.addlistener<string, string>(Eventdefine.ChoiceTimeShow,ChoiceTimeShow);

        //门禁事件查询
        eventCenterPanel._DoornameInput.onEndEdit.AddListener((name) => {
            if (!string.IsNullOrEmpty(name))
                Doorusername = name;
            else
                Doorusername = "";
        });
        eventCenterPanel._DoorCardnumInput.onEndEdit.AddListener((num) => {
            if (!string.IsNullOrEmpty(num))
                DoorcardNum = num;
            else
                DoorcardNum = "";

            Log.Debug("门禁人员id:" + DoorcardNum);
        });
  

 


    }

    private void ChoiceTimeShow(string dateStart,string dateEnd) 
    {

        eventCenterPanel.DateChoiceBtn.transform.Find("Day1").GetComponent<Text>().text=dateStart;

        eventCenterPanel.DateChoiceBtn.transform.Find("Day2").GetComponent<Text>().text = dateEnd;

    }





    //刷新下拉框显示
    private void UpdateDropdownView(TMP_Dropdown doornameDrop, List<NVRInformation> nvrs)
    {
     
        //清空下下拉框数据
        doornameDrop.options.Clear();
        NVRInformation nvr0 = new NVRInformation();
        nvr0.cameraname = "请选择门禁点名称";
        if(!nvrs.Exists(t=>t.cameraname== "请选择门禁点名称"))
        nvrs.Insert(0, nvr0);
        TMP_Dropdown.OptionData tempData;
        for (int i = 0; i < nvrs.Count; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = nvrs[i].cameraname;
            doornameDrop.options.Add(tempData);
        }
        //把第一条数据显示为默认
        doornameDrop.captionText.text = nvrs[0].cameraname;
    }
    private void UpdateDropdownView(TMP_Dropdown doornameDrop, List<string> nvrs)
    {
        //清空下下拉框数据
        doornameDrop.options.Clear();
        TMP_Dropdown.OptionData tempData;
        for (int i = 0; i < nvrs.Count; i++)
        {
            tempData = new TMP_Dropdown.OptionData();
            tempData.text = nvrs[i];
            doornameDrop.options.Add(tempData);
        }
        //把第一条数据显示为默认
        doornameDrop.captionText.text = nvrs[0];
    }


    //门禁类型
    private string DoorTypeDropdownItemChanged(int index)
    {
        // 下拉框项目索引值
         Log.Debug(index);
        // 获取下拉框项目文本值
        string type = "";
        if(index!=0)
         type = eventCenterPanel._DoortypeDrop.options[index].text;
        return type;
    }
    //门禁
    private string DoorAreanameDropdownItemChanged(int index)
    {
        // 下拉框项目索引值
         Log.Debug(index);
        // 获取下拉框项目文本值
        var text = eventCenterPanel._DoorArenameDrop.options[index].text;
        return text;
    }
    


    private void OnDestroy()
    {
        EventCenter.RemoveListener<string>(Eventdefine.pagetxt, pageshowcallback);
        EventCenter.RemoveListenerTwo<string, string>(Eventdefine.ChoiceTimeShow, ChoiceTimeShow);
    }
    void Init()
    {

        userGroup = new UserGroup();

    }

    void pageshowcallback(string pages)
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
        string param = "idNum=" + idNum + "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString();
      
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestDoorEventUrl + param, PagehttpCallback, callBak, false, false, true);
        Log.Debug("请求的门禁信息：" + RequestDoorEventUrl + param);
        PageLoadingController.IsSearch = false;
        //GameStart.Instance.ShowTip("请求门禁事件展示成功，正在加载，请稍后！");
        GameStart.Instance.ShowSearchTxt("正在加载，请稍后");
    }


    /// <summary>
    /// 门禁事件查询分页请求信息----------------------------------------------------------------------------------------------------------------
    /// </summary>
    public void HttpPageSearchGetData(string idNum, int pageNum, int PageSize, int type, int issued, Action<List<object>, int> callback, string DoorUsername = null, string doorcardNum = null, string Doorevntype = null, string DoorareaName = null)
    {
        string param = "idNum=" + DoorcardNum + "&pageNum=" + pageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&dateStart="+ZCalendarEvent.EventSearchStartTime+"&dateEnd="+ZCalendarEvent.EventSearchEndTime + "&eventType=" + Dooreventype + "&userName=" + Doorusername + "&camName=" + DoorName;
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestDoorEventUrl + param, PagehttpCallback, callback, false, false, true);  
        Log.Debug("请求查询的门禁信息：" + RequestDoorEventUrl + param);
 
        //GameStart.Instance.ShowTip("请求查询门禁事件成功，正在加载，请稍后！");
        GameStart.Instance.ShowSearchTxt("正在加载，请稍后");
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
    ///门禁事件返回信息数据结构
    ///
    [Serializable]
    public class DoorEventData 
    {
        public int id;
        public string userName = string.Empty;
        public string idNum = string.Empty;
        public string eventType = string.Empty;
        public string time = string.Empty;
        public string devName = string.Empty;
        public string faceBase64 = string.Empty;

    
    }

    [Serializable]
    public class AllDoorEvent 
    {
        public List<DoorEventData> records = new List<DoorEventData>();
        public int total;
        public int size;
        public int current;
        public Array orders;
        public bool optimizeCountSql;
        public bool searchCount;
        public object maxLimit;
        public object countId;
        public int pages;
    
    }



    /// <summary>
    /// 反序列化分页请求返回的Json
    /// </summary>
    public void HttpPageDeserialization(string ReceiveStr)
    {
        Init();

        try
        {
            Log.Debug("接受到的信息：" + ReceiveStr);
            AllDoorEvent userInfoarr = JsonUtility.FromJson<AllDoorEvent>(ReceiveStr);
            Log.Error(userInfoarr.records[0].devName);
            Log.Debug(userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString());
            EventCenter.BroadCast(Eventdefine.pagetxt, userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString());
            //PagingLoad.transform.Find("PapeBox/Text").GetComponent<Text>().text = userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString();
            PeopleTotalCount = userInfoarr.total;
            Log.Debug("人员总数：" + PeopleTotalCount);
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
