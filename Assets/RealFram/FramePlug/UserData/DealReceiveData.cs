using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using LitJson;
using System.Threading;
using Microsoft.SqlServer.Server;
using UnityEngine;

public class DealReceiveData : MonoSingleton<DealReceiveData>
{
    string jsonPath = string.Empty;
    // Start is called before the first frame update
    void Start()
    {
        //生成测试数据
        //productTestdata();
        //测试序列化
        //TestDeserialization();

        //TestSerialization("{}");
        //UserInfoSerialization(@" {
        //    ""id"": ""6801"",
        //    ""username"": ""kll"",
        //    ""salaryNum"": ""234"",
        //    ""phoneNum"": ""123123"",
        //    ""unitName"": ""stgergsdg"",
        //    ""idNum"": ""234235"",
        //    ""address"": ""ser"",
        //    ""remarks"": ""wu"",
        //    ""faceBase64"": ""C:\\facepicture\\234235.jpg"",
        //    ""auth"": [
        //        {
        //            ""id"": ""342"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""all"",
        //            ""type"": 0,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 1,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 2,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 3,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 4,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 5,
        //            ""idNum"": ""234235""
        //        },
        //        {
        //            ""id"": ""343"",
        //            ""authDate"": ""2023-01-01 2099-01-01"",
        //            ""authTime"": ""00:00:00 23:59:59"",
        //            ""authWeek"": ""1,2,3"",
        //            ""type"": 6,
        //            ""idNum"": ""234235""
        //        }
        //    ]
        //}");
    }

    //void productTestdata() 
    //{
    //    List<UserInfo> UserList = new List<UserInfo>();


    //    UserInfo user = new UserInfo();
    //    List<Auth> userauthlist = new List<Auth>();
    //    user.userName = "张三";
    //    user.phoneNum = "18567901234";
    //    user.idNum = "378267587652345657";
    //    user.unitName = "炼钢厂";
    //    user.salaryNum = "00001";
    //    user.address = "生活区1栋";
    //    user.remarks = "";
    //    user.faceBase64 = "";
    //    Auth auth1 = new Auth();
    //    auth1.authDate = "2022.1.1-2023.12.30";
    //    auth1.authTime = "6:00-18:00";
    //    auth1.authWeek = "1，2";
    //    auth1.type = 0;
    //    Auth auth2 = new Auth();
    //    auth2.authDate = "2022.6.1-2024.12.30";
    //    auth2.authTime = "16:00-23:00";
    //    auth2.authWeek = "3，4,5";
    //    auth2.type = 1;
    //    userauthlist.Add(auth1);
    //    userauthlist.Add(auth2);
    //    user.authlist = userauthlist;

    //    UserInfo user2 = new UserInfo();
    //    List<Auth> userauthlist2 = new List<Auth>();
    //    user2.userName = "李四";
    //    user2.phoneNum = "18567901234";
    //    user2.idNum = "378267587652345657";
    //    user2.unitName = "焦化厂";
    //    user2.salaryNum = "00001";
    //    user2.address = "生活区2栋";
    //    user2.remarks = "";
    //    user2.faceBase64 = "";
    //    Auth auth3 = new Auth();
    //    auth3.authDate = "2022.1.1-2023.12.30";
    //    auth3.authTime = "6:00-18:00";
    //    auth3.authWeek = "2";
    //    auth3.type = 0;
    //    userauthlist2.Add(auth3);
    //    user2.authlist = userauthlist2;


    //    UserInfo user3 = new UserInfo();
    //    List<Auth> userauthlist3 = new List<Auth>();
    //    user3.userName = "王五";
    //    user3.phoneNum = "18567901234";
    //    user3.idNum = "378267587652345657";
    //    user3.unitName = "炼钢厂";
    //    user3.salaryNum = "00001";
    //    user3.address = "生活区3栋";
    //    user3.remarks = "";
    //    user3.faceBase64 = "";
    //    Auth auth5 = new Auth();
    //    auth5.authDate = "2022.1.1-2023.12.30";
    //    auth5.authTime = "6:00-18:00";
    //    auth5.authWeek = "3,4";
    //    auth5.type = 0;
    //    Auth auth6 = new Auth();
    //    auth6.authDate = "2022.6.1-2024.12.30";
    //    auth6.authTime = "16:00-23:00";
    //    auth6.authWeek = "6";
    //    auth6.type = 1;
    //    userauthlist3.Add(auth5);
    //    userauthlist3.Add(auth6);
    //    user3.authlist = userauthlist3;


    //    UserList.Add(user);
    //    UserList.Add(user2);
    //    UserList.Add(user3);


    //    UserInfoArr userarr = new UserInfoArr(UserList);
    //    SaveUserinfoConfiguration_outside(userarr);
    //}

    public void UserInfoSerialization(string str)
    {
        if (string.IsNullOrEmpty(str)) 
        {
            Log.Debug("从服务器接收到的人员信息为空");
            return;
        }
        UserInfo user;
        jsonPath = Path.Combine(Application.streamingAssetsPath, "UserData");
        if (!Directory.Exists(jsonPath))
        {
            Directory.CreateDirectory(jsonPath);
        }
        try
        {
            user = JsonUtility.FromJson<UserInfo>(str);
            Log.Debug("Mqtt更新的用户信息:"+user);
            //for (int i = 0; i < user.auth.Count; i++)
            //{
            //    switch (user.auth[i].type)
            //    {
            //        case (int)Usertype.employee:
            //             Log.Debug("新增“在职员工”");
            //            OpreatUserData(user, "employee.json", (int)Usertype.employee);
            //            break;
            //        case (int)Usertype.visitor:
            //             Log.Debug("新增“访客”");
            //            OpreatUserData(user, "visitor.json", (int)Usertype.visitor);
            //            break;
            //        case (int)Usertype.family_mem:
            //             Log.Debug("新增“家属人员”");
            //            OpreatUserData(user, "family_mem.json", (int)Usertype.family_mem);
            //            break;
            //        case (int)Usertype.retirees:
            //            OpreatUserData(user, "retirees.json", (int)Usertype.retirees);
            //             Log.Debug("新增“退休人员”");
            //            break;
            //        case (int)Usertype.outside_construction_workers:
            //            OpreatUserData(user, "outside_construction_workers.json", (int)Usertype.outside_construction_workers);
            //             Log.Debug("新增“外施工人员”");
            //            break;
            //        case (int)Usertype.outside_contractor_pers:
            //            OpreatUserData(user, "outside_contractor_pers.json", (int)Usertype.outside_contractor_pers);
            //             Log.Debug("新增“外包工人员”");
            //            break;
            //        case (int)Usertype.spec_approved_pers:
            //            OpreatUserData(user, "spec_approved_pers.json", (int)Usertype.spec_approved_pers);
            //             Log.Debug("新增“特批人员”");
            //            break;
            //        default:
            //            break;
                 
            //    }
            //}
            SaveFacePic(user.faceBase64, @"C:\facepicture", user.idNum);
            user.faceBase64 = @"C:\facepicture\" + user.idNum + ".jpg";
        }
        catch (Exception e)
        {
            Log.Debug(e.ToString());
        }
      
  

    }

    private void OpreatUserData(UserInfo user,string filepath,int typeid)
    {
        List<UserInfo> list = new List<UserInfo>();
        UserInfoArr userInfo_Arr = new UserInfoArr(list);
        var file = Path.Combine(jsonPath, filepath);
        if (!File.Exists(file)) { File.CreateText(file).Dispose(); }
        string jsonstr = File.ReadAllText(file, Encoding.UTF8);
        if (!string.IsNullOrEmpty(jsonstr)) 
            userInfo_Arr = JsonUtility.FromJson<UserInfoArr>(jsonstr);
        user.id = userInfo_Arr.records.Count.ToString();
        userInfo_Arr.records.Add(user);
        SaveUserinfoConfiguration_outside(userInfo_Arr, file);

    }




    /// <summary>
    /// 反序列化分页请求返回的Json
    /// </summary>
    public void HttpPageDeserialization(string ReceiveStr)
    {
        //jsonPath = Path.Combine(Application.streamingAssetsPath, "Configurations");
        //var file = Path.Combine(jsonPath, "testuser.json");
        //string jsonstr = File.ReadAllText(file, Encoding.UTF8);
        try
        {
            UserInfoArr userInfoarr = JsonUtility.FromJson<UserInfoArr>(ReceiveStr);
            Log.Debug("人数:" + userInfoarr.records.Count);
            Log.Debug("pages:"+userInfoarr.pages);
            for (int i = 0; i < userInfoarr.records.Count; i++)
            {
                for (int j = 0; j < userInfoarr.records[i].auth.Count; j++)
                {
                     Log.Debug("------------------------------------------------");
                     Log.Debug("授权周:" + userInfoarr.records[i].auth[j].authWeek);
                     Log.Debug("授权时间:" + userInfoarr.records[i].auth[j].authTime);
                     Log.Debug("授权日期:" + userInfoarr.records[i].auth[j].authDate);
                    switch (userInfoarr.records[i].auth[j].type)
                    {
                        case (int)Usertype.employee:
                            UserGroup.Instance.employeelist.Add(userInfoarr.records[i]);
                            break;
                        case (int)Usertype.visitor:
                            UserGroup.Instance.visitorlist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“访客”");
                            break;
                        case (int)Usertype.family_mem:
                            UserGroup.Instance.family_memlist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“家属人员”");
                            break;
                        case (int)Usertype.retirees:
                            UserGroup.Instance.retireeslist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“退休人员”");
                            break;
                        case (int)Usertype.outside_construction_workers:
                            UserGroup.Instance.construction_workerslist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“外施工人员”");
                            break;
                        case (int)Usertype.outside_contractor_pers:
                            UserGroup.Instance.contractor_perslist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“外包工人员”");
                            break;
                        case (int)Usertype.spec_approved_pers:
                            UserGroup.Instance.spec_approved_perslist.Add(userInfoarr.records[i]);
                             Log.Debug("用户类型是“特批人员”");
                            break;
                        default:
                            break;
                    }

                }
                if (!string.IsNullOrEmpty(userInfoarr.records[i].faceBase64))
                {
                    //Base64StrToImage(userInfoarr.arr[i].faceBase64, @"C:\facepicture", userInfoarr.arr[i].idNum);
                    SaveFacePic(userInfoarr.records[i].faceBase64, @"C:\facepicture", userInfoarr.records[i].idNum);
                }
            }
        }
        catch (Exception e)
        {
            Log.Debug(e .Message);
        }
      

    }
    /// <summary>
    /// 本地序列化
    /// </summary>
    /// <param name="userarr"></param>
    public void SaveUserinfoConfiguration_outside(UserInfoArr userarr, string file)
    {

        List<UserInfo> newusers = new List<UserInfo>();
        foreach (var item in userarr.records)
        {
            newusers.Add(item);
        }

        var info = JsonUtility.ToJson(new UserInfoArr(newusers), true);
        File.WriteAllText(file, info, Encoding.UTF8);
        userarr.records.Clear();
        newusers.Clear();
    }

    /// <summary>
    /// 将Base64字符串转换为Image对象
    /// </summary>
    /// <param name="base64Str">base64字符串</param>
    /// <returns></returns>
    public static Bitmap Base64StrToImage(string base64Str)
    {
        Bitmap bitmap = null;

        try
        {
            byte[] arr = Convert.FromBase64String(base64Str);
            MemoryStream ms = new MemoryStream(arr);
            Bitmap bmp = new Bitmap(ms);
            ms.Close();
            bitmap = bmp;
        }
        catch (Exception ex)
        {
            Log.Debug(ex.ToString());
        }

        return bitmap;
    }

    /// <summary>
    /// Base64编码转为人脸图片保存本地
    /// </summary>
    /// <param name="base64Str"></param>
    /// <param name="savePath"></param>
    /// <param name="idnum"></param>
    public void SaveFacePic(string base64Str, string savePath, string idnum)
    {
        try
        {
            // 将Base64字符串转换为字节数组
            byte[] bytes = Convert.FromBase64String(base64Str);

            // 创建一个新的Texture2D对象并将字节数组加载到其中
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            // 将纹理编码为PNG格式的字节数组
            byte[] pngBytes = texture.EncodeToJPG();
            string pname = idnum + ".jpg";
            string path = Path.Combine(savePath, pname);
            File.WriteAllBytes(path, pngBytes);

             Log.Debug("保存图片到" + path);
        }
        catch (Exception e)
        {
             Log.Debug("保存图片失败: " + e.Message);
        }
    }

    /// <summary>
    /// 将Base64字符串转换为图片并保存到本地
    /// </summary>
    /// <param name="base64Str">base64字符串</param>
    /// <param name="savePath">图片保存地址，如：/Content/Images/10000.png</param>
    /// <returns></returns>
    /*
    public static bool Base64StrToImage(string base64Str, string savePath,string idnum)
    {
        var ret = true;

        try
        {
            var bitmap = Base64StrToImage(base64Str);

            if (bitmap != null)
            {
                Log.Debug("bitmap不为空");
                //创建文件夹
                //var folderPath = savePath.Substring(0, savePath.LastIndexOf('/'));
                //if (!Directory.Exists(savePath))
                //{
                //    Directory.CreateDirectory(savePath);
                //}
                //图片后缀格式
                var suffix ="jpg".ToLower();
                //var suffix = savePath.Substring(savePath.LastIndexOf('.') + 1, savePath.Length - savePath.LastIndexOf('.') - 1).ToLower();
                var suffixName = suffix == "png" ? ImageFormat.Png :
                    suffix == "jpg" || suffix == "jpeg" ? ImageFormat.Jpeg :
                    suffix == "bmp" ? ImageFormat.Bmp :
                    suffix == "gif" ? ImageFormat.Gif : ImageFormat.Jpeg;

                //这里复制一份对图像进行保存，否则会出现“GDI+ 中发生一般性错误”的错误提示
                var bmpNew = new Bitmap(bitmap);
                //bmpNew.Save(Path.Combine(savePath, idnum), suffixName);
                //bmpNew.Dispose();

                bitmap.Dispose();


                //以下代码为保存图片时，设置压缩质量    
                EncoderParameters ep = new EncoderParameters();
                long[] qy = new long[1];
                qy[0] = 1;//设置压缩的比例1-100    
                EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                ep.Param[0] = eParam;
                try
                {
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICIinfo = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPG"))
                        {
                            jpegICIinfo = arrayICI[x];
                            break;
                        }
                    }
                    if (jpegICIinfo != null)
                    {
                        //这里有坑
                        bmpNew.Save(Path.Combine(savePath, idnum), jpegICIinfo, ep);//dFile是压缩后的新路径   
                    }
                    else
                    {
                        Log.Debug("别的格式?   "+ Path.Combine(savePath, idnum));
                        bmpNew.Save(Path.Combine(savePath, idnum),ImageFormat.Jpeg);//这里有坑
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Debug(ex);
                    return false;
                }
                finally
                {

                    bmpNew.Dispose();
                }


            }
            else
            {
                Log.Debug("bitmap为空");
                ret = false;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex.ToString());
            ret = false;
        }

        return ret;
    }*/   //有问题
}
