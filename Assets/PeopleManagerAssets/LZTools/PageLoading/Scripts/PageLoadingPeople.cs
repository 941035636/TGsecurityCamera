using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
using UnityTimer;
using System;
using System.IO;

public class PageLoadingPeople : PageLoading
{

    void Start()
    {
       
        //TypeMenuController.Ins.ChooseType = TypeMenuController.typeArr[0].typeId;
    }

    public override void GetPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        Log.Debug("请求的人员类型是XX：" + TypeMenuController.Ins.ChooseType);
        LzPeopleJsonParsing.Instance.HttpPageGetData(null, indexPage, pageNum, TypeMenuController.Ins.ChooseType,-1, callBak);
        
    }
    public override void GetSearchPageDataEvent(int indexPage, int pageNum,Action<List<object>, int> callBak)
    {
        LzPeopleJsonParsing.Instance.HttpPageSearchGetData(PeopleController.cardNum,indexPage,pageNum,TypeMenuController.Ins.ChooseType,callBak,PeopleController.userName);

    }

    public override void SetPageItemEvent(Transform item, object itemData, int itemIndex)
    {
        SaveFacePic(((UserInfo)itemData).faceBase64, @"c:\facepicture", ((UserInfo)itemData).idNum);
        item.GetComponent<PeopleInfo>().username = ((UserInfo)itemData).username;
        item.GetComponent<PeopleInfo>().salaryNum = ((UserInfo)itemData).salaryNum;
        item.GetComponent<PeopleInfo>().phoneNum = ((UserInfo)itemData).phoneNum;
        item.GetComponent<PeopleInfo>().unitName = ((UserInfo)itemData).unitName;
        item.GetComponent<PeopleInfo>().idNum = ((UserInfo)itemData).idNum;
        item.GetComponent<PeopleInfo>().address = ((UserInfo)itemData).address;
        item.GetComponent<PeopleInfo>().remarks = ((UserInfo)itemData).remarks;
        item.GetComponent<PeopleInfo>().base64 = ((UserInfo)itemData).faceBase64;
        item.GetChild(0).Find("username").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().username;
        item.GetChild(0).Find("salaryNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().salaryNum;
        item.GetChild(0).Find("phoneNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().phoneNum;
        item.GetChild(0).Find("unitName").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().unitName;
        item.GetChild(0).Find("idNum").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().idNum;
        item.GetChild(0).Find("address").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().address;
        item.GetChild(0).Find("remarks").GetComponent<Text>().text = item.GetComponent<PeopleInfo>().remarks;
        item.GetChild(2).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(3).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(1).GetComponent<Button>().gameObject.SetActive(true);

        foreach (var item1 in ((UserInfo)itemData).auth)
        {
            Log.Debug("日期:" + item1.authDate);

            if (item1.type == TypeMenuController.Ins.ChooseType)
            {
                item.GetComponent<PeopleInfo>().authDate = item1.authDate;
             
                item.GetComponent<PeopleInfo>().authTime = item1.authTime;
                item.GetComponent<PeopleInfo>().type = item1.type;
            }
        }
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
