using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
using UnityTimer;
using System;
using System.IO;

public class PageLoadingVisitor : PageLoading
{

    void Awake()
    {
        //Init();
        //Jump2Page(2);
        //LzVistorJsonParsing.Instance.HTTPGetUserType();
        //Log.Error("先请求人员类型");
        if (VisitorTypeMenuController.Ins.VistortypeArr != null && VisitorTypeMenuController.Ins.VistortypeArr.Count > 0)
            VisitorTypeMenuController.Ins.ChooseType = VisitorTypeMenuController.Ins.VistortypeArr[0].id;
        else
            VisitorTypeMenuController.Ins.ChooseType = -1;
    }

    public override void GetPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        Log.Debug("请求的人员类型是XX：" + VisitorTypeMenuController.Ins.ChooseType);
        if(VisitorTypeMenuController.Ins.ChooseType!=-1)
        LzVistorJsonParsing.Instance.HttpPageGetData(null, indexPage, pageNum, VisitorTypeMenuController.Ins.ChooseType, -1, callBak);

    }
    public override void GetSearchPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        LzVistorJsonParsing.Instance.HttpPageSearchGetData(VistorController.cardNum, indexPage, pageNum, VisitorTypeMenuController.Ins.ChooseType, callBak, VistorController.userName);

    }

    public override void SetPageItemEvent(Transform item, object itemData, int itemIndex)
    {
        SaveFacePic(((UserInfo)itemData).faceBase64, @"c:\facepicture", ((UserInfo)itemData).idNum);
        item.GetComponent<VistorInfo>().username = ((UserInfo)itemData).username;
        item.GetComponent<VistorInfo>().salaryNum = ((UserInfo)itemData).salaryNum;
        item.GetComponent<VistorInfo>().phoneNum = ((UserInfo)itemData).phoneNum;
        item.GetComponent<VistorInfo>().unitName = ((UserInfo)itemData).unitName;
        item.GetComponent<VistorInfo>().idNum = ((UserInfo)itemData).idNum;
        item.GetComponent<VistorInfo>().address = ((UserInfo)itemData).address;
        item.GetComponent<VistorInfo>().remarks = ((UserInfo)itemData).remarks;
        item.GetComponent<VistorInfo>().base64 = ((UserInfo)itemData).faceBase64;
        item.GetChild(0).Find("username").GetComponent<Text>().text = item.GetComponent<VistorInfo>().username;
        item.GetChild(0).Find("salaryNum").GetComponent<Text>().text = item.GetComponent<VistorInfo>().salaryNum;
        item.GetChild(0).Find("phoneNum").GetComponent<Text>().text = item.GetComponent<VistorInfo>().phoneNum;
        item.GetChild(0).Find("unitName").GetComponent<Text>().text = item.GetComponent<VistorInfo>().unitName;
        item.GetChild(0).Find("idNum").GetComponent<Text>().text = item.GetComponent<VistorInfo>().idNum;
        item.GetChild(0).Find("address").GetComponent<Text>().text = item.GetComponent<VistorInfo>().address;
        item.GetChild(0).Find("remarks").GetComponent<Text>().text = item.GetComponent<VistorInfo>().remarks;
        item.GetChild(2).GetComponent<Button>().gameObject.SetActive(false);
        //item.GetChild(3).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(1).GetComponent<Button>().gameObject.SetActive(true);

        foreach (var item1 in ((UserInfo)itemData).auth)
        {
            Log.Debug("日期:" + item1.authDate);

            if (item1.type == VisitorTypeMenuController.Ins.ChooseType)
            {
                item.GetComponent<VistorInfo>().authDate = item1.authDate;

                item.GetComponent<VistorInfo>().authTime = item1.authTime;
                item.GetComponent<VistorInfo>().type = item1.type;
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
