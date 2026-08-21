using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace zFramework.Media
{


    

    public class Userauthority
    {
        //监控点配置
        public CameraSet cameraSet;
        //门禁点配置
        public DoorSet doorSet;
        //系统用户管理权限
        public bool SystemUserManager;
        //平台人员信息管理权限
        public bool PeopleManager;
   

    }

    #region  监控点配置
    [Serializable]
    public class CameraSet
    {

        //监控分组
        public CameraGroupWrapper CameraGroup;
        //云台控制
        public bool IsPTZ;


    }
    /// <summary>
    /// 区域监控分组信息
    /// </summary>
    [Serializable]
    public class CameraGroupWrapper
    {
        public List<CameraGroupInfor> CameraGroupList;
        public CameraGroupWrapper(List<CameraGroupInfor> arr)
        {
            this.CameraGroupList = arr;
        }


    }

    //监控分组信息
    [Serializable]
    public class CameraGroupInfor
    {
        ////区域名
        public string GroupName;
        //区域id
        public int AreaId;
        ////组下的监控
        public List<CameraAuth> CameraInfos;
        public CameraGroupInfor(string groupname, List<CameraAuth> cameraauth, int Id = 0)
        {
            this.GroupName = groupname;
            this.CameraInfos = cameraauth;
            this.AreaId = Id;
        }
    }
    //监控信息
    public class CameraAuth
    {
        //监控设备id
        public int Id;
        //设备配置权限，增，删，改，查，配置分组
        public bool IsSet;
        //预览
        public bool Previw;
        //回放
        public bool Replay;
        //监控事件，算法
        public bool Event;


    }
    #endregion

    #region 门禁点配置
    [Serializable]
    public class DoorSet
    {
   
        //门禁分组
        public DoorGroupWrapper DoorGroup;


    }
    /// <summary>
    /// 区域门禁分组信息
    /// </summary>
    [Serializable]
    public class DoorGroupWrapper
    {
        public List<DoorGroupInfor> DoorGroupList;
        public DoorGroupWrapper(List<DoorGroupInfor> arr)
        {
            this.DoorGroupList = arr;
        }


    }
    //门禁分组信息
    [Serializable]
    public class DoorGroupInfor
    {
        //区域名
        public string GroupName;
        //区域id
        public int AreaId;
        ////组下的门禁设备
        public List<DoorAuth> DoorAuthList;
        public DoorGroupInfor(string groupname, List<DoorAuth> cameraauth, int Id = 0)
        {
            this.GroupName = groupname;
            this.DoorAuthList = cameraauth;
            this.AreaId = Id;
        }
    }
    //门禁信息
    public class DoorAuth
    {
        //门禁设备id
        public int Id;
        //设备配置权限，增，删，改，查，配置分组
        public bool IsSet;
        //门禁事件
        public bool Event;


    }
    #endregion








  


 
}

