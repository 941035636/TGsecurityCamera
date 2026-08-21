using System;
using System.Collections.Generic;
using zFramework.Media;


#region  人员基础信息

[Serializable]
//人员基础数据
public class UserInfo
{

    public string id;
    public string username;                               //姓名
    public string salaryNum;                             //工资编号
    public string phoneNum;                             //手机号
    public string unitName;                            //单位名称
    public string idNum;                              //身份证号 
    public string address;                            //家庭住址
    public string remarks;                         //备注
    public string faceBase64;                  //人脸信息
    public List<Auth> auth;                          //可变类型，不唯一(一个人可能对应多种情况)


}
[Serializable]
public class Auth
{
    public string id;
    public string authDate;                       //授权时间(某天到某天)
    public string authTime;                      //授权时间(几点到几点)
    public string authWeek;                      //授权时间(周几到周几) 传阿拉伯数字，"all"代表周一到周日全部
    public int type;                          //用户类型
    public string idNum;
}
#endregion


#region  人员与用户类型绑定/门禁设备与区域绑定
//用户类型
public enum Usertype
{
    employee = 1,         //在职员工
    retirees = 2,           //离退休人员
    family_mem = 3,            //家属人员
    outside_construction_workers = 4,       //外施工人员
    outside_contractor_pers = 5,      //外包公人员
    visitor = 6,         //访客
    spec_approved_pers = 7           //特批人员



}
[Serializable]
public class UserType
{
    public int id;
    public string typeName;// 人员类型名字
    public string remark;
    public int typeId; //人员类型

}

//区域
public enum Area
{
    Earth1 = 1,                                  //东一
    Earth2 = 2,                                 //东二
    Earth4 = 3,                                //东四
    West1 = 4,                                //西一
    West2 = 5,                               //西二
    West3 = 6,                              //西三
    West4 = 7,                             //西四
    West5 = 8,                            //西五
    West6 = 9,                           //西六
    LifeArea1 = 10,                      //生活区一
    LifeArea2 = 11,                     //生活区二
    ParkLot2 = 12,                     //二号停车场
    ParkLot3 = 13,                    //三号停车场
    StaffClub = 14,                   //职工俱乐部
    test = 100
}
//人员类型与区域绑定
public class UserTypeArea
{
    private static UserTypeArea instance;
    public static UserTypeArea GetInstance()
    {

        if (instance == null)
        {
            instance = new UserTypeArea();
        }
        return instance;
    }
    public List<Area> employeelist;
    public List<Area> retireeslist;
    public List<Area> family_memlist;
    public List<Area> outside_construction_workerslist;
    public List<Area> outside_contractor_perslist;
    public List<Area> visitorlist;
    public List<Area> spec_approved_perslist;
    public Dictionary<Usertype, List<Area>> usertypeareaDic = new Dictionary<Usertype, List<Area>>();     //人员类型与区域绑定

    public void init()
    {
        employeelist = new List<Area>();
        retireeslist = new List<Area>();
        family_memlist = new List<Area>();
        outside_construction_workerslist = new List<Area>();
        outside_contractor_perslist = new List<Area>();
        visitorlist = new List<Area>();
        spec_approved_perslist = new List<Area>();
    }   //存储区域


}
//门禁设备信息
public class Equipdoor
{
    public string host;
    public SDKTYPE type;
    public string mapping; // 映射主机，外网访问
    public bool enableMapping;
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
            return Convert.ToUInt32(arr.Length == 1 ? "80" : arr[1]);
        }
    }
    [Newtonsoft.Json.JsonIgnore]
    public string ActiveHost => enableMapping ? mapping : host;
}


//人员类别(分组)与人员信息绑定
public class UserGroup : Singleton<UserGroup>
{
    public List<UserInfo> employeelist = new List<UserInfo>();       //存储人员信息
    public List<UserInfo> retireeslist = new List<UserInfo>();
    public List<UserInfo> family_memlist = new List<UserInfo>();
    public List<UserInfo> construction_workerslist = new List<UserInfo>();
    public List<UserInfo> contractor_perslist = new List<UserInfo>();
    public List<UserInfo> visitorlist = new List<UserInfo>();
    public List<UserInfo> spec_approved_perslist = new List<UserInfo>();
    public Dictionary<Usertype, List<UserInfo>> UsergroupDic = new Dictionary<Usertype, List<UserInfo>>();     //人员信息与用户类型(分组)绑定
    public void initUserGroup()
    {
        UsergroupDic.Add(Usertype.employee, employeelist);
        UsergroupDic.Add(Usertype.retirees, retireeslist);
        UsergroupDic.Add(Usertype.family_mem, family_memlist);
        UsergroupDic.Add(Usertype.outside_construction_workers, construction_workerslist);
        UsergroupDic.Add(Usertype.outside_contractor_pers, contractor_perslist);
        UsergroupDic.Add(Usertype.visitor, visitorlist);
        UsergroupDic.Add(Usertype.spec_approved_pers, spec_approved_perslist);

    }

}

//区域(分组)与门禁设备绑定
public class EquipdoorArea
{
    public List<Equipdoor> equipdoorlist = new List<Equipdoor>();       //存储门禁设备
    public Dictionary<Area, List<Equipdoor>> AreaequipDic = new Dictionary<Area, List<Equipdoor>>();     //门径设备与区域(分组)绑定

}

#endregion
