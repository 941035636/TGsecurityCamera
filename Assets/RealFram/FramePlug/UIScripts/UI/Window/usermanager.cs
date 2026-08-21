using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zFramework.Media;


//public class treedataList
//{
//    public List<treedata> treeList;
//    public treedataList()
//    {
//        treeList = new List<treedata>();
//    }
//}

//public class treedata
//{
//    public int DevType;
//    public string Root;
//    public string Parents;
//    public string Child;


//}

public class usermanager : Window
{
    public AreaGroupDevList CameraAreaGroupDevs;//存储区域和设备信息

    public override void Awake(params object[] paralist)
    {
        //请求监控和门禁分组信息
        GetCamreaGroupDevs();
    }


    void GetCamreaGroupDevs()
    {
        HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraGroupDevsCallback, false, false, false);
    }
    public void GetCameraGroupDevsCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {

            CameraAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevList>(args.Value);
        }
    }
    [Serializable]
    public class AreaGroupDevList
    {
        public List<GetAreaGroupDevs> records = new List<GetAreaGroupDevs>();

    }
    //请求分组设备信息数据结构
    [Serializable]
    public class GetAreaGroupDevs
    {
        public int id;
        public string areaName = string.Empty;
        public string remark = string.Empty;
        public List<NVRInformation> devList = new List<NVRInformation>();
        public int areaType;//1是门禁，2是监控
    }
}
