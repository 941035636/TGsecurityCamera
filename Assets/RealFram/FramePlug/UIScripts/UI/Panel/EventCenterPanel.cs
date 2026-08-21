using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;

public class EventCenterPanel : MonoBehaviour
{

    public Transform Content;
    public Transform evenTshowParent;
    public static bool IsstartEventShow = false;
    private uint iLastErr = 0;
    private string strErr;
    int alarmId;
    //public Transform PageLoading;
    public Transform CameraPageLoading;
    public Transform DoorPageLoading;
    public Toggle CameraEventTog;
    public Toggle DoorEventTog;
    public Transform CameraRightBg;
    public Transform DoorRightBg;

    //门禁事件搜索
    public TMP_InputField _DoornameInput;
    public TMP_InputField _DoorCardnumInput;
    public TMP_Dropdown _DoortypeDrop;
    public TMP_Dropdown _DoorArenameDrop;
    public Button DoorSearchBtn;
    public Button DateChoiceBtn;
    public GameObject _zcalendar;
    //监控事件搜索
    public TMP_InputField _CameranameInput;
    public TMP_InputField _CameraCardnumInput;
    public TMP_Dropdown _CameratypeDrop;
    public TMP_Dropdown _CameraArenameDrop;
    public Button CameraSearchBtn;


    private void Awake()
    {
        //注册门禁时间回调函数
        Log.Debug("注册门禁事件回调函数");
        EventCenter.addlistener<EventData>(Eventdefine.DoorEquipListener, DealEventMesg);
        IsstartEventShow = true;
   
        //登录门禁报警事件       //迁移到服务中
        //if (HikvisonNVR.LoginhandleDic.Count!=0)
        //{
        //    foreach (var item in HikvisonNVR.LoginhandleDic)
        //    {
        //        Log.Debug("登录句柄,IP:" + item.Key+"  handle:"+item.Value);
            
        //        if (item.Key.Contains(".26.")|| item.Key.Contains(".25.") || item.Key.Contains(".15."))
        //        {
        //            SetAlarm(item.Value);
        //        }
        //    }
        //}



    }

    private void SetAlarm(int loginhandle) 
    {
        CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
        struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
        struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
        struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
        struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测
        struAlarmParam.byDeployType = 1;//实时布防
        Log.Debug("开始布防........");
        alarmId = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(loginhandle, ref struAlarmParam);
        if (alarmId < 0)
        {
            iLastErr = CHCNetSDK.NET_DVR_GetLastError();
            strErr = "布防失败，错误号：" + iLastErr; //布防失败，输出错误号
            Log.Debug(strErr);

        }
        else
        {
            Log.Debug("布防成功，返回句柄" + alarmId);

        }
    }

    private void DealEventMesg(EventData data)
    {
        Transform EvObj = ObjectManager.Instance.InstantiateObject(ConStr.REALEVENTPREFAB).transform;
        EvObj.transform.SetParent(evenTshowParent);
        EvObj.GetChild(0).GetComponent<TextMeshProUGUI>().text = data.CardNum;
        EvObj.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.Eventtype;
        if (data.Eventtype=="人脸认证通过")
        EvObj.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.green;
        else if (data.Eventtype == "人脸认证失败")
            EvObj.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.red;
        EvObj.GetChild(2).GetComponent<TextMeshProUGUI>().text = data.EventTime;
        EvObj.GetChild(3).GetComponent<TextMeshProUGUI>().text = data.DevIP;
        EvObj.transform.localScale = new Vector3(1f, 1f, 1f);
        EvObj.GetChild(4).GetComponent<Button>().onClick.AddListener(()=> 
        {
            Content.GetChild(1).gameObject.SetActive(true);
            Content.GetChild(2).gameObject.SetActive(true);
            Log.Debug("人脸抓拍图片:"+data.facepic);
            Content.GetChild(2).Find("Imgbg/EventImg").GetComponent<Image>().sprite =data.facepic;
        });



    }


    private void OnDestroy()
    {
        IsstartEventShow = false;
        Log.Debug("注销事件界面展示");
        EventCenter.RemoveListener<EventData>(Eventdefine.DoorEquipListener, DealEventMesg);
    }
}
