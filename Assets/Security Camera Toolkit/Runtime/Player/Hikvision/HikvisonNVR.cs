// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.

using Hikvision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using zFramework.Media.Internal;

namespace zFramework.Media
{
    public class HikvisonNVR : NVR
    {

        /// <summary>
        /// 断言 NVR 是否登录
        /// </summary>
        public override bool IsLogin => (int)(loginHandle ?? -1) > -1;
        public static Dictionary<string, int> LoginhandleDic = new Dictionary<string, int>();
        public static Dictionary<string, string> SerinumDic = new Dictionary<string, string>();
        public HikvisonNVR(NVRInformation data) : base(data) { }
        private CHCNetSDK.MSGCallBack_V31 m_falarmData_V31 = null;//报警回调函数
        byte[] faceby;
        public override bool CleanUp()
        {
            var state = CHCNetSDK.NET_DVR_Cleanup();
            if (!state)
            {
                Log.Debug($"{nameof(HikvisonNVR)}: SDK Clean UP 失败");
            }
            return state;
        }

        public override bool InitSDK()

        {
            CHCNetSDK.NET_DVR_GetLastError();
            CHCNetSDK.NET_DVR_SetConnectTime(2000, 3);
            CHCNetSDK.NET_DVR_SetReconnect(1000, 1);
            var state = CHCNetSDK.NET_DVR_Init();
            if (!state)
            {
                Log.Debug($"{nameof(HikvisonNVR)}: SDK 初始化失败");
            }


            //Sdk针对门禁设备设置报警回调函数
            if (m_falarmData_V31 == null)
            {

                m_falarmData_V31 = new CHCNetSDK.MSGCallBack_V31(MsgCallback_V31);
                Log.Debug("设置报警回调函数");
            }
            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero);// 注册回调函数，接收设备报警消息等。TRUE表示成功，FALSE表示失败


            return state;
        }
        //设置SDK报警回调
        public bool MsgCallback_V31(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            // //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
            //print(lCommand + " : " + pAlarmer.sSerialNumber + " : " + pAlarmInfo + " : " + dwBufLen + " : " + pUser);

            return true;//回调函数需要有返回，表示正常接收到数据
        }
        //设置sdk报警回调
        public void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            switch (lCommand)
            {

                case CHCNetSDK.COMM_ALARM_ACS://门禁主机报警上传
                                              //ProcessCommAlarm_AcsAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    CHCNetSDK.NET_DVR_ACS_ALARM_INFO struAcsAlarm = new CHCNetSDK.NET_DVR_ACS_ALARM_INFO();


                    uint dwSize = (uint)Marshal.SizeOf(struAcsAlarm);
                    struAcsAlarm = (CHCNetSDK.NET_DVR_ACS_ALARM_INFO)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_DVR_ACS_ALARM_INFO));

                    //报警设备IP地址
                    string strIP = System.Text.Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');




                    //报警时间：年月日时分秒
                    string strTimeYear = (struAcsAlarm.struTime.dwYear).ToString();
                    string strTimeMonth = (struAcsAlarm.struTime.dwMonth).ToString("d2");
                    string strTimeDay = (struAcsAlarm.struTime.dwDay).ToString("d2");
                    string strTimeHour = (struAcsAlarm.struTime.dwHour).ToString("d2");
                    string strTimeMinute = (struAcsAlarm.struTime.dwMinute).ToString("d2");
                    string strTimeSecond = (struAcsAlarm.struTime.dwSecond).ToString("d2");
                    string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;


                    //print("图片参数"+struAcsAlarm.dwPicDataLen + "  " + struAcsAlarm.pPicData);
                    string stringAlarm = "IP：" + strIP + "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
                   + "，身份证号:" + struAcsAlarm.struAcsEventInfo.dwEmployeeNo + "，报警触发时间：" + strTime;


                    DoorAlarmData alarmData = new DoorAlarmData();
                    //保存抓拍图片
                    if ((struAcsAlarm.dwPicDataLen != 0) && (struAcsAlarm.pPicData != IntPtr.Zero))
                    {
                        string str = "C:\\Picture\\[" + strIP + "]_lUerID_[" + pAlarmer.lUserID + "]_" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0') + " , " + strTimeSecond + " ,  " + ".jpg";
                        FileStream fs = new FileStream(str, FileMode.Create);
                        int iLen = (int)struAcsAlarm.dwPicDataLen;
                        byte[] by = new byte[iLen];
                        faceby = by;
                        Marshal.Copy(struAcsAlarm.pPicData, by, 0, iLen);
                        fs.Write(by, 0, iLen);
                        fs.Close();
                    }


                    /* 0x4b --人脸认证通过，0x4c--人脸认证失败 */
                   
                    if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4b")//人脸认证成功
                    {
                        //         print("图片参数"+struAcsAlarm.dwPicDataLen + "  " + struAcsAlarm.pPicData);
                        //     string stringAlarm = "IP：" + strIP +" 职工 "+ "，卡号：" + System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0')
                        //    + "，身份证号:" + struAcsAlarm.struAcsEventInfo.dwEmployeeNo +  "，报警触发时间：" + strTime ;
                    
           
                        alarmData.eventType = "人脸认证通过";
                        Log.Debug("人脸认证通过" + alarmData.eventType);


                        LoomAlarm.QueueOnMainThread((param) =>
                        {
                            //调用主线程UI实例化
                            if (EventCenterPanel.IsstartEventShow)
                            {       // 将图片数据加载到Texture2D
                                Texture2D tex = new Texture2D(1, 1);
                                tex.LoadImage(faceby);
                                alarmData.faceBase64 = Texture2DToBase64(tex);
                                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                                EventData data = new EventData(System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0'), "人脸认证通过", strTime, strIP, sprite);
                                //门禁事件界面主动生成消息注销，太耗费性能，改为从服务器分页请求
                                // EventCenter.BroadCast<EventData>(Eventdefine.DoorEquipListener, data);
                            }

                            //向服务器广播事件
                            alarmData.eventType = "人脸认证通过";
                            alarmData.equipIp = strIP;
                            alarmData.idNum = System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0');
                            alarmData.time = strTime;
                            alarmData.userName = System.Text.Encoding.UTF8.GetString(struAcsAlarm.sNetUser);
                            string eventstr = JsonUtility.ToJson(alarmData, true);
                            Log.Debug("向服务器推送的门禁事件:" + eventstr);
                            //向服务器广播事件
                            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/door/tg/door/alarm", UploadDoorEventCallback, true, true, false, eventstr);


                        }, null);

                        //UpdateClientList(stringAlarm);
                    }

                    //认证失败
                    if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "4c")
                    {
                      
                        LoomAlarm.QueueOnMainThread((param) =>
                        {
                            //调用主线程UI实例化
                            if (EventCenterPanel.IsstartEventShow)
                            {
                                // 将图片数据加载到Texture2D
                                Texture2D tex = new Texture2D(1, 1);
                                tex.LoadImage(faceby);
                                alarmData.faceBase64 = Texture2DToBase64(tex);
                                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                                EventData data = new EventData(System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0'), "人脸认证失败", strTime, strIP, sprite);
                               //门禁事件界面主动生成消息注销，太耗费性能，改为从服务器分页请求
                                // EventCenter.BroadCast<EventData>(Eventdefine.DoorEquipListener, data);

                            }

                            alarmData.eventType = "人脸认证失败";
                            alarmData.equipIp = strIP;
                            alarmData.idNum = System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0');
                            alarmData.time = strTime;
                            alarmData.userName = "";
                            string eventstr = JsonUtility.ToJson(alarmData, true);
                            Log.Debug("向服务器推送的门禁事件:" + eventstr);
                            //向服务器广播事件
                            HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/door/tg/door/alarm", UploadDoorEventCallback, true, true, false, eventstr);


                        }, null);

        

                    }


                    if (Convert.ToString(struAcsAlarm.dwMinor, 16) == "06")
                    {

                        LoomAlarm.QueueOnMainThread((param) =>
                        {
                            //调用主线程UI实例化
                            if (EventCenterPanel.IsstartEventShow)
                            {
                                // 将图片数据加载到Texture2D
                                Texture2D tex = new Texture2D(1, 1);
                                tex.LoadImage(faceby);
                                alarmData.faceBase64 = Texture2DToBase64(tex);
                                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                                EventData data = new EventData(System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0'), "人脸认证失败", strTime, strIP, sprite);
                                //门禁事件界面主动生成消息注销，太耗费性能，改为从服务器分页请求
                                // EventCenter.BroadCast<EventData>(Eventdefine.DoorEquipListener, data);

                            }

                            alarmData.eventType = "黑名单人员";
                            alarmData.equipIp = strIP;
                            alarmData.idNum = System.Text.Encoding.UTF8.GetString(struAcsAlarm.struAcsEventInfo.byCardNo).TrimEnd('\0');
                            alarmData.time = strTime;
                            alarmData.userName = "";
                            string eventstr = JsonUtility.ToJson(alarmData, true);
                            //向服务器广播事件
                            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/door/tg/door/alarm", UploadDoorEventCallback, true, true, false, eventstr);


                        }, null);



                    }

                    Log.Debug("验证姓名:" + alarmData.userName);
                     Log.Debug("验证ID:" + alarmData.idNum);
                    break;

                default:
                    {
                        //报警设备IP地址
                        //string strIP = System.Text.Encoding.UTF8.GetString(pAlarmer.sDeviceIP).TrimEnd('\0');
                        //print(strIP);

                        ////报警信息类型
                        //string stringAlarm = "报警上传，信息类型：0x" + Convert.ToString(lCommand, 16);

                        //}
                    }
                    break;

            }
        }


        //Texture2DToBase
        public String Texture2DToBase64(Texture2D t2d)
        {
            byte[] bytesArr = t2d.EncodeToJPG();
            string strbaser64 = Convert.ToBase64String(bytesArr);
            return strbaser64;
        }



        private void UploadDoorEventCallback(HttpCallBackArgs args)
        {
            if (!string.IsNullOrEmpty(args.Value))
            {
                Log.Debug("向服务器推送门禁事件后的回调："+args.Value);
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// <br>异步操作的 API ，请在调用时保持克制，在 UI 层面做一个互锁，简易示例见 NVRController</br>
        /// </summary>
        /// <returns></returns>
        public override async Task LoginAsync(NVRInformation nvr,bool needPlay)
        {

            if (!IsLogin)
            {
                var result = await Task.Run(() =>
                {
                    CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
                    loginHandle = CHCNetSDK.NET_DVR_Login_V30(data.Ip, (int)data.Port, nvr.userName, nvr.password, ref DeviceInfo);
                    Log.Error("IP:"+data.Ip+"   port:"+data.Port+" username:"+nvr.userName+" pawd:"+ nvr.password+"  needplay:"+needPlay+" loginhandle:"+LoginHandle);
                    if (!LoginhandleDic.ContainsKey(data.Ip))
                    {

                     
                      
                        LoginhandleDic.Add(data.Ip, (int)loginHandle);
                    }
                    else
                    {
                       
                        LoginhandleDic[data.Ip] = (int)loginHandle;
                    }

                    var Result = (int)loginHandle != -1;
                    //Log.Error(Result);
                    if (Result)
                    {
                        // 向挂载的监控发送 登录状态 , 注意，此动作在非Unity主线程中进行
                        Log.Debug($"{data.type} - {data.ActiveHost} NVR 登录成功：{loginHandle}");
                        if (!SerinumDic.ContainsKey(data.Ip))
                        {
                            SerinumDic.Add(data.Ip, System.Text.Encoding.UTF8.GetString(DeviceInfo.sSerialNumber));
                        }
                        else 
                        {
                            SerinumDic[data.Ip] = System.Text.Encoding.UTF8.GetString(DeviceInfo.sSerialNumber);
                        }

                        Log.Error("该设备类型:" + data.equiptype + "  IP:" + data.Ip + "   是否播放监控:" + needPlay);
                        if (string.Equals(nvr.equiptype, "编码设备")&&needPlay) 
                        {

                            Loom.Post(() => {

                                Log.Error("正在准备播放："+nvr.Ip+"  devid:"+nvr.id);
                                EventCenter.BroadCast(Eventdefine.CameraPlayEvent, nvr);
                            });


                        }

                    }
                    else
                    {
                        Log.Error("nvr:"+nvr.equiptype);
                        if (string.Equals(nvr.equiptype, "编码设备") && needPlay)
                        {

                            GameStart.Instance.ShowTip("设备已离线，请检查网络!");

                        }
                        Log.Debug(data.ActiveHost+"+ 登录失败,登录句柄："+LoginHandle);
                        Log.Debug($"{data.type} - {data.ActiveHost} NVR 登录失败,ErrorCode = {CHCNetSDK.NET_DVR_GetLastError()}     " + data.Ip + " " + (int)data.Port + " " + data.userName + "  " + data.password);
                    }
                    return Result;
                });
                if (result)
                {
                    await base.LoginAsync(nvr,needPlay);
                    Log.Debug($"{nameof(HikvisonNVR)}: 所有 SecurityCamera 同步 NVR 登录状态成功");

                }
            }
            else
            {
                //LoginhandleDic.Clear();
                //LoginhandleDic.Add(data.Ip, (int)loginHandle);
                Log.Debug($"{data.type} - {data.ActiveHost} SDK 已经登录");
                if (string.Equals(data.equiptype, "编码设备") && needPlay)
                {

                    Loom.Post(() => {

                        EventCenter.BroadCast(Eventdefine.CameraPlayEvent, nvr);
                    });


                }
            }
        }

        public override async Task LogoutAsync()
        {
            if (IsLogin)
            {
                // 登出事件的广播必须先通知到各个监控，在登出前需确保各个监控停止播放
                await base.LogoutAsync();
                await Task.Run(() =>
                    {
                        var state = CHCNetSDK.NET_DVR_Logout_V30((int)loginHandle);
                        if (state)
                        {
                            loginHandle = null;
                            Log.Debug($"{nameof(HikvisonNVR)}: {data.ActiveHost} - {loginHandle} 登出成功");
                            if (LoginhandleDic.ContainsKey(data.Ip))
                            {
                                LoginhandleDic.Remove(data.Ip);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"{nameof(HikvisonNVR)}: {data.ActiveHost} - {loginHandle} 登出失败");
                            if (LoginhandleDic.ContainsKey(data.Ip))
                            {
                                LoginhandleDic.Remove(data.Ip);
                            }
                        }
                    });
            }
        }
    }
}
