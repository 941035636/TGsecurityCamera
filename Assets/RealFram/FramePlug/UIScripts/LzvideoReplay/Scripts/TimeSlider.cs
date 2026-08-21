using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using static ReplayUIController;
using System.Threading;
using UnityTimer;
using LitJson;
using System.Text;
using UnityEngine.Networking;
using System.Collections;

public class TimeSlider : SingletonManager<TimeSlider>, IEndDragHandler
{
    public Slider timeSlider;

    public TMP_Text timeText;
    public Transform Videoplayer;
    public static string SliderTime = string.Empty;
    // Start is called before the first frame update
    void Start()
    {

        timeSlider.minValue = 0;
        timeSlider.maxValue = 86400;
        timeSlider.value = 43200;

        timeSlider.onValueChanged.AddListener((value) =>
        {
            //if(Input.GetMouseButtonUp(0))
            Changed(value);
        });
        SliderTime = timeText.text.Replace(":", "");
    }
    public void Changed(float value)
    {

        int hours = Mathf.FloorToInt(value / 3600f);
        int minutes = Mathf.FloorToInt((value - hours * 3600f) / 60f); // 将剩余秒数转换为分钟数
        int seconds = Mathf.FloorToInt(value - hours * 3600f - minutes * 60f); // 将剩余秒数转换为秒数
        string time = string.Format("{0:00}{1:00}{2:00}", hours, minutes, seconds);
        ReplayUIController.Ins._videoArea.transform.GetChild(0).GetComponent<VideoInfo>().RequestHourMinSec = time;
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds); // 将小时数、分钟数、秒数组合成时间格式字符串


    }
    // Update is called once per frame
    void Update()
    {

    }
    public static bool Isdrag = false;
    string SendstrQuikPlay = string.Empty;
    public void OnEndDrag(PointerEventData eventData)
    {
        //先关闭
     

        Isdrag = true;
        IsnewPlay = false;
        //ReplayUIController.Ins.RateTxt.text = "1X";
        print("提交快进时间:" + timeText.text);
        //Videoplayer.GetComponent<VideoPlayBack>().Pause();
        SendPlayback playback = new SendPlayback();
        playback.camId = UIManger.ChoiceCameraId;
        playback.userId = PlayerPrefs.GetInt("userId");
        //playback.camId = 212;
        //playback.userId = 123;
        playback.startTime = ReplayUIController.startDate.Trim() + timeText.text.Replace(":", "");
        playback.endTime = ReplayUIController.startDate.Trim() + "235959";
        playback.oldUrl = ReplayUIController.Ins.OldUrl;
        SendstrQuikPlay = JsonUtility.ToJson(playback);


        //退出的时候想服务器发送关闭该userid下的camid设备
        //ExitReplay exitReplay = new ExitReplay(long.Parse(PlayerPrefs.GetInt("userId").ToString()), UIManger.ChoiceCameraId);
        ////ExitReplay exitReplay = new ExitReplay(123, 212);
        //string str = JsonUtility.ToJson(exitReplay);
        //string Url = "http://" + GameStart.IP + "/api/video/playback/del/path";
        ////HttpNetManager.GetInstance().SendDataStr(Url, ExitReplayCallback, true, true, false, str);
        //Log.Debug("平台退出向服务器发送关闭录像:" + str + "   Url:" + Url);
        //Log.Debug("向服务器发送的Json:" + SendstrQuikPlay);
        ////SliderTime = timeText.text.Replace(":", "");
        //StartCoroutine(PostData(str,Url));
        EventCenter.BroadCast(Eventdefine.SendPlayback, SendstrQuikPlay);
    }

    void ExitReplayCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("收到退出回放服务器返回消息:" + args.Value);
            EventCenter.BroadCast(Eventdefine.SendPlayback, SendstrQuikPlay);
        }
    }
    public IEnumerator PostData(string jsondata,string url)
    {
  
        byte[] databyte = Encoding.UTF8.GetBytes(jsondata);
        UnityWebRequest _request = new UnityWebRequest(AppRuntimeConfig.NormalizeApiUrl(url), UnityWebRequest.kHttpVerbPOST);
        _request.uploadHandler = new UploadHandlerRaw(databyte);
        _request.downloadHandler = new DownloadHandlerBuffer();

        _request.SetRequestHeader("Content-Type", "application/json");
        HttpNetManager.PrepareRequest(_request);
        yield return _request.SendWebRequest();

        if (_request.error!=null)
        {
            Debug.LogError(_request.error);
        }
        else
        {

        }
        _request.Dispose();
    }


    

    [Serializable]
    public class PlaybackUrl
    {

        public string statusDesc = string.Empty;
        public string rstpAddr = string.Empty;
    }

}
