using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//IP：192.168.20.100的门禁主机报警信息，dwMajor：0x5，dwMinor：0x4b，卡号：54615465161，身份证号:2，读卡器编号：1，报警触发时间：2023-03-26 15:49:22，事件流水号：3691
[SerializeField]
public class DoorAlarmData 
{
    public string userName;//姓名
    public string idNum;//身份证号
    public string eventType;//事件类型
    public string time;//时间
    public string equipIp;//门禁点IP
    public string faceBase64;//人脸抓拍照片

}
