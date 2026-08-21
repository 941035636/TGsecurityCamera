using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static ReplayUIController;

public class DateBtn : SingletonManager<DateBtn>
{
    private Toggle[] toggles;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void BindToggle()
    {
        //找到所有的toggles
        Log.Error("生成的天数:" + transform.GetComponentsInChildren<Toggle>().Length);
        toggles = transform.GetComponentsInChildren<Toggle>();
        Log.Error("生成的天数：" + toggles.Length);

        //给toggle添加事件
        for (int i = 0; i < toggles.Length; i++)
        {
            //这一步是必须记录的，用来区分那个toggle
            int K = i;
            // toggles[K].onValueChanged.AddListener((bool value) => SetEveryToggle(value, K));
            toggles[K].onValueChanged.AddListener((ison) => { ToggleDebug(K, ison); });
        }
    }
    public void ToggleDebug(int index, bool value)
    {
        Log.Error("index:" + index);
        if (value)
        {
            ReplayUIController.IsnewPlay = true;
            ReplayUIController.Ins._videoArea.transform.GetChild(0).GetComponent<VideoInfo>().RequesYear = transform.GetChild(index).transform.GetChild(1).name;
            ReplayUIController.Ins._videoArea.transform.GetChild(0).GetComponent<VideoInfo>().RequestMonthDay = transform.GetChild(index).name;
            transform.GetChild(index).GetComponent<Toggle>().interactable = false;
            Log.Debug("开启" + index + "年:" + transform.GetChild(index).transform.GetChild(1).name + "月:" + transform.GetChild(index).name);
            UnityTimer.Timer.Register(0.5f, () =>
            {
                if (UIManger.ChoiceCameraId != 0 && TimeSlider.SliderTime != "000000")//&&VideoPlayBack.isPlaying
                {
                    SendPlayback playback = new SendPlayback();
                    playback.camId = UIManger.ChoiceCameraId;
                    playback.userId = PlayerPrefs.GetInt("userId");
                    //playback.startTime = Totaltime;
                    playback.startTime = transform.GetChild(index).transform.GetChild(1).name + transform.GetChild(index).name + TimeSlider.SliderTime;
                    playback.oldUrl = ReplayUIController.Ins.OldUrl;
                    string Sendstr = JsonUtility.ToJson(playback);
                    Log.Debug("向服务器发送的Json:" + Sendstr);
                    ReplayUIController.Ins.RateTxt.text = "1X";
                    EventCenter.BroadCast<string>(Eventdefine.SendPlayback, Sendstr);
                    //Log.Error("正在播放？"+VideoPlayBack.isPlaying);

                }

                else if (UIManger.ChoiceCameraId == 0)
                {

                    GameStart.Instance.ShowTip("请选择要回放的监控设备");
                   

                }
                else if (TimeSlider.SliderTime == "000000")
                {

                    GameStart.Instance.ShowTip("请选择要回放的时间段");
               
                }


            });
            ReplayUIController.startDate = transform.GetChild(index).transform.GetChild(1).name + transform.GetChild(index).name;



        }
        else
        {
            transform.GetChild(index).GetComponent<Toggle>().interactable = true;
            Log.Debug("关闭" + index);

            //退出的时候想服务器发送关闭该userid下的camid设备
            ExitReplay exitReplay = new ExitReplay(long.Parse(PlayerPrefs.GetInt("userId").ToString()), UIManger.ChoiceCameraId,ReplayUIController.Ins.OldUrl);
            string str = JsonUtility.ToJson(exitReplay);
            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/video/playback/path", ExitReplayCallback, true, true, false, str);
            Log.Debug("平台退出向服务器发送关闭录像");

        }


        void ExitReplayCallback(HttpCallBackArgs args)
        {
            if (!string.IsNullOrEmpty(args.Value))
            {
                Log.Debug("收到退出回放服务器返回消息:" + args.Value);
            }
        }

    }
    void SetEveryToggle(bool value, int conut)
    {
        if (value)
        {
            Log.Debug("选择了" + name);

        }
        // if (j == 0 && value)
        // {
        //      Log.Debug("选择了" + value);
        // }
        // if (j == 1 && value)
        // {
        //      Log.Debug("选择了" + value);

        // }
        // if (j == 2 && value)
        // {
        //      Log.Debug("选择了" + value);

        // }
    }

}
