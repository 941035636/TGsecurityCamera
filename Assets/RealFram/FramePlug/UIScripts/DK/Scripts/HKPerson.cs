using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using zFramework.Media;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using static PeopleController;

public class HKPerson : MonoSingleton<HKPerson>
{
    private static HKPerson _Instance;
    public static HKPerson GetInstance()
    {
        if (_Instance == null)
        {
            _Instance = new HKPerson();
        }
        return _Instance;
    }
    // public static HKPerson instance;
    public int LoginHandle = -1;
    private Int32 m_lGetUserCfgHandle = -1;
    //public Int32 m_lSetUserCfgHandle = -1;
    private Int32 m_lDelUserCfgHandle = -1;
    private CHCNetSDK.NET_DVR_CONTROL_GATEWAY dwGatewayIndex;
    private string str;

    private IntPtr m_ptrRealHandle;
    private CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo;
    private CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;
    private CHCNetSDK.NET_DVR_DEVICEINFO_V30 deviceinfo;
    int id = 0;
    private bool tex;
    CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;


    private bool m_bInitSDK = false;

    //Delet User Info Success

    //public Button btn_Login;
    //public Button btn_Set;
    //public Button btn_Out;
    //public Button btn_Delete;
    //public string delete_Id;
    private string[] strArray = { "1&nn&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&21324650&/Face1.jpg",
        "2&mm&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&54615465161&/Face2.jpg",
        "3&qq&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&3545648212&/Face3.jpg",
        "4&22&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&1314653133&/Face4.jpg" ,
         "5&mm&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&54615465164&/Face5.jpg",
        "6&qq&blackList&2023-03-11T16:03:08&2023-04-11T16:05:00&male&3545648125&/Face6.jpg",
        "7&22&normal&2023-03-11T16:03:08&2023-04-11T16:05:00&male&1314653136&/Face7.jpg" ,
    };



    void Start()
    {
        //InitSDKs();
        ////btn_Login.onClick.AddListener(Login);
        //btn_Set.onClick.AddListener(UserSet);
        //// btn_Out.onClick.AddListener(LogOut(LoginHandle));
        //btn_Delete.onClick.AddListener(delete);

        //print(m_lDelUserCfgHandle);
        //m_lSetUserCfgHandle = -1;

    }

    //public void delete()
    //{
    //    DeleteUser(delete_Id);
    //}
    public void UserSet()
    {
        //for (int i = 0; i < strArray.Length; i++)
        //{
        //    //string[] sArray = strArray[i].Split('&');
        //    print(strArray[0]);
        //    //print(sArray[0][1]);
        //    UserAdd(strArray[i].Split('&')[0], strArray[i].Split('&')[1], strArray[i].Split('&')[2], strArray[i].Split('&')[3], strArray[i].Split('&')[4], strArray[i].Split('&')[5]);
        //    CardAdd(strArray[i].Split('&')[0], strArray[i].Split('&')[6]);
        //    FaceData(strArray[i].Split('&')[0], strArray[i].Split('&')[7]);
        //}

       // StartCoroutine(Loop());//开启协程
        //UserAdd();
        //CardAdd();
        //FaceData();
    }

    #region  初始化
    /// <summary>
    /// 初始化
    /// </summary>

    public void InitSDKs()
    {
        //初始化  开始时调用，一个程序只能调用一次
        m_bInitSDK = CHCNetSDK.NET_DVR_Init();

        if (m_bInitSDK == false)
        {
             Log.Debug("初始化失败");
            return;
        }
        else
        {
            CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog001\\", true);
             Log.Debug("初始化成功" + m_bInitSDK);

            // Login();
        }
    }
    #endregion

  
    #region  登录门禁系统
    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="Ip"></param>
    /// <param name="Port"></param>
    /// <param name="UserName"></param>
    /// <param name="PassWord"></param>
    public int Login(string ip, int port, string username, string password)
    {
        if (LoginHandle < 0)
        {
            //初始化
            struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();
            //设备IP
            struLogInfo.sDeviceAddress = System.Text.Encoding.Default.GetBytes(ip);
            //端口
            struLogInfo.wPort = (ushort)port;
            //用户名
            struLogInfo.sUserName = System.Text.Encoding.Default.GetBytes(username);
            //设备密码
            struLogInfo.sPassword = System.Text.Encoding.Default.GetBytes(password);

            //if (LoginCallBack == null)
            //{
            //    LoginCallBack = CHCNetSDK.LOGINRESULTCALLBACK();//注册回调函数

            //}

            //struLogInfo.cbLoginResult = LoginCallBack;
            struLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 

            DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();
            deviceinfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            //登录设备  ---根据设备返回值m_UserID判断哪个设备
            //m_UserID =CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);//每台设备只需要登录一次，接口返回-1表示登录失败，其他值表示返回的用户ID值。用户ID具有唯一性，后续对设备的操作都需要通过此ID实现
            LoginHandle = CHCNetSDK.NET_DVR_Login_V30(ip, port, username, password, ref deviceinfo);
            //m_UserID =CHCNetSDK.NET_DVR_Login_V30(GameView.instance.ip.text, 8000, GameView.instance.username.text, GameView.instance.possworld.text, ref deviceinfo);
            if (LoginHandle < 0)
            {
                 Log.Debug("登录失败" + "回调值" + LoginHandle);
                //LogOut(LoginHandle);
                return LoginHandle;
            }

            else
            {
                 Log.Debug("登录设备返回值" + LoginHandle);
                //  Log.Debug("登录返回值" +CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo));
                 Log.Debug("ID" + struLogInfo.sDeviceAddress);
                 Log.Debug("设备信息" + deviceinfo);
                
                return LoginHandle;


            }


        }
        return LoginHandle;

    }
    #endregion

    

    #region   建立人员信息长链接
    public int OpenDeviceConnect(int loginhande, int type)
    {
     
        string sURL = string.Empty;
        IntPtr ptrURL = IntPtr.Zero;
        switch (type)
        {
            case 0: //人员信息
                sURL = "PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json"; 
                ptrURL = Marshal.StringToHGlobalAnsi(sURL);
                id = CHCNetSDK.NET_DVR_StartRemoteConfig(loginhande, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrURL, sURL.Length, null, IntPtr.Zero);
                if (id>-1)
                {
                    foreach (var item in HikvisonNVR.LoginhandleDic.Keys)
                    {
                        if (string.Equals(HikvisonNVR.LoginhandleDic[item], loginhande))
                        {
                            if (!PeopleJsonParsing.IPBandConnectHandleUserDic.ContainsKey(item))
                            {
                                PeopleJsonParsing.IPBandConnectHandleUserDic.Add(item, id);
                                Log.Debug("人员信息句柄字典添加:" + item + "  " + id); 
                            }
                            else
                            {
                                PeopleJsonParsing.IPBandConnectHandleUserDic[item] = id;
                            }
                        
                        }
                    }

                }
            
               
                break;
            case 1: //卡片信息
                sURL = "PUT /ISAPI/AccessControl/CardInfo/SetUp?format=json";
                ptrURL = Marshal.StringToHGlobalAnsi(sURL);
                id = CHCNetSDK.NET_DVR_StartRemoteConfig(loginhande, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrURL, sURL.Length, null, IntPtr.Zero);
                if (id > -1)
                {
                    foreach (var item in HikvisonNVR.LoginhandleDic.Keys)
                    {
                        if (string.Equals(HikvisonNVR.LoginhandleDic[item], loginhande))
                        {
                          
                            if (!PeopleJsonParsing.IPBandConnectHandleCardDic.ContainsKey(item))
                            {
                                PeopleJsonParsing.IPBandConnectHandleCardDic.Add(item, id);
                                Log.Debug("卡片信息句柄字典添加:" + item + "  " + id);
                            }
                            else
                            {
                                PeopleJsonParsing.IPBandConnectHandleCardDic[item] = id;
                            }

                        }
                    }
                }
            
                break;
            case 2: //人脸信息
                sURL = "PUT /ISAPI/Intelligent/FDLib/FDSetUp?format=json"; 
                ptrURL = Marshal.StringToHGlobalAnsi(sURL);
                id = CHCNetSDK.NET_DVR_StartRemoteConfig(loginhande, CHCNetSDK.NET_DVR_FACE_DATA_RECORD, ptrURL, sURL.Length, null, IntPtr.Zero);
                if (id > -1)
                {
                    foreach (var item in HikvisonNVR.LoginhandleDic.Keys)
                    {
                        if (string.Equals(HikvisonNVR.LoginhandleDic[item], loginhande))
                        {
                           
                            if (!PeopleJsonParsing.IPBandConnectHandleFaceDic.ContainsKey(item))
                            {
                                PeopleJsonParsing.IPBandConnectHandleFaceDic.Add(item, id);
                                Log.Debug("人脸信息句柄字典添加:" + item + "  " + id);
                            }
                            else
                            {
                                PeopleJsonParsing.IPBandConnectHandleFaceDic[item] = id;
                            }
                        }
                    }
                }
            
                break;
            default:
                break;
        }
        //设置
        //string sURL = "PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json";
        //新增
        //string sURL = "POST/ISAPI/AccessControl/UserInfo/Record?format=json ";

        //修改
        //string sURL = "PUT /ISAPI/AccessControl/UserInfo/Modify?format=json";
        //IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);//将托管 String 的内容复制到非托管内存，并在复制时转换为 ANSI 格式。

        //建立长连接   m_UserID： NET_DVR_Login_V40等登录接口的返回值；CHCNetSDK.NET_DVR_JSON_CONFIG：配置命令；ptrURL：输入参数；sURL.Length：输入缓冲的大小
      
        //判断长连接返回值
        if (id < 0)
        {
             Log.Debug("NET_DVR_StartRemoteConfig fail [url:PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json] error:" + CHCNetSDK.NET_DVR_GetLastError());
            Marshal.FreeHGlobal(ptrURL);// 释放以前从进程的非托管内存中分配的内存。

        }

        return id;

    }

    public void adduser(string id, string name, string userType, string beginTime, string endTime, int connecthandle)//string id, string name, string userType,  string beginTime, string endTime, string gender,string doorTight, int doorNo, string planTemplateNo
    {
        //string sURL = "PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json";
        //IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);//将托管 String 的内容复制到非托管内存，并在复制时转换为 ANSI 格式。
        //SendUserInfo();

        AddInfo(id, name, userType, beginTime, endTime, connecthandle);
        // SendUserInfo();
        //Marshal.FreeHGlobal(ptrURL);// 释放以前从进程的非托管内存中分配的内存。
        // Log.Debug(CHCNetSDK.NET_DVR_GetLastError());//0表示没有错误

    }
    #endregion



    #region 下发卡号
    public void CardAdd(string id, string card, int connectid)//string id,string card
    {

        string carURL = "PUT /ISAPI/AccessControl/CardInfo/SetUp?format=json"; //卡号json报文
        //新增
        //string carURL = "POST/ISAPI/AccessControl/UserInfo/Record?format=json ";
        IntPtr pcarURL = Marshal.StringToHGlobalAnsi(carURL);//将托管 String 的内容复制到非托管内存，并在复制时转换为 ANSI 格式。
        //建立长连接
        //m_lSetUserCfgHandle =CHCNetSDK.NET_DVR_StartRemoteConfig(LoginHandle,CHCNetSDK.NET_DVR_JSON_CONFIG, pcarURL, carURL.Length, null, IntPtr.Zero);
        //判断长连接返回值
        if (connectid < 0)
        {
             Log.Debug("NET_DVR_StartRemoteConfig fail [url:PUT /ISAPI/AccessControl/UserInfo/SetUp?format=json] error:" + CHCNetSDK.NET_DVR_GetLastError());
            Marshal.FreeHGlobal(pcarURL);//释放以前从进程的非托管内存中分配的内存。
            return;
        }
        else
        {
            Marshal.FreeHGlobal(pcarURL);// 释放以前从进程的非托管内存中分配的内存。

            SendCardData(id, card, connectid);//id,card
        }
    }
    #endregion


    public void AddInfo(string id, string name, string userType, string beginTime, string endTime, int connecthandle)//,string doorTight,int doorNo,string planTemplateNo
    {

        CUserInfoCfg JsonUserInfo = new CUserInfoCfg();//定义一个 名为JsonUserInfo 的CUserInfoCfg类
        JsonUserInfo.UserInfo = new CUserInfo();//初始化
        JsonUserInfo.UserInfo.employeeNo = id;//编号ID
        JsonUserInfo.UserInfo.name = name;//姓名
        JsonUserInfo.UserInfo.userType =userType;
        JsonUserInfo.UserInfo.Valid = new CValid();//初始化
        JsonUserInfo.UserInfo.Valid.enable = true;
        JsonUserInfo.UserInfo.Valid.beginTime = beginTime;//起始时间
        JsonUserInfo.UserInfo.Valid.endTime = endTime;//结束时间
        JsonUserInfo.UserInfo.Valid.timeType = "local";//时间类型：local-设备本地时间，UTC-UTC时间
        //JsonUserInfo.UserInfo.gender = gender;//性别，人脸图片对应的人员性别，male-男，female-女，unknown-未知


        JsonUserInfo.UserInfo.doorRight = "1";  //门权限（代表对门 1、门 3 有权限）（锁权限，此处为锁 ID，可填写多个，代表对锁 1、锁 3 有权限）
        JsonUserInfo.UserInfo.RightPlan = new List<CRightPlan>();
        CRightPlan JsonRightPlan = new CRightPlan();
        JsonRightPlan.doorNo = 1;
        JsonRightPlan.planTemplateNo = "1";
        JsonUserInfo.UserInfo.RightPlan.Add(JsonRightPlan);



        string strJsonUserInfo = JsonConvert.SerializeObject(JsonUserInfo, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        //Log.Error("人员信息转字符串:"+strJsonUserInfo);
        byte[] byJsonUserInfo = System.Text.Encoding.UTF8.GetBytes(strJsonUserInfo);
        IntPtr ptrJsonUserInfo = Marshal.AllocHGlobal(byJsonUserInfo.Length);//通过使用指定的字节数，从进程的非托管内存中分配内存
        Marshal.Copy(byJsonUserInfo, 0, ptrJsonUserInfo, byJsonUserInfo.Length);//将数据从一堆托管8位无符号整数数组复制到非托管内存指针

        IntPtr ptrJsonData = Marshal.AllocHGlobal(1024);//通过使用指定的字节数，从进程的非托管内存中分配内存

        for (int i = 0; i < 1024; i++)
        {
            Marshal.WriteByte(ptrJsonData, i, 0);//将单个字节值写入到非托管内存
        }

        int dwState = (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS;
        uint dwReturned = 0;
        while (true)
        {
            //下发
            dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(connecthandle, ptrJsonUserInfo, (uint)byJsonUserInfo.Length, ptrJsonData, 1024, ref dwReturned);

            string strJsonData = Marshal.PtrToStringAnsi(ptrJsonData);
            Log.Debug("输出:"+strJsonData);
            if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT)
            {
                Thread.Sleep(10);
                continue;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED)
            {
                 Log.Debug("Set User Fail error:" + CHCNetSDK.NET_DVR_GetLastError());
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS)
            {
                //返回NET_SDK_CONFIG_STATUS_SUCCESS代表流程走通了，但并不代表下发成功，比如有些设备可能因为人员已存在等原因下发失败，所以需要解析Json报文
                CResponseStatus JsonResponseStatus = new CResponseStatus();
                JsonResponseStatus = JsonConvert.DeserializeObject<CResponseStatus>(strJsonData);
                JsonResponseStatus.errorMsg = "OK";
                if (JsonResponseStatus.statusCode == 1)
                {
                    // Log.Debug("Set User Success");

                    //foreach (var item in PeopleJsonParsing.Instance.IPBandConnectHandleUserDic)
                    //{
                    //    if (item.Value == connecthandle)
                    //    {
                    //        Log.Debug(id + "下发到IP为 ： " + item.Key + "的设备成功");
                    //        if (PeopleJsonParsing.instance.UidSuccessDic.ContainsKey(item.Key))
                    //        {
                    //            PeopleJsonParsing.instance.UidSuccessDic[item.Key].Add(id);
                    //        }
                    //        //这里做记录IP下发到哪些设备，和开始下发的时候哪些IP需要下发  <容器比对>筛选出没有下发成功的人员id
                    //    }

                    //}
                }
                else
                {
                     Log.Debug("Set User Fail, ResponseStatus.statusCode" + JsonResponseStatus.statusCode+" 错误详细信息:"+JsonResponseStatus.errorMsg+" 状态描述:"+JsonResponseStatus.statusString);
                }
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FINISH)
            {
                //下发人员时：dwState其实不会走到这里，因为设备不知道我们会下发多少个人，所以长连接需要我们主动关闭
                 Log.Debug("Set User Finish");
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION)
            {
                Log.Debug(dwState);
                 Log.Debug("Set User Exception error:" + CHCNetSDK.NET_DVR_GetLastError());
                Log.Debug("重新建立人员基础信息长连接");
                CHCNetSDK.NET_DVR_StopRemoteConfig(connecthandle);
                int chandle = OpenDeviceConnect(HikvisonNVR.LoginhandleDic.ElementAt(0).Value, 0);
                if (!PeopleJsonParsing.IPBandConnectHandleUserDic.ContainsKey(HikvisonNVR.LoginhandleDic.ElementAt(0).Key))
                {
                    PeopleJsonParsing.IPBandConnectHandleUserDic.Add(HikvisonNVR.LoginhandleDic.ElementAt(0).Key, chandle);
                    Log.Debug("释放长连接并重新建立长连接，人员信息句柄字典添加:" + chandle + "  " + id);
                }
                else
                {
                    PeopleJsonParsing.IPBandConnectHandleUserDic[HikvisonNVR.LoginhandleDic.ElementAt(0).Key] = chandle;
                }

                break;
            }
            else
            {
                 Log.Debug("unknown Status error:" + CHCNetSDK.NET_DVR_GetLastError());
                //  Log.Debug(J);
                break;
            }
        }

        Marshal.FreeHGlobal(ptrJsonUserInfo);// 释放以前从进程的非托管内存中分配的内存。
        Marshal.FreeHGlobal(ptrJsonData);// 释放以前从进程的非托管内存中分配的内存。


        //关闭长连接，释放资源
        //if (connecthandle != -1)
        //{
        //    if (CHCNetSDK.NET_DVR_StopRemoteConfig(connecthandle))
        //    {
        //        connecthandle = -1;
        //        print("关了");
        //    }
        //}
    }



    #region  卡号

    private void SendCardData(string id, string card, int connecthandle)//string id,string card
    {
        CCardInfoCfg JsonCardInfo = new CCardInfoCfg();
        JsonCardInfo.CardInfo = new CCardInfo();
        JsonCardInfo.CardInfo.employeeNo = id;//编号
        JsonCardInfo.CardInfo.cardNo = card;//卡号
        JsonCardInfo.CardInfo.cardType = "normalCard";//卡片类型
        string strJsonCardInfo = JsonConvert.SerializeObject(JsonCardInfo, Formatting.Indented,
                                                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        IntPtr ptrJsonCardInfo = Marshal.StringToHGlobalAnsi(strJsonCardInfo);

        IntPtr ptrJsonData = Marshal.AllocHGlobal(1024);
        for (int i = 0; i < 1024; i++)
        {
            Marshal.WriteByte(ptrJsonData, i, 0);
        }

        int dwState = 0;
        uint dwReturned = 0;
        while (true)
        {
            //下发人员数据
            dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(connecthandle, ptrJsonCardInfo, (uint)strJsonCardInfo.Length, ptrJsonData, 1024, ref dwReturned);
            string strJsonData = Marshal.PtrToStringAnsi(ptrJsonData);
            Log.Debug("输出:" + strJsonData);
            if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT)
            {
                Thread.Sleep(10);
                continue;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED)
            {
                 Log.Debug("Set Card Fail error:" + CHCNetSDK.NET_DVR_GetLastError());
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS)
            {
                CResponseStatus JsonResponseStatus = new CResponseStatus();
                JsonResponseStatus = JsonConvert.DeserializeObject<CResponseStatus>(strJsonData);

                if (JsonResponseStatus.statusCode == 1)
                {
                    //foreach (var item in PeopleJsonParsing.Instance.IPBandConnectHandleCardDic)
                    //{
                    //    if (item.Value == connecthandle)
                    //    {
                    //        Log.Debug(id + "    身份证信息下发到IP为：" + item.Key + "的设备成功");
                    //        if (PeopleJsonParsing.instance.CardSuccessDic.ContainsKey(item.Key))
                    //        {
                    //            PeopleJsonParsing.instance.CardSuccessDic[item.Key].Add(id);
                    //        }
                    //    }

                    //}
                }
                else
                {
                     Log.Debug("Set Card Fail, ResponseStatus.statusCode:" + JsonResponseStatus.statusCode);
                    Log.Debug("Error:" + CHCNetSDK.NET_DVR_GetLastError());
                }
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION)
            {
                 Log.Debug("Set Card Exception error:" + CHCNetSDK.NET_DVR_GetLastError());
                Log.Debug("重新建立人员卡号信息长连接");
                CHCNetSDK.NET_DVR_StopRemoteConfig(connecthandle);
                int chandle = OpenDeviceConnect(HikvisonNVR.LoginhandleDic.ElementAt(0).Value, 1);
                if (!PeopleJsonParsing.IPBandConnectHandleCardDic.ContainsKey(HikvisonNVR.LoginhandleDic.ElementAt(0).Key))
                {
                    PeopleJsonParsing.IPBandConnectHandleCardDic.Add(HikvisonNVR.LoginhandleDic.ElementAt(0).Key, chandle);
                    Log.Debug("释放长连接并重新建立长连接，人员卡号句柄字典添加:" + chandle + "  " + id);
                }
                else
                {
                    PeopleJsonParsing.IPBandConnectHandleCardDic[HikvisonNVR.LoginhandleDic.ElementAt(0).Key] = chandle;
                }

                break;
            }
            else
            {
                 Log.Debug("unknown Status error:" + CHCNetSDK.NET_DVR_GetLastError());
                break;
            }
        }
        //关闭长连接，释放资源
        //if (m_lSetUserCfgHandle != -1)
        //{
        //    if (CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle))
        //    {
        //        m_lSetUserCfgHandle = -1;
        //    }
        //}
        Marshal.FreeHGlobal(ptrJsonCardInfo);
        Marshal.FreeHGlobal(ptrJsonData);
    }



    #endregion

    //下发成功向服务器返回结果信息
    private Task SendToserverTask(string idNum,int devid) 
    {
        Task t = new Task(() => {
            Thread.Sleep(1);
            HttpNetManager.GetInstance().SendDataStr("",sendToservercallBack,true ,true,false,"");
        });
        t.Start();
        return t;
    }
    void sendToservercallBack(HttpCallBackArgs args) 
    {


    }






    #region    人脸数据


    public void FaceData(string id,int connectid)//string id,string url
    {
        //string path = Application.streamingAssetsPath + "/Face7.jpg";
        // string path = Application.streamingAssetsPath + url;
        string path = @"C:\facepicture";
        //string path = Application.streamingAssetsPath + "/FacePicture";
        //判断人脸照片路径是否为null
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string filepath = Path.Combine(path, id) + ".jpg";
        //string filepath = path+"/"+id + ".jpg";
        //string sURL = "PUT /ISAPI/Intelligent/FDLib/FDSetUp?format=json";
        //IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sURL);
        //connectid = CHCNetSDK.NET_DVR_StartRemoteConfig(loginid, CHCNetSDK.NET_DVR_FACE_DATA_RECORD, ptrURL, sURL.Length, null, IntPtr.Zero);
        //if (connectid == -1)
        //{
        //    Marshal.FreeHGlobal(ptrURL);
        //     Log.Debug("NET_DVR_StartRemoteConfig fail [url:PUT /ISAPI/Intelligent/FDLib/FDSetUp?format=json] error:" + CHCNetSDK.NET_DVR_GetLastError());
        //    return;
        //}
        //Marshal.FreeHGlobal(ptrURL);

        CSetFaceDataCond JsonSetFaceDataCond = new CSetFaceDataCond();
        JsonSetFaceDataCond.faceLibType = "blackFD";
        JsonSetFaceDataCond.FDID = "1";
        JsonSetFaceDataCond.FPID = id;//工号
        string strJsonSearchFaceDataCond = JsonConvert.SerializeObject(JsonSetFaceDataCond, Formatting.Indented,
                                                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        IntPtr ptrJsonSearchFaceDataCond = Marshal.StringToHGlobalAnsi(strJsonSearchFaceDataCond);

        CHCNetSDK.NET_DVR_JSON_DATA_CFG struJsonDataCfg = new CHCNetSDK.NET_DVR_JSON_DATA_CFG();
        struJsonDataCfg.dwSize = (uint)Marshal.SizeOf(struJsonDataCfg);
        struJsonDataCfg.lpJsonData = ptrJsonSearchFaceDataCond;
        struJsonDataCfg.dwJsonDataSize = (uint)strJsonSearchFaceDataCond.Length;
        if (!File.Exists(filepath))
        {
             Log.Debug("The picture does not exist!");
            Marshal.FreeHGlobal(ptrJsonSearchFaceDataCond);
            return;
        }
        FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate);

         Log.Debug(fs.Name);
        if (0 == fs.Length)
        {
             Log.Debug("The picture is 0k,please input another picture!");
            Marshal.FreeHGlobal(ptrJsonSearchFaceDataCond);
            fs.Close();
            return;
        }
        if (200 * 1024 < fs.Length)
        {
             Log.Debug("The picture is larger than 200k,please input another picture!");
            Marshal.FreeHGlobal(ptrJsonSearchFaceDataCond);
            fs.Close();
            return;
        }

        struJsonDataCfg.dwPicDataSize = (uint)fs.Length;//图片内容大小
        int iLen = (int)struJsonDataCfg.dwPicDataSize;
        byte[] by = new byte[iLen];
        struJsonDataCfg.lpPicData = Marshal.AllocHGlobal(iLen);//分配内存空间
        fs.Read(by, 0, iLen);
        Marshal.Copy(by, 0, struJsonDataCfg.lpPicData, iLen);

        fs.Close();
        IntPtr ptrJsonDataCfg = Marshal.AllocHGlobal((int)struJsonDataCfg.dwSize);
        Marshal.StructureToPtr(struJsonDataCfg, ptrJsonDataCfg, false);

        IntPtr ptrJsonResponseStatus = Marshal.AllocHGlobal(1024);
        for (int i = 0; i < 1024; i++)
        {
            Marshal.WriteByte(ptrJsonResponseStatus, i, 0);
        }
        int dwState = (int)CHCNetSDK.NET_SDK_GET_NEXT_STATUS_SUCCESS;
        uint dwReturned = 0;
        while (true)
        {
            dwState = CHCNetSDK.NET_DVR_SendWithRecvRemoteConfig(connectid, ptrJsonDataCfg, struJsonDataCfg.dwSize, ptrJsonResponseStatus, 1024, ref dwReturned);;
            string strResponseStatus = Marshal.PtrToStringAnsi(ptrJsonResponseStatus);
            Log.Debug("输出:" + strResponseStatus);
            if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT)
            {
                Thread.Sleep(10);
                continue;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED)
            {
                 Log.Debug("Set Face Error:" + CHCNetSDK.NET_DVR_GetLastError());
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS)
            {
                CResponseStatus JsonResponseStatus = new CResponseStatus();
                JsonResponseStatus = JsonConvert.DeserializeObject<CResponseStatus>(strResponseStatus);
                JsonResponseStatus.errorMsg = "OK";
                if (JsonResponseStatus.statusCode == 1)
                {
                    foreach (var item in PeopleJsonParsing.IPBandConnectHandleFaceDic)
                    {
                        if (item.Value == connectid)
                        {
                            if (TypeMenuController.UserTypebandsDevs[TypeMenuController.Ins.ChooseType].Exists(t=>t.Ip==item.Key))
                            {
                                NVRInformation nVR = TypeMenuController.UserTypebandsDevs[TypeMenuController.Ins.ChooseType].Find(t => t.Ip == item.Key);

                                Log.Debug(id + "    人脸信息下发到IP为：" + item.Key +"设备id为:"+nVR.id+ "的设备成功");
                                //if (PeopleJsonParsing.instance.FaceSuccessDic.ContainsKey(item.Key))
                                //{
                                //    PeopleJsonParsing.instance.FaceSuccessDic[item.Key].Add(id);
                                //}
                                //人脸信息是最后一步，只要人脸信息下发成功了就可以视为成功，像服务器推送下发成功的人员id和设备id
                                Log.Debug("像服务器推送下发成功的人员id:"+id+"设备id:"+nVR.id);
                            }
                            Log.Debug(id + "    人脸信息下发到IP为：" + item.Key + "的设备成功");

                        }

                    }

                   
                }
                else
                {
                     Log.Debug("Set Face Fail, ResponseStatus.statusCode = " + JsonResponseStatus.statusCode);
                    Log.Debug(CHCNetSDK.NET_DVR_GetLastError());
                    // Log.Debug(ptrJsonDataCfg);
                    // Log.Debug(filepath);
                }
                break;
            }
            else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION)
            {
                 Log.Debug("Set Face Exception Error:" + CHCNetSDK.NET_DVR_GetLastError());
                Log.Debug("重新建立人员人脸信息长连接");
                CHCNetSDK.NET_DVR_StopRemoteConfig(connectid);
                int chandle = OpenDeviceConnect(HikvisonNVR.LoginhandleDic.ElementAt(0).Value, 2);
                if (!PeopleJsonParsing.IPBandConnectHandleFaceDic.ContainsKey(HikvisonNVR.LoginhandleDic.ElementAt(0).Key))
                {
                    PeopleJsonParsing.IPBandConnectHandleFaceDic.Add(HikvisonNVR.LoginhandleDic.ElementAt(0).Key, chandle);
                    Log.Debug("释放长连接并重新建立长连接，人员人脸句柄字典添加:" + chandle + "  " + id);
                }
                else
                {
                    PeopleJsonParsing.IPBandConnectHandleFaceDic[HikvisonNVR.LoginhandleDic.ElementAt(0).Key] = chandle;
                }
                break;
            }
            else
            {
                 Log.Debug("unknown Status Error:" + CHCNetSDK.NET_DVR_GetLastError());
                break;
            }
        }
        ////关闭长连接，释放资源
        //if (m_lSetUserCfgHandle > 0)
        //{
        //    CHCNetSDK.NET_DVR_StopRemoteConfig(m_lSetUserCfgHandle);
        //    m_lSetUserCfgHandle = -1;
        //}


        Marshal.FreeHGlobal(ptrJsonDataCfg);
        Marshal.FreeHGlobal(ptrJsonResponseStatus);
    }
    #endregion

    #region  删除人员信息
    IntPtr lpOutputParam;
    public void DeleteUser(string id)
    {
         Log.Debug("删除");
        IntPtr ptrOutBuf = Marshal.AllocHGlobal(1024);
        IntPtr ptrStatusBuffer = Marshal.AllocHGlobal(1024);
        for (int i = 0; i < 1024; i++)
        {
            Marshal.WriteByte(ptrOutBuf, i, 0);
            Marshal.WriteByte(ptrStatusBuffer, i, 0);
        }

        CHCNetSDK.NET_DVR_XML_CONFIG_INPUT struInput = new CHCNetSDK.NET_DVR_XML_CONFIG_INPUT();
        CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT struOuput = new CHCNetSDK.NET_DVR_XML_CONFIG_OUTPUT();

        string sUrl = "PUT /ISAPI/AccessControl/UserInfoDetail/Delete?format=json";
        IntPtr ptrURL = Marshal.StringToHGlobalAnsi(sUrl);
        struInput.dwSize = (uint)Marshal.SizeOf(struInput);
        struInput.lpRequestUrl = ptrURL;
        struInput.dwRequestUrlLen = (uint)sUrl.Length;

        CUserInfoDetailCfg JsonUserInfoDetailCfg = new CUserInfoDetailCfg();
        JsonUserInfoDetailCfg.UserInfoDetail = new CUserInfoDetail();
        JsonUserInfoDetailCfg.UserInfoDetail.mode = "byEmployeeNo";
        JsonUserInfoDetailCfg.UserInfoDetail.EmployeeNoList = new List<CEmployeeNoList>();
        CEmployeeNoList singleEmployeeNoList = new CEmployeeNoList();
        singleEmployeeNoList.employeeNo = id;
        JsonUserInfoDetailCfg.UserInfoDetail.EmployeeNoList.Add(singleEmployeeNoList);
        string strUserInfoDetailCfg = JsonConvert.SerializeObject(JsonUserInfoDetailCfg);
        IntPtr ptrUserInfoDetailCfg = Marshal.StringToHGlobalAnsi(strUserInfoDetailCfg);

        struInput.lpInBuffer = ptrUserInfoDetailCfg;
        struInput.dwInBufferSize = (uint)strUserInfoDetailCfg.Length;

        struOuput.dwSize = (uint)Marshal.SizeOf(struOuput);
        struOuput.lpOutBuffer = ptrOutBuf;
        struOuput.dwOutBufferSize = 1024;
        struOuput.lpStatusBuffer = ptrStatusBuffer;
        struOuput.dwStatusSize = 1024;

        IntPtr ptrInput = Marshal.AllocHGlobal(Marshal.SizeOf(struInput));
        Marshal.StructureToPtr(struInput, ptrInput, false);
        IntPtr ptrOuput = Marshal.AllocHGlobal(Marshal.SizeOf(struOuput));
        Marshal.StructureToPtr(struOuput, ptrOuput, false);
        if (!CHCNetSDK.NET_DVR_STDXMLConfig(LoginHandle, ptrInput, ptrOuput))
        {
             Log.Debug("NET_DVR_STDXMLConfig fail [url:PUT /ISAPI/AccessControl/UserInfoDetail/Delete?format=json] error:" + CHCNetSDK.NET_DVR_GetLastError());
            Marshal.FreeHGlobal(ptrOutBuf);
            Marshal.FreeHGlobal(ptrStatusBuffer);
            Marshal.FreeHGlobal(ptrUserInfoDetailCfg);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOuput);
            Marshal.FreeHGlobal(ptrURL);
            return;
        }
        else
        {
            string strResponseStatus = Marshal.PtrToStringAnsi(struOuput.lpOutBuffer);
            CResponseStatus JsonResponseStatus = new CResponseStatus();
            JsonResponseStatus = JsonConvert.DeserializeObject<CResponseStatus>(strResponseStatus);
            if (JsonResponseStatus.statusCode != 1)
            {
                 Log.Debug("NET_DVR_STDXMLConfig Return ResponseStatus.statusCode:" + JsonResponseStatus.statusCode);
            }

            Marshal.FreeHGlobal(ptrOutBuf);
            Marshal.FreeHGlobal(ptrStatusBuffer);
            Marshal.FreeHGlobal(ptrUserInfoDetailCfg);
            Marshal.FreeHGlobal(ptrInput);
            Marshal.FreeHGlobal(ptrOuput);
            Marshal.FreeHGlobal(ptrURL);
        }

        if (-1 != m_lDelUserCfgHandle)
        {
            if (CHCNetSDK.NET_DVR_StopRemoteConfig(m_lDelUserCfgHandle))
            {
                m_lDelUserCfgHandle = -1;
            }
        }
        string sUrlDeleteProcess = "GET /ISAPI/AccessControl/UserInfoDetail/DeleteProcess?format=json";
        IntPtr ptrUrlDeleteProcess = Marshal.StringToHGlobalAnsi(sUrlDeleteProcess);
        m_lDelUserCfgHandle = CHCNetSDK.NET_DVR_StartRemoteConfig(LoginHandle, CHCNetSDK.NET_DVR_JSON_CONFIG, ptrUrlDeleteProcess, sUrlDeleteProcess.Length, null, IntPtr.Zero);
        if (m_lDelUserCfgHandle < 0)
        {
             Log.Debug("NET_DVR_StartRemoteConfig fail [url:GET /ISAPI/AccessControl/UserInfoDetail/DeleteProcess?format=json] error:" + CHCNetSDK.NET_DVR_GetLastError());
            Marshal.FreeHGlobal(ptrUrlDeleteProcess);
            return;
        }
        else
        {
            IntPtr ptrJsonData = Marshal.AllocHGlobal(1024);
            for (int i = 0; i < 1024; i++)
            {
                Marshal.WriteByte(ptrJsonData, i, 0);
            }
            int dwState = (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS;
            while (true)
            {
                dwState = CHCNetSDK.NET_DVR_GetNextRemoteConfig(m_lDelUserCfgHandle, ptrJsonData, 1024);
                if (dwState == -1)
                {
                    uint a = CHCNetSDK.NET_DVR_GetLastError();
                }
                string strJsonData = Marshal.PtrToStringAnsi(ptrJsonData);
                if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_NEEDWAIT)
                {
                    Thread.Sleep(10);
                    continue;
                }
                else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FAILED)
                {
                     Log.Debug("Get DelUser Process Fail error:" + CHCNetSDK.NET_DVR_GetLastError());
                }
                else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_SUCCESS)
                {
                    CUserInfoDetailDeleteProcessCfg JsonUserInfoSearchCfg = new CUserInfoDetailDeleteProcessCfg();
                    JsonUserInfoSearchCfg = JsonConvert.DeserializeObject<CUserInfoDetailDeleteProcessCfg>(strJsonData);
                    if (JsonUserInfoSearchCfg.UserInfoDetailDeleteProcess == null)
                    {
                        //null说明返回的Json报文不是UserInfoSearch，而是ResponseStatus
                        CResponseStatus JsonResponseStatus = new CResponseStatus();
                        JsonResponseStatus = JsonConvert.DeserializeObject<CResponseStatus>(strJsonData);
                        if (JsonResponseStatus.statusCode == 1)
                        {
                            //不会走到这里
                             Log.Debug("Get DelUser Process Success");
                        }
                        else
                        {
                             Log.Debug("Get DelUser Process Fail, ResponseStatus.statusCode" + JsonResponseStatus.statusCode);
                        }
                    }
                    else
                    {
                        //解析UserInfoDetailDeleteProcess报文
                        if (JsonUserInfoSearchCfg.UserInfoDetailDeleteProcess.status == "success")
                        {
                             Log.Debug("Del User Success");
                        }
                        else if (JsonUserInfoSearchCfg.UserInfoDetailDeleteProcess.status == "failed")
                        {
                             Log.Debug("Del User Failed");
                        }
                        else if (JsonUserInfoSearchCfg.UserInfoDetailDeleteProcess.status == "processing")
                        {
                             Log.Debug("Del User processing");
                        }
                    }
                    break;
                }
                else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_FINISH)
                {
                     Log.Debug("Get DelUser Process Finish");
                    break;
                }
                else if (dwState == (int)CHCNetSDK.NET_SDK_SENDWITHRECV_STATUS.NET_SDK_CONFIG_STATUS_EXCEPTION)
                {
                     Log.Debug("Get DelUser Process Exception error:" + CHCNetSDK.NET_DVR_GetLastError());
                    break;
                }
                else
                {
                     Log.Debug("unknown Status Error:" + CHCNetSDK.NET_DVR_GetLastError());
                    break;
                }
            }
            Marshal.FreeHGlobal(ptrJsonData);
        }
        if (-1 != m_lDelUserCfgHandle)
        {
            if (CHCNetSDK.NET_DVR_StopRemoteConfig(m_lDelUserCfgHandle))
            {
                m_lDelUserCfgHandle = -1;
            }
        }
        Marshal.FreeHGlobal(ptrUrlDeleteProcess);
    }
    #endregion
    public void cbLoginCallBack(int lUserID, uint dwResult, ref CHCNetSDK.NET_DVR_DEVICEINFO_V30 lpDeviceInfo, IntPtr pUser)
    {
        string strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;
        //ID = strLoginCallBack;
    }

    /// <summary>
    /// NET_DVR_Logout（）注销设备，每台设备调用一次；NET_DVR_Cleanup()释放SDK所有资源
    /// </summary>
    public void LogOut(string ip, int LoginHandle)
    {

        //CHCNetSDK.NET_DVR_Logout(LoginHandle);//注销
        //CHCNetSDK.NET_DVR_Cleanup();//释放sdk资源
        NVRController.Instance().Logout();
        Log.Debug("IP:" + ip + "登出");
    }

    private void OnDisable()
    {
         Log.Debug("OnDisable");
        foreach (var item in HikvisonNVR.LoginhandleDic)
        {
            LogOut(item.Key, item.Value);
        }
    }
}
