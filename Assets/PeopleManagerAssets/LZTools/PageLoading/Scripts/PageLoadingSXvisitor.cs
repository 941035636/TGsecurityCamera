using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
using UnityTimer;
using System;
using System.IO;
using Newtonsoft.Json;
using static PeopleController;

public class PageLoadingSXvisitor : PageLoading
{


    public override void GetPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {

        SXVisitorJsonParsing.Instance.HttpPageGetData(indexPage, pageNum, callBak);

    }
    public override void GetSearchPageDataEvent(int indexPage, int pageNum, Action<List<object>, int> callBak)
    {
        SXVisitorJsonParsing.Instance.HttpPageSearchGetData(SXVisitorJsonParsing.cardNum, indexPage, pageNum, TypeMenuController.Ins.ChooseType, callBak, SXVisitorJsonParsing.userName, SXVisitorJsonParsing.devId);

    }

    UserAdd userAdd = new UserAdd();
    string authTime = string.Empty;
    string typeName = string.Empty;
    public override void SetPageItemEvent(Transform item, object itemData, int itemIndex)
    {


        item.GetChild(0).Find("username").GetComponent<Text>().text = ((UserAddFail)itemData).username;
        item.GetChild(0).Find("phoneNum").GetComponent<Text>().text = ((UserAddFail)itemData).idNum;
        item.GetChild(0).Find("unitName").GetComponent<Text>().text = ((UserAddFail)itemData).unitName;
        item.GetChild(0).Find("idNum").GetComponent<Text>().text = ((UserAddFail)itemData).devName;
        item.GetChild(0).Find("address").GetComponent<Text>().text = ((UserAddFail)itemData).reason;
        item.GetComponent<PeopleInfo>().authTime = ((UserAddFail)itemData).time;
        item.GetComponent<PeopleInfo>().typeName = ((UserAddFail)itemData).unitName;
        item.GetChild(3).GetComponent<Button>().gameObject.SetActive(true);
        item.GetChild(2).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(1).GetComponent<Button>().gameObject.SetActive(false);
        item.GetChild(3).GetComponent<Button>().onClick.RemoveAllListeners();
        item.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            authTime = item.GetComponent<PeopleInfo>().authTime;
            typeName= item.GetComponent<PeopleInfo>().typeName;
            StartCoroutine(RequesDevtInfo(((UserAddFail)itemData).devId, ((UserAddFail)itemData).idNum));

        });
    }

    IEnumerator RequesDevtInfo(int devid, string idNum)
    {

        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/cam/info?camId=" + devid, GetDevInfo, false, false, false, null);
        yield return new WaitForSeconds(0.5f);
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/user/client/info?&pageNum=1&pageSize=1&idNum=" + idNum, GetPersonInfo, false, false, false, null);
        yield return new WaitForSeconds(0.5f);
        if (!string.IsNullOrEmpty(userAdd.idNum)  && !string.IsNullOrEmpty(userAdd.authDate)&&userAdd.toAddList.Count!=0)
        {
            string str = JsonConvert.SerializeObject(userAdd);
            Log.Error("重新下发的人员信息:"+str);
            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/mqtt/push/info", ModifyOnePersonAuth, true, true, false, str);
        }
        else
        {
            if (string.IsNullOrEmpty(userAdd.idNum))
                GameStart.Instance.ShowTip("重新下发人员信息不合法:身份证号为空");
            //else if (string.IsNullOrEmpty(userAdd.faceBase64))
            //    GameStart.Instance.ShowTip("重新下发人员信息不合法:人脸照片为空");
            else if (string.IsNullOrEmpty(userAdd.authDate))
                GameStart.Instance.ShowTip("重新下发人员信息不合法:权限时间为空");
            else if (userAdd.toAddList.Count==0)
                GameStart.Instance.ShowTip("重新下发人员信息不合法:门禁设备为空");

        }


    }
    private void ModifyOnePersonAuth(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value) && args.Value.Contains("200"))
        {
            GameStart.Instance.ShowTip("重新下发该人员权限信息成功!");

        }
    }
    private void GetDevInfo(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            //Debug.LogError("请求的设备信息:" + args.Value);
            DevInfoArr dev = JsonConvert.DeserializeObject<DevInfoArr>(args.Value);
            if (dev.records.Count > 0)
            {
                //Debug.LogError("设备名:" + dev.records[0].cameraname);
                userAdd.toAddList.Add(dev.records[0]);
            }
            else
            {
                Debug.LogError("设备容器为空");
            }

        }


    }
    private void GetPersonInfo(HttpCallBackArgs args)
    {

        if (!string.IsNullOrEmpty(args.Value))
        {
            //Debug.LogError("请求的人员信息:" + args.Value);
            UserInfoArrPeople user = JsonConvert.DeserializeObject<UserInfoArrPeople>(args.Value);
            if (user.records.Count > 0)
            {
                //Debug.LogError("人名:" + user.records[0].faceBase64);
                userAdd.username = user.records[0].username;
                userAdd.faceBase64 = user.records[0].faceBase64;
                userAdd.idNum = user.records[0].idNum;
                userAdd.is2issued = "";
                userAdd.salaryNum = user.records[0].salaryNum;
                if (!string.IsNullOrEmpty(authTime)) 
                {
                    //2023-07-12T00:00:00-2023-07-12T23:59:59
                    userAdd.authDate = authTime.Split('T')[0] + "," + authTime.Split('T')[1].Split('-')[1]+"-" + authTime.Split('T')[1].Split('-')[2]+"-"+ authTime.Split('T')[1].Split('-')[3];
                }
                   
                userAdd.authTime = "";
                userAdd.type = 888;
                userAdd.typeName = typeName;
            }
            else
                Debug.LogError("人员容器为空");
        }
    }

    [Serializable]
    public class User
    {

        public string id;
        public string username;                               //姓名1
        public string salaryNum;                             //工资编号1
        public string phoneNum;                             //手机号1
        public string unitName;                            //单位名称1
        public string idNum;                              //身份证号 1
        public string address;                            //家庭住址1
        public string remarks;                         //备注1
        public string faceBase64;                  //人脸信息
        public List<Auth> auth;


    }
}
