using UnityEngine;
using Dahuavision;
using System;
using System.Threading.Tasks;
using zFramework.Media.Internal;

namespace zFramework.Media
{
    public class DahuavisonNVR : NVR
    {
        public override bool IsLogin => (int)(loginHandle ?? 0) > 0;

        public DahuavisonNVR(NVRInformation data) : base(data) { }

        public override bool CleanUp()
        {
            NETClient.Cleanup();
            return true;
        }

        public override bool InitSDK()
        {
          
            var state = NETClient.Init(DisConnectCallBack, IntPtr.Zero, null);
            if (!state)
            {
                Log.Debug($"{nameof(DahuavisonNVR)}: SDK 初始化失败");
            }
            return state;
        }
        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {

        }
        /// <summary>
        /// 异步登录的API
        /// </summary>
        /// <returns></returns>
        public override async Task LoginAsync(NVRInformation nvrinfo,bool needPlay) 
        {
            if (!IsLogin)
            {
                var result = await Task.Run(() =>
                {

                    NET_DEVICEINFO_Ex DeviceInfo = new NET_DEVICEINFO_Ex();
                    loginHandle = NETClient.Login(data.Ip, ushort.Parse(data.Port.ToString()), data.userName, data.password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref DeviceInfo);
                 
                    var Result = (IntPtr)loginHandle != IntPtr.Zero;
                    if (Result)
                    {
                       
                        // 向挂载的监控发送 登录状态 , 注意，此动作在非Unity主线程中进行
                         Log.Debug($"{data.type} - {data.ActiveHost}  登录成功：{loginHandle}");
                    }
                    else
                    {
                        Debug.LogWarning($"{data.type} - {data.ActiveHost}  登录失败,ErrorCode = {NETClient.GetLastError()}");
                    }
                    return Result;
                });
                if (result)
                {
                    await base.LoginAsync(nvrinfo,needPlay);
                     Log.Debug($"{nameof(DahuavisonNVR)}: 所有 SecurityCamera 同步 NVR 登录状态成功");
                    if (string.Equals(data.equiptype, "编码设备")&& needPlay)
                    {

                        Loom.Post(() => {

                            EventCenter.BroadCast(Eventdefine.CameraPlayEvent, data);
                        });


                    }
                }
            }
            else
            {
                 Log.Debug($"{data.type} - {data.ActiveHost} SDK 已经登录");
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
                    var state = NETClient.Logout((IntPtr)loginHandle);
                    if (state)
                    {
                        loginHandle = null;
                         Log.Debug($"{nameof(DahuavisonNVR)}: {data.ActiveHost} - {loginHandle} 登出成功");
                    }
                    else
                    {
                        Debug.LogWarning($"{nameof(DahuavisonNVR)}: {data.ActiveHost} - {loginHandle} 登出失败");
                    }
                });
            }
        }

    }
}

