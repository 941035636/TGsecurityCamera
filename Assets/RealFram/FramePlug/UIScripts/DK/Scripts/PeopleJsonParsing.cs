using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using UIWidgets.Examples.Shops;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;

public class PeopleJsonParsing : MonoSingleton<PeopleJsonParsing>
{

    private static PeopleJsonParsing _Instance;
    public static PeopleJsonParsing GetInstance()
    {
        if (_Instance == null)
        {
            _Instance = new PeopleJsonParsing();
        }

        return _Instance;
    }


    UserGroup userGroup = new UserGroup();


    [HideInInspector]
    public int startpage = 1;
    [HideInInspector]
    public bool againrequest = false;
    [HideInInspector]
    public int typePage = 0;
    [HideInInspector]
    public static Dictionary<string, int> IPBandConnectHandleUserDic = new Dictionary<string, int>();
    [HideInInspector]
    public static Dictionary<string, int> IPBandConnectHandleCardDic = new Dictionary<string, int>();
    [HideInInspector]
    public static Dictionary<string, int> IPBandConnectHandleFaceDic = new Dictionary<string, int>();

    [HideInInspector]
    public List<int> StartIssueUser = new List<int>();//存放需要下发的人员基础信息的id
    [HideInInspector]
    public List<int> StartIssueIdCard = new List<int>();//存放需要下发的人员信息的
    public Transform SendPanelTrans;
    ////存储下发成功的身份id   设备IP对应人员
    //public Dictionary<string, List<string>> UidSuccessDic = new Dictionary<string, List<string>>();
    ////public List<string> UidList = new List<string>();
    ////存储下发成功的身份证id
    //public Dictionary<string, List<string>> CardSuccessDic = new Dictionary<string, List<string>>();
    ////public List<string> CardList = new List<string>();
    ////存储下发成 的人脸id
    //public Dictionary<string, List<string>> FaceSuccessDic = new Dictionary<string, List<string>>();
    //public List<string> FaceList = new List<string>();

    public List<NVRInformation> AreaallEquip = new List<NVRInformation>();
    public int SendDeviceCount = 0;
    public int nowSendIndex = 0;
    //初始化SDK并逐个登录当前用户类型对应的门禁设备
    public void LoginDevices(int type)
    {




        AreaallEquip.Clear();
        Log.Debug("分组下发");
        HikvisonNVR.LoginhandleDic.Clear();

     

        foreach (var item in TypeMenuController.UserTypebandsDevs)
        {
            Log.Debug("人员类型：" + item.Key + "设备容器：" + item.Value.Count);
            if (item.Key == TypeMenuController.Ins.ChooseType)
            {
                AreaallEquip = item.Value;
            }
            //for (int i = 0; i < item.Value.Count; i++)
            //{
            //    List<string> UidList = new List<string>();
            //    List<string> CardList = new List<string>();
            //    List<string> FaceList = new List<string>();
            //    UidSuccessDic.Add(item.Value[i].Ip, UidList);
            //    CardSuccessDic.Add(item.Value[i].Ip,CardList);
            //    FaceSuccessDic.Add(item.Value[i].Ip,FaceList);


            //}
        }


        SendDeviceCount = AreaallEquip.Count();
        nowSendIndex = 0;
        if (AreaallEquip.Count != 0)
            NVRController.Instance().LoginSingleBySend(AreaallEquip[nowSendIndex], type);



    }
    //点击当前人员类型页面一键下发按钮的时候调用开启下发长连接
    public void OpenConnectAllDevs(string host)
    {

        Log.Debug(host + " 登录成功，开始建立长连接");
        if (HikvisonNVR.LoginhandleDic.Count > 0)
        {

            if (HikvisonNVR.LoginhandleDic.ContainsKey(host))
            {
                Log.Debug("建立IP:" + host + "登陆句柄：" + HikvisonNVR.LoginhandleDic[host] + "的长连接");
                if (!IPBandConnectHandleUserDic.ContainsKey(host))
                    HKPerson.GetInstance().OpenDeviceConnect(HikvisonNVR.LoginhandleDic[host], 0);
                if (!IPBandConnectHandleCardDic.ContainsKey(host))
                    HKPerson.GetInstance().OpenDeviceConnect(HikvisonNVR.LoginhandleDic[host], 1);
                if (!IPBandConnectHandleFaceDic.ContainsKey(host))
                    HKPerson.GetInstance().OpenDeviceConnect(HikvisonNVR.LoginhandleDic[host], 2);
            }
            else 
            {
                Log.Debug("登录句柄字典中不存在host："+host);
            }
        }
        else 
        {
            Log.Debug("登录句柄字典个数为0");
        }




    }


    /// <summary>
    /// 分页请求信息
    /// </summary>
    /// <param name="idNum">传值的就是单个请求，不传代表请求所有，分页请求不用传   </param>
    /// <param name="PageNum"> 请求第几页</param>
    /// <param name="PageSize">一页多少条数据</param>
    public void HttpPageGetData(string idNum, int PageNum, int PageSize, int Type)
    {
        string url = GameStart.ApiUrl("/api/personnel/tg/user/client?");
        string param = "idNum=" + idNum + "&pageNum=" + PageNum.ToString() + "&pageSize=" + PageSize.ToString() + "&type=" + Type.ToString() + "&issued=-1";
        HttpNetManager.GetInstance().SendDataStr(url + param, PagehttpCallback, false, false, true);
        Log.Debug("请求的Http连接:" + url + param);
        //进度条当前进度赋值
        SendPanelTrans.Find("BG/Slider").GetComponent<Slider>().value = nowSendIndex * TotalPeopleCount + PageNum;
       
    }
    //分页Http请求get回调
    public void PagehttpCallback(HttpCallBackArgs args)
    {

        print("Http收到服务器信息:" + args.Value);

        if (!string.IsNullOrEmpty(args.Value))
        {
            //进度条总量赋值
            UserInfoArr Users = JsonUtility.FromJson<UserInfoArr>(args.Value);
            TotalPeopleCount = Users.total;
            SendPanelTrans.Find("BG/Slider").GetComponent<Slider>().maxValue = TotalPeopleCount * SendDeviceCount;
            StartSerialAsync(args.Value);

            //HttpPageGetData("", ++startpage, 1, TypeMenuController.Ins.ChooseType);
        }

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





    void StartSerialAsync(string receivestr) => _ = SerialObjAsync(receivestr);
    async Task SerialObjAsync(string receivestr)
    {
        await Task.Delay(TimeSpan.FromTicks(1));

        UserInfoArr userInfoarr = JsonUtility.FromJson<UserInfoArr>(receivestr);
        HttpPageDeserialization(userInfoarr);
    }

    /// <summary>
    /// 反序列化分页请求返回的Json
    /// </summary>
    public int TotalPeopleCount = 0;
    public void HttpPageDeserialization(UserInfoArr userInfoarr)
    {


        if (userInfoarr.records.Count == 0)
        {
            againrequest = false;
            int loginhandle;
            if (HikvisonNVR.LoginhandleDic.TryGetValue(AreaallEquip[nowSendIndex].host.Split(':')[0], out loginhandle))

                //关闭该设备的长连接
                if (CHCNetSDK.NET_DVR_StopRemoteConfig(loginhandle))
                    Log.Debug("关闭LoginHandle为" + loginhandle + "的UserInfo长连接");
            if (CHCNetSDK.NET_DVR_StopRemoteConfig(loginhandle))
                Log.Debug("关闭LoginHandle为" + loginhandle + "的CardInfo长连接");
            if (CHCNetSDK.NET_DVR_StopRemoteConfig(loginhandle))
                Log.Debug("关闭LoginHandle为" + loginhandle + "的FaceInfo长连接");
            NVRController.Instance().Logout(AreaallEquip[nowSendIndex].host.Split(':')[0]);//登出设备
            //移除连接句柄                                             
            IPBandConnectHandleUserDic.Clear();
            IPBandConnectHandleCardDic.Clear();
            IPBandConnectHandleFaceDic.Clear();
            HikvisonNVR.LoginhandleDic.Remove(AreaallEquip[nowSendIndex].host.Split(':')[0]);

            ++nowSendIndex;
            startpage = 1;
            if (nowSendIndex < SendDeviceCount)
            {
                NVRController.Instance().LoginSingleBySend(AreaallEquip[nowSendIndex], TypeMenuController.Ins.ChooseType);
                Log.Debug("再登录:" + AreaallEquip[nowSendIndex].host + "门禁设备");
                return;
            }
            else
            {
                Log.Debug("发送完");
                return;
            }


        }
        else
        {
            againrequest = true;
            //Log.Error("本次请求的人数:" + userInfoarr.records.Count);
            for (int i = 0; i < userInfoarr.records.Count; i++)
            {
                if (!string.IsNullOrEmpty(userInfoarr.records[i].faceBase64))
                {
                    SaveFacePic(userInfoarr.records[i].faceBase64, @"C:\facepicture", userInfoarr.records[i].idNum);
                    userInfoarr.records[i].faceBase64 = null;
       

                }





            }

            //下发到门禁设备
            //BrocastUserInfoToDevices(userInfoarr.records);



        }

    }
    //遍历本页请求的人员信息下发给对应的门禁设备
    private void BrocastUserInfoToDevices(List<UserInfo> userlist)
    {
        Log.Debug("要下发的人员类型容器容量：" + userlist.Count);
        //for (int i = 0; i < userlist.Count; i++)
        //{

        //    SubUserToTypeDevice(userlist[i], type);

        //}
        startUserAsync(userlist);
        //if (againrequest)
        //{
        //    ++startpage;
        //    Log.Debug("再次请求第" + startpage + "页");
        //    HttpPageGetData("", startpage, 1, type);
        //}

    }

    //调用异步
    private void startUserAsync(List<UserInfo> userlist) => _ = SubAllUserInfo(userlist);

    //异步下发单个人员信息
    async Task SubAllUserInfo(List<UserInfo> userlist)
    {
        var tasks1 = new List<Task>();
        for (int i = 0; i < userlist.Count; i++)
            tasks1.Add(SubOneUserInfo(userlist[i], IPBandConnectHandleUserDic.ElementAt(0).Value));
        await Task.WhenAll(tasks1);
        var tasks2 = new List<Task>();
        for (int i = 0; i < userlist.Count; i++)
            tasks2.Add(SubOneCardInfo(userlist[i], IPBandConnectHandleCardDic.ElementAt(0).Value));
        await Task.WhenAll(tasks2);
        var tasks3 = new List<Task>();
        for (int i = 0; i < userlist.Count; i++)
            tasks3.Add(SubOneFaceInfo(userlist[i], IPBandConnectHandleFaceDic.ElementAt(0).Value));
        await Task.WhenAll(tasks3);
        userGroup.employeelist.Clear();
        HttpPageGetData("", ++startpage, 1, TypeMenuController.Ins.ChooseType);
        Resources.UnloadUnusedAssets();
        GC.Collect();
        Log.Debug("异步下发单个用户信息完成再次请求第" + startpage + "页");
      
    }
    //异步下发单个人员信息
    async Task SubOneUserInfo(UserInfo user, int handle)
    {

        await Task.Delay(TimeSpan.FromTicks(1));
        string datetimeStart = "2023-03-11T16:03:08";
        string datetimeEnd = "2036-06-11T16:05:00";
        HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, handle);//下发人员基本信息
        Log.Debug("异步下发人员信息:" + user.username + "完成:" + handle);
    }

    //异步下发单个身份证信息
    async Task SubOneCardInfo(UserInfo user, int handle)
    {

        await Task.Delay(TimeSpan.FromTicks(1));
        HKPerson.instance.CardAdd(user.idNum, user.idNum, handle);//下发身份证号
        Log.Debug("异步下发身份证号:" + user.username + "完成:" + handle);
    }
    //异步下发单个人脸信息
    async Task SubOneFaceInfo(UserInfo user, int handle)
    {
        await Task.Delay(TimeSpan.FromTicks(1));
        HKPerson.instance.FaceData(user.idNum, handle);//下发人脸  
        Log.Debug("异步下发人脸:" + user.username + "完成:" + handle);


    }






    /// <summary>
    /// 下发该人员信息到对应的单一类型的区域设备上
    /// </summary>
    /// <param name="user"></param>
    /// <param name="type"></param>
    public void SubUserToTypeDevice(UserInfo user, int type)

    {
        switch (type)
        {
            case (int)Usertype.employee:
                foreach (var area in UserTypeArea.GetInstance().usertypeareaDic[Usertype.employee])
                {
                    if (DoorJson.AreadeviceDic.ContainsKey(area))
                    {
                        for (int i = 0; i < DoorJson.AreadeviceDic[area].CameraInfos.Count; i++)
                        {


                            //string datestr= GetDateTimeByuserType(Usertype.employee,user.auth);
                            //string datetimeStart=datestr.Split('&')[0].Split('-')[0]+"T"+datestr.Split('&')[1].Split('-')[0];
                            //string datetimeEnd=datestr.Split('&')[0].Split('-')[1]+"T"+datestr.Split('&')[1].Split('-')[1];
                            string datetimeStart = "2023-03-11T16:03:08";
                            string datetimeEnd = "2023-06-11T16:05:00";
                            if (IPBandConnectHandleUserDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把   身份信息   ：   " + user.idNum + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);
                                HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人员基本信息

                            }
                            if (IPBandConnectHandleCardDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把     身份证信息：     " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.CardAdd(user.idNum, user.idNum, IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发身份证号

                            }
                            if (IPBandConnectHandleFaceDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把  人脸信息：        " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.FaceData(user.idNum, IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人脸                                                                                                                                                               
                            }


                        }
                    }




                }
                break;
            case (int)Usertype.retirees:
                Log.Debug("退休员工的区域容器大小:" + UserTypeArea.GetInstance().usertypeareaDic[Usertype.retirees].Count);
                foreach (var area in UserTypeArea.GetInstance().usertypeareaDic[Usertype.retirees])
                {
                    if (DoorJson.AreadeviceDic.ContainsKey(area))
                    {
                        for (int i = 0; i < DoorJson.AreadeviceDic[area].CameraInfos.Count; i++)
                        {


                            //string datestr= GetDateTimeByuserType(Usertype.employee,user.auth);
                            //string datetimeStart=datestr.Split('&')[0].Split('-')[0]+"T"+datestr.Split('&')[1].Split('-')[0];
                            //string datetimeEnd=datestr.Split('&')[0].Split('-')[1]+"T"+datestr.Split('&')[1].Split('-')[1];
                            string datetimeStart = "2023-03-11T16:03:08";
                            string datetimeEnd = "2023-06-11T16:05:00";
                            if (IPBandConnectHandleUserDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把   身份信息   ：   " + user.idNum + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);
                                HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人员基本信息

                            }
                            if (IPBandConnectHandleCardDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把     身份证信息：     " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.CardAdd(user.idNum, user.idNum, IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发身份证号

                            }
                            if (IPBandConnectHandleFaceDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把  人脸信息：        " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.FaceData(user.idNum, IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人脸                                                                                                                                                               
                            }


                        }
                    }




                }
                break;
            case (int)Usertype.family_mem:
                foreach (var area in UserTypeArea.GetInstance().usertypeareaDic[Usertype.family_mem])
                {
                    if (DoorJson.AreadeviceDic.ContainsKey(area))
                    {
                        for (int i = 0; i < DoorJson.AreadeviceDic[area].CameraInfos.Count; i++)
                        {


                            //string datestr= GetDateTimeByuserType(Usertype.employee,user.auth);
                            //string datetimeStart=datestr.Split('&')[0].Split('-')[0]+"T"+datestr.Split('&')[1].Split('-')[0];
                            //string datetimeEnd=datestr.Split('&')[0].Split('-')[1]+"T"+datestr.Split('&')[1].Split('-')[1];
                            string datetimeStart = "2023-03-11T16:03:08";
                            string datetimeEnd = "2023-06-11T16:05:00";
                            if (IPBandConnectHandleUserDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把   身份信息   ：   " + user.idNum + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);
                                HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人员基本信息

                            }
                            if (IPBandConnectHandleCardDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把     身份证信息：     " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.CardAdd(user.idNum, user.idNum, IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发身份证号

                            }
                            if (IPBandConnectHandleFaceDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                            {
                                Log.Debug("把  人脸信息：        " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                HKPerson.instance.FaceData(user.idNum, IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人脸                                                                                                                                                               
                            }


                        }
                    }




                }
                break;
            default:
                break;
        }






    }
    //下发具体人员信息到对应区域的门禁上
    public void SubUserToDevice(UserInfo user, int type)

    {

        foreach (var item in user.auth)
        {
            switch (item.type)
            {
                case (int)Usertype.employee:
                    foreach (var area in UserTypeArea.GetInstance().usertypeareaDic[Usertype.employee])
                    {
                        if (DoorJson.AreadeviceDic.ContainsKey(area))
                        {
                            for (int i = 0; i < DoorJson.AreadeviceDic[area].CameraInfos.Count; i++)
                            {


                                //string datestr= GetDateTimeByuserType(Usertype.employee,user.auth);
                                //string datetimeStart=datestr.Split('&')[0].Split('-')[0]+"T"+datestr.Split('&')[1].Split('-')[0];
                                //string datetimeEnd=datestr.Split('&')[0].Split('-')[1]+"T"+datestr.Split('&')[1].Split('-')[1];
                                string datetimeStart = "2023-03-11T16:03:08";
                                string datetimeEnd = "2023-04-11T16:05:00";
                                if (IPBandConnectHandleUserDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把   身份信息   ：   " + user.idNum + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);
                                    HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人员基本信息

                                }
                                if (IPBandConnectHandleCardDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把     身份证信息：     " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                    HKPerson.instance.CardAdd(user.idNum, user.idNum, IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发身份证号

                                }
                                if (IPBandConnectHandleFaceDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把  人脸信息：        " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                    HKPerson.instance.FaceData(user.idNum, IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人脸                                                                                                                                                               
                                }


                            }
                        }




                    }
                    break;
                case (int)Usertype.retirees:
                    foreach (var area in UserTypeArea.GetInstance().usertypeareaDic[Usertype.retirees])
                    {
                        if (DoorJson.AreadeviceDic.ContainsKey(area))
                        {
                            for (int i = 0; i < DoorJson.AreadeviceDic[area].CameraInfos.Count; i++)
                            {


                                //string datestr= GetDateTimeByuserType(Usertype.employee,user.auth);
                                //string datetimeStart=datestr.Split('&')[0].Split('-')[0]+"T"+datestr.Split('&')[1].Split('-')[0];
                                //string datetimeEnd=datestr.Split('&')[0].Split('-')[1]+"T"+datestr.Split('&')[1].Split('-')[1];
                                string datetimeStart = "2023-03-11T16:03:08";
                                string datetimeEnd = "2023-04-11T16:05:00";
                                if (IPBandConnectHandleUserDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把   身份信息   ：   " + user.idNum + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);
                                    HKPerson.Instance.adduser(user.idNum, user.username, "normal", datetimeStart, datetimeEnd, IPBandConnectHandleUserDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人员基本信息

                                }
                                if (IPBandConnectHandleCardDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把     身份证信息：     " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                    HKPerson.instance.CardAdd(user.idNum, user.idNum, IPBandConnectHandleCardDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发身份证号

                                }
                                if (IPBandConnectHandleFaceDic.ContainsKey(DoorJson.AreadeviceDic[area].CameraInfos[i].Ip))
                                {
                                    Log.Debug("把  人脸信息：        " + user.username + "     下发到东一门IP为   " + DoorJson.AreadeviceDic[area].CameraInfos[i].Ip + "   的设备上连接句柄:" + IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);

                                    HKPerson.instance.FaceData(user.idNum, IPBandConnectHandleFaceDic[DoorJson.AreadeviceDic[area].CameraInfos[i].Ip]);//下发人脸                                                                                                                                                               
                                }


                            }
                        }




                    }
                    break;
                default:
                    break;
            }

        }

    }

    private string GetDateTimeByuserType(Usertype type, List<Auth> auths)
    {
        for (int i = 0; i < auths.Count; i++)
        {
            if (auths[i].type == (int)type)
            {

                return auths[i].authDate + "&" + auths[i].authTime;
            }
        }
        return null;

    }


}
