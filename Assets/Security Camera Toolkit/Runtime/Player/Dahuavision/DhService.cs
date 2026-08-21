using System;
using UnityEngine;
using Dahuavision;
using static zFramework.Media.DHPlaySDK;
namespace zFramework.Media 
{
    public class DhService : CameraService
    {  //RealPlay 返回句柄参数，0代表实时播放失败。
        private IntPtr realHandle = IntPtr.Zero;
        //public override bool HasLogin => (int)(IntPtr)(loginHandle ?? 0) > 0; //
        public override bool HasLogin => (loginHandle?? null)!=null;
        

        public override bool IsRealPlaying => realHandle !=IntPtr.Zero;
        //private Dahuavision.callback realDataBack;
        private fRealDataCallBackEx2 realDataBack;
        private fRealPlayDisConnectCallBack fRealPlayDisBack;
        public override void PlayReal()
        {
            realDataBack = new fRealDataCallBackEx2(dataBackFunc);
            fRealPlayDisBack = new fRealPlayDisConnectCallBack(disconnectbackFun);
            realHandle = NETClient.RealPlay((IntPtr)loginHandle, facade.channel,IntPtr.Zero,EM_RealPlayType.Realplay);
            if ((int)realHandle>0)
            {
                 Log.Debug($" { facade.channel } 实时预览成功");
                //GameStart.Instance.ShowTip($" { facade.channel } 实时预览成功");

                NETClient.SetRealDataCallBack(realHandle, realDataBack, IntPtr.Zero,EM_REALDATA_FLAG.DATA_WITH_FRAME_INFO|EM_REALDATA_FLAG.PCM_AUDIO_DATA
                    |EM_REALDATA_FLAG.RAW_DATA|EM_REALDATA_FLAG.YUV_DATA);
            }
            else
            {
                 Log.Debug($"预览失败 - { NETClient.GetLastError()}");
                GameStart.Instance.ShowTip($"预览失败 - { NETClient.GetLastError()}"+"句柄:"+ realHandle);
            }
       
        }
        private int lPort = -1;
        private DECCBFUN decondCallBack;
        public DhService(SecurityCamera facade) : base(facade) { }
        private void dataBackFunc(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr param, IntPtr dwUser) 
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
            if ((int)realHandle > 0)
            {
                var temp = realHandle; //避免多次访问 SDK 
                realHandle = IntPtr.Zero;
                var result = NETClient.StopRealPlay(temp);
                 Log.Debug($"{nameof(DhService)}: 停止实时播放{(result ? "成功" : $"失败,Errorcode = {NETClient.GetLastError()}")}");
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
        private void disconnectbackFun(IntPtr lRealHandle, EM_REALPLAY_DISCONNECT_EVENT_TYPE dwEventType, IntPtr param, IntPtr dwUser) 
        {
        
        }

    }
}

