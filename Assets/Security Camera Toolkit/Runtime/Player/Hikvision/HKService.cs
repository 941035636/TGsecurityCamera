// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.

using Hikvision;
using System;
using UnityEngine;
using static zFramework.Media.DHPlaySDK;

namespace zFramework.Media
{
    public class HKService : CameraService
    {
        //loginHandle 海康是 int 类型 ，小于 -1 代表未成功登录

        public override bool HasLogin => (int)(loginHandle ?? -1) > -1;
        public override bool IsRealPlaying => realHandle > -1;

        //RealPlay 返回句柄参数，-1代表实时播放失败。
        private int realHandle = -1;
        private CHCNetSDK.REALDATACALLBACK realDataBack;
        public bool CheckOnlineStatus() 
        {
            bool isOnLine = CHCNetSDK.NET_DVR_RemoteControl((int)loginHandle, 20005, IntPtr.Zero, 0);
            return isOnLine;
        }
        public override void PlayReal()
        {
            CHCNetSDK.NET_DVR_PREVIEWINFO previewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            previewInfo.hPlayWnd = IntPtr.Zero;
            previewInfo.lChannel = facade.channel;
            previewInfo.dwStreamType = (uint)facade.steamType;
            previewInfo.dwLinkMode = 0;
            previewInfo.bBlocked = true;
            previewInfo.dwDisplayBufNum = 15;
            realDataBack = new CHCNetSDK.REALDATACALLBACK(DataBackFunc);
            realHandle = CHCNetSDK.NET_DVR_RealPlay_V40((int)loginHandle, ref previewInfo, realDataBack, IntPtr.Zero);
            if (realHandle > -1)
            {
                 Log.Debug($" { facade.channel } 实时预览成功");
                //GameStart.Instance.ShowTip("实时预览成功");
            }
            else
            {
                 Log.Debug($"预览失败 - { CHCNetSDK.NET_DVR_GetLastError()}");
                GameStart.Instance.ShowTip($"预览失败 - { CHCNetSDK.NET_DVR_GetLastError()}");
            }
            //CHCNetSDK.NET_DVR_OpenSound(realHandle);
        }

        private int lPort = -1;
        private DECCBFUN decondCallBack;
        private void DataBackFunc(int lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr pUser)
        {
            //需要解码
            if (lPort <= -1)
            {

                if (!PLAY_GetFreePort(ref lPort))
                {
                    Debug.LogWarning($"分配播放库通道号失败：  {PLAY_GetLastErrorEx()}");
                    return;
                }

                if (!PLAY_SetStreamOpenMode(lPort, IsRealPlaying ? STREAME_REALTIME : STREAME_FILE))
                {
                    Debug.LogWarning($"设置实时流播放模式失败：{PLAY_GetLastErrorEx()}");
                    return;
                }
                if (!PLAY_OpenStream(lPort, IntPtr.Zero, 0, 2 * 1024 * 1024))
                {
                    Debug.LogWarning($"打开码流失败! {PLAY_GetLastErrorEx()}");
                    return;
                }
                decondCallBack = new DECCBFUN(DecodeCallback);
                if (!PLAY_SetDecCallBack(lPort, decondCallBack))
                {
                    Debug.LogWarning($"设置解码回调函数失败! {0}");
                    return;
                }

                if (!PLAY_SetDecCBStream(lPort, 3))
                {
                     Log.Debug($"设置解码格式! {0}");
                    return;
                }

                if (!PLAY_Play(lPort, IntPtr.Zero))
                {
                     Log.Debug($"开始解码失败! { PLAY_GetLastErrorEx()}");
                    return;
                }
                PLAY_SetPlaySpeed(lPort, 1f);
            }
            else if (dwBufSize > 0)
            {
                if (!PLAY_InputData(lPort, pBuffer, dwBufSize))
                {
                    Debug.LogWarning($"{nameof(HKService)}: 播放库数据装载失败 errorcode = {PLAY_GetLastErrorEx()}");
                }
            }
        }
        public HKService(SecurityCamera facade) : base(facade) { }
        private void DecodeCallback(int nPort, IntPtr pBuf, int nSize, ref FRAME_INFO pFrameInfo, IntPtr pUserData, int nReserved2)
        {
            if (IsRealPlaying && !isPause && pFrameInfo.nType == 3)
            {
                // 先访问 VideoRenderer 是否视频帧队列已满，满了就把当前推进来的数据不管
                var blocked = frameBlocked?.Invoke() ?? true;
                if (!blocked)
                {
                    var frame = new I422VideoFrame(pFrameInfo.nWidth, pFrameInfo.nHeight, pBuf);
                    frameReady?.Invoke(frame);
                }
            }
        }

        public override void StopPlay()
        {
            StopDecoding();
            if (realHandle > -1)
            {
                var temp = realHandle; //避免多次访问 SDK 
                realHandle = -1;
                var result = CHCNetSDK.NET_DVR_StopRealPlay(temp);
                 Log.Debug($"{nameof(HKService)}: 停止实时播放{(result ? "成功" : $"失败,Errorcode = {CHCNetSDK.NET_DVR_GetLastError()}")}");
            }
            base.StopPlay();
        }

        protected override void StopDecoding()
        {
            if (lPort > -1)
            {
                var temp = lPort;
                lPort = -1;
                if (!PLAY_Stop(temp))
                {
                     Log.Debug($"停止解码失败， ErrorCode = {PLAY_GetLastErrorEx()}");
                }
                if (!PLAY_CloseStream(temp))
                {
                     Log.Debug($"关闭解码流失败，ErrorCode = {PLAY_GetLastErrorEx()}");
                }
            }
        }
        /// <summary>
        /// 调用云台
        /// </summary>
        public override void CloundBtnLeft_MouseDown() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle,(uint)CHCNetSDK.PAN_LEFT,0,2);


        }
        public override void CloundBtnLeft_MouseUp() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.PAN_LEFT, 1, 2);
        }
        public override void CloundBtnRight_MouseDown() 
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.PAN_RIGHT, 0, 2);
        }
        public override void CloundBtnRight_MouseUp() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.PAN_RIGHT, 1, 2);
        }

        public override void CloundBtnUp_MouseDown() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.TILT_UP, 0, 2);
        }
       
        public override void CloundBtnUp_MouseUp() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.TILT_UP, 1, 2);
        }

        public override void CloundBtnDown_MouseDown() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.TILT_DOWN, 0, 2);
        }
        public override void CloundBtnDown_MouseUp() {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.TILT_DOWN, 1, 2);
        }
        public override void CloundBtnZoomIn_MouseDown() //放大
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.ZOOM_IN, 0, 4);
        }
        public override void CloundBtnZoomIn_MouseUp() 
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.ZOOM_IN, 1, 4);

        }

        public override void ClondBtnZoomOut_MouseDown() //缩小
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.ZOOM_OUT, 0, 4);
        }

        public override void ClondBtnZoomOut_MouseUp() 
        {
            CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, CHCNetSDK.ZOOM_OUT, 1, 4);

        }

        ///// <summary>
        ///// 云台向左开始旋转
        ///// </summary>
        //private void btnLeft_MouseDown()
        //{

        //        CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, (uint)CHCNetSDK.PAN_LEFT, 0, 4);

        //}/// <summary>
        ///// 云台向左停止旋转
        ///// </summary>
        //private void btnLeft_MouseUp()
        //{

        //    CHCNetSDK.NET_DVR_PTZControlWithSpeed(realHandle, (uint)CHCNetSDK.PAN_LEFT, 1, 4);

        //}
    }
}