using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityTimer;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using zFramework.Media;
using static PeopleController;
using static MainPreviewUi;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using System.Threading;
using System.Collections.Concurrent;
using static LoginManager;

public class GameStart : MonoSingleton<GameStart>
{
    public string jsonName = "NvrConfiguration.json";
    public string jsonPath = string.Empty;
    public Transform WindRoot;
    private GameObject m_obj;
    public List<NVRInformation> Cameranvrs;
    //public List<CameraGroupInformation> cameragroupList;
    public Dictionary<string, List<NVRInformation>> CameraGroupDic = new Dictionary<string, List<NVRInformation>>();
    public Dictionary<string, List<NVRInformation>> DoorGroupDic = new Dictionary<string, List<NVRInformation>>();
    public List<CameraGroupInformation> DoorGroupList = new List<CameraGroupInformation>();
    public List<CameraGroupInformation> CameraGroupList = new List<CameraGroupInformation>();

    //保存从服务器请求下来的门禁设备列表
    public static EquipInfoArr AllDoorEquips;


    public static string IP { get { return AppRuntimeConfig.ApiHost; } }
    public static string MqttIP { get { return AppRuntimeConfig.Settings.mqttHost; } }
    public static string ApiBaseUrl { get { return AppRuntimeConfig.ApiBaseUrl; } }
    public static string ApiUrl(string path) { return AppRuntimeConfig.ApiUrl(path); }


    public readonly ConcurrentQueue<string> MessageQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    public void RunOnMainThread(Action action)
    {
        if (action != null)
        {
            mainThreadActions.Enqueue(action);
        }
    }
    protected override void Awake()
    {
        AppRuntimeConfig.Reload();
        Log.Configure(AppRuntimeConfig.Settings.enableDebugLogs);
        jsonPath = Path.Combine(Application.streamingAssetsPath, "Configurations");
        base.Awake();
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    void Start()
    {
        //LoadConfiger();
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform, transform.Find("UIRoot/WndRoot") as RectTransform, transform.Find("UIRoot/UICamera").GetComponent<Camera>(), transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>());
        RegisterUI();
        //加载64个视频预览框
        StartCoroutine(ProLoadObj());

        ////加载场景
        //UIManager.Instance.PopUpWnd(ConStr.ROOTBG, true);

        LoadCameraData();
        InitUserGroupDic();

        //注释掉Mqtt,人员更新完成之后开启
        StartMqtt(AppRuntimeConfig.Settings.mqttUserName, AppRuntimeConfig.Settings.mqttPassword,
            GameStart.MqttIP, AppRuntimeConfig.Settings.mqttPort);




        //GameMapManager.Instance.Init(this);
        //GameMapManager.Instance.LoadScene("startscene");
        //UIManager.Instance.PopUpWnd(ConStr.FIRSTPAGE, true);//首页
        UIManager.Instance.PopUpWnd(ConStr.LOGINPAGE, true);//登录页
        //定时向服务器请求该用户是否超时
        //InvokeRepeating("LoginCheck", 10,600);
        InitDataByServer();
    }

    //初始化服务器请求下来的数据
    private void InitDataByServer() 
    {

        RequestAllDoorDevs();//门禁设备
    }




    /// <summary>
    /// 登录校验
    /// </summary>
    private void LoginCheck() 
    {
        // Password-based silent re-login was removed because PlayerPrefs is not a secure credential store.
        // Session validation should use a dedicated token endpoint once the server contract is available.
      

    }
    public void LoginCallback(HttpCallBackArgs args)
    {


        if (!string.IsNullOrEmpty(args.Value))
        {
            LoginResData response = JsonUtility.FromJson<LoginResData>(args.Value);
             Log.Debug("登录校验返回状态码：" +response.code);
            if (response.code == 200)
            {
               
            }
            else
            {
                UIManager.Instance.CloseWnd(ConStr.USERMANAGER, true);
                UIManager.Instance.CloseWnd(ConStr.PEOPLEMANPAGE, true);
                UIManager.Instance.CloseWnd(ConStr.VISTORMANPAGE, true);
                UIManager.Instance.CloseWnd(ConStr.VIDEOREPLAY, true);
                //GameObject videoReplayObj = WindRoot.transform.Find("VideoReplayPageAVPro(Clone)").gameObject;
                //if (videoReplayObj != null)
                //{
                //    GameObject.Destroy(videoReplayObj);
                //}
                UIManager.Instance.CloseWnd(ConStr.EventCenter, true);
                UIManager.Instance.CloseWnd(ConStr.MAINPREVIEW, true);
                UIManager.Instance.CloseWnd(ConStr.FIRSTPAGE, true);
                UIManager.Instance.PopUpWnd(ConStr.LOGINPAGE, true);//登录页
                ShowTip(response.msg);
                PlayerPrefs.SetInt("IsLogin", 0);
                HttpNetManager.GetInstance().CancelAllRequests();
                HttpNetManager.ClearAuthorizationToken();
                LoginManager.Ins.ResetPermissions();
            }


        }
    }
   
  
    //private void OnEnable()
    //{
    //    //用户权限信息获取
    //    GetuserAuthFromServer(LoginManager.RoleName);
    //}

    ///// <summary>
    ///// 获取该账号对应的角色权限
    ///// </summary>
    ///// <param name="token"></param>
    //void GetuserAuthFromServer(string rolename)
    //{
    //    if (!string.IsNullOrEmpty(rolename))
    //        HttpNetManager.GetInstance().SendDataStr("http://" + HttpNetManager.LocalIP + "/api/perm/tg/role/info?roleName=" + rolename, GetAuthcallBack, false, false, false);
    //    else
    //        Log.Debug("rolename is null");
    //}
    /// <summary>
    /// 获取角色信息回调
    /// </summary>
    /// <param name="args"></param>
    //void GetAuthcallBack(HttpCallBackArgs args)
    //{
    //    CameraSetAuthList.Clear();
    //    DoorSetAuthList.Clear();
    //    PreviewAuthList.Clear();
    //    ReplayAuthList.Clear();
    //    CameraEventAuthList.Clear();
    //    DoorEventAuthList.Clear();
    //    UserManagerAuthList.Clear();
    //    PeopleManagerAuthList.Clear();

    //    if (!string.IsNullOrEmpty(args.Value))
    //    {
    //        Log.Debug("角色权限信息:" + args.Value);
    //        roleListData authList = JsonConvert.DeserializeObject<roleListData>(args.Value);
    //        foreach (var item in authList.roleList[0].cameraSetDic)
    //        {
    //            Log.Debug("模块名称:" + item.Key);
    //            switch (item.Key)
    //            {
    //                case "monitor_config":
    //                    IsCameraSet = true;
    //                    CameraSetAuthList = item.Value;
    //                    break;
    //                case "door_config":
    //                    IsDoorSet = true;
    //                    DoorSetAuthList = item.Value;
    //                    break;
    //                case "monitor_preview":
    //                    IsPreview = true;
    //                    PreviewAuthList = item.Value;
    //                    break;
    //                case "monitor_playback":
    //                    IsReplay = true;
    //                    ReplayAuthList = item.Value;
    //                    break;
    //                case "monitor_event":
    //                    IsCameraEvent = true;
    //                    CameraEventAuthList = item.Value;
    //                    break;
    //                case "door_event":
    //                    IsDoorEvent = true;
    //                    DoorEventAuthList = item.Value;
    //                    break;
    //                case "user_management":
    //                    IsUserManager = true;
    //                    UserManagerAuthList = item.Value;
    //                    break;
    //                case "personnel_management":
    //                    IsPeopleManager = true;
    //                    PeopleManagerAuthList = item.Value;
    //                    break;
    //                default:
    //                    break;
    //            }

    //        }
    //    }

    //    else
    //    {
    //        ShowTip("获取角色权限失败!");
    //    }

    //}


    //初始话存储人员基础信息的字典
    void InitUserGroupDic()
    {
        UserGroup.Instance.initUserGroup();
    }
    //开启Mqtt
    void StartMqtt(string username, string pwd, string ip, int port)
    {
        try
        {
            mqttSub.Instance.MqttConnect(username, pwd, ip, port);
        }
        catch (Exception e)
        {

            ShowTip("网络连接失败");
        }
     
    }
    //注册UI窗口
    void RegisterUI()
    {
        UIManager.Instance.Register<FirstUi>(ConStr.FIRSTPAGE);
        UIManager.Instance.Register<MainPreviewUi>(ConStr.MAINPREVIEW);
        UIManager.Instance.Register<ManManagentUi>(ConStr.MANDMANAGENT);
        UIManager.Instance.Register<PeopleManagerPageUi>(ConStr.PEOPLEMANPAGE);
        UIManager.Instance.Register<VideoReplayUi>(ConStr.VIDEOREPLAY);
        UIManager.Instance.Register<EventCenterUi>(ConStr.EventCenter);
        UIManager.Instance.Register<VistorManagerUi>(ConStr.VISTORMANPAGE);
        UIManager.Instance.Register<TipUi>(ConStr.TIP);
        UIManager.Instance.Register<SearchUi>(ConStr.SEARCHBG);
        UIManager.Instance.Register<UserManagerUi>(ConStr.USERMANAGER);
        UIManager.Instance.Register<LoginUi>(ConStr.LOGINPAGE);
    }
    IEnumerator ProLoadObj()
    {
        int count = Mathf.Max(0, AppRuntimeConfig.Settings.videoPreloadCount);
        int perFrame = Mathf.Max(1, AppRuntimeConfig.Settings.videoPreloadPerFrame);
        for (int i = 0; i < count; i++)
        {
            GameObject render = ObjectManager.Instance.InstantiateObject(ConStr.RENDERVIDEO, false, false);
            ObjectManager.Instance.ReleaseObject(render);
            if ((i + 1) % perFrame == 0)
            {
                yield return null;
            }
        }

    }
    private void OnDestroy()
    {
        mqttSub.Instance.MqttDisConnect();
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
        Log.Debug("清空编辑器缓存");
    }

    //加载配置表
    void LoadConfiger()
    {
        //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
    }
    //加载本地数据
    void LoadCameraData()
    {
        Cameranvrs = LoadNvrConfigBynative();
        CameraGroupDic = CameraGroupConfiguration.Instance().LoadCameragroupConfigBynative();
        DoorGroupDic = CameraGroupConfiguration.Instance().LoadDoorgroupConfigBynative();
        DoorGroupList = CameraGroupConfiguration.Instance().LoadDoorgroupInfoList();
        CameraGroupList = CameraGroupConfiguration.Instance().LoadCameragroupInfoList();
    }

    /// <summary>
    /// 本地加载监控信息nvr信息
    /// </summary>
    public List<NVRInformation> LoadNvrConfigBynative()
    {
        var file = Path.Combine(jsonPath, jsonName);
        if (File.Exists(file))
        {
            var info = File.ReadAllText(file);
            var obj = JsonUtility.FromJson<Wrapper>(info);
            if (null != obj)
            {
                Cameranvrs = obj.arr;
                return Cameranvrs;
            }
        }
        else
        {
            Debug.LogWarning($"{nameof(NVRManager)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

        }
        return new List<NVRInformation>();
    }

    [Serializable]
    public class Wrapper
    {
        public List<NVRInformation> arr;
        public Wrapper(List<NVRInformation> arr)
        {
            this.arr = arr;
        }
    }

    void Update()
    {
        UIManager.Instance.OnUpdate();
        Action action;
        int actionCount = 0;
        while (actionCount < 32 && mainThreadActions.TryDequeue(out action))
        {
            action();
            actionCount++;
        }
        //Mqtt接收到消息后会把消息存到队列，当队列有消息时向服务器发送请求数据
        string message;
        if (MessageQueue.TryDequeue(out message))
        {
            HttpByMqttSend(message);
        }

    }


    BlackUser blackuser;
    //由Mqtt主动发起的Http请求
    void HttpByMqttSend(string message)
    {
        if (!string.IsNullOrEmpty(message) && message.StartsWith("topic/notice:", StringComparison.Ordinal))//算法配置返回的主题信息
        {
            string url = GameStart.ApiUrl("/api/alarm/info?pageNum=1&pageSize=1");
            if(MainPreviewPanel.IstartFaceMesg)
            HttpNetManager.GetInstance().SendDataStr(url, MqtthttpAlthomriCallback, false, false, true);
        }



        //else if (message.Split(':')[0] == "topic/update")//人员信息更新返回的主题信息 后边加到服务中
        //{
        //    Log.Debug("更新的人员信息id：" + message.Split(':')[1].ToString());
        //    //  HttpNetManager.GetInstance().SendDataStr("http://192.168.110.2:7711/api/personnel/tg/user/client?idNum=" + message.Split(':')[1].ToString(), MqtthttpPeopleupdateCallback, false, false, true);

        //    string url = "http://192.168.110.2:7711/api/personnel/tg/user/client?idNum=" + message.Split(':')[1].ToString() + "&pageNum=1&pageSize=1&type=-1&issued=-1";
        //    HttpNetManager.GetInstance().SendDataStr(url, MqtthttpPeopleupdateCallback, false, false, false);
        //}
        //else if (message.Split(':')[0] == "topic/blackList")//黑名单信息推送 后边加到服务中
        //{
        //    string userinfo = message.Split('{')[1].Split('}')[0].ToString();
        //    Log.Debug("Mqtt接收到黑名单人员信息：" + userinfo);
        //    blackuser = JsonUtility.FromJson<BlackUser>("{" + userinfo + "}");
        //    //下发流程1.请求所有的门禁信息
        //    RequestAllDoorDevs();
        //}



    }
    [Serializable]
    //人员基础数据
    public class BlackUser
    {

        //public string id;
        public string username;                               //姓名1
        public string salaryNum;                             //工资编号1
        public string phoneNum;                             //手机号1
        public string unitName;                            //单位名称1
        public string idNum;                              //身份证号 1
        public string address;                            //家庭住址1
        public string remarks;                         //备注1
        public string faceBase64;                  //人脸信息
        public int is2issued;


    }
    /// <summary>
    /// 分页请求信息
    /// </summary>
    /// <param name="idNum">传值的就是单个请求，不传代表请求所有，分页请求不用传   </param>
    /// <param name="PageNum"> 请求第几页</param>
    /// <param name="PageSize">一页多少条数据</param>
    void HttpPageGetData(string idNum, int PageNum, int PageSize)
    {
        string url = GameStart.ApiUrl("/api/personnel/tg/user/client?");
        string param = "idNum=" + idNum + "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString();
         Log.Debug("发送http请求:" + url + param);
        HttpNetManager.GetInstance().SendDataStr(url + param, PagehttpCallback, false, false, true);
    }

    //分页Http请求get回调
    public void PagehttpCallback(HttpCallBackArgs args)
    {

         Log.Debug("Http收到服务器算法信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
            DealReceiveData.instance.HttpPageDeserialization(args.Value);



    }

    //人员更新回调
    public void MqtthttpPeopleupdateCallback(HttpCallBackArgs args)
    {

         Log.Debug("Http收到服务器请求人员信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            Althorims althorims = JsonUtility.FromJson<Althorims>(args.Value);
            for (int i = 0; i < althorims.records.Count; i++)
            {

            }
        }
        //DealReceiveData.instance.UserInfoSerialization(args.Value);

    }
    //更新人员信息下发到设备
    private void updatePeopleToDevs(List<NVRInformation> nvrs, UserInfoArr userInfoArr)
    {

        for (int i = 0; i < userInfoArr.records.Count; i++)//遍历人员信息容器
        {

            if (!string.IsNullOrEmpty(userInfoArr.records[i].faceBase64))
            {
                //先保存人脸信息照片 
                PeopleJsonParsing.instance.SaveFacePic(userInfoArr.records[i].faceBase64, @"C:\facepicture", userInfoArr.records[i].idNum);
                userInfoArr.records[i].faceBase64 = null;
            }
            //下发流程 ，登录全部的设备并建立长连接并下发人员信息
            for (int j = 0; j < nvrs.Count; j++)
            {

                NVRController.Instance().LoginUserToDoor("normal", nvrs[j].host, userInfoArr.records[i].idNum, userInfoArr.records[i].username, nvrs[j]);


            }



        }


    }
 
    //请求所有的门禁设备信息
    private void RequestAllDoorDevs()
    {
        HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/cam/info?equiptype=门禁设备", ReadequipCallback, false, false, false, null);
    }
    //读取门禁信息回调
 
    public void ReadequipCallback(HttpCallBackArgs args)
    {

        if (!string.IsNullOrEmpty(args.Value))
        {
            AllDoorEquips = JsonUtility.FromJson<EquipInfoArr>(args.Value);
            Log.Debug("门禁信息:" + args.Value);
        }
        else
        {
            GameStart.Instance.ShowTip("请求服务器失败");
        }

    }

    




    //算法回调
    public void MqtthttpAlthomriCallback(HttpCallBackArgs args)
    {
         Log.Debug("Http收到服务器推送的算法监测信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            Althorims althorims = JsonUtility.FromJson<Althorims>(args.Value);
            for (int i = 0; i < althorims.records.Count; i++)
            {
                if(int.Parse( althorims.records[i].camId)==MainPreviewUi.devid)
                EventCenter.BroadCast(Eventdefine.FacemesgListener, althorims.records[i]);
            }
        }

    }
    private void OnDisable()
    {
        mqttSub.Instance.MqttDisConnect();
    }
    /// <summary>
    /// Base64编码转为人脸图片保存本地
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="savePath"></param>
    /// <param name="idnum"></param>
    public void SaveFacePic(string base64Str, string savePath, string idnum)
    {

        try
        {
            Texture2D texture = new Texture2D(2, 2);
            // 将Base64字符串转换为字节数组
            byte[] bytes = Convert.FromBase64String(base64Str);

            // 创建一个新的Texture2D对象并将字节数组加载到其中
            texture.LoadImage(bytes);
            // 将纹理编码为PNG格式的字节数组
            byte[] pngBytes = texture.EncodeToJPG();
            string pname = idnum + ".jpg";
            string path = Path.Combine(savePath, pname);
            File.WriteAllBytes(path, pngBytes);
            Destroy(texture);
             Log.Debug("保存图片到" + path);


        }
        catch (Exception e)
        {
             Log.Debug("保存图片失败: " + e.Message);
        }
        //yield return new WaitForEndOfFrame();

    }
#region   ShowMesg

    public void ShowTip(string Mesg)
    {
        ObjectManager.Instance.InstantiateObjectAsync(ConStr.TIP, Tipcallback, LoadResPriority.RES_MIDDLE, false, Mesg);
    }

    public void ShowSearchTxt(string msg) 
    {
        ObjectManager.Instance.InstantiateObjectAsync(ConStr.SEARCHBG, Searchcallback, LoadResPriority.RES_MIDDLE, false, msg);
    }
    public void DestorySearchTxt() 
    {

        if (WindRoot.transform.Find("searchbg(Clone)") != null)
        {
           GameObject.Destroy(WindRoot.transform.Find("searchbg(Clone)").gameObject);
        }
    
    }



    private void Tipcallback(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
        Log.Error("Tip");
        GameObject tipobj = obj as GameObject;
        tipobj.transform.SetParent(WindRoot);
        resetPrefab(tipobj);
        tipobj.transform.Find("Txt").GetComponent<TextMeshProUGUI>().text = param1 as string;
        //UnityTimer.Timer.Register(2f, () => { GameObject.Destroy(tipobj); });
        StartCoroutine(DestoryTip(tipobj));
    }
    IEnumerator DestoryTip(GameObject obj) 
    {

        yield return new WaitForSeconds(1f);
        //GameObject.Destroy(obj);
        ObjectManager.Instance.ReleaseObject(obj);
    
    }




    private void Searchcallback(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {

        if (WindRoot.transform.Find("searchbg(Clone)") == null)
        {
            GameObject searchobj = obj as GameObject;
            searchobj.transform.SetParent(WindRoot);

            searchobj.transform.localScale = Vector3.one;
            searchobj.transform.localRotation = Quaternion.identity;
            searchobj.transform.localPosition = new Vector3(109, -90, 0);
            searchobj.transform.Find("searchTxt").GetComponent<TextMeshProUGUI>().text = param1 as string;


        }
        else 
        {
            WindRoot.transform.Find("searchbg(Clone)/searchTxt").GetComponent<TextMeshProUGUI>().text = param1 as string;



        }
     
    
    }
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = new Vector3(697.25f, -414.9f, 0);
    }
#endregion
    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
         Log.Debug("清空编辑器缓存");
#endif
        NVRController.Instance().Logout();

    }
}
[Serializable]
public class Althorims
{
    public List<althorimData> records = new List<althorimData>();
    public int total;//总计多少条
    public int size;//一页多少条
    public int current;//第几页
    public List<object> orders;
    public bool optimizeCountSql;
    public bool searchCount;
    public object maxLimit;
    public string countId;
    public int pages;//一共多少页
}
[Serializable]
public class althorimData
{
    public int id;
    public string camId = string.Empty;//监控id
    public int eventTime;
    public string idNum = string.Empty;
    public string personAction = string.Empty;
    public string isFire = string.Empty;
    public string carNum = string.Empty;
    public string type = string.Empty;
    public string img = string.Empty;
    public string userName = string.Empty;
    public string camName = string.Empty;
}
