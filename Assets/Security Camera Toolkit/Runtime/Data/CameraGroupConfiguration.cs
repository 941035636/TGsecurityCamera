using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LitJson;
namespace zFramework.Media
{
    public class CameraGroupConfiguration : ScriptableObject
    {
        public string jsonCameraGroupName = "CameraGroupConfiguration.json";
        public string jsonDoorGroupName = "DoorGroupConfiguration.json";
        public string jsonPath = Path.Combine(Application.streamingAssetsPath, "Configurations");
        //public List<CameraGroupInformation> cameras = new List<CameraGroupInformation>();
        public Dictionary<string, List<NVRInformation>> cameragroupdic = new Dictionary<string, List<NVRInformation>>();
        private static CameraGroupConfiguration instance;
        public static CameraGroupConfiguration Instance()
        {
            if (instance == null)
            {
                instance = new CameraGroupConfiguration();
            }
            return instance;

        }

        /// <summary>
        /// 外部调用保存本地监控分组信息
        /// </summary>
        /// <param name="nvrs"></param>
        //public void SaveCameraGroupConfiguration_outside(List<CameraGroupInformation> cameras)
        //{
        //    if (!Directory.Exists(jsonPath))
        //    {
        //        Directory.CreateDirectory(jsonPath);
        //    }
        //    var info = JsonUtility.ToJson(new GroupWrapper(cameras), true);
        //    var file = Path.Combine(jsonPath, jsonName);
        //    File.WriteAllText(file, info, Encoding.UTF8);
        //}
        public void SaveCameraGroupConfiguration_outside(Dictionary<string, List<NVRInformation>> cameragroupDic)
        {

            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            List<CameraGroupInformation> ListCameraGroup = new List<CameraGroupInformation>();

            foreach (var item in cameragroupDic)
            {
                ListCameraGroup.Add(new CameraGroupInformation(item.Key, item.Value));
            }
            var info = JsonUtility.ToJson(new GroupWrapper(ListCameraGroup), true);
            //var info = JsonMapper.ToJson(new GroupWrapper( cameragroupDic));
            var file = Path.Combine(jsonPath, jsonCameraGroupName);
            File.WriteAllText(file, info, Encoding.UTF8);
        }
        public void SaveDoorGroupConfiguration_outside(Dictionary<string, List<NVRInformation>> cameragroupDic)
        {

            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            List<CameraGroupInformation> ListDoorGroup = new List<CameraGroupInformation>();

            foreach (var item in cameragroupDic)
            {
                ListDoorGroup.Add(new CameraGroupInformation(item.Key, item.Value));
            }
            var info = JsonUtility.ToJson(new GroupWrapper(ListDoorGroup), true);
            //var info = JsonMapper.ToJson(new GroupWrapper( cameragroupDic));
            var file = Path.Combine(jsonPath, jsonDoorGroupName);
            File.WriteAllText(file, info, Encoding.UTF8);
        }

        public void SaveDoorGroupList(List<CameraGroupInformation> cameragroupList)
        {

            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
          
            var info = JsonUtility.ToJson(new GroupWrapper(cameragroupList), true);
            var file = Path.Combine(jsonPath, jsonDoorGroupName);
            File.WriteAllText(file, info, Encoding.UTF8);
        }


        /// <summary>
        /// 本地加载cameragroup信息
        /// </summary>
        //public List<CameraGroupInformation> LoadCameragroupConfigBynative()
        //{
        //    var file = Path.Combine(jsonPath, jsonName);
        //    if (File.Exists(file))
        //    {
        //        var info = File.ReadAllText(file);
        //        var obj = JsonUtility.FromJson<GroupWrapper>(info);
        //        if (null != obj)
        //        {
        //            cameras = obj.arr;
        //            return cameras;
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

        //    }
        //    return new List<CameraGroupInformation>();
        //}
        public Dictionary<string, List<NVRInformation>> LoadCameragroupConfigBynative()
        {
            var file = Path.Combine(jsonPath, jsonCameraGroupName);
            Dictionary<string, List<NVRInformation>> CameraGroupDic = new Dictionary<string, List<NVRInformation>>();
            if (File.Exists(file))
            {
                var info = File.ReadAllText(file);
                var obj = JsonUtility.FromJson<GroupWrapper>(info);
             
                if (null != obj)
                {
                    //cameragroupdic = obj.cameragroupDic;
                    for (int i = 0; i < obj.CameraGroupList.Count; i++)
                    {
                        CameraGroupDic.Add(obj.CameraGroupList[i].GroupName, obj.CameraGroupList[i].CameraInfos);
                    }
                    return CameraGroupDic;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

            }
            return CameraGroupDic;
        }
        public Dictionary<string, List<NVRInformation>> LoadDoorgroupConfigBynative()
        {
            var file = Path.Combine(jsonPath, jsonDoorGroupName);
            Dictionary<string, List<NVRInformation>> DoorGroupDic = new Dictionary<string, List<NVRInformation>>();
            if (File.Exists(file))
            {
                var info = File.ReadAllText(file);
                var obj = JsonUtility.FromJson<GroupWrapper>(info);

                if (null != obj)
                {
                    //cameragroupdic = obj.cameragroupDic;
                    for (int i = 0; i < obj.CameraGroupList.Count; i++)
                    {
                        DoorGroupDic.Add(obj.CameraGroupList[i].GroupName, obj.CameraGroupList[i].CameraInfos);
                        
                    }
                    return DoorGroupDic;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

            }
            return DoorGroupDic;
        }
        //返回门禁区域信息
        public List<CameraGroupInformation> LoadDoorgroupInfoList()
        {
            var file = Path.Combine(jsonPath, jsonDoorGroupName);
            if (File.Exists(file))
            {
                var info = File.ReadAllText(file);
                var obj = JsonUtility.FromJson<GroupWrapper>(info);

                if (null != obj)
                {
                 
                    return obj.CameraGroupList;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

            }
            return null;
        }
        //返回监控区域信息
        public List<CameraGroupInformation> LoadCameragroupInfoList()
        {
            var file = Path.Combine(jsonPath, jsonCameraGroupName);
            if (File.Exists(file))
            {
                var info = File.ReadAllText(file);
                var obj = JsonUtility.FromJson<GroupWrapper>(info);

                if (null != obj)
                {

                    return obj.CameraGroupList;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

            }
            return null;
        }

        public GroupWrapper LoadDoorEquipGroupByNative()
        {
            var file = Path.Combine(jsonPath, jsonDoorGroupName);
       
            if (File.Exists(file))
            {
                var info = File.ReadAllText(file);
                GroupWrapper group  = JsonUtility.FromJson<GroupWrapper>(info);

                if (null != group)
                {
                    return group;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(CameraGroupInformation)}:不存在 json 配置文件 ，Path 见 ↓ \n{jsonPath} ");

            }
            return null;

        }

    }

}

