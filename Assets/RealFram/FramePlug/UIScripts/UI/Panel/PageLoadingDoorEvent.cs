using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
using static DoorEventJsonParse;

public class PageLoadingDoorEvent : PageLoading
{
    public Transform Middle;
    public Transform Down;
    void Awake()
    {
        //Init();
        //Jump2Page(2);
        //LzPeopleJsonParsing.Instance.HTTPGetUserType();
        //Log.Error("先请求人员类型");
    }

    public override void GetPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        Log.Debug("请求的人员类型是XX：" + 1 + "    第几页：" + indexPage + "一页多少个:" + pageNum);
        DoorEventJsonParse.GetInstance().HttpPageGetData(null, indexPage, pageNum, 1, -1, callBak);

    }

    public override void GetSearchPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        DoorEventJsonParse.GetInstance().HttpPageSearchGetData(null, indexPage, pageNum, 1, -1, callBak);
    }
    //public void UpdatePeople()
    //{

    //    EventCenter.addlistener(Eventdefine.OnDataUpdate, pageLoadingController.UpdateData);

    //}
    public override void SetPageItemEvent(Transform item, object itemData, int itemIndex)
    {
        string IdMark = ((DoorEventData)itemData).idNum + GetTimeList(((DoorEventData)itemData).time);
        SaveFacePic(((DoorEventData)itemData).faceBase64, @"c:\doorevent", IdMark);
        item.GetComponent<PeopleInfo>().username = ((DoorEventData)itemData).userName;
        item.GetComponent<PeopleInfo>().idNum = ((DoorEventData)itemData).idNum;

        item.GetComponent<PeopleInfo>().phoneNum = ((DoorEventData)itemData).eventType;
        


        item.GetComponent<PeopleInfo>().unitName =((DoorEventData)itemData).time;
        //item.GetComponent<PeopleInfo>().unitName = ((althorimData)itemData).eventTime.ToString();
        item.GetComponent<PeopleInfo>().equipName = ((DoorEventData)itemData).devName.ToString();


        item.GetChild(0).Find("username").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().username;
        item.GetChild(0).Find("salaryNum").gameObject.SetActive(false);
        item.GetChild(0).Find("phoneNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().phoneNum;
        item.GetChild(0).Find("unitName").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().unitName;
        item.GetChild(0).Find("idNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().equipName;
        item.GetChild(0).Find("address").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().idNum;
        //item.GetChild(0).Find("address").GetComponent<Text>().gameObject.SetActive(false);
        item.GetChild(0).Find("remarks").GetComponent<Text>().gameObject.SetActive(false);
        item.GetChild(1).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(3).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(2).GetComponent<Button>().gameObject.SetActive(true);
        item.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
        item.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
            Middle.gameObject.SetActive(true);
            Down.gameObject.SetActive(true);
            Down.GetComponent<EventDownManager>().GetDoorCoverImg(IdMark);
        });


        //foreach (var item1 in ((UserInfo)itemData).auth)
        //{
        //    Log.Debug("日期:" + item1.authDate);

        //    if (item1.type == TypeMenuController.Ins.ChooseType)
        //    {
        //        item.GetComponent<PeopleInfo>().authDate = item1.authDate;

        //        item.GetComponent<PeopleInfo>().authTime = item1.authTime;
        //        item.GetComponent<PeopleInfo>().type = item1.type;
        //    }
        //}
    }

    public string TimeToTime(int time)
    {
        System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));//获取时间戳
        System.DateTime dt = startTime.AddSeconds(time);
        string t = dt.ToString("yyyy/MM/dd HH:mm:ss");//转化为日期时间
        return t;


    }

    /// <summary>
    /// 处理string类型的time,并获得其int类型的list
    /// </summary>
    /// <param name="a">time格式：“2021/12/30 17:59:25”</param>
    /// <returns>返回格式“[2021,12,30,17,59,25]”</returns>
    string times = string.Empty;
    private string GetTimeList(string a)
    {
        //List<int> listValue = new List<int>();
        times = "";
        a = Regex.Replace(a, @"\s", ",");//空格转换为“,”
        a = a.Replace(":", ",");
        a = a.Replace("-", ",");
        //a = a.Replace("am", "");//删除am字符
        string[] newA = a.Split(',');
        for (int i = 0; i < newA.Length; i++)
        {
            int index = i;
            if (string.IsNullOrEmpty(newA[index])) newA[index] = "00";
            //listValue.Add(int.Parse(newA[index]));
            times += newA[index];
        }
        return times;
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

            if (!Directory.Exists(@"c:\doorevent"))
            {
                Directory.CreateDirectory(@"c:\doorevent");
            }

            Texture2D texture = new Texture2D(2, 2);
            // 将Base64字符串转换为字节数组
            byte[] bytes = Convert.FromBase64String(base64Str);

            // 创建一个新的Texture2D对象并将字节数组加载到其中
            texture.LoadImage(bytes);
            // 将纹理编码为PNG格式的字节数组
            byte[] pngBytes = texture.EncodeToJPG();
            string pname = idnum.Trim() + ".jpg";
            string path = Path.Combine(savePath, pname);
            File.WriteAllBytes(path, pngBytes);
            Destroy(texture);
             Log.Debug("保存图片到" + path);


        }
        catch (Exception e)
        {
             Log.Debug("保存图片失败: " + e.Message);
        }
        //yield return new WaitForEndOfFrame();

    }

}
