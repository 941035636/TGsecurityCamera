using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UMP;
using UnityEngine.UI;
using UnityTimer;
using DG.Tweening;
using RenderHeads.Media.AVProVideo;
using UMP;
using UnityEngine.Experimental.PlayerLoop;
using System.Security.Policy;
using System;

public class VideoPlayBack : SingletonManager<VideoPlayBack>
{
    /*
    public bool isLoad = false;
    public static bool isCounting = true;
    public static bool isPlaying = false;
    public Animator loadingani;
    UniversalMediaPlayer umpPlayer;
    private void OnDestroy()
    {
        //Timer.Pause(timer);
        //loadingani.gameObject.SetActive(false);
    }


    private void Start()
    {


        umpPlayer = transform.GetComponent<UniversalMediaPlayer>();
        //注册视频播放触发事件
        if (ReplayUIController.Ins.IsUseAvProReplay)
            this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Events.AddListener(MediaEventHandler);
        //添加ump事件
        AddUmpEvent();

    }

    public void AddVideo(string rtsp)
    {
        transform.GetComponent<RawImage>().enabled = true;
        //transform.GetComponent<UniversalMediaPlayer>().Path = rtsp;
        //if (isLoad == false)
        //{
        //    transform.GetComponent<UniversalMediaPlayer>().Play();

        //}
        transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath = rtsp;
        if (isLoad == false)
        {
            transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Play();
        }
        isLoad = true;
    }
    public void PauseVideo()
    {
        //transform.GetComponent<UniversalMediaPlayer>().Pause();
        //isCounting = false;
        transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Pause();
        isCounting = false;

    }

    public void Pause()
    {


        if (ReplayUIController.Ins.IsUseAvProReplay)
            transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Pause();
        else
        {
            //transform.GetComponent<UniversalMediaPlayer>().OnPlayerPaused();
            umpPlayer.Pause();
        }


        if (timer != null)
            Timer.Pause(timer);
        //isCounting = false;
    }

    public void Play()
    {

        //if (ReplayUIController.Ins.IsUseAvProReplay)
        //    transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Play();
        //else

        umpPlayer.Play();
        if (timer != null)
            Timer.Resume(timer);
    }

    public void LoadingReplay()
    {
        loadingani.gameObject.SetActive(true);
        //ReplayUIController.Ins._playVideoTog.interactable = false;
        loadingani.Play("loadani");
        isPlaying = false;


    }


    public void PlayVideoByAvpro(string rtsp)
    {

        //if (timer != null)
        //    Timer.Pause(timer);

        if (TimeSlider.Isdrag)
        {
            TimeSlider.Ins.timeSlider.value = int.Parse(TimeSlider.Ins.timeText.text.Split(':')[0].Trim()) * 3600 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[1].Trim()) * 60 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[2].Trim());

        }
        else if (ReplayUIController.IsnewPlay)
        {
            //this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.SetPlaybackRate(1);
            TimeSlider.Ins.timeSlider.value = int.Parse(TimeSlider.Ins.timeText.text.Split(':')[0].Trim()) * 3600 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[1].Trim()) * 60 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[2].Trim());
        }

        StartCoroutine(Playvideo(rtsp));




    }



    private void AddUmpEvent()
    {

        umpPlayer.AddPlayingEvent(() =>
        {

            loadingani.gameObject.SetActive(false);
            ReplayUIController.Ins._playVideoTog.interactable = true;
            transform.GetComponent<RawImage>().color = new Color(255, 255, 255, 255);
            isPlaying = true;
            ReplayUIController.IsnewPlay = false;
            //Timer.Register(1f, () =>
            //{
            //    if (transform.GetComponent<UniversalMediaPlayer>().Path != null)
            //    {
            //        transform.GetComponent<UniversalMediaPlayer>().Play();
            //        Log.Debug("点了播放");
            //        //isCounting = true;

            //        //IsreadyPlay = false;



            //    }

            //});
            if (timer != null)
                Timer.Resume(timer);

            if (TimeSlider.Isdrag)
            {
                TimeSlider.Ins.timeSlider.value = int.Parse(TimeSlider.Ins.timeText.text.Split(':')[0].Trim()) * 3600 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[1].Trim()) * 60 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[2].Trim());

            }
            else if (ReplayUIController.IsnewPlay)
            {

                TimeSlider.Ins.timeSlider.value = int.Parse(TimeSlider.Ins.timeText.text.Split(':')[0].Trim()) * 3600 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[1].Trim()) * 60 + int.Parse(TimeSlider.Ins.timeText.text.Split(':')[2].Trim());
            }


        });
        umpPlayer.AddStoppedEvent(() =>
        {
            if (timer != null)
                Timer.Pause(timer);


        });
        umpPlayer.AddEncounteredErrorEvent(() =>
        {

            Log.Error("Error");

            Timer.Register(1f, () =>
            {
                if (transform.GetComponent<UniversalMediaPlayer>().Path != null)
                {
                    umpPlayer.Play();




                }

            });

        });


    }

    public void PlayVideoByUmp(string url)
    {
        loadingani.gameObject.SetActive(true);
        ReplayUIController.Ins._playVideoTog.interactable = false;
        ReplayUIController.Ins._playVideoTog.isOn = true;
        loadingani.Play("loadani");
        isPlaying = false;

        switch (ReplayUIController.Ins.rate)
        {
            case 1:
                CountSeconds();
                break;
            case 2:
                CountSeconds2();
                break;
            case 3:
                CountSeconds3();
                break;
            case 4:
                CountSeconds4();
                break;
            default:
                break;
        }


        if (timer != null)
            Timer.Pause(timer);
        StartCoroutine(PlayvideoByUmp(url));




    }



    // 视频播放时间触发
    private void MediaEventHandler(RenderHeads.Media.AVProVideo.MediaPlayer arg0, MediaPlayerEvent.EventType arg1, ErrorCode arg2)
    {
        switch (arg1)
        {
            case MediaPlayerEvent.EventType.Closing:
                Debug.Log("关闭播放器触发");
                //this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, playUrl, false);
                //this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Play();
                break;
            case MediaPlayerEvent.EventType.Error:
                Debug.Log("报错误时触发");
                break;
            case MediaPlayerEvent.EventType.FinishedPlaying://注意：如果视频设置为循环播放模式，则不触发此项
                Debug.Log("播放完成触发");
                break;
            case MediaPlayerEvent.EventType.FirstFrameReady:
                Debug.Log("准备完触发");
                this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Play();
                break;
            case MediaPlayerEvent.EventType.MetaDataReady:
                Debug.Log("媒体数据准备准备中触发");
                break;

            case MediaPlayerEvent.EventType.ReadyToPlay:
                this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Play();
                Debug.Log("准备去播放触发");
                break;
            case MediaPlayerEvent.EventType.Started://注意：每暂停之后的开始播放都会触发一次
                Debug.Log("开始播放触发");

                loadingani.gameObject.SetActive(false);
                ReplayUIController.Ins._playVideoTog.interactable = true;
                //transform.GetComponent<RawImage>().color = new Color(255, 255, 255, 255);
                isPlaying = true;
                ReplayUIController.IsnewPlay = false;
                if (timer != null)
                    Timer.Resume(timer);
                switch (ReplayUIController.Ins.rate)
                {
                    case 1:
                        CountSeconds();
                        break;
                    case 2:
                        CountSeconds2();
                        break;
                    case 3:
                        CountSeconds3();
                        break;
                    case 4:
                        CountSeconds4();
                        break;
                    default:
                        break;
                }
                break;

            default:
                break;
        }
    }

    string playUrl = string.Empty;
    IEnumerator Playvideo(string url)
    {
        playUrl = url;
        yield return new WaitForSeconds(6f);

        this.transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().OpenVideoFromFile(RenderHeads.Media.AVProVideo.MediaPlayer.FileLocation.AbsolutePathOrURL, url, false);





    }



    IEnumerator PlayvideoByUmp(string url)
    {

        yield return new WaitForSeconds(1);
        if (transform.GetComponent<UniversalMediaPlayer>().IsPlaying)
            umpPlayer.Stop();
        umpPlayer.Path = url;
        umpPlayer.Play();


    }

    public void FastPlay()
    {

        if (ReplayUIController.Ins.IsUseAvProReplay)
        {
            if (ReplayUIController.Ins.rate < 4)
            {
                ReplayUIController.Ins.rate = ++transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_PlaybackRate;

                transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.SetPlaybackRate(ReplayUIController.Ins.rate);
                Log.Debug("播放速率：" + transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.GetPlaybackRate());
                //transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Pause();
                //transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Play();
            }



        }
        else
        {
            if (ReplayUIController.Ins.rate < 8)
            {
                ReplayUIController.Ins.rate = ++transform.GetComponent<UniversalMediaPlayer>().PlayRate;
                //transform.GetComponent<UniversalMediaPlayer>().Play();
            }



        }


        switch (ReplayUIController.Ins.rate)
        {
            case 1:
                CountSeconds();
                break;
            case 2:
                CountSeconds2();
                break;
            case 3:
                CountSeconds3();
                break;
            case 4:
                CountSeconds4();
                break;
            default:
                break;
        }


    }
    public void SlowPlay()
    {

        if (ReplayUIController.Ins.IsUseAvProReplay)
        {
            if (ReplayUIController.Ins.rate > 1)
            {
                ReplayUIController.Ins.rate = --transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_PlaybackRate;
                transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.SetPlaybackRate(ReplayUIController.Ins.rate);
                //transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Pause();
                //transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Control.Play();
            }

        }
        else
        {
            if (ReplayUIController.Ins.rate > 1)
            {
                ReplayUIController.Ins.rate = --transform.GetComponent<UniversalMediaPlayer>().PlayRate;
                //transform.GetComponent<UniversalMediaPlayer>().Play();


            }

        }

        switch (ReplayUIController.Ins.rate)
        {
            case 1:
                CountSeconds();
                break;
            case 2:
                CountSeconds2();
                break;
            case 3:
                CountSeconds3();
                break;
            case 4:
                CountSeconds4();
                break;
            default:
                break;
        }

    }


    public void TimeChange() { }
    Timer timer;
    void CountSeconds()
    {
        if (timer != null)
            timer.Pause();
        timer = Timer.Register(1f, TimeJia, isLooped: isCounting);
    }
    void CountSeconds2()
    {
        if (timer != null)
            timer.Pause();
        timer = Timer.Register(0.5f, TimeJia, isLooped: isCounting);
    }
    void CountSeconds3()
    {
        if (timer != null)
            timer.Pause();
        timer = Timer.Register(0.333f, TimeJia, isLooped: isCounting);
    }
    void CountSeconds4()
    {
        if (timer != null)
            timer.Pause();
        timer = Timer.Register(0.25f, TimeJia, isLooped: isCounting);
    }
    void TimeJia()
    {

        TimeSlider.Ins.timeSlider.value++;

    }
    public void StopVideo()
    {
        transform.GetComponent<RawImage>().enabled = false;
        //transform.GetComponent<UniversalMediaPlayer>().Stop();
        transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().Stop();
        transform.GetComponent<RenderHeads.Media.AVProVideo.MediaPlayer>().m_VideoPath = null;
        //transform.GetComponent<UniversalMediaPlayer>().Path = null;
        transform.GetComponent<RawImage>().enabled = false;
        //isCounting = false;
        isLoad = false;
    }

    */
}
