// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.
namespace zFramework.Media
{
    using UnityEngine;
    using static NVRManager;
    public class SecurityCamera : MonoBehaviour, INVRStateHandler
    {
        [Header("NVR 主机"), StringPopup(typeof(NVRConfiguration), "GetNVRHosts", "NVR 未配置")]
        public string host;
        public SDKTYPE sdk;
        // 指向 NVR 中的通道，海康不接 NVR 也行 channel 默认为 1 
        // 注意监控的 通道取值范围会因各个厂商而异
        [Header("NVR 通道:")]
        public int channel;
        [Header("主/辅 流:")]
        public STREAM steamType = STREAM.MAIN;
        public string passward;
        public VideoRenderer monitor;
        public CameraService player = null;
        private bool playRequested;
        private bool pausedByVisibility;
        public string Host { get => host; }
        public bool IsLogin { get => null != player && player.HasLogin; }

        public int devid = 0;
        //private void Start() => SetupPlayer();

        public void SetupPlayer()
        {
            if (player != null)
            {
                Stop();
                DisconnectNVR(this);
            }
            player = CreateCamera(sdk, this);
            if (player != null)
            {
                ConnectNVR(this);
            }
        }

        //实时
        public void PlayReal()
        {
            if (player == null)
            {
                Log.Debug("播放操作失败：播放器初始化失败");
                return;
            }
            if (!player.HasLogin)
            {
                playRequested = true;
                Log.Debug("NVR 尚未登录，登录完成后自动开始播放");
                return;
            }
            playRequested = false;
            if (!player.IsRealPlaying)
            {
                monitor?.StartRendering(player);
                player.PlayReal();
            }
            else
            {
                Log.Debug("播放操作忽略：已经在播放中");
                //GameStart.Instance.ShowTip($"播放操作失败：{(player.HasLogin ? "已经在播放中" : "还没有登录")}");
            }
        }
  
        //暂停
        public void Pause()
        {
            if (player != null && !player.isPause)
            {
                player.Pause();
                monitor?.PauseRendering();
            }
        }
        //停止
        public void Stop()
        {
            playRequested = false;
            if (player != null)
            {
                monitor?.StopRendering();
                player.StopPlay();
            }
        }
        //恢复
        public void Resume()
        {
            if (player != null && player.isPause)
            {
                player.Resume();
                monitor?.ResumeRendering();
            }
        }

        private void OnDisable()
        {
            if (player != null && player.IsRealPlaying && !player.isPause)
            {
                pausedByVisibility = true;
                Pause();
            }
        }

        private void OnEnable()
        {
            if (pausedByVisibility)
            {
                pausedByVisibility = false;
                Resume();
            }
        }

        public  void CloundBtnLeft_MouseDown() {
            if (player != null )
            {
                player.CloundBtnLeft_MouseDown();
              
            }
        }
        public  void CloundBtnLeft_MouseUp() {
            if (player != null)
            {
                player.CloundBtnLeft_MouseUp();

            }
        }
        public  void CloundBtnRight_MouseDown() {
            if (player != null)
            {
                player.CloundBtnRight_MouseDown();

            }
        }
        public  void CloundBtnRight_MouseUp() {
            if (player != null)
            {
                player.CloundBtnRight_MouseUp();

            }
        }

        public  void CloundBtnUp_MouseDown() {
            if (player != null)
            {
              
                player.CloundBtnUp_MouseDown();
                Log.Debug("云台向上转");
            }
        }
        public  void CloundBtnUp_MouseUp() {
            if (player != null)
            {
                player.CloundBtnUp_MouseUp();
                Log.Debug("云台停止向上转");
            }
        }

        public  void CloundBtnDown_MouseDown() {
            if (player != null)
            {
                player.CloundBtnDown_MouseDown();

            }
        }
        public  void CloundBtnDown_MouseUp() {
            if (player != null)
            {
                player.CloundBtnDown_MouseUp();

            }
        }

        public void CloundBtnZoomIn_MouseUp()
        {
            if (player != null)
            {
                player.CloundBtnZoomIn_MouseUp();
            }
        }

        public void CloundBtnZoomIn_MouseDown()
        {
            if (player != null)
            {
                player.CloundBtnZoomIn_MouseDown();

            }
        }

        public void CloundBtnZoomOut_MouseDown()
        {
            if (player != null)
            {
                player.ClondBtnZoomOut_MouseDown();

            }
        }

        public void CloundBtnZoomOut_MouseUp()
        {
            if (player != null)
            {
                player.ClondBtnZoomOut_MouseUp();

            }
        }

        private void OnDestroy()
        {
            // 默认脚本执行顺序下，Security Camera 有几率退出比 NVRManager 要早，所以先 try 为敬
            // 实际开发中，记得在推出前需要主动销毁监控
            try
            {
                
                Stop();
                DisconnectNVR(this);
            }
            catch (System.Exception e)
            {
                 Log.Debug($"{nameof(SecurityCamera)}: {e.ToString()}");
            }
        }

        public void OnExitCamera() 
        {
            try
            {
                pausedByVisibility = false;
                Stop();
                DisconnectNVR(this);
            }
            catch (System.Exception e)
            {
                 Log.Debug($"{nameof(SecurityCamera)}: {e.ToString()}");
            }

        } 
        //组件校验
        private void OnValidate()
        {
            if (!monitor)
            {
                monitor = GetComponentInChildren<VideoRenderer>();
            }
            if (!monitor)
            {
                Debug.LogWarning($"{nameof(SecurityCamera)}: 请挂载 VideoRenderer ！");
            }
        }

        #region NVR State Callbacks
        public void OnLogin(object loginHandle)
        {
            player?.SetLoginHandle(loginHandle);
            if (loginHandle != null && playRequested)
            {
                PlayReal();
            }
        }

        public void OnLogout()
        {
            playRequested = false;
            monitor?.StopRendering();
            player?.StopPlay();
            player?.SetLoginHandle(null);
        }
        #endregion
    }
}
