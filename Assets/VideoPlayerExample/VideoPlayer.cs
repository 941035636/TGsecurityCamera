using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VideoPlayer : MonoBehaviour
{
    /// <summary>
    /// 使用预加载模式**********两个MidiaPlayer切换，使得两个视频切换时不出现黑屏现象，切换更流畅**********************
    /// </summary>
    public MediaPlayer _mediaPlayer;
    public MediaPlayer _mediaPlayerB;
    [Header("视屏显示面板")]
    public DisplayUGUI _mediaDisplay;
    [Header("播放暂停按钮")]
    public Button PlayButton;
    public Button PauseButton;
    [Header("视频时间滑动条")]
    public Slider VideoTimeSlider;
    private float VideoSliderValue;
    //滑动时间轴时使用
    private bool _wasPlayingOnScrub;
    [Header("音量控制Slider")]
    public Slider AudioSlider;
    //当前音量
    private float CurAudioSliderValue;
    [Header("当前时间显示")]
    public Text CurTimeText;
    [Header("总时间显示")]
    public Text TotalTimeText;

    [Header("是否多个声音—暂未用到")]
    public Toggle _MuteToggle;

    /// <summary>
    /// 获取当前正在使用的MideoPlayer
    /// </summary>
    public MediaPlayer PlayingPlayer
    {
        get
        {
            if (LoadingPlayer == _mediaPlayer)
            {
                return _mediaPlayerB;
            }
            return _mediaPlayer;
        }
    }

    private MediaPlayer _loadingPlayer;
    /// <summary>
    /// 两个MidiaPlayer切换，使得两个视频切换时不出现黑屏现象，切换更流畅
    /// </summary>
    public MediaPlayer LoadingPlayer
    {
        get
        {
            return _loadingPlayer;
        }
    }
    //每次播放时都设置一次Loop （根据IsLoop的值）
    bool IsLoop = false;
    private void Awake()
    {
        _loadingPlayer = _mediaPlayerB;
        PlayButton.onClick.AddListener(Play);
        PauseButton.onClick.AddListener(Pause);
        if (AudioSlider)
            AudioSlider.onValueChanged.AddListener(OnAudioVolumeSlider);
        //给VideoTimeSlider添加PointerDown和PointerUp事件
        if (VideoTimeSlider)
        {
            VideoTimeSlider.onValueChanged.AddListener(delegate (float f) { OnVideoSeekSlider(); });

            EventTrigger videosliderTrigger = VideoTimeSlider.GetComponent<EventTrigger>();
            if (!videosliderTrigger)
                videosliderTrigger = VideoTimeSlider.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry downenter = new EventTrigger.Entry();
            downenter.eventID = EventTriggerType.PointerDown;
            downenter.callback.AddListener(OnVideoSliderDown);
            videosliderTrigger.triggers.Add(downenter);
            EventTrigger.Entry upenter = new EventTrigger.Entry();
            upenter.eventID = EventTriggerType.PointerUp;
            upenter.callback.AddListener(OnVideoSliderUp);
            videosliderTrigger.triggers.Add(upenter);
        }
    }



    void Start()
    {
        if (PlayingPlayer)
        {//添加Video准备、开始、第一针、完成等执行事件
            PlayingPlayer.Events.AddListener(OnVideoEvent);

            if (LoadingPlayer)
            {
                LoadingPlayer.Events.AddListener(OnVideoEvent);
            }

            if (AudioSlider)
            {
                // Volume
                if (PlayingPlayer.Control != null)
                {
                    float volume = PlayingPlayer.Control.GetVolume();
                    CurAudioSliderValue = volume;
                    AudioSlider.value = volume;
                }
            }
        }
    }
    /// <summary>
    /// 打开视频文件
    /// </summary>
    /// <param name="VideoPath路径"></param>
    /// <param name="IsAutoStart自动开启"></param>
    /// <param name="isUrl是网络路径false为StreamAssets下路径"></param>
    public void OnOpenVideoFile(string VideoPath, bool IsAutoStart = true, bool isUrl = false)
    {
        //设置路径
        LoadingPlayer.m_VideoPath = VideoPath;
        if (string.IsNullOrEmpty(LoadingPlayer.m_VideoPath))
        {
            LoadingPlayer.CloseVideo();
        }
        else
        {
            //设置模式
            MediaPlayer.FileLocation _location = MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder;
            if (isUrl)
                _location = MediaPlayer.FileLocation.AbsolutePathOrURL;
            //播放视屏（每播放一次，在FirstFrameReady事件时切换到下一个MediaPlayer）
            LoadingPlayer.OpenVideoFromFile(_location, LoadingPlayer.m_VideoPath, IsAutoStart);
            LoadingPlayer.m_Loop = IsLoop;
            SetPlayBtnState(_mediaPlayer.m_AutoStart);
        }
    }
    /// <summary>
    /// 设置Loop
    /// </summary>
    /// <param name="IsLoop"></param>
    public void SetLoop(bool IsLoop)
    {
        this.IsLoop = IsLoop;
        PlayingPlayer.m_Loop = IsLoop;
    }
    void Update()
    {
        if (PlayingPlayer && PlayingPlayer.Info != null && PlayingPlayer.Info.GetDurationMs() > 0f)
        {
            float time = PlayingPlayer.Control.GetCurrentTimeMs();
            float duration = PlayingPlayer.Info.GetDurationMs();
            float d = Mathf.Clamp(time / duration, 0.0f, 1.0f);
            CurTimeText.text = (time / 1000f).ToString("0.00");
            TotalTimeText.text = (duration / 1000f).ToString("0.00");
            //  Log.Debug(string.Format("time: {0}, duration: {1}, d: {2}", time, duration, d));

            VideoSliderValue = d;
            VideoTimeSlider.value = d;
        }
    }

    /// <summary>
    /// 每播放一次，在FirstFrameReady事件时切换到下一个MediaPlayer 
    /// 播放第一针后将当前播放视屏显示在面板上
    /// </summary>
    private void SwapPlayers()
    {
        // Pause the previously playing video
        PlayingPlayer.Control.Pause();

        // Swap the videos
        if (LoadingPlayer == _mediaPlayer)
        {
            _loadingPlayer = _mediaPlayerB;
        }
        else
        {
            _loadingPlayer = _mediaPlayer;
        }

        // Change the displaying video
        _mediaDisplay.CurrentMediaPlayer = PlayingPlayer;
    }

    /// <summary>
    /// 设置多个语音模式
    /// </summary>
    public void OnMuteChange()
    {
        if (PlayingPlayer)
        {
            PlayingPlayer.Control.MuteAudio(_MuteToggle.isOn);
        }
        if (LoadingPlayer)
        {
            LoadingPlayer.Control.MuteAudio(_MuteToggle.isOn);
        }
    }
    //播放并切换按钮在状态
    public void Play()
    {
        if (PlayingPlayer)
        {
            PlayingPlayer.Control.Play();
            SetPlayBtnState(true);
        }
    }
    //暂停并切换按钮在状态
    public void Pause()
    {
        if (PlayingPlayer)
        {
            PlayingPlayer.Control.Pause();
            SetPlayBtnState(false);
        }
    }
    //设置 “播放和暂停按钮”激活隐藏
    private void SetPlayBtnState(bool IsPlay)
    {
        PauseButton.gameObject.SetActive(IsPlay);
        PlayButton.gameObject.SetActive(!IsPlay);
    }

    #region *****************VideoSlider功能*******************
    public void OnVideoSliderDown(BaseEventData arg0)
    {
        if (PlayingPlayer)
        {
            _wasPlayingOnScrub = PlayingPlayer.Control.IsPlaying();
            if (_wasPlayingOnScrub)
            {
                Pause();
            }
            OnVideoSeekSlider();
        }
    }
    public void OnVideoSeekSlider()
    {
        if (PlayingPlayer && VideoTimeSlider && VideoTimeSlider.value != VideoSliderValue)
        {
            PlayingPlayer.Control.Seek(VideoTimeSlider.value * PlayingPlayer.Info.GetDurationMs());
        }
    }
    public void OnVideoSliderUp(BaseEventData arg0)
    {
        if (_wasPlayingOnScrub)
        {
            Play();
            _wasPlayingOnScrub = false;

            //SetButtonEnabled("PlayButton", false);
            //SetButtonEnabled("PauseButton", true);
        }
    }
    #endregion

    /// <summary>
    /// 音量改变时
    /// </summary>
    /// <param name="value"></param>
    public void OnAudioVolumeSlider(float value)
    {
        if (PlayingPlayer && value != CurAudioSliderValue)
        {
            PlayingPlayer.Control.SetVolume(value);
        }
        if (LoadingPlayer && value != CurAudioSliderValue)
        {
            LoadingPlayer.Control.SetVolume(value);
        }
    }

    /// <summary>
    /// 从头开始播放
    /// </summary>
    public void RePlay()
    {
        if (PlayingPlayer)
        {
            PlayingPlayer.Control.Rewind();
        }
    }

    // Callback function to handle events
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch (et)
        {
            case MediaPlayerEvent.EventType.ReadyToPlay:
                break;
            case MediaPlayerEvent.EventType.Started:
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                SwapPlayers();
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying:
                break;
        }

         Log.Debug("Event: " + et.ToString());
    }

}
