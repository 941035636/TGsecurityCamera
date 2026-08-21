using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoInfo : MonoBehaviour
{
    public string IP;
    public string CamId;
    public string Rtsp;
    public string RequestHourMinSec;
    public string RequestMonthDay;
    public string RequesYear;

    public void SendRequest()
    {
        string url = "http://192.168.20.142:7711/playback/addr?camId=120&startTime=20230222";
        Network.Ins.RequestRTSP(url);
    }
}
