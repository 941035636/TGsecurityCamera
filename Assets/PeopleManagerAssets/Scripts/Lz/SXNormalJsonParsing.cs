using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using ZTools;
using static PeopleController;

public class SXNormalJsonParsing : MonoSingleton<SXNormalJsonParsing>
{

    string RequestInfoUrl = @"http://" + GameStart.IP + "/api/personnel/tg/user/dev/status?";

    public GameObject UserContent;
    public GameObject PagingLoad;
    string jsonPath = string.Empty;
    //UserGroup userGroup = new UserGroup();
    [SerializeField]//必须要加
    public List<object> ShowTenPeople = new List<object>();
    public int PeopleTotalCount;
    public Text numPage;

    public TMP_InputField nameInput;
    public TMP_InputField numInput;
    public TMP_Dropdown _DoorArenameDrop;
    public static string userName = string.Empty;
    public static string cardNum = string.Empty;
    public static int devId;
    public Button AllSendBtn;
    List<NVRInformation> alldoors = new List<NVRInformation>();


    UserAdd userAdd = new UserAdd();
    string authTime = string.Empty;
    string typeName = string.Empty;
    void OnEnable()
    {
        PageLoadingController.IsSearch = false;
        PagingLoad.GetComponent<PageLoadingSXnormal>().Init();
    }

    private void Start()
    {
        nameInput.onEndEdit.AddListener((txt) =>
        {
            if (!string.IsNullOrEmpty(txt))
                userName = txt;
            else
                userName = "";

        });

        numInput.onEndEdit.AddListener((txt) =>
        {
            //Regex reg = new Regex("(\\d{15})|(^\\d{18})|(\\d{17}(\\d|X|x)$)");
            Regex reg = new Regex("(^\\d{15}$)|(^\\d{18}$)|(^\\d{17}(\\d|X|x)$)");
            //Regex reg = new Regex("/^[1-9]\\d{5}(18|19|20|(3\\d))\\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\\d{3}[0-9Xx]$/;\r\n ");
            if (reg.IsMatch(txt))
            {
                numInput.text = txt;
                cardNum = txt;
            }
            else
            {
                if (numInput.text == "")
                {
                    numInput.text = "";

                }
                else
                {
                    numInput.text = txt.Substring(0, txt.Length - 1);
                }
                cardNum = "";
                numInput.text = "";
                GameStart.Instance.ShowTip("身份证号不合法！");


            }


        });
        alldoors = GameStart.AllDoorEquips.records;
        UpdateDropdownView(_DoorArenameDrop, alldoors);
        _DoorArenameDrop.onValueChanged.AddListener((value) =>
        {

            if (value == 0)
                devId = 0;
            else
                devId = DoorAreanameDropdownItemChanged(value);
            Log.Debug("选择门禁:" + devId);

        });
    }

    private void UpdateDropdownView(TMP_Dropdown doornameDrop, List<NVRInformation> nvrs)
    {

        //清空下下拉框数据
        doornameDrop.options.Clear();
        NVRInformation nvr0 = new NVRInformation();
        nvr0.cameraname = "请选择门禁点名称";
        if (!nvrs.Exists(t => t.cameraname == "请选择门禁点名称"))
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
    //门禁
    private int DoorAreanameDropdownItemChanged(int index)
    {
        // 下拉框项目索引值
        Log.Debug(index);
        // 获取下拉框项目文本值
        var text = _DoorArenameDrop.options[index].text;
        for (int i = 0; i < alldoors.Count; i++)
        {
            if (string.Equals(_DoorArenameDrop.options[index].text, alldoors[i].cameraname))
            {
                return alldoors[i].id;
            }
        }
        return 0;
    }
    /// <summary>
    /// 分页请求信息
    /// </summary>
    /// <param name="idNum">传值的就是单个请求，不传代表请求所有，分页请求不用传   </param>
    /// <param name="PageNum"> 请求第几页</param>
    /// <param name="PageSize">一页多少条数据</param>
    public void HttpPageGetData(int PageNum, int PageSize, Action<List<object>, int> callBak)
    {
        string param = "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&type=1";

        Log.Error("请求筛选的人员信息:" + RequestInfoUrl + param);
        HttpNetManagerPeople.GetInstance().SendDataStr(RequestInfoUrl + param, PagehttpCallback, callBak, false, false, true);
        PageLoadingController.IsSearch = false;
    }


    /// <summary>
    /// http查询人员信息----------------------------------------------------------------------------------------------------------------
    /// </summary>
    public void HttpPageSearchGetData(string idNum, int pageNum, int PageSize, int type, Action<List<object>, int> callback, string Username = null, int devId = 0)
    {
        string param = string.Empty;
        if (devId == 0)
            param = "pageNum=" + pageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&username=" + Username + "&idNum=" + idNum + "&type=1";
        else
            param = "pageNum=" + pageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&username=" + Username + "&idNum=" + idNum + "&devId=" + devId + "&type=1";
        HttpNetManagerPeople.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/user/dev/status?" + param, PagehttpCallback, callback, false, false, true);
        Log.Debug("请求查询的人员信息：" + "http://" + GameStart.IP + "/api/personnel/tg/user/dev/status?" + param);

    }


    //分页Http请求get回调
    public void PagehttpCallback(HttpCallBackArgsPeople args)
    {

        print("Http收到服务器信息:" + args.Value);
        ShowTenPeople.Clear();

        if (!string.IsNullOrEmpty(args.Value))
        {
            HttpPageDeserialization(args.Value);
            List<object> itemList = ShowTenPeople;
            args.callBak(itemList, PeopleTotalCount);
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

            UserInfoSXModel userInfoarr = JsonUtility.FromJson<UserInfoSXModel>(ReceiveStr);
            numPage.text = userInfoarr.current.ToString() + "/" + userInfoarr.pages.ToString();
            PeopleTotalCount = userInfoarr.total;
            for (int i = 0; i < userInfoarr.records.Count; i++)
            {
                //if (!userInfoarr.records[i].unitName.Contains("访客"))
                ShowTenPeople.Add(userInfoarr.records[i]);


            }
            //初始化本页批量下发
            AllSendBtn.onClick.RemoveAllListeners();
            AllSendBtn.onClick.AddListener(() =>
            {

                AllSend();
            });
        }
        catch (Exception e)
        {
            Log.Debug(e.Message);
        }


    }

    private void AllSend()
    {

        if (ShowTenPeople.Count > 0)
        {
            for (int i = 0; i < ShowTenPeople.Count; i++)
            {
                int temp = i;
                StartCoroutine(RequesDevtInfo(((UserAddFail)ShowTenPeople[temp]).devId, ((UserAddFail)ShowTenPeople[temp]).idNum));
            }
        }
        else 
        {
            GameStart.instance.ShowTip("当前无下发内容");
        }
       
    }


    IEnumerator RequesDevtInfo(int devid, string idNum)
    {
        userAdd = new UserAdd();
        yield return new WaitForEndOfFrame();
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/cam/info?camId=" + devid, GetDevInfo, false, false, false, null);
        yield return new WaitForSeconds(0.5f);
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/user/client/info?&pageNum=1&pageSize=1&idNum=" + idNum, GetPersonInfo, false, false, false, null);
        yield return new WaitForSeconds(0.5f);
        if (!string.IsNullOrEmpty(userAdd.idNum) && !string.IsNullOrEmpty(userAdd.faceBase64) && !string.IsNullOrEmpty(userAdd.authDate) && userAdd.toAddList.Count != 0)
        {
            string str = JsonConvert.SerializeObject(userAdd);
            Log.Error("重新下发的人员信息:" + str);
            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/mqtt/push/info", ModifyOnePersonAuth, true, true, false, str);
        }
        else
        {
            if (string.IsNullOrEmpty(userAdd.idNum))
                GameStart.Instance.ShowTip("重新下发人员信息不合法:身份证号为空");
            else if (string.IsNullOrEmpty(userAdd.faceBase64))
                GameStart.Instance.ShowTip("重新下发人员信息不合法:人脸照片为空");
            else if (string.IsNullOrEmpty(userAdd.authDate))
                GameStart.Instance.ShowTip("重新下发人员信息不合法:权限时间为空");
            else if (userAdd.toAddList.Count == 0)
                GameStart.Instance.ShowTip("重新下发人员信息不合法:门禁设备为空");

        }


    }
    private void ModifyOnePersonAuth(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains("200"))
        {
            GameStart.Instance.ShowTip("重新下发该人员权限信息成功!");

        }
    }
    private void GetDevInfo(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            //Debug.LogError("请求的设备信息:" + args.Value);
            DevInfoArr dev = JsonConvert.DeserializeObject<DevInfoArr>(args.Value);
            if (dev.records.Count > 0)
            {
                //Debug.LogError("设备名:" + dev.records[0].cameraname);
                userAdd.toAddList.Add(dev.records[0]);
            }
            else
            {
                Debug.LogError("设备容器为空");
            }

        }


    }
    private void GetPersonInfo(HttpCallBackArgs args)
    {

        if (!string.IsNullOrEmpty(args.Value))
        {
            //Debug.LogError("请求的人员信息:" + args.Value);
            UserInfoArrPeople user = JsonConvert.DeserializeObject<UserInfoArrPeople>(args.Value);
            if (user.records.Count > 0)
            {
                //Debug.LogError("人名:" + user.records[0].faceBase64);
                userAdd.username = user.records[0].username;
                userAdd.faceBase64 = user.records[0].faceBase64;
                userAdd.idNum = user.records[0].idNum;
                userAdd.is2issued = "";
                userAdd.salaryNum = user.records[0].salaryNum;
                if (!string.IsNullOrEmpty(authTime))
                {
                    //2023-07-12T00:00:00-2023-07-12T23:59:59
                    userAdd.authDate = authTime.Split('T')[0] + "," + authTime.Split('T')[1].Split('-')[1] + "-" + authTime.Split('T')[1].Split('-')[2] + "-" + authTime.Split('T')[1].Split('-')[3];
                }

                userAdd.authTime = "";
                userAdd.type = 888;
                userAdd.typeName = typeName;
            }
            else
                Debug.LogError("人员容器为空");
        }
    }



}
