using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZTools;
using UnityEngine.UI;
using System.IO;

public class PageLoadingCameraEvent : PageLoading
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
        CameraEventJsonParse.GetInstance().HttpPageGetData(null, indexPage, pageNum, 1, -1, callBak);

    }
    public override void GetSearchPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        CameraEventJsonParse.GetInstance().HttpPageSearchGetData(null, indexPage, pageNum, 1, -1, callBak);
    }

    //public void UpdatePeople()
    //{

    //    EventCenter.addlistener(Eventdefine.OnDataUpdate, pageLoadingController.UpdateData);

    //}
    public override void SetPageItemEvent(Transform item, object itemData, int itemIndex)
    {
        if (((althorimData)itemData).type == "face_rec")
            SaveFacePic(((althorimData)itemData).img, @"c:\faceevent", ((althorimData)itemData).camId + ((althorimData)itemData).idNum);
        else
            SaveFacePic(((althorimData)itemData).img, @"c:\faceevent", ((althorimData)itemData).camId + ((althorimData)itemData).eventTime);
        item.GetComponent<PeopleInfo>().username = ((althorimData)itemData).userName;
        //item.GetComponent<PeopleInfo>().salaryNum = ((UserInfo)itemData).salaryNum;
        if (((althorimData)itemData).type == "face_rec")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "人脸识别";
            if (string.IsNullOrEmpty(((althorimData)itemData).idNum))
                item.GetComponent<PeopleInfo>().address = "null";
            else
                item.GetComponent<PeopleInfo>().address = ((althorimData)itemData).idNum;
        }
        else if (((althorimData)itemData).type == "person_group")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "人员聚集";
            item.GetComponent<PeopleInfo>().address = "";
        }
        else if (((althorimData)itemData).type == "person_fall")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "人员跌倒";
            item.GetComponent<PeopleInfo>().address = "";
        }
        else if (((althorimData)itemData).type == "person_brust")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "人员闯入";
            item.GetComponent<PeopleInfo>().address = "";
        }
        else if (((althorimData)itemData).type == "fire_smoke")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "火灾烟雾";
            item.GetComponent<PeopleInfo>().address = "";
        }
        else if (((althorimData)itemData).type == "car_plate")
        {
            item.GetComponent<PeopleInfo>().phoneNum = "车牌识别";
            if (string.IsNullOrEmpty(((althorimData)itemData).carNum))
                item.GetComponent<PeopleInfo>().address = "null";
            else
                item.GetComponent<PeopleInfo>().address = ((althorimData)itemData).carNum;
        }

        item.GetComponent<PeopleInfo>().unitName = TimeToTime(((althorimData)itemData).eventTime);
        //item.GetComponent<PeopleInfo>().unitName = ((althorimData)itemData).eventTime.ToString();
        item.GetComponent<PeopleInfo>().equipName = ((althorimData)itemData).camName;


        item.GetChild(0).Find("username").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().username;
        item.GetChild(0).Find("salaryNum").gameObject.SetActive(false);
        item.GetChild(0).Find("phoneNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().phoneNum;
        item.GetChild(0).Find("unitName").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().unitName;
        item.GetChild(0).Find("idNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().equipName;
        item.GetChild(0).Find("address").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().address;
        item.GetChild(0).Find("remarks").GetComponent<Text>().gameObject.SetActive(false);
        item.GetChild(1).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(2).GetComponent<Button>().gameObject.SetActive(true);
        item.GetChild(3).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
        item.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
        {
            Middle.gameObject.SetActive(true);
            Down.gameObject.SetActive(true);
            if (((althorimData)itemData).type == "face_rec")
                Down.GetComponent<EventDownManager>().GetCoverImg(((althorimData)itemData).camId + ((althorimData)itemData).idNum);
            else
            {
                Down.GetComponent<EventDownManager>().GetCoverImg(((althorimData)itemData).camId + ((althorimData)itemData).eventTime.ToString());
            }
        });



    }

    public string TimeToTime(int time)
    {
        System.DateTime startTime = System.TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));//获取时间戳
        System.DateTime dt = startTime.AddSeconds(time);
        string t = dt.ToString("yyyy/MM/dd HH:mm:ss");//转化为日期时间
        return t;


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
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            Texture2D texture = new Texture2D(2, 2);
            // 将Base64字符串转换为字节数组
            byte[] bytes = Convert.FromBase64String(base64Str);

            // 创建一个新的Texture2D对象并将字节数组加载到其中
            texture.LoadImage(bytes);
            // 将纹理编码为PNG格式的字节数组
            byte[] pngBytes = texture.EncodeToJPG();
            string pname = idnum + ".jpg";
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
