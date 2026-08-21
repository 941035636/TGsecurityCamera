

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zFramework.Media;



//{
//  "typeList": [
//    {
//      "id": 12,
//      "typeName": "全职",
//      "areaList": [
//        {
//          "id": 1，
//          "areaName": "东区1",
//          "devList": [
//            {
//              "id": 2,
//              "devName": "cam02"
//            },
//            {
//              "id": 4,
//              "devName": "cam04"
//            }
//          ]
//        }

//      ]
//    }
//  ]
//}



//    {
//    "id": 1,
//    "typeName": "在职员工",
//    "areaList": [
//        {
//            "areaName": "10",
//            "id": 2,
//            "devList": [
//                {
//                    "id": 1,
//                    "devName": "生活区一号门进3"
//                },
//                {
//                    "id": 1,
//                    "devName": "生活区一号门进4"
//                }
//            ]
//        }
//    ]
//}

///人员类型区域容器

[Serializable]
public class AllPersontypeAuth
{

    public List<PersontypeAuthInfo> typeList;
    public AllPersontypeAuth()
    {
        typeList = new List<PersontypeAuthInfo>();
    }
}

/// <summary>
/// 人员类型权限对应区域设备
/// </summary>
[Serializable]
public class PersontypeAuthInfo
{
    public int typeId;  //人员类型id
    public string typeName; //人员类型名字
    public List<AreaInformation> areaList;
    public PersontypeAuthInfo()
    {
        areaList = new List<AreaInformation>();
    }
}
[Serializable]
public class AreaInformation
{
    //区域名
    public string areaName;
    public int id;//区域id
    public List<Device> devList;
    public AreaInformation()
    {
        devList = new List<Device>();
    }

}
[Serializable]
public class Device
{
    public int id;//设备id
    public string devName;//设备名字
}
