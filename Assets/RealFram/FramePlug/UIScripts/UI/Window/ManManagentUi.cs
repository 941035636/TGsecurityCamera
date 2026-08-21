using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using TMPro;
using static UnityEngine.Camera;
using System.Linq;
using System.Drawing;
using LitJson;
using Newtonsoft.Json;
using static zFramework.Media.NVRConfiguration;
using System.Text;
using UnityEditor;
using UnityTimer;
using Vuplex.WebView;
using Vuplex.WebView.Demos;
using System.Security.Policy;
using SuperTreeView;





public class ManManagentUi : Window
{
    #region
    public bool LoadDataBYServer = true;
    public ManManagentPanel _mainManagentPanel;
    #region 添加面板字段
    private string camera_name;
    private string ip_adress;
    private int port = 0;
    private string username;
    private string pwd;
    private SDKTYPE sdktype;
    private int channel = 1;
    #endregion
    public static GameObject cameraitem;
    public static bool IsInitCamera = false;
    public static bool IsInitDoor = false;
    public static bool IsInitDoorGroup = false;
    public static bool IsInitCameraGroup = false;
    private bool IsCamera = false;
    private bool IsDoor = false;

    private string ChoiceDoorArea = string.Empty;
    private string ChoiceCameraArea = string.Empty;
    List<NVRInformation> adddoorlist = new List<NVRInformation>();//选择导入分组的门禁设备
    List<NVRInformation> addCameralist = new List<NVRInformation>();//选择导入分组的编码设备

    private List<NVRInformation> DeleteCameraList = new List<NVRInformation>();//选中要删除的监控设备添加进集合
    private List<NVRInformation> DeleteDoorList = new List<NVRInformation>();//选中要删除的门禁设备添加进集合
    private string ChoiceCameraGroup = string.Empty;//选中的监控分组
    private int ChoiceCameraGroupId = 0;//选中的监控分组ID
    //private Transform CameraGroupParent;

    private Transform CameraTreeView;
    private Transform DoorTreeView;
    private string NewCameraGroupName = string.Empty;
    public List<NVRInformation> DoorIdList = new List<NVRInformation>();
    public List<NVRInformation> CameraIdList = new List<NVRInformation>();
    private Transform DoorEquipManager;
    private Transform SetSuanfapanel;
    Dictionary<string, List<NVRInformation>> DoorAreaDevsDic = new Dictionary<string, List<NVRInformation>>();//从服务器请求的数据中把门禁区域和设备信息绑定
    Dictionary<string, List<NVRInformation>> CameraAreaDevsDic = new Dictionary<string, List<NVRInformation>>();//从服务器请求的数据中把监控区域和设备信息绑定
    /// 获取监控区域分组设备信息
    public AreaGroupDevList CameraAreaGroupDevs;
    AreaDevsData areaDevsData;
    #endregion



    public override void Awake(params object[] paralist)
    {

        _mainManagentPanel = GameObject.GetComponent<ManManagentPanel>();
        SetSuanfapanel = _mainManagentPanel.transform.Find("SetSuanfaPanel");
        DoorEquipManager = _mainManagentPanel.RightDoorGroupPage.Find("GroupBg/Scroll View/Viewport/Content/DoorEquipManager");
        //CameraGroupParent = _mainManagentPanel.RightCameraGroupPage.Find("GroupBg/Scroll View/Viewport/Content/DoorEquipManager");
        CameraTreeView = _mainManagentPanel.RightCameraGroupPage.Find("GroupBg/Tree/TreeScrollView/Viewport/TreeView");
        DoorTreeView = _mainManagentPanel.RightDoorGroupPage.Find("GroupBg/Tree/TreeScrollView/Viewport/TreeView");
        Init();

        EventCenter.addlistener<string, List<NVRInformation>>(Eventdefine.InitDoorGroupsDevs, InitDoorGroupsDevs);//添加生成门禁分组某一区域下设备信息的监听
        EventCenter.addlistener<string, List<NVRInformation>>(Eventdefine.InitCameraGroupsDevs, InitCameraGroupsDevs);//添加生成监控分组某一区域下设备信息的监听
        EventCenter.addlistener(Eventdefine.OndestoryOneAreaDevs,OndestroyAreaEquips);
    }

    public override void OnDisable()
    {
        EventCenter.RemoveListenerTwo<string, List<NVRInformation>>(Eventdefine.InitDoorGroupsDevs, InitDoorGroupsDevs);
        EventCenter.RemoveListenerTwo<string, List<NVRInformation>>(Eventdefine.InitCameraGroupsDevs, InitCameraGroupsDevs);
        EventCenter.RemoveListener(Eventdefine.OndestoryOneAreaDevs, OndestroyAreaEquips);
    }

    /// <summary>
    /// 初始化添加监听
    /// </summary>
    private void Init()
    {
        AddToggleClickListener(_mainManagentPanel.AddCamera_Toggle, AddEquipClick);
        AddToggleClickListener(_mainManagentPanel.AddDoor_Toggle, AddDoorClick);
        AddButtonClickListener(_mainManagentPanel.AddCamera_Btn, AddEquipItem);
        AddButtonClickListener(_mainManagentPanel.CancleCameraadd_Button, CancleAddPanel);
        AddToggleClickListener(_mainManagentPanel.EquipMantog, CameraManListener);
        AddToggleClickListener(_mainManagentPanel.DoorMantog, DoorManListener);
        AddToggleClickListener(_mainManagentPanel.camera_Tog, CameraShowListener);
        AddToggleClickListener(_mainManagentPanel.cameragroup_Tog, CameraGroupShowListener);

        //删除监控分组注释
        //AddToggleClickListener(_mainManagentPanel.DaleteCameraGroup_Toggle, DeleteCameraGroup);

        AddToggleClickListener(_mainManagentPanel.door_Tog, DoorShowListener);
        AddToggleClickListener(_mainManagentPanel.doorgroup_Tog, DoorGroupShowListener);
        AddToggleClickListener(_mainManagentPanel.CameraRefrush_Toggle, CameraRefrushLoginState);
        AddToggleClickListener(_mainManagentPanel.DoorRefrush_Toggle, DoorRefrushLoginState);
        AddToggleClickListener(_mainManagentPanel.DeleteCamera_Toggle, DeleteCameraItem);
        AddToggleClickListener(_mainManagentPanel.DeleteDoor_Toggle, DeleteDoorItem);
        //AddToggleClickListener(_mainManagentPanel.RightDoorGroupPage.Find("TopTogGroup/add_Tog").GetComponent<Toggle>(), AddDoorGroup);
        //AddToggleClickListener(_mainManagentPanel.RightCameraGroupPage.Find("TopTogGroup/add_Tog").GetComponent<Toggle>(), AddCameraGroup);
        AddToggleClickListener(_mainManagentPanel.RightDoorGroupPage.Find("TopTogGroup/daoru_Tog").GetComponent<Toggle>(), ImportDoor);
        AddToggleClickListener(_mainManagentPanel.RightCameraGroupPage.Find("TopTogGroup/daoru_Tog").GetComponent<Toggle>(), ImportCamera);
        _mainManagentPanel.Name_input.onEndEdit.AddListener((text) =>
        {

            camera_name = text;
        });
        _mainManagentPanel.Ip_input.onEndEdit.AddListener((text) =>
        {

            if (ValidateIP(text))
            {
                ip_adress = text;
            }
            else
            {
                Log.Debug("IP不合法");
            }
        });
        _mainManagentPanel.Point_input.onEndEdit.AddListener((text) =>
        {

            if (ValidatePort(text))
            {
                port = int.Parse(text);
            }
            else
            {
                Log.Debug("端口不合法");
            }
        });
        _mainManagentPanel.User_input.onEndEdit.AddListener((text) =>
        {
            username = text;
        });
        _mainManagentPanel.Pwd_input.onEndEdit.AddListener((text) =>
        {
            pwd = text;
        });
        _mainManagentPanel.DeviceBrand_drop.onValueChanged.AddListener((value) =>
        {
            Log.Error("出发value:"+value);
            switch (value)
            {
                case 0:
                    sdktype = SDKTYPE.HK;
                    channel = 1;
                    _mainManagentPanel.Channel_input.text = "1";
                    _mainManagentPanel.Channel_input.interactable = false;
                    _mainManagentPanel.DeviceBrand_drop.captionText.text = _mainManagentPanel.DeviceBrand_drop.options[0].text;
                    break;
                case 1:
                    sdktype = SDKTYPE.DH;
                    channel = 0;
                    _mainManagentPanel.Channel_input.text = "0";
                    _mainManagentPanel.Channel_input.interactable = false;
                    _mainManagentPanel.DeviceBrand_drop.captionText.text = _mainManagentPanel.DeviceBrand_drop.options[1].text;
                    break;
                case 2:
                    sdktype = SDKTYPE.YS;
                    _mainManagentPanel.Channel_input.text = "1";
                    _mainManagentPanel.Channel_input.interactable = false;
                    _mainManagentPanel.DeviceBrand_drop.captionText.text = _mainManagentPanel.DeviceBrand_drop.options[2].text;
                    break;
                default:
                    break;
            }
        });
        //根据本地监控json数据生成cameraitem并依次登录
        //_mainManagentPanel.camera_Tog.isOn = true;
        //_mainManagentPanel.CameraRefrush_Toggle.onValueChanged.Invoke(true);
        //门禁导入页面关闭
        _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/down/closebtn").GetComponent<Button>().onClick.RemoveAllListeners();
        _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/down/closebtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg").gameObject.SetActive(true);
            _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg").gameObject.SetActive(false);
        });
        //监控导入页面关闭
        _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/down/closebtn").GetComponent<Button>().onClick.RemoveAllListeners();
        _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/down/closebtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg").gameObject.SetActive(true);
            _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg").gameObject.SetActive(false);
        });

        //门禁导入页面导入

        _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/down/importbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/down/importbtn").GetComponent<Button>().onClick.AddListener(() =>
        {

            if (LoadDataBYServer)
            {
                DoorIdList.Clear();
                for (int i = 0; i < adddoorlist.Count; i++)
                {
                    DoorIdList.Add(adddoorlist[i]);
                    GameObject dooritemS = ObjectManager.Instance.InstantiateObject(ConStr.DOORItemSHORT);
                    dooritemS.transform.SetParent(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content"));
                    resetPrefab(dooritemS);
                    Log.Debug("服务端：" + ChoiceDoorArea + "导入" + adddoorlist[i].cameraname + "IP:" + adddoorlist[i].Ip);
                    dooritemS.transform.Find("itemnameTxt").GetComponent<TextMeshProUGUI>().text = adddoorlist[i].cameraname;
                    for (int j = 0; j < _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").childCount; j++)
                    {
                        if (_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(j).Find("doornametxt").GetComponent<TextMeshProUGUI>().text == adddoorlist[i].cameraname)
                        {
                            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(j).gameObject);

                        }

                    }
                }

                //向服务器发送导入的分组信息

                addArea area = new addArea();
                area.areaName = ChoiceDoorArea;


                for (int i = 0; i < DoorAreaGroupDevs.data.Count; i++)
                {
                    if (string.Equals(DoorAreaGroupDevs.data[i].areaName, ChoiceDoorArea))
                    {
                        area.id = DoorAreaGroupDevs.data[i].id;
                    }
                }

                addArea darea = new addArea();
                darea.areaName = TreeManager.ChoiceAreaName;
                darea.id = int.Parse(TreeManager.ChoiceAreaId);
                darea.devList = adddoorlist;
                darea.areaType = 1;//门禁
                try
                {
                    string areajson = JsonUtility.ToJson(darea);
                    Log.Debug("添加门禁区域设备信息:" + areajson);

                    HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/device/info", AddAreaCallback, true, false, darea);
                }
                catch (Exception e)
                {

                    Log.Debug(e.ToString());
                }



                //清空导入容器
                adddoorlist.Clear();

            }



        });


        //监控导入页面导入
        _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/down/importbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/down/importbtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (LoadDataBYServer)
            {
                CameraIdList.Clear();
                for (int i = 0; i < addCameralist.Count; i++)
                {
                    CameraIdList.Add(addCameralist[i]);
                    GameObject dooritemS = ObjectManager.Instance.InstantiateObject(ConStr.DOORItemSHORT);
                    dooritemS.transform.SetParent(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content"));
                    resetPrefab(dooritemS);
                    Log.Debug("服务端：" + TreeManager.ChoiceAreaName + "导入" + addCameralist[i].cameraname + "IP:" + addCameralist[i].Ip);
                    dooritemS.transform.Find("itemnameTxt").GetComponent<TextMeshProUGUI>().text = addCameralist[i].cameraname;
                    for (int j = 0; j < _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").childCount; j++)
                    {
                        if (_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(j).Find("doornametxt").GetComponent<TextMeshProUGUI>().text == addCameralist[i].cameraname)
                        {
                            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(j).gameObject);

                        }

                    }
                }

                //向服务器发送导入的监控分组信息

                addArea area = new addArea();
                area.areaName = TreeManager.ChoiceAreaName;
                if(!string.IsNullOrEmpty( TreeManager.ChoiceAreaId))
                area.id = int.Parse(TreeManager.ChoiceAreaId);
                area.devList = CameraIdList;
                area.areaType = 2;//监控
                try
                {
                    string areajson = JsonUtility.ToJson(area);
                    Log.Debug("添加区域设备信息:" + areajson);

                    HttpNetManager.GetInstance().SendDataObj("http://" +GameStart.IP  + "/api/personnel/tg/area/device/info", AddMonitorAreaCallback, true, false, area);
                }
                catch (Exception e)
                {

                    Log.Debug(e.ToString());
                }



                //清空导入容器
                addCameralist.Clear();
            }



        });


        //_mainManagentPanel.transform.Find("SetSuanfaPanel/addpanel/addbtn").GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    _mainManagentPanel.transform.Find("SetSuanfaPanel").gameObject.SetActive(false);
        //    //添加算法逻辑
        //});
        //_mainManagentPanel.transform.Find("SetSuanfaPanel/addpanel/canclebtn").GetComponent<Button>().onClick.AddListener(() =>
        //{

        //    _mainManagentPanel.transform.Find("SetSuanfaPanel").gameObject.SetActive(false);
        //});

    }




    #region   监控模块逻辑
    ///设备
      #region   添加设备相关逻辑
    /// <summary>
    /// 添加设备
    /// </summary>
    private void AddEquipItem()

    {

        if (string.IsNullOrEmpty(ip_adress) || !ValidateIP(ip_adress) || port == 0 || !ValidatePort(port.ToString()))
        {
            Log.Debug("输入有误");
            return;
        }
        else
        {

            //添加监控和门禁或者门禁设备
            AddCameraDevs();
            CancleAddPanel();
        }

    }
    private void CancleAddPanel()
    {
        Transform.Find("AddequipMaskPanel").gameObject.SetActive(false);

    }
    //添加监控设备
    private void AddEquipClick(bool ison)
    {
        //权限控制
        if (LoginManager.Ins.IsCameraSet)
        {
            if (ison)
            {
                Log.Debug("添加监控设备");
                Transform.Find("AddequipMaskPanel").gameObject.SetActive(true);
                _mainManagentPanel.Name_input.text = string.Empty;
                camera_name = string.Empty;
                _mainManagentPanel.Ip_input.text = string.Empty;
                ip_adress = string.Empty;
                _mainManagentPanel.Point_input.text = string.Empty;
                port = 0;
                _mainManagentPanel.User_input.text = string.Empty;
                username = string.Empty;
                _mainManagentPanel.Pwd_input.text = string.Empty;
                pwd = string.Empty;
                _mainManagentPanel.Channel_input.text = string.Empty;
          
                _mainManagentPanel.DeviceBrand_drop.onValueChanged.Invoke(0);
            }

        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有监控点配置权限！");
        }



    }

    /// <summary>
    /// 删除监控设备
    /// </summary>
    /// <param name="ison"></param>
    private void DeleteCameraItem(bool ison)
    {
        //权限控制
        if (LoginManager.Ins.IsCameraSet)
        {
            if (ison)
            {
                 Log.Debug("删除监控");

                for (int i = 0; i < DeleteCameraList.Count; i++)
                {
                    int index = i;
                    Log.Debug("删除了:" + DeleteCameraList[index].id);
                    DeleteID deleteID = new DeleteID();
                    deleteID.id = DeleteCameraList[index].id;
                    string strd = JsonUtility.ToJson(deleteID, true);
                    Log.Debug(strd);
                    DeleteCameraList.RemoveAt(index);
                    HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/del/cam/info", DeleteDevCallback, true, true, false, strd);


                }
                //for (int i = 0; i < DeleteCameraList.Count; i++)
                //{


                //    if (GameStart.Instance.Cameranvrs.Contains(DeleteCameraList[i]))
                //    {
                //        Log.Debug("删除了:" + DeleteCameraList[i].Ip);
                //        GameStart.Instance.Cameranvrs.Remove(DeleteCameraList[i]);
                //    }

                //}
                //    //本地保存
                //    NVRConfiguration.GetInstance().SaveNvrConfiguration_outside(GameStart.Instance.Cameranvrs);
                //    //上传服务器
                //    IsInitCamera = false;
                //    initCameraItemByNative();

            }

        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有监控点配置权限！");
        }



    }
    /// <summary>
    /// 删除监控回调
    /// </summary>
    /// <param name="args"></param>
    public void DeleteDevCallback(HttpCallBackArgs args)
    {
        Log.Debug("删除设备收到服务器消息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains("200"))
        {
            GameStart.Instance.ShowTip("删除监控设备成功!");
        }
        Timer.Register(0.2f, () =>
        {  //重新请求展示刷新
            for (int i = 0; i < _mainManagentPanel.EquipContains.childCount; i++)
            {
                GameObject.Destroy(_mainManagentPanel.EquipContains.GetChild(i).gameObject);
            }
            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=编码设备", ReadCameraCallback, false, false, false, null);
        });
    }



    /// <summary>
    /// 添加监控设备或者门禁设备
    /// </summary>
    NVRInformation nvr;
    private void AddCameraDevs()
    {
        //1、本地保存数据信息
        nvr = new NVRInformation();
        //if (GameStart.Instance.Cameranvrs != null)
        //    foreach (var item in GameStart.Instance.Cameranvrs)
        //    {
        //        if (!string.IsNullOrEmpty(ip_adress) && ValidateIP(ip_adress) && string.Equals(ip_adress, item.host.Split(':')[0]))
        //        {
        //            Log.Debug("Ip已经存在:" + ip_adress);
        //            return;
        //        }
        //    }
        nvr.host = ip_adress + ":" + port.ToString();
        nvr.type = sdktype;
        nvr.channel = channel;
        nvr.mapping = "";
        nvr.enableMapping = false;
        nvr.userName = username;
        nvr.password = pwd;
        nvr.enable = true;
        nvr.description = "";
        nvr.cameraname = camera_name;
        nvr.connecttype = "IP/域名";
        nvr.equiptype = IsCamera ? "编码设备" : "门禁设备";
        nvr.num = "";
        nvr.pwdsecurity = "";
        nvr.netstate = "";

        //3、向服务器提交添加信息
        string nvrstr = JsonUtility.ToJson(nvr, true);
         Log.Debug("向服务器提交添加设备信息:" + nvrstr);
        HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info", AddDevCallback, true, true, false, nvrstr);
        Log.Debug("http://" +GameStart.IP  + "/api/personnel/cam/info");





    }
    /// <summary>
    /// 添加设备回调
    /// </summary>
    /// <param name="args"></param>
    public void AddDevCallback(HttpCallBackArgs args)
    {

        AddValue addValue = JsonUtility.FromJson<AddValue>(args.Value);
        Log.Debug("Http收到服务器返回的添加设备信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value) && addValue.code == "200")
        {
            nvr.id = addValue.data.id;
            GameStart.Instance.Cameranvrs.Add(nvr);
            Log.Debug(" GameStart.Instance.Cameranvrs:" + GameStart.Instance.Cameranvrs.Count);
            SaveNvrConfiguration_outside(GameStart.Instance.Cameranvrs);

            //更新nvr列表
            //LoadNvrConfiguration();

            //初始化新添加的camera
            NVRConfiguration.GetInstance().nvrs.Add(nvr);
            NVRManager.Instance.InitSingleSDKNewAdd(nvr);

            //登录
            NVRController.Instance().Login(nvr,false);

            //2、UI实例化
            if (IsCamera)
            {
                initCameraItem(nvr, false);

            }

            if (IsDoor)
                initDoorItem(nvr, false);


        }
        else if (addValue.code == "400")
        {
            Log.Debug(addValue.msg);
            GameStart.Instance.ShowTip("添加设备失败！");
        }


    }

    #endregion

    #region 设备登录状态刷新

    /// <summary>
    /// 监控设备登陆状态刷新
    /// </summary>
    /// <param name="ison"></param>
    void CameraRefrushLoginState(bool ison)
    {

        //for (int i = 0; i < _mainManagentPanel.EquipContains.childCount; i++)
        //{
        //    string host = _mainManagentPanel.EquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text;
        //    NVRController.Instance().Login(host,false);




        //}



    }
    #endregion


    /// <summary>
    /// 监控设备展示监听
    /// </summary>
    /// <param name="ison"></param>
    void CameraShowListener(bool ison)
    {
        if (ison)
        {
            IsCamera = true;
            IsDoor = false;
            ManManagentPanel.cameraOnline = 0;
            ManManagentPanel.cameraOutline = 0;

            if (LoadDataBYServer)
            {
                for (int i = 0; i < _mainManagentPanel.EquipContains.childCount; i++)
                {
                    GameObject.Destroy(_mainManagentPanel.EquipContains.GetChild(i).gameObject);
                }
                HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=编码设备", ReadCameraCallback, false, false, false, null);
            }

            _mainManagentPanel.camera_Tog.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/监控-1");
        }
        else
        {
            _mainManagentPanel.camera_Tog.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/监控-0");

        }

        _mainManagentPanel.RightCameraEquipPage.gameObject.SetActive(ison);

    }
    /// <summary>
    /// 读取监控设备信息的回调
    /// </summary>
    EquipInfoArr AllCameraEquips;
    public void ReadCameraCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("监控信息:" + args.Value);
            AllCameraEquips = JsonUtility.FromJson<EquipInfoArr>(args.Value);
            initCameraItemByServer(AllCameraEquips.records);
        }
        else
        {
            GameStart.Instance.ShowTip("请求服务器失败");
        }


    }

    //根据服务器返回的设备容器初始化cameraitem
    void initCameraItemByServer(List<NVRInformation> nvrlist)
    {

        foreach (var item in nvrlist)
        {
            initCameraItem(item, true);
        }
        //ManManagentPanel.cameraOnline = 0;
        //ManManagentPanel.cameraOutline = 0;
    }
    /// <summary>
    /// 实例化并初始化cameraitem
    /// </summary>
    /// <param name="nvr"></param>
    /// 
    AlgorithmNative native1 = new AlgorithmNative();
    void initCameraItem(NVRInformation nvr, bool needLogin)
    {
        if (nvr.equiptype == "编码设备")
        {
            Log.Debug("初始化监控设备");
            cameraitem = ObjectManager.Instance.InstantiateObject(ConStr.CAMERAITEM);
            cameraitem.transform.SetParent(_mainManagentPanel.EquipContains);
            cameraitem.transform.localRotation = Quaternion.identity;
            cameraitem.transform.localScale = Vector3.one;
            cameraitem.transform.Find("NameTxt").GetComponent<TextMeshProUGUI>().text = nvr.cameraname;
            cameraitem.transform.Find("TypeTxt").GetComponent<TextMeshProUGUI>().text = nvr.connecttype;
            cameraitem.transform.Find("IPTxt").GetComponent<TextMeshProUGUI>().text = nvr.host;
            cameraitem.transform.Find("EquiptypeTxt").GetComponent<TextMeshProUGUI>().text = nvr.equiptype;
            cameraitem.transform.Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = nvr.num;
            cameraitem.transform.Find("SeafTxt").GetComponent<TextMeshProUGUI>().text = nvr.pwdsecurity;
            cameraitem.transform.Find("NetTxt").GetComponent<TextMeshProUGUI>().text = nvr.netstate;
            cameraitem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            cameraitem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {
                if (ison)
                {

                    DeleteCameraList.Add(nvr);
                    Log.Debug("勾选了：" + nvr.cameraname+"id:"+nvr.id);
                }
                else
                {
                    if (DeleteCameraList.Contains(nvr))
                    {
                        DeleteCameraList.Remove(nvr);
                        Log.Debug("取消勾选了:" + nvr.cameraname);
                    }

                }

            });
            cameraitem.transform.Find("OperateBtn").GetComponent<Button>().onClick.RemoveAllListeners();
            cameraitem.transform.Find("OperateBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                //打开设备设置网页
                OpenWebGl(_mainManagentPanel.RightCameraEquipPage, "://" + nvr.Ip + "/doc/page/preview.asp");
                //WWW a = new WWW(Application.streamingAssetsPath + "/testvideo.html");
                //OpenWebGl(_mainManagentPanel.RightCameraEquipPage,   a.url);
                Log.Debug("打开监控设置网页:" + "://" + nvr.Ip + "/doc/page/preview.asp");
                //Log.Debug("打开视频网页："  + a.url);

            });


            cameraitem.transform.Find("SuanfaBtn").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {

                //string althriomstr1 = File.ReadAllText(Application.streamingAssetsPath + "/AlthriomSet/Althriom.txt");
                //if (!string.IsNullOrEmpty(althriomstr1))
                //{
                //    native1 = JsonUtility.FromJson<AlgorithmNative>(althriomstr1);

                //    AlgorithmSet algorithminit = native1.AlgorithnativeList.Find(t => t.camId == nvr.id);
                //    if (algorithminit != null && algorithminit.algorithmStatusObjList.Count != 0)
                //        for (int i = 0; i < algorithminit.algorithmStatusObjList[0].algorithmName.Split(',').Length; i++)
                //        {
                //            Log.Debug("本地添加：" + algorithminit.algorithmStatusObjList[0].algorithmName.Split(',')[i]);
                //            AlthriomnameList.Add(algorithminit.algorithmStatusObjList[0].algorithmName.Split(',')[i]);
                //        }
                //}

                //打开界面请求已经开启了哪些算法的接口
                Log.Debug("获取该路视频算法配置信息url："+ "http://" + GameStart.IP + "/api/tg/cam/algorithm/status?camId=" + nvr.id);
                HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/tg/cam/algorithm/status?camId=" + nvr.id, GetAlgorithmSetCallback, false, false, false, "");


                SetSuanfapanel.gameObject.SetActive(true);
                SetSuanfapanel.Find("addpanel/NameTxt/bg/Name").GetComponent<TextMeshProUGUI>().text = nvr.cameraname;
                SetSuanfapanel.Find("addpanel/IPTxt/bg/Name").GetComponent<TextMeshProUGUI>().text = nvr.host.Split(':')[0];
                SetSuanfapanel.Find("addpanel/facetog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/facetog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("face_rec"))

                            AlthriomnameList.Add("face_rec");

                    }
                    else
                    {

                        if (AlthriomnameList.Contains("face_rec"))
                            AlthriomnameList.Remove("face_rec");
                    }
                });
                SetSuanfapanel.Find("addpanel/cartog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/cartog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("car_plate"))
                            AlthriomnameList.Add("car_plate");
                    }
                    else
                    {

                        if (AlthriomnameList.Contains("car_plate"))
                            AlthriomnameList.Remove("car_plate");
                    }
                });
                SetSuanfapanel.Find("addpanel/jujitog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/jujitog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("person_group"))
                            AlthriomnameList.Add("person_group");
                    }
                    else
                    {
                        if (AlthriomnameList.Contains("person_group"))
                            AlthriomnameList.Remove("person_group");
                    }
                });
                SetSuanfapanel.Find("addpanel/diedaotog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/diedaotog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("person_fall"))
                            AlthriomnameList.Add("person_fall");
                    }
                    else
                    {

                        if (AlthriomnameList.Contains("person_fall"))
                            AlthriomnameList.Remove("person_fall");
                    }
                });
                SetSuanfapanel.Find("addpanel/firetog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/firetog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("fire_smoke"))
                            AlthriomnameList.Add("fire_smoke");
                    }
                    else
                    {
                        if (AlthriomnameList.Contains("fire_smoke"))
                            AlthriomnameList.Remove("fire_smoke");
                    }
                });
                SetSuanfapanel.Find("addpanel/Areatog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/Areatog").GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                {

                    if (value)
                    {

                        if (!AlthriomnameList.Contains("person_brust"))
                            AlthriomnameList.Add("person_brust");
                    }
                    else
                    {

                        if (AlthriomnameList.Contains("person_brust"))
                            AlthriomnameList.Remove("person_brust");
                    }
                });
                AlgorithmNative native = new AlgorithmNative();

                SetSuanfapanel.Find("addpanel/addbtn").GetComponent<Button>().onClick.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/addbtn").GetComponent<Button>().onClick.AddListener(() =>
                {
                    AlgorithmSet Aset = new AlgorithmSet();
                    Aset.camId = nvr.id;
                    Aset.algorithmStatusObjList.Add(algorithmStatus);
                    if (AlthriomnameList.Count != 0)
                    {
                        if (IsExitAlthorimList.Count != 0) //修改
                        {
                            algorithmStatus.status = 2;
                            for (int i = 0; i < AlthriomnameList.Count; i++)
                            {
                                if (i == 0)
                                    algorithmStatus.algorithmName = AlthriomnameList[0];
                                else
                                    algorithmStatus.algorithmName += "," + AlthriomnameList[i];
                            }
                           
                        }
                        else //新增 
                        {
                            algorithmStatus.status = 1; 
                            for (int i = 0; i < AlthriomnameList.Count; i++)
                            {
                                if (i == 0)
                                    algorithmStatus.algorithmName = AlthriomnameList[0];
                                else
                                    algorithmStatus.algorithmName += "," + AlthriomnameList[i];
                            }
                        }
                       
                    }
                    else //全关 
                    {
                        algorithmStatus.status = 0;
                        algorithmStatus.algorithmName="";
                    }

                    string AsetStr = JsonUtility.ToJson(Aset, true);
                    //向服务器发送该设备的算法配置信息
                    HttpNetManager.GetInstance().SendDataStr("http://"+GameStart.IP+"/api/update/cam/algorithm", AlgorithmSetCallback, true, true, false, AsetStr);
                    Log.Debug("向服务器发送算法配置信息1：" + AsetStr);
                    SetSuanfapanel.gameObject.SetActive(false);
                    //AlthriomnameList.Clear();//保存完清空
                });
                SetSuanfapanel.Find("addpanel/canclebtn").GetComponent<Button>().onClick.RemoveAllListeners();
                SetSuanfapanel.Find("addpanel/canclebtn").GetComponent<Button>().onClick.AddListener(() =>
                {
                    SetSuanfapanel.gameObject.SetActive(false);

                });
            });
            if (needLogin)
                NVRController.Instance().Login(nvr,false);
        }
        else
        {

        }


    }
    [Serializable]
    public class GetAlgoriData
    {
        public string msg = string.Empty;
        public int code;
        public List<AlgoriData> data = new List<AlgoriData>(0);
    }
    [Serializable]
    public class AlgoriData
    {
        public int devId;
        public int status;
        public string dtype = string.Empty;
    }
    //获取算法配置信息回调
    public List<string> IsExitAlthorimList = new List<string>();
    private void GetAlgorithmSetCallback(HttpCallBackArgs args)
    {
        IsExitAlthorimList.Clear();


        Log.Debug("收到返回自获取算法配置信息的回调：" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            GetAlgoriData data = JsonUtility.FromJson<GetAlgoriData>(args.Value);
            if (data.code == 200&&data.data.Count!=0)
            {
                AlgoriData algoriData = data.data[0];

                if (!string.IsNullOrEmpty(algoriData.dtype))
                {
                    if (algoriData.dtype.Contains("face_rec"))
                    {
                        SetSuanfapanel.Find("addpanel/facetog").GetComponent<Toggle>().isOn = true;
                        if(!IsExitAlthorimList.Contains("face_rec"))
                        IsExitAlthorimList.Add("face_rec");
                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/facetog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("face_rec"))
                            IsExitAlthorimList.Remove("face_rec");
                    }


                    if (algoriData.dtype.Contains("car_plate"))
                    {
                        SetSuanfapanel.Find("addpanel/cartog").GetComponent<Toggle>().isOn = true;
                        if (!IsExitAlthorimList.Contains("car_plate"))
                            IsExitAlthorimList.Add("car_plate");
                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/cartog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("car_plate"))
                            IsExitAlthorimList.Remove("car_plate");
                    }


                    if (algoriData.dtype.Contains("person_group"))
                    {
                        SetSuanfapanel.Find("addpanel/jujitog").GetComponent<Toggle>().isOn = true;
                        if (!IsExitAlthorimList.Contains("person_group"))
                            IsExitAlthorimList.Add("person_group");
                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/jujitog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("person_group"))
                            IsExitAlthorimList.Remove("person_group");
                    }


                    if (algoriData.dtype.Contains("person_brust"))
                    {
                        SetSuanfapanel.Find("addpanel/Areatog").GetComponent<Toggle>().isOn = true;
                        if (!IsExitAlthorimList.Contains("person_brust"))
                            IsExitAlthorimList.Add("person_brust");
                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/Areatog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("person_brust"))
                            IsExitAlthorimList.Remove("person_brust");
                    }


                    if (algoriData.dtype.Contains("person_fall"))
                    {
                        SetSuanfapanel.Find("addpanel/diedaotog").GetComponent<Toggle>().isOn = true;
                        if (!IsExitAlthorimList.Contains("person_fall"))
                            IsExitAlthorimList.Add("person_fall");

                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/diedaotog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("person_fall"))
                            IsExitAlthorimList.Remove("person_fall");
                    }


                    if (algoriData.dtype.Contains("fire_smoke"))
                    {
                        SetSuanfapanel.Find("addpanel/firetog").GetComponent<Toggle>().isOn = true;
                        if (!IsExitAlthorimList.Contains("fire_smoke"))
                            IsExitAlthorimList.Add("fire_smoke");
                    }
                    else 
                    {
                        SetSuanfapanel.Find("addpanel/firetog").GetComponent<Toggle>().isOn = false;
                        if (IsExitAlthorimList.Contains("fire_smoke"))
                            IsExitAlthorimList.Remove("fire_smoke");
                    }
                }
            }
            else
            {
                GameStart.Instance.ShowTip(data.msg);

                SetSuanfapanel.Find("addpanel/facetog").GetComponent<Toggle>().isOn = false;
                SetSuanfapanel.Find("addpanel/cartog").GetComponent<Toggle>().isOn = false;
                SetSuanfapanel.Find("addpanel/jujitog").GetComponent<Toggle>().isOn = false;
                SetSuanfapanel.Find("addpanel/Areatog").GetComponent<Toggle>().isOn = false;
                SetSuanfapanel.Find("addpanel/diedaotog").GetComponent<Toggle>().isOn = false;
                SetSuanfapanel.Find("addpanel/firetog").GetComponent<Toggle>().isOn = false;
            }
        }

    }
    ///分组

    #region   展示监控分组
    /// <summary>
    /// 监控分组监听
    /// </summary>
    /// <param name="ison"></param>
    void CameraGroupShowListener(bool ison)
    {
        if (ison)
        {
            //HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=编码设备", AllCamerasacllback, false, false, false, null);
            RequestCamreaGroup();

        }
        else
        {
            if (CameraTreeView.childCount != 0)
            {
                for (int i = CameraTreeView.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (CameraTreeView.GetChild(temp).GetComponent<TreeViewItem>() != null && CameraTreeView.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManager.GetInstance().OnDeleteBtnClicked(CameraTreeView.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }
        }
        _mainManagentPanel.RightCameraGroupPage.gameObject.SetActive(ison);

    }
    public void AllCamerasacllback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("监控信息:" + args.Value);
            AllCameraEquips = JsonUtility.FromJson<EquipInfoArr>(args.Value);

        }
        else
        {
            GameStart.Instance.ShowTip("请求服务器失败");
        }


    }


    /// <summary>
    /// 请求监控分组
    /// </summary>
    void RequestCamreaGroup()
    {

        if (LoadDataBYServer)
        {
            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/all/area/info?areaType=2", GetCameraGroupCallback, false, false, false);

        }

    }


    /// <summary>
    /// 从服务器获取监控分组信息
    /// </summary>
    /// <param name="args"></param>
    List<OutlineInfo> outlineInfoList = new List<OutlineInfo>();
    public void GetCameraGroupCallback(HttpCallBackArgs args)
    {

        outlineInfoList.Clear();
        CameraAreaDevsDic.Clear();
        Log.Debug("从服务器接收到的监控区域分组设备信息：" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {

            if (_mainManagentPanel.CameramTreeView.transform.childCount != 0)
            {
                for (int i = _mainManagentPanel.CameramTreeView.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (_mainManagentPanel.CameramTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && _mainManagentPanel.CameramTreeView.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManager.GetInstance().OnDeleteBtnClicked(_mainManagentPanel.CameramTreeView.transform.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }

            CameraAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevList>(args.Value);
            //解析层级关系，生成层及目录

            //拆分接收的树级目录
            for (int i = 0; i < CameraAreaGroupDevs.data.Count; i++)
            {
                ProductTreeDic(CameraAreaGroupDevs.data[i]);
            }
            //生成层级目录
            InitCameraGroup(outlineInfoList);
        }
    }


    /// <summary>
    /// 递归拆分层级目录架构
    /// </summary>
    /// <param name="devs"></param>
    private void ProductTreeDic(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        outline.Type = 2;
        outlineInfoList.Add(outline);
        //if (!CameraAreaDevsDic.ContainsKey(outline.OutlineName))
        //    CameraAreaDevsDic.Add(outline.OutlineName, devs.devList);//区域和设备信息列表绑定
        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                ProductTreeDic(devs.children[i]);
            }
        }

    }

    List<TreeViewItem> allCameraTreeViewItemList = new List<TreeViewItem>();
    /// <summary>
    ///  生成监控树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitCameraGroup(List<OutlineInfo> outlineInfoList)
    {
        allCameraTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            if (string.IsNullOrEmpty(outlineInfoList[i].ParentId) || int.Parse(outlineInfoList[i].ParentId) == 0)
            {
                item1 = _mainManagentPanel.CameramTreeView.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());

                allCameraTreeViewItemList.Add(item1);
            }
            else
            {
                for (int j = 0; j < allCameraTreeViewItemList.Count; j++)
                {
                    if (allCameraTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[i].ParentId))
                    {
                        TreeViewItem childItem = allCameraTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                        childItem.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                        childItem.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                        childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                        childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                        childItem.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                        childItem.GetComponent<ItemScript>().children = outlineInfoList[i].Children;
                        allCameraTreeViewItemList.Add(childItem);
                    }
                }
            }
        }


    }
    /// <summary>
    /// 获取某一监控分组下所有的设备信息
    /// </summary>
    /// <param name="args"></param>
    //public void GetOneCameraGroupDevs(HttpCallBackArgs args)
    //{
    //    Log.Debug("该分组下设备信息：" + args.Value);
    //    if (!string.IsNullOrEmpty(args.Value))
    //    {

    //        areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
    //        for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
    //        {
    //            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
    //        }
    //        if (areaDevsData.records.Count != 0)
    //        {
    //            for (int i = 0; i < areaDevsData.records.Count; i++)
    //            {
    //                int index = i;
    //                //ObjectManager.Instance.InstantiateObjectAsync(ConStr.CAMERAFILETOG, InitCameraGroupsDevs, LoadResPriority.RES_MIDDLE, false, CameraAreaGroupDevs.records[index].devList);
    //                InitCameraGroupsDevs(areaDevsData.records[index].devList);
    //            }
    //        }


    //    }
    //}
    private void InitCameraGroupsDevs(string areaname, List<NVRInformation> listnvr)
    {
        if (!CameraAreaDevsDic.ContainsKey(areaname))
        {
            CameraAreaDevsDic.Add(areaname, listnvr);//区域和设备信息列表绑定
            Log.Debug(areaname + "    " + listnvr.Count);
        }
        for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
        }

        for (int i = 0; i < listnvr.Count; i++)
        {
            initCameragroupitem(listnvr[i]);
        }

        ///如果当前导入设备界面正在开启，那么每点击一次区域刷新一次
        if (_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg").gameObject.activeInHierarchy)
        {
            FindAreaCamera(TreeManager.ChoiceAreaName);

        }



    }
    /// <summary>
    ///新建区域分组选择时删除之前分组展示的设备信息
    /// </summary>
    private void OndestroyAreaEquips() {
        for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
        }

        for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 实例化分组下的监控实例
    /// </summary>
    private void initCameragroupitem(NVRInformation nvr)
    {


        GameObject dooritem = ObjectManager.Instance.InstantiateObject(ConStr.DOORItem);
        dooritem.transform.SetParent(_mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content"));
        resetPrefab(dooritem);
        dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().text = nvr.cameraname;
        dooritem.transform.Find("IPTxt").GetComponent<TextMeshProUGUI>().text = nvr.Ip;
        dooritem.transform.Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = nvr.num;
        dooritem.transform.Find("NetTxt").GetComponent<TextMeshProUGUI>().text = nvr.netstate;
        dooritem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        dooritem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                Log.Debug("选择了监控:" + nvr.cameraname);
            }
        });
        dooritem.transform.Find("editorbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        dooritem.transform.Find("editorbtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Log.Debug("编辑:" + nvr.cameraname);
            dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().Select();
            dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().onEndEdit.AddListener((txt) =>
            {
                nvr.cameraname = txt;
                string str = JsonConvert.SerializeObject(nvr);
                Log.Error("监控改名信息："+str);
                HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/dev/info", ModifyequipcallBack, true, true, false, str);

            });
        });
        dooritem.transform.Find("delbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        dooritem.transform.Find("delbtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Log.Debug("移除:" + nvr.cameraname);
        });
    }


    #endregion


    #region  分组导入设备

    /// <summary>
    ///    监控分组导入监控设备
    /// </summary>
    /// <param name="ison"></param>
    private void ImportCamera(bool ison)
    {
        if (LoginManager.Ins.IsCameraSet)
        {

            _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg").gameObject.SetActive(false);
            _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg").gameObject.SetActive(true);
            Log.Debug("导入区域:" + TreeManager.ChoiceAreaName);
            FindAreaCamera(TreeManager.ChoiceAreaName);

        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有监控点配置权限！");
        }

    }
    /// <summary>
    /// 查找监控分组下已经存在哪些设备，可以导入哪些设备
    /// </summary>
    /// <param name="areaname"></param>
    private void FindAreaCamera(string areaname)
    {
        addCameralist.Clear();
        //先清除
        for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content").childCount; i++)
        {

            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content").GetChild(i).gameObject);
        }
        for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(i).gameObject);

        }


        //已经导入
        List<NVRInformation> devlist = new List<NVRInformation>();
        if (CameraAreaDevsDic.TryGetValue(areaname, out devlist))
        {
            if (devlist.Count != 0)
            {
                for (int i = 0; i < devlist.Count; i++)
                {
                    GameObject dooritemS = ObjectManager.Instance.InstantiateObject(ConStr.DOORItemSHORT);
                    dooritemS.transform.SetParent(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content"));
                    resetPrefab(dooritemS);
                    dooritemS.transform.Find("itemnameTxt").GetComponent<TextMeshProUGUI>().text = devlist[i].cameraname;
                }
            }
        }



        //可以导入
        _mainManagentPanel.StartCoroutine(CameraCanImportByServer(areaname, CameraAreaDevsDic));


    }
    /// <summary>
    /// 比对筛选可以导入哪些设备
    /// </summary>
    /// <param name="areaname"></param>
    /// <param name="AreaDevsDic"></param>
    /// <returns></returns>
    IEnumerator CameraCanImportByServer(string areaname, Dictionary<string, List<NVRInformation>> AreaDevsDic)
    {
        yield return new WaitForEndOfFrame();
        foreach (var item in AllCameraEquips.records)
        {

            if (!string.IsNullOrEmpty(areaname) && item.equiptype == "编码设备")
            {

                if (AreaDevsDic.ContainsKey(areaname) && !AreaDevsDic[areaname].Contains(item))
                {

                    GameObject importcameraitem = ObjectManager.Instance.InstantiateObject(ConStr.IMPORTDOORITEM);
                    importcameraitem.transform.SetParent(_mainManagentPanel.RightCameraGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content"));
                    resetPrefab(importcameraitem);
                    importcameraitem.transform.Find("doornametxt").GetComponent<TextMeshProUGUI>().text = item.cameraname;
                    importcameraitem.transform.Find("choicetog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    importcameraitem.transform.Find("choicetog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
                    {
                        if (ison)
                        {
                            addCameralist.Add(item);


                        }
                        else
                        {
                            addCameralist.Remove(item);
                        }
                    });
                }


            }


        }

    }

    /// <summary>
    /// 向服务器发送导入监控设备信息的回调
    /// </summary>
    /// <param name="args"></param>
    public void AddMonitorAreaCallback(HttpCallBackArgs args)
    {
        Log.Debug("收到导入设备回传信息:" + args.Value);
        AddValue addValue = JsonUtility.FromJson<AddValue>(args.Value);
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains(":200"))
        {
            GameStart.Instance.ShowTip("导入监控设备成功");
            //刷新一下界面
            //刷新
            //for (int i = 0; i < _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
            //{
            //    GameObject.Destroy(_mainManagentPanel.RightCameraGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
            //}
            //再次请求服务器刷新

            //HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraGroupDevs, false, false, false);

            _mainManagentPanel.RightCameraGroupPage.Find("ItemsBg").gameObject.SetActive(true);
            _mainManagentPanel.RightCameraGroupPage.Find("importGroupBg").gameObject.SetActive(false);

        }
        else
        {
            GameStart.Instance.ShowTip("导入设备失败");
        }




    }


    #endregion


    #region  添加分组

    #endregion

    #endregion

    #region  门禁模块逻辑

    #region 门禁登录状态刷新

    /// <summary>
    /// 门禁刷新
    /// </summary>
    /// <param name="ison"></param>
    void DoorRefrushLoginState(bool ison)
    {

        //for (int i = 0; i < _mainManagentPanel.DoorEquipContains.childCount; i++)
        //{
        //    string host = _mainManagentPanel.DoorEquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text;
        //    NVRController.Instance().LoginDoor(host);



        //}



    }

    #endregion

    #region 门禁设备相关
    /// <summary>
    /// 添加门禁设备
    /// </summary>
    /// <param name="ison"></param>
    private void AddDoorClick(bool ison)
    {
        //权限控制
        if (LoginManager.Ins.IsDoorSet)
        {
            if (ison)
            {
                Transform.Find("AddequipMaskPanel").gameObject.SetActive(true);
                _mainManagentPanel.Name_input.text = string.Empty;
                camera_name = string.Empty;
                _mainManagentPanel.Ip_input.text = string.Empty;
                ip_adress = string.Empty;
                _mainManagentPanel.Point_input.text = string.Empty;
                port = 0;
                _mainManagentPanel.User_input.text = string.Empty;
                username = string.Empty;
                _mainManagentPanel.Pwd_input.text = string.Empty;
                pwd = string.Empty;
            }
        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有门禁点配置权限！");
        }

    }
    /// <summary>
    /// 删除门禁设备
    /// </summary>
    /// <param name="ison"></param>
    void DeleteDoorItem(bool ison)
    {



        if (LoginManager.Ins.IsDoorSet)
        {
            if (ison)
            {
                for (int i = 0; i < DeleteDoorList.Count; i++)
                {
                    int index = i;
                    Log.Debug("删除了:" + DeleteDoorList[index].id);
                    DeleteID deleteID = new DeleteID();
                    deleteID.id = DeleteDoorList[index].id;
                    string strd = JsonUtility.ToJson(deleteID, true);
                    Log.Debug(strd);
                    HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/del/cam/info", DeleteDoorCallback, true, true, false, strd);

                }

            }
        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有门禁点配置权限！");
        }


    }
    /// <summary>
    /// 删除门禁回调
    /// </summary>
    /// <param name="args"></param>
    public void DeleteDoorCallback(HttpCallBackArgs args)
    {
        Log.Debug("删除设备收到服务器消息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains("200"))
        {
            GameStart.Instance.ShowTip("删除门禁设备成功!");
        }
        Timer.Register(0.2f, () =>
        {  //重新请求展示刷新
            for (int i = 0; i < _mainManagentPanel.DoorEquipContains.childCount; i++)
            {
                GameObject.Destroy(_mainManagentPanel.DoorEquipContains.GetChild(i).gameObject);
            }
            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=门禁设备", ReadDoordevsCallback, false, false, false, null);
        });
    }


    ///门禁设备
    /// <summary>
    /// 门禁设备展示监听
    /// </summary>
    /// <param name="ison"></param>
    void DoorShowListener(bool ison)
    {
        if (ison)
        {
            IsCamera = false;
            IsDoor = true;
            Log.Debug("门禁");
            ManManagentPanel.GetInstance().doorOnline = 0;
            ManManagentPanel.GetInstance().doorOutline = 0;
            if (LoadDataBYServer)
            {
                //请求服务器
                HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=门禁设备", ReadDoordevsCallback, false, false, false, null);
            }

            _mainManagentPanel.door_Tog.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/门禁-1");
        }
        else
        {
            _mainManagentPanel.door_Tog.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/门禁-0");
        }
        _mainManagentPanel.RightDoorEquipPage.gameObject.SetActive(ison);
        _mainManagentPanel.DoorRefrush_Toggle.onValueChanged.Invoke(true);
        Debug.LogError("SADASDASDASDASDASDSADASDSA");
    }

    /// <summary>
    /// 读取门禁信息回调
    /// </summary>
    EquipInfoArr AllDoorEquips;
    public void ReadDoordevsCallback(HttpCallBackArgs args)
    {

        if (!string.IsNullOrEmpty(args.Value))
        {
            AllDoorEquips = JsonUtility.FromJson<EquipInfoArr>(args.Value);
            initDoorItemByServer(AllDoorEquips.records);
            Log.Debug("门禁信息:" + args.Value);
        }
        else
        {
            GameStart.Instance.ShowTip("请求服务器失败");
        }

    }
    //根据服务器返回的设备容器初始化dooritem
    void initDoorItemByServer(List<NVRInformation> nvrlist)
    {
        for (int i = 0; i < _mainManagentPanel.DoorEquipContains.childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.DoorEquipContains.GetChild(i).gameObject);
        }

        foreach (var item in nvrlist)
        {
            initDoorItem(item, true);
        }

        //ManManagentPanel.doorOutline = 0;    
        //ManManagentPanel.doorOnline = 0;



    }
    /// <summary>
    /// 门禁设备初始化
    /// </summary>
    /// <param name="nvr"></param>
    /// <param name="needLogin"></param>
    void initDoorItem(NVRInformation nvr, bool needLogin)
    {
        if (nvr.equiptype == "门禁设备")
        {
            cameraitem = ObjectManager.Instance.InstantiateObject(ConStr.CAMERAITEM);
            cameraitem.transform.SetParent(_mainManagentPanel.DoorEquipContains);
            cameraitem.transform.localRotation = Quaternion.identity;
            cameraitem.transform.localScale = Vector3.one;
            cameraitem.transform.Find("NameTxt").GetComponent<TextMeshProUGUI>().text = nvr.cameraname;
            cameraitem.transform.Find("TypeTxt").GetComponent<TextMeshProUGUI>().text = nvr.connecttype;
            cameraitem.transform.Find("IPTxt").GetComponent<TextMeshProUGUI>().text = nvr.host;
            cameraitem.transform.Find("EquiptypeTxt").GetComponent<TextMeshProUGUI>().text = nvr.equiptype;
            cameraitem.transform.Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = nvr.num;
            cameraitem.transform.Find("SeafTxt").GetComponent<TextMeshProUGUI>().text = nvr.pwdsecurity;
            cameraitem.transform.Find("NetTxt").GetComponent<TextMeshProUGUI>().text = nvr.netstate;
            cameraitem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {
                if (ison)
                {

                    DeleteDoorList.Add(nvr);
                    Log.Debug("勾选了：" + nvr.cameraname);
                }
                else
                {
                    if (DeleteDoorList.Contains(nvr))
                    {
                        DeleteDoorList.Remove(nvr);
                        Log.Debug("取消勾选了:" + nvr.cameraname);
                    }

                }

            });
            cameraitem.transform.Find("SuanfaBtn").gameObject.SetActive(false);
            cameraitem.transform.Find("OperateBtn").GetComponent<Button>().onClick.RemoveAllListeners();
            cameraitem.transform.Find("OperateBtn").GetComponent<Button>().onClick.AddListener(() =>
            {

                //打开内嵌网页设置界面
                OpenWebGl(_mainManagentPanel.RightDoorEquipPage, "://" + nvr.Ip + "/doc/index.html");
            });
            if (needLogin) 
            {
                NVRController.Instance().LoginDoor(nvr);
            }
               
        }


    }
    #endregion


    ///门禁分组

    #region  展示门禁分组信息

    /// <summary>
    /// 门禁分组展示监听
    /// </summary>
    /// <param name="ison"></param>
    void DoorGroupShowListener(bool ison)
    {
        if (ison)
        {
            //HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=门禁设备", AllDoorsacllback, false, false, false, null);
            InitDoorGroup();
            _mainManagentPanel.RightDoorGroupPage.gameObject.SetActive(ison);
        }
        else
        {
      
        }
        if (DoorTreeView.childCount != 0)
        {
            for (int i = DoorTreeView.childCount - 1; i >= 0; i--)
            {

                int temp = i;
                if (DoorTreeView.GetChild(temp).GetComponent<TreeViewItem>() != null && DoorTreeView.GetChild(temp).gameObject.activeInHierarchy)
                {

                    TreeManager.GetInstance().OnDeleteBtnClicked(DoorTreeView.GetChild(0).GetComponent<TreeViewItem>());
                }

            }

        }
        _mainManagentPanel.RightDoorGroupPage.gameObject.SetActive(ison);

    }
    public void AllDoorsacllback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("门禁信息:" + args.Value);
            AllDoorEquips = JsonUtility.FromJson<EquipInfoArr>(args.Value);

        }
        else
        {
            GameStart.Instance.ShowTip("请求服务器失败");
        }


    }
    /// <summary>
    /// 实例化门禁分组
    /// </summary>
    void InitDoorGroup()
    {
        if (LoadDataBYServer)
        {
            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/all/area/info?areaType=1", GetDoorGroupCallback, false, false, false);

        }

    }

    public AreaGroupDevList DoorAreaGroupDevs;
    /// <summary>
    /// 获取门禁区域分组设备信息回调
    /// </summary>
    /// <param name="args"></param>
    List<OutlineInfo> DooroutlineInfoList = new List<OutlineInfo>();
    public void GetDoorGroupCallback(HttpCallBackArgs args)
    {
        Log.Debug("从服务器接收到的门禁区域分组信息：" + args.Value);
        DooroutlineInfoList.Clear();
        DoorAreaDevsDic.Clear();
        if (!string.IsNullOrEmpty(args.Value))
        {

            DoorAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevList>(args.Value);

            //拆分接收的树级目录
            for (int i = 0; i < DoorAreaGroupDevs.data.Count; i++)
            {
                ProductDoorTreeDic(DoorAreaGroupDevs.data[i]);
            }

            //生成层级目录
            InitDoorGroup(DooroutlineInfoList);

        }
    }
    /// <summary>
    /// 递归拆分门禁层及目录架构
    /// </summary>
    /// <param name="devs"></param>
    private void ProductDoorTreeDic(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Type = 1;
        DooroutlineInfoList.Add(outline);


        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                ProductDoorTreeDic(devs.children[i]);
            }
        }

    }


    List<TreeViewItem> allDoorTreeViewItemList = new List<TreeViewItem>();
    /// <summary>
    /// 生成门禁树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitDoorGroup(List<OutlineInfo> outlineInfoList)
    {
        allDoorTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            if (string.IsNullOrEmpty(outlineInfoList[i].ParentId) || int.Parse(outlineInfoList[i].ParentId) == 0)
            {
                item1 = _mainManagentPanel.DoorTreeView.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                item1.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                //item1.GetComponent<ItemScript>().clickBtn.onClick.AddListener(() =>
                //{

                //    string url = "http://" +GameStart.IP  + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=" + item1.GetComponent<ItemScript>().id + "&areaName=" + item1.GetComponent<ItemScript>().labelText.text + "&areaType=1";
                //    Log.Debug("Url:" + url);
                //HttpNetManager.GetInstance().SendDataStr(url, GetOneDoorGroupDevs, false, false, false);
                //});

                allDoorTreeViewItemList.Add(item1);
            }
            else
            {
                for (int j = 0; j < allDoorTreeViewItemList.Count; j++)
                {
                    if (allDoorTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[i].ParentId))
                    {
                        TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                        childItem.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                        childItem.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                        childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                        childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                        childItem.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                        //childItem.GetComponent<ItemScript>().clickBtn.onClick.AddListener(() =>
                        //{
                        //    string url = "http://" +GameStart.IP  + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=" + childItem.GetComponent<ItemScript>().id + "&areaName=" + childItem.GetComponent<ItemScript>().labelText.text + "&areaType=1";
                        //    Log.Debug("Url:" + url);
                        //    HttpNetManager.GetInstance().SendDataStr(url, GetOneDoorGroupDevs, false, false, false);

                        //});

                        allDoorTreeViewItemList.Add(childItem);
                    }
                }
            }
        }
        //if (item1 != null)
        //    TreeManager.GetInstance().OnItemCustomEvent(item1, CustomEvent.ItemClicked, "门禁");

    }

    #endregion


    #region 导入门禁设备


    /// <summary>
    /// 导入门禁设备按钮监听
    /// </summary>
    /// <param name="ison"></param>
    private void ImportDoor(bool ison)
    {
        if (LoginManager.Ins.IsDoorSet)
        {

            _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg").gameObject.SetActive(false);
            _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg").gameObject.SetActive(true);
            Log.Debug("导入区域:" + TreeManager.ChoiceAreaName);
            FindAreaDoors(TreeManager.ChoiceAreaName);


        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有门禁点配置权限！");
        }

    }
    /// <summary>
    /// 查找这个门禁分组下已经存在哪些设备，不存在哪些设备
    /// </summary>
    /// <param name="areaname"></param>
    private void FindAreaDoors(string areaname)
    {
        adddoorlist.Clear();

        //先清除
        for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content").childCount; i++)
        {

            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content").GetChild(i).gameObject);
        }
        for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content").GetChild(i).gameObject);
        }


        List<NVRInformation> devlist = new List<NVRInformation>();
        //已经导入
        if (DoorAreaDevsDic.TryGetValue(areaname, out devlist))
        {
            if (devlist.Count != 0)
            {
                for (int i = 0; i < devlist.Count; i++)
                {
                    GameObject dooritemS = ObjectManager.Instance.InstantiateObject(ConStr.DOORItemSHORT);
                    dooritemS.transform.SetParent(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgalready/Scrollitem/Viewport/Content"));
                    resetPrefab(dooritemS);
                    dooritemS.transform.Find("itemnameTxt").GetComponent<TextMeshProUGUI>().text = devlist[i].cameraname;
                }
            }
        }
        //可以导入
        ///
        _mainManagentPanel.StartCoroutine(DoorCanImportByServer(areaname, DoorAreaDevsDic));



    }
    /// <summary>
    /// 筛选出可以导入的门禁设备
    /// </summary>
    /// <param name="areaname"></param>
    /// <param name="AreaDevsDic"></param>
    /// <returns></returns>
    IEnumerator DoorCanImportByServer(string areaname, Dictionary<string, List<NVRInformation>> AreaDevsDic)
    {
        yield return new WaitForEndOfFrame();
        foreach (var item in AllDoorEquips.records)
        {

            if (!string.IsNullOrEmpty(areaname) && item.equiptype == "门禁设备")
            {

                if (AreaDevsDic.ContainsKey(areaname) && !AreaDevsDic[areaname].Contains(item))
                {
                    GameObject importdooritem = ObjectManager.Instance.InstantiateObject(ConStr.IMPORTDOORITEM);
                    importdooritem.transform.SetParent(_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg/bgimport/Scroll View/Viewport/Content"));
                    resetPrefab(importdooritem);
                    importdooritem.transform.Find("doornametxt").GetComponent<TextMeshProUGUI>().text = item.cameraname;
                    importdooritem.transform.Find("choicetog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    importdooritem.transform.Find("choicetog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
                    {
                        if (ison)
                        {
                            adddoorlist.Add(item);


                        }
                        else
                        {
                            adddoorlist.Remove(item);
                        }
                    });
                }


            }


        }

    }


    /// <summary>
    /// 导入门禁设备http回调
    /// </summary>
    /// <param name="args"></param>
    public void AddAreaCallback(HttpCallBackArgs args)
    {
        Log.Debug("收到导入门禁设备回传信息:" + args.Value);
        AddValue addValue = JsonUtility.FromJson<AddValue>(args.Value);
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains(":200"))
        {
            GameStart.Instance.ShowTip("导入门禁设备成功！");
            //刷新一下界面
            //刷新
            //for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
            //{
            //    GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
            //}
            //再次请求服务器刷新

            //HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=1", GetDoorGroupDevs, false, false, false);
            //Log.Error("刷新");
            _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg").gameObject.SetActive(true);
            _mainManagentPanel.RightDoorGroupPage.Find("importGroupBg").gameObject.SetActive(false);

        }
        else
        {
            GameStart.Instance.ShowTip("导入设备失败");
        }
        //nvr.id = addValue.data.id;
        //GameStart.Instance.Cameranvrs.Add(nvr);
        //SaveNvrConfiguration_outside(GameStart.Instance.Cameranvrs);
    }




    #endregion


    #region  获取某个门禁分组下的设备信息

    /// <summary>
    /// 获取某个门禁分组下的设备信息
    /// </summary>
    /// <param name="args"></param>
    //public void GetOneDoorGroupDevs(HttpCallBackArgs args)
    //{
    //    Log.Debug("该门禁分组下设备信息：" + args.Value);
    //    if (!string.IsNullOrEmpty(args.Value))
    //    {

    //        areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
    //        for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
    //        {
    //            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
    //        }
    //        if (areaDevsData.records.Count != 0)
    //        {
    //            for (int i = 0; i < areaDevsData.records.Count; i++)
    //            {
    //                int index = i;
    //                InitDoorGroupsDevs(areaDevsData.records[index].devList);
    //            }
    //        }


    //    }
    //}


    /// <summary>
    /// 实例化分组下的门禁实例
    /// </summary>
    /// <param name="listnvr"></param>
    private void InitDoorGroupsDevs(string areaname, List<NVRInformation> listnvr)
    {
        if (!DoorAreaDevsDic.ContainsKey(areaname))
        {
            DoorAreaDevsDic.Add(areaname, listnvr);//区域和设备信息列表绑定
            Log.Debug(areaname + "    " + listnvr.Count);
        }

        for (int i = 0; i < _mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").childCount; i++)
        {
            GameObject.Destroy(_mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content").GetChild(i).gameObject);
        }
        for (int i = 0; i < listnvr.Count; i++)
        {
            initDoorgroupitem(listnvr[i]);
        }
        ///如果当前导入设备界面正在开启，那么每点击一次区域刷新一次
        if (_mainManagentPanel.RightDoorGroupPage.Find("importGroupBg").gameObject.activeInHierarchy)
        {
            FindAreaDoors(TreeManager.ChoiceAreaName);

        }

    }
    /// <summary>
    /// 实例化分组下的门禁实例
    /// </summary>
    private void initDoorgroupitem(NVRInformation nvr)
    {


        GameObject dooritem = ObjectManager.Instance.InstantiateObject(ConStr.DOORItem);
        dooritem.transform.SetParent(_mainManagentPanel.RightDoorGroupPage.Find("ItemsBg/Scroll View/Viewport/Content"));
        resetPrefab(dooritem);
        dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().text = nvr.cameraname;
        dooritem.transform.Find("IPTxt").GetComponent<TextMeshProUGUI>().text = nvr.Ip;
        dooritem.transform.Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = nvr.num;
        dooritem.transform.Find("NetTxt").GetComponent<TextMeshProUGUI>().text = nvr.netstate;
        dooritem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        dooritem.transform.Find("choiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                Log.Debug("选择了门禁:" + nvr.cameraname);
            }
        });
        dooritem.transform.Find("editorbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        dooritem.transform.Find("editorbtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Log.Debug("编辑:" + nvr.cameraname);
            dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().Select();
            dooritem.transform.Find("NameTxt").GetComponent<TMP_InputField>().onEndEdit.AddListener((txt) =>
            {
              
                nvr.cameraname = txt;
                string str = JsonConvert.SerializeObject(nvr);
                HttpNetManager.GetInstance().SendDataStr("http://"+GameStart.IP+ "/api/personnel/dev/info",ModifyequipcallBack,true,true,false,str);
                Log.Error(nvr.host+"改名");

            });
        });
        dooritem.transform.Find("delbtn").GetComponent<Button>().onClick.RemoveAllListeners();
        dooritem.transform.Find("delbtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            Log.Debug("移除:" + nvr.cameraname);
            GameStart.Instance.ShowTip("不允许移除门禁设备!");
        });
    }
    private void ModifyequipcallBack(HttpCallBackArgs args) 
    {
        if (!string.IsNullOrEmpty(args.Value)) 
        {
            if (args.Value.Contains("200"))
                GameStart.Instance.ShowTip("改名成功");
            else if(args.Value.Contains("400")||args.Value.Contains("401"))
                GameStart.Instance.ShowTip("改名失败");
            Log.Debug(args.Value);
        }
    }
    #endregion


    #endregion


    #region  添加设备同步本地文件
    public string jsonName = "NvrConfiguration.json";
    public string jsonPath = Path.Combine(Application.streamingAssetsPath, "Configurations");
    public void SaveNvrConfiguration_outside(List<NVRInformation> nvrs)
    {
        string path = Application.streamingAssetsPath;
        jsonPath = Path.Combine(path, "Configurations");
        Log.Debug("保存路径:" + jsonPath);
        if (!Directory.Exists(jsonPath))
        {
            Directory.CreateDirectory(jsonPath);
        }
        List<NVRInformation> newnvrs = new List<NVRInformation>();
        foreach (var item in nvrs)
        {
            newnvrs.Add(item);
        }
        var info = JsonUtility.ToJson(new Wrapper(newnvrs), true);
        var file = Path.Combine(jsonPath, jsonName);
        Log.Debug("本地保存:" + info + "路径:" + file);
        File.WriteAllText(file, info, Encoding.UTF8);

    }
    public List<NVRInformation> nvrs = new List<NVRInformation>();
    public void LoadNvrConfiguration()
    {
        jsonPath = Path.Combine(Application.streamingAssetsPath, "Configurations");
        var file = Path.Combine(jsonPath, jsonName);
        if (File.Exists(file))
        {

            var info = File.ReadAllText(file);
            var obj = JsonUtility.FromJson<Wrapper>(info);
            if (null != obj)
            {
                nvrs = obj.arr;
            }
        }
        else
        {
            Debug.LogWarning($"{nameof(NVRManager)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");
        }
    }

    #endregion

    #region 算法配置相关


    [Serializable]
    public class AlgorithmNative
    {
        public List<AlgorithmSet> AlgorithnativeList = new List<AlgorithmSet>();

    }
    [Serializable]
    public class AlgorithmSet
    {

        public int camId;
        public List<AlgorithmStatus> algorithmStatusObjList = new List<AlgorithmStatus>();
    }
    [Serializable]
    public class AlgorithmStatus
    {
        public int status;
        public string algorithmName = string.Empty;

    }
    public string Althriom = string.Empty;
    public List<string> AlthriomnameList = new List<string>();
    AlgorithmStatus algorithmStatus = new AlgorithmStatus();

    //算法配置信息回调
    [Serializable]
    public class response 
    {
        public string msg = string.Empty;
        public int code;
    }
    private void AlgorithmSetCallback(HttpCallBackArgs args)
    {
        Log.Debug("收到返回自算法配置信息的回调：" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
         response rs =  JsonConvert.DeserializeObject<response>(args.Value);
            if (rs.code == 200)
                GameStart.Instance.ShowTip(rs.msg);
            else 
            {
                GameStart.Instance.ShowTip("算法配置失败！");
            }
        }

    }

    #endregion

    #region 内嵌网页相关
    //内嵌网页
    CanvasWebViewPrefab _focusedPrefab;
    HardwareKeyboardListener _hardwareKeyboardListener;
    async void OpenWebGl(Transform parent, string URL)
    {
        var mainWebViewPrefab = CanvasWebViewPrefab.Instantiate();
        mainWebViewPrefab.Resolution = 1.5f;
        mainWebViewPrefab.PixelDensity = 2;
        mainWebViewPrefab.Native2DModeEnabled = true;
        mainWebViewPrefab.transform.SetParent(parent, false);
        //注册关闭页面回调
        mainWebViewPrefab.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            mainWebViewPrefab.Destroy();
            GameObject.Destroy(_hardwareKeyboardListener.gameObject);
        });


        var rectTransform = mainWebViewPrefab.transform as RectTransform;
        rectTransform.anchoredPosition3D = Vector3.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        mainWebViewPrefab.transform.localScale = Vector3.one;
        _focusedPrefab = mainWebViewPrefab;

        _setUpKeyboards();

        // Wait for the CanvasWebViewPrefab to initialize, because the CanvasWebViewPrefab.WebView property
        // is null until the prefab has initialized.
        await mainWebViewPrefab.WaitUntilInitialized();

        // The CanvasWebViewPrefab has initialized, so now we can use its WebViewPrefab.WebView property.
        var webViewWithPopups = mainWebViewPrefab.WebView as IWithPopups;
        if (webViewWithPopups == null)
        {
            mainWebViewPrefab.WebView.LoadHtml(NOT_SUPPORTED_HTML);
            return;
        }

         Log.Debug("Loading Pinterest as an example because it uses popups for third party login. Click 'Login', then select Facebook or Google to open a popup for authentication.");
        mainWebViewPrefab.WebView.LoadUrl(URL);
        
        webViewWithPopups.SetPopupMode(PopupMode.LoadInNewWebView);
        webViewWithPopups.PopupRequested += async (webView, eventArgs) =>
        {
             Log.Debug("Popup opened with URL: " + eventArgs.Url);
            var popupPrefab = CanvasWebViewPrefab.Instantiate(eventArgs.WebView);
            popupPrefab.Resolution = mainWebViewPrefab.Resolution;
            _focusedPrefab = popupPrefab;

            popupPrefab.transform.SetParent(parent, false);
            var popupRectTransform = popupPrefab.transform as RectTransform;
            popupRectTransform.anchoredPosition3D = Vector3.zero;
            popupRectTransform.offsetMin = Vector2.zero;
            popupRectTransform.offsetMax = Vector2.zero;
            popupPrefab.transform.localScale = Vector3.one;
            // Place the popup in front of the main webview.
            var localPosition = popupPrefab.transform.localPosition;
            localPosition.z = 0.1f;
            popupPrefab.transform.localPosition = localPosition;

            await popupPrefab.WaitUntilInitialized();
            popupPrefab.WebView.CloseRequested += (popupWebView, closeEventArgs) =>
            {
                 Log.Debug("Closing the popup");
                _focusedPrefab = mainWebViewPrefab;
                popupPrefab.Destroy();
            };
        };


    }
    void _setUpKeyboards()
    {

        // Send keys from the hardware (USB or Bluetooth) keyboard to the webview.
        // Use separate KeyDown() and KeyUp() methods if the webview supports
        // it, otherwise just use IWebView.SendKey().
        // https://developer.vuplex.com/webview/IWithKeyDownAndUp
        _hardwareKeyboardListener = HardwareKeyboardListener.Instantiate();
        _hardwareKeyboardListener.KeyDownReceived += (sender, eventArgs) =>
        {
            var webViewWithKeyDown = _focusedPrefab.WebView as IWithKeyDownAndUp;
            if (webViewWithKeyDown != null)
            {
                webViewWithKeyDown.KeyDown(eventArgs.Value, eventArgs.Modifiers);
            }
            else
            {
                _focusedPrefab.WebView.SendKey(eventArgs.Value);
            }
        };
        _hardwareKeyboardListener.KeyUpReceived += (sender, eventArgs) =>
        {
            var webViewWithKeyUp = _focusedPrefab.WebView as IWithKeyDownAndUp;
            webViewWithKeyUp?.KeyUp(eventArgs.Value, eventArgs.Modifiers);
        };
    }

    const string NOT_SUPPORTED_HTML = @"
            <body>
                <style>
                    body {
                        font-family: sans-serif;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                        line-height: 1.25;
                    }
                    div {
                        max-width: 80%;
                    }
                    li {
                        margin: 10px 0;
                    }
                </style>
                <div>
                    <p>
                        Sorry, but this 3D WebView package doesn't support yet the <a href='https://developer.vuplex.com/webview/IWithPopups'>IWithPopups</a> interface. Current packages that support popups:
                    </p>
                    <ul>
                        <li>
                            <a href='https://developer.vuplex.com/webview/StandaloneWebView'>3D WebView for Windows and macOS</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidWebView'>3D WebView for Android</a>
                        </li>
                        <li>
                            <a href='https://developer.vuplex.com/webview/AndroidGeckoWebView'>3D WebView for Android with Gecko Engine</a>
                        </li>
                    </ul>
                </div>
            </body>
        ";

    #endregion

    #region 数据模型


    [Serializable]
    public class Datas
    {

        public List<Element> records = new List<Element>();

    }
    [Serializable]
    public class Element
    {

        public int id;//区域id
        public string areaName = string.Empty;//区域名字
        public string remark = string.Empty;
        public int areaType;
        public int parentId;//父级id
        public string level = string.Empty;
        public List<NVRInformation> devList = new List<NVRInformation>();//设备容器
        public List<Element> children = new List<Element>();//子元素


    }

    //新增区域导入设备时向服务器发送
    [Serializable]
    public class addArea
    {
        public int id;
        public string areaName = string.Empty;
        public string remark = string.Empty;
        public List<NVRInformation> devList = new List<NVRInformation>();
        public int areaType;//1是门禁，2是监控
    }

    //修改完人员类型对应权限后上传给服务器数据结构
    [Serializable]
    public class AllAreaGroup 
    {
        public List<List<GetAreaGroupDevs>> typeList = new List<List<GetAreaGroupDevs>>();
    
    }

    //更改层及目录后的数据结构   
    [Serializable]
    public class AreaGroupDevList
    {
        public string msg = string.Empty;
        public int code;
        public List<GetAreaGroupDevs> data = new List<GetAreaGroupDevs>();

    }
    //请求分组设备信息数据结构

    [Serializable]
    public  class GetAreaGroupDevs
    {
        public int id;
        public string areaName = string.Empty;
        public string remark = string.Empty;
        public int areaType;
        public int parentId;
        public string level;
        public List<NVRInformation> devList = new List<NVRInformation>();
        public List<GetAreaGroupDevs> children = new List<GetAreaGroupDevs>();
        //public List<GetAreaGroupDevs> childs = new List<GetAreaGroupDevs>();
        //[Serializable]
        //public class children : GetAreaGroupDevs
        //{
        //    public List<GetAreaGroupDevs> _childs = new List<GetAreaGroupDevs>();

        //    public override List<GetAreaGroupDevs> childs 
        //    {
        //        get 
        //        {
        //            return _childs.ConvertAll<GetAreaGroupDevs>(x=>x);
        //        }
        //        set 
        //         {
        //            _childs = value;

        //        }

        //    }


        //}
    }



    //请求设备信息数据结构
    [Serializable]
    public class EquipInfoArr
    {
        public List<NVRInformation> records;
        public int total;//总计多少条
        public int size;//一页多少条
        public int current;//第几页
        public List<object> orders;
        public bool optimizeCountSql;
        public bool searchCount;
        public object maxLimit;
        public string countId;
        public int pages;//一共多少页
        public EquipInfoArr(List<NVRInformation> arr)
        {
            this.records = arr;
        }
    }

    //区域设备信息数据结构
    [Serializable]
    public class AreaDevsData
    {
        public List<GetAreaGroupDevs> records = new List<GetAreaGroupDevs>();

    }


    //添加设备返回的数据结构
    [Serializable]
    public class AddValue
    {
        public string msg;
        public string code;
        public data data;
    }


    [Serializable]
    public class data
    {
        public int id;
    }

    [Serializable]
    public class DeleteID
    {
        public int id;
    }
    #endregion

    #region  杂项

    /// <summary>验证IP是否合法</summary>
    public static bool ValidateIP(string strIP)
    {
        if (string.IsNullOrEmpty(strIP)) return false;
        Regex validipregex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
        return (strIP != "" && validipregex.IsMatch(strIP.Trim())) ? true : false;
    }
    /// <summary>验证端口号</summary>
    public static bool ValidatePort(string strPort)
    {
        if (string.IsNullOrEmpty(strPort)) return false;
        Regex validipregex = new Regex(@"^([0-9]|[1-9]\d|[1-9]\d{2}|[1-9]\d{3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$");
        return (strPort != "" && validipregex.IsMatch(strPort.Trim())) ? true : false;
    }
    /// <summary>验证IP:Port是否合法</summary>
    public static bool ValidateIPAndPort(string strIPAndPort)
    {
        if (string.IsNullOrEmpty(strIPAndPort)) return false;
        Regex validipregex = new Regex(@"^(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d{2}|2[0-4]\d|25[0-5]):([0-9]|[1-9]\d|[1-9]\d{2}|[1-9]\d{3}|[1-5]\d{4}|6[0-4]\d{3}|65[0-4]\d{2}|655[0-2]\d|6553[0-5])$");
        return (strIPAndPort != "" && validipregex.IsMatch(strIPAndPort.Trim())) ? true : false;
    }

    /// <summary>
    /// 重置预制体
    /// </summary>
    /// <param name="obj"></param>
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
    }

    public override bool OnMessage(UIMsgID msgID, params object[] paralist)
    {

        if (msgID == UIMsgID.ManCamera)
        {
            _mainManagentPanel.camera_Tog.isOn = true;
        }
        else if (msgID == UIMsgID.ManDoor)
        {
            _mainManagentPanel.door_Tog.isOn = true;
        }
        return true;
    }


    void CameraManListener(bool ison)
    {
        if (ison)
        {

        }
        else
        {

        }
        for (int i = 1; i < _mainManagentPanel.EquipMantog.transform.parent.childCount; i++)
        {
            _mainManagentPanel.EquipMantog.transform.parent.GetChild(i).gameObject.SetActive(ison);
        }
        _mainManagentPanel.SetContentSizeActive();
    }
    void DoorManListener(bool ison)
    {
        if (ison)
        {

        }
        else
        {

        }
        for (int i = 1; i < _mainManagentPanel.DoorMantog.transform.parent.childCount; i++)
        {
            _mainManagentPanel.DoorMantog.transform.parent.GetChild(i).gameObject.SetActive(ison);
        }
        _mainManagentPanel.SetContentSizeActive();
    }

    #endregion

}
