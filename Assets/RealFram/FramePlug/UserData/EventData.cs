using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventData 
{
    public string CardNum;//身份证号
    public string Eventtype;//报警类型
    public string EventTime;//报警时间
    public string DevIP;//设备IP
    public Sprite facepic;//人脸抓拍图片

    public EventData( string idNum, string eventtype,string eventtime,string eventip,Sprite sp) 
    {
        this.CardNum = idNum;
        this.Eventtype = eventtype;
        this.EventTime = eventtime;
        this.DevIP = eventip;
        this.facepic = sp;
    }
}
