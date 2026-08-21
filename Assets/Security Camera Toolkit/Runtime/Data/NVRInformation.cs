// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace zFramework.Media
{
    [Serializable]
    public struct NVRInformation
    {
        /// <summary>
        /// 监控地址，结构必须是 ip:port ,形如 127.0.0.1:8083 
        /// <para>这个 ip 是内网布线图上的 ip ，唯一 id 一般的存在</para>
        /// </summary>
        public string host;
        public SDKTYPE type;
        public string mapping; // 映射主机，外网访问
        public bool enableMapping; //映射使能，true 则使用映射主机访问监控，请注意如果多个 NVR  RTSP 端口为 554，只会有一个有效，请务必避免端口冲突
        public string userName;
        public string password;
        public bool enable;
        public string description;
        public int channel;
        public string cameraname;
        public string connecttype;
        public string equiptype;
        public string num;
        public string pwdsecurity;
        public string netstate;
        public int id; //设备id
        [Newtonsoft.Json.JsonIgnore]
        public string Ip
        {
            get
            {
                var temp = enableMapping ? mapping : host;
                return temp.Trim().Split(':')[0];
            }
        }
        //端口
        [Newtonsoft.Json.JsonIgnore]
        public uint Port
        {
            get
            {
                var temp = enableMapping ? mapping : host;
                var arr = temp.Trim().Split(':');
                return Convert.ToUInt32(arr.Length==1?"80":arr[1]);
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public string ActiveHost => enableMapping ? mapping : host;
    }



    /// <summary>
    /// 监控分组信息
    /// </summary>
    [Serializable]
    public class GroupWrapper
    {
        public List<CameraGroupInformation> CameraGroupList;
        public GroupWrapper(List<CameraGroupInformation> arr)
        {
            this.CameraGroupList = arr;
        }


    }

    [Serializable]
    public class CameraGroupInformation
    {
        ////区域名
        public string GroupName;
        //区域id
        public int id;
        ////组下的监控
        public List<NVRInformation> CameraInfos;
        public CameraGroupInformation(string groupname, List<NVRInformation> camerainfos, int Id = 0)
        {
            this.GroupName = groupname;
            this.CameraInfos = camerainfos;
            this.id = Id;
        }
    }
}
