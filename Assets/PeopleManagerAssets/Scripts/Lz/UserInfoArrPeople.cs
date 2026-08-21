using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zFramework.Media;

[Serializable]
public class UserInfoArrPeople
{
    public List<UserInfo> records;
    public int total;//总计多少条
    public int size;//一页多少条
    public int current;//第几页
    public List<object> orders;
    public bool optimizeCountSql;
    public bool searchCount;
    public object maxLimit;
    public string countId;
    public int pages;//一共多少页
    public UserInfoArrPeople(List<UserInfo> arr)
    {
        this.records = arr;
    }
}

[Serializable]
public class DevInfoArr
{
    public List<NVRInformation> records;
    public int total;//总计多少条
    public int size;//一页多少条
    public int current;//第几页
    public List<object> orders;
    public bool optimizeCountSql;
    public bool searchCount;
    public object maxLimit;
    public string countId;
    public int pages;//一共多少页
    public DevInfoArr(List<NVRInformation> arr)
    {
        this.records = arr;
    }
}

[Serializable]
public class UserInfoSXModel
{
    public List<UserAddFail> records;
    public int total;//总计多少条
    public int size;//一页多少条
    public int current;//第几页
    public List<object> orders;
    public bool optimizeCountSql;
    public bool searchCount;
    public object maxLimit;
    public string countId;
    public int pages;//一共多少页
    public UserInfoSXModel(List<UserAddFail> arr)
    {
        this.records = arr;
    }
}


[Serializable]
public class UserAddFail
{
    public int status;
    public string idNum = string.Empty;
    public int devId;
    public string reason = string.Empty;
    public string remark = string.Empty;
    public string username = string.Empty;
    public string phoneNum = string.Empty;
    public string unitName = string.Empty;
    public string devName = string.Empty;
    public string time = string.Empty;

}