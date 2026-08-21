// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.
namespace zFramework.Media
{
    public class CameraService : IVideoSource
    {
        public SecurityCamera facade;
        public object loginHandle; //登录句柄，数据类型：int 、c#指针 
        public virtual bool HasLogin { get; }
        public virtual bool IsRealPlaying { get; }
        public bool Enabled => IsRealPlaying;

        //暂停
        public bool isPause = false;
        public bool isplay = false;
        object eventlocka = new object();
        object eventlockb = new object();
        protected bool isVideoRendererReady = false;
        protected bool isFrameBlockedSignalReady = false;
        /// <inheritdoc/>
        public event I422AVideoFrameDelegate OnVideoFrameReady
        {
            add
            {
                lock (eventlocka)
                {
                    frameReady += value;
                    isVideoRendererReady = true;
                }
            }
            remove
            {
                lock (eventlocka)
                {
                    frameReady -= value;
                    isVideoRendererReady = frameReady != null;
                }
            }
        }
        protected I422AVideoFrameDelegate frameReady;
        /// <inheritdoc/>
        public event ProcessInterruptSignal OnInterruptedSignal
        {
            add
            {
                lock (eventlockb)
                {
                    frameBlocked += value;
                    isFrameBlockedSignalReady = true;
                }
            }
            remove
            {
                lock (eventlockb)
                {
                    frameBlocked -= value;
                    isFrameBlockedSignalReady = frameBlocked != null;
                }
            }
        }
        protected ProcessInterruptSignal frameBlocked;

        public CameraService() { }
        public CameraService(SecurityCamera camera) => facade = camera;

        public void SetLoginHandle(object handle) => loginHandle = handle;
        protected virtual void StopDecoding() { }

        /// <summary>
        /// 实时播放
        /// </summary>
        public virtual void PlayReal() { isplay = true; }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public virtual void Pause() => isPause = true;

        /// <summary>
        /// 恢复播放
        /// </summary>
        public virtual void Resume() => isPause = false;

        /// <summary>
        /// 结束播放
        /// </summary>
        public virtual void StopPlay() { isPause = false; isplay = false; }


        public virtual void CloundBtnLeft_MouseDown() { }
        public virtual void CloundBtnLeft_MouseUp() { }
        public virtual void CloundBtnRight_MouseDown() { }
        public virtual void CloundBtnRight_MouseUp() { }

        public virtual void CloundBtnUp_MouseDown() { }
        public virtual void CloundBtnUp_MouseUp() { }

        public virtual void CloundBtnDown_MouseDown() { }
        public virtual void CloundBtnDown_MouseUp() { }
        public virtual void CloundBtnZoomIn_MouseDown() { }
        public virtual void CloundBtnZoomIn_MouseUp() { }
        public virtual void ClondBtnZoomOut_MouseDown() { }
        public virtual void ClondBtnZoomOut_MouseUp() { }
    }
    /// <summary>
    /// Delegate used for events when an I422-encoded video frame has been produced
    /// and is ready for consumption.
    /// </summary>
    /// <param name="frame">The newly available I422-encoded video frame.</param>
    public delegate void I422AVideoFrameDelegate(I422VideoFrame frame);
    public delegate bool ProcessInterruptSignal();
}
