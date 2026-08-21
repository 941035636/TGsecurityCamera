// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;

namespace zFramework.Media
{
    //For Demo
    public class NVRController : MonoBehaviour
    {
        SecurityCamera cam;


        private static NVRController instance;
        public static NVRController Instance()
        {
            if (instance == null)
            {
                instance = new NVRController();
            }
            return instance;
        }

        private void Awake()
        {
            //Login();
        }
        private void Start()
        {
            cam = GetComponent<SecurityCamera>();

        }

        private void OnSteamTypeChanged(int arg0)
        {
            cam.Stop();
            cam.steamType = (STREAM)arg0;
            cam.PlayReal();
             Log.Debug($"{nameof(NVRController)}: 完成主辅流的切换");
        }

        public void Login() => _ = LoginAsync(false);
        public void Logout() => _ = LogoutAsync();

        public void Login(NVRInformation nvr,bool needPlay) => _ = LoginAsync(nvr, needPlay);
        public void Logout(string host) => _ = LogoutAsync(host);
        //门禁登录控制
        public void LoginDoor(NVRInformation nvr) => _ = LoginDoorAsync(nvr);

        public void LoginSingleBySend(NVRInformation nVR, int type) => _ = LoginSingleAsync(nVR, type);

        public void LoginUserToDoor(string usertype,string host, string idNum ,string username , NVRInformation nvrinfo,int type = 0) => LoginAsyncUserToDoor(usertype,host,idNum,username, nvrinfo,type);

        public void HttpAsyncGet(string idnum, int pagenum, int pagesize, int type) => _ = HttpAsyncGetdata(idnum, pagenum, pagesize, type);

        async Task LoginAsync(bool needPlay)
        {
            //login.text = "登录中";
            //login_bt.interactable = false;
            await NVRManager.LoginAllAsync(needPlay);
            //login.text = "已登录";
            //logout.text = "登出";
            //login_bt.interactable = true;

        }
        //下发人员信息给门禁的异步登录方法
        async void LoginAsyncUserToDoor(string usertype,string host,string idNum,string username, NVRInformation nvrinfo, int type)
        {

            await NVRManager.LoginAsync(nvrinfo, false);
            if (HikvisonNVR.LoginhandleDic[host.Split(':')[0]] != -1)
            {
              
                ////登录成功，建立长连接
                PeopleJsonParsing.GetInstance().OpenConnectAllDevs(host.Split(':')[0]);

            }
            else
            {
                Log.Debug(host + "登录不成功");
            }
            //await HttpAsyncGetdata("", 1, 1, type);
            await SendUserinfoToDevAsync(usertype,idNum,username, host.Split(':')[0]);

        }

       /// <summary>
       /// 下发人员信息到门禁设备
       /// </summary>
       /// <param name="type">人员类型： 普通、黑名单</param>
       /// <param name="idNum">人员id</param>
       /// <param name="username">人员姓名</param>
       /// <param name="host"></param>
       /// <returns></returns>
        async Task SendUserinfoToDevAsync(string type,string idNum,string username,string host)
        {
       
            string datetimeStart = "2023-03-11T16:03:08";
            string datetimeEnd = "2035-06-11T16:05:00";
            await Task.Delay(TimeSpan.FromTicks(1));
            Log.Debug("host:" + host + "     PeopleJsonParsing.GetInstance().IPBandConnectHandleUserDic.ContainsKey(host):" + PeopleJsonParsing.IPBandConnectHandleUserDic.ContainsKey(host));
            if (PeopleJsonParsing.IPBandConnectHandleUserDic.ContainsKey(host)) 
                HKPerson.GetInstance().adduser(idNum, username, type, datetimeStart, datetimeEnd, PeopleJsonParsing.IPBandConnectHandleUserDic[host]);//下发人员基本信息
                await Task.Delay(TimeSpan.FromTicks(1));
            if (PeopleJsonParsing.IPBandConnectHandleCardDic.ContainsKey(host))
                HKPerson.GetInstance().CardAdd(idNum, idNum, PeopleJsonParsing.IPBandConnectHandleCardDic[host]);//下发身份证号
                await Task.Delay(TimeSpan.FromTicks(1));
            if (PeopleJsonParsing.IPBandConnectHandleFaceDic.ContainsKey(host))
                HKPerson.GetInstance().FaceData(idNum, PeopleJsonParsing.IPBandConnectHandleFaceDic[host]);//下发人脸  
            
         

        }
        //关闭设备长连接并登出设备
        async Task CloseDevice(string host)
        {
            await Task.Delay(TimeSpan.FromTicks(1));
            if (HikvisonNVR.LoginhandleDic.ContainsKey(host))
            {
        
                //关闭该设备的长连接
                if (CHCNetSDK.NET_DVR_StopRemoteConfig(HikvisonNVR.LoginhandleDic[host]))
                    Log.Debug("关闭ip为" + host + "的UserInfo长连接");
                if (CHCNetSDK.NET_DVR_StopRemoteConfig(HikvisonNVR.LoginhandleDic[host]))
                    Log.Debug("关闭ip为" + host + "的CardInfo长连接");
                if (CHCNetSDK.NET_DVR_StopRemoteConfig(HikvisonNVR.LoginhandleDic[host]))
                    Log.Debug("关闭ip为" + host + "的FaceInfo长连接");
                NVRController.Instance().Logout(host);//登出设备
                                                                                               //移除连接句柄                                             
               PeopleJsonParsing.IPBandConnectHandleUserDic.Remove(host);
                PeopleJsonParsing.IPBandConnectHandleCardDic.Remove(host);
                PeopleJsonParsing.IPBandConnectHandleFaceDic.Remove(host);
                HikvisonNVR.LoginhandleDic.Remove(host);
            }

           

        }



        async Task HttpAsyncGetdata(string idnum, int pagenum, int pagesize, int type)
        {
            await Task.Delay(TimeSpan.FromTicks(1));
            PeopleJsonParsing.Instance.HttpPageGetData(idnum, pagenum, pagesize, type);
        }



        async Task LoginAsync(NVRInformation nvr,bool needPlay)
        {

            await NVRManager.LoginAsync(nvr,needPlay);
          
            EventCenter.BroadCast<string>(Eventdefine.DeviceNetSate, nvr.host.Split(':')[0]);
       
        }
        //门禁登录单个设备,控制门禁状态
        async Task LoginDoorAsync(NVRInformation nvr)
        {
            Debug.LogError("登录门禁");
            await NVRManager.LoginAsync(nvr,false);
            EventCenter.BroadCast<string>(Eventdefine.DoorNetState, nvr.host.Split(':')[0]);
 
        }

        //异步登录单个门禁设备用于下发
        async Task LoginSingleAsync(NVRInformation nvr, int type)
        {
            await NVRManager.LoginAsync(nvr,false);
            int loginhandle;
            if (HikvisonNVR.LoginhandleDic.TryGetValue(nvr.host.Split(':')[0], out loginhandle) && loginhandle > -1)
                await ConnectAsync(loginhandle);
            else
            {
                Log.Debug(nvr.host + "登录失败,无法建立长连接,登录下一个");
                PeopleJsonParsing.Instance.nowSendIndex++;
                PeopleJsonParsing.Instance.startpage = 1;
                if (PeopleJsonParsing.Instance.nowSendIndex < PeopleJsonParsing.Instance.SendDeviceCount)
                {
                    LoginSingleBySend(PeopleJsonParsing.Instance.AreaallEquip[PeopleJsonParsing.Instance.nowSendIndex], TypeMenuController.Ins.ChooseType);
                    Log.Debug("再登录:" + PeopleJsonParsing.Instance.AreaallEquip[PeopleJsonParsing.Instance.nowSendIndex].host + "门禁设备");
                    return;
                }
            }

            await HttpAsyncGetData("", 1, 1, type);


        }
        //单个建立长连接
        async Task ConnectAsync(int loginhandle)
        {
            await Task.Delay(TimeSpan.FromTicks(1));
            if (loginhandle > -1)
            {
                HKPerson.Instance.OpenDeviceConnect(loginhandle, 0);
                HKPerson.Instance.OpenDeviceConnect(loginhandle, 1);
                HKPerson.Instance.OpenDeviceConnect(loginhandle, 2);
            }
        }

        //异步HTTp请求
        async Task HttpAsyncGetData(string idnum, int pagenum, int pagesize, int type)
        {
            Log.Debug("请求Http服务器");
            await Task.Delay(TimeSpan.FromTicks(1));
            PeopleJsonParsing.Instance.HttpPageGetData(idnum, pagenum, pagesize, type);

        }

        async Task LogoutAsync(string host)
        {
            Log.Error("登出:" + host);
            await NVRManager.LogoutAsync(host);
        }

        async Task LogoutAsync()
        {
            //logout.text = "登出中";
            //logout_bt.interactable = false;
            await NVRManager.LogoutAllAsync();
            //logout.text = "已登出";
            //login.text = "登录";
            //logout_bt.interactable = true;
        }
    }
}
