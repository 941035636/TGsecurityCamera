using Newtonsoft.Json;
using SuperTreeView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityTimer;
using zFramework.Media;
using ZTools;
using static ManManagentUi;
using static TypeMenuController;
using static zFramework.Media.CameraGroupConfiguration;



[Serializable]
public class Devs
{
    public int id;  //设备id
    public string devName = string.Empty;
}
[Serializable]
public class Areas
{
    public string areaName = string.Empty;
    public int id;  //区域id
    public List<Devs> devList = new List<Devs>();

}
[Serializable]
public class TypeArea
{
    public int typeId;  //人员类型id
    public string typeName = string.Empty;
    public List<Areas> areaList = new List<Areas>();
}
[Serializable]
public class Types
{
    public List<TypeArea> typeList = new List<TypeArea>();

}








public class PeopleController : SingletonManager<PeopleController>
{
    public bool IsServerQuestData = true;
    public GameObject ChangePersonPage;
    public Text XingMing;
    public Text BianHao;
    public Text ShouJiHao;
    public Text DanWei;
    public Text ShenFenZheng;
    public Text ZhuZhi;
    public Text BeiZhu;
    public TextMeshProUGUI LeiXing;
    public Text StartDate;
    public Text EndDate;
    public Text StartTime;
    public Text EndTime;
    public Button XiuGaiBtn;
    public GameObject QuYu;
    public TreeView QuyuTreeManger;
    public RawImage Face;
    public Text NumPage;
    public Button SendUserToDoorBtn;
    public Button AuthManagerBtn;//权限管理按钮
    public Toggle ShaiXuanTog;//筛选按钮
    public Button backbtn;//筛选页面返回按钮
    public Transform ShaiXuanPanel;
    public GameObject AreaScroll;//区域选择界面
    public Button saveAuthBtn;//保存修改区域权限
    public Button backBtn;//返回
    public Button closeBtn;
    public Transform SendPanel;

    public Button PiliangSendBtn;
    public Button ShowDateZCalendarBtn;
    public GameObject _zcalendar;
    public TreeView treemanager;

    public TMP_InputField nameInput;
    public TMP_InputField numInput;
    public Toggle xiafatog;

    private bool IsInitAuthManager;
    private Transform DoorEquipManager;
    AllPersontypeAuth allPersontypeAuth;

    public static string userName=string.Empty;
    public static string cardNum=string.Empty;

    void callback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Error(args.Value);
        }
    
    
    }

    void Start()
    {

        xiafatog.onValueChanged.AddListener((ison) => {


            HttpNetManager.GetInstance().SendDataStr(GameStart.ApiUrl("/api/door/tg/door/dev/user/issued?devId=151&issued="),callback,false,false,false);
        
        
        
        });


        DoorEquipManager = AreaScroll.transform.Find("Viewport/Content/DoorEquipManager");
        backbtn = ShaiXuanPanel.Find("backbtn").GetComponent<Button>();
        ShowDateZCalendarBtn.onClick.AddListener(ShowDateZCalendar);

        PrepareCustomIssueEntry();

        //分组下发人员信息到具体门禁设备的调用
        SendUserToDoorBtn.onClick.AddListener(() =>
        {
            if (LoginManager.Ins.IsPeopleManager)
            {
                PersonIssuePanelController.Ensure(this).Show();
            }
            else
            {
                GameStart.Instance.ShowTip("当前用户没有人员管理权限");
            }

        });
        PiliangSendBtn.onClick.AddListener(() =>
        {
            if (LoginManager.Ins.IsPeopleManager)
            {

            }
            else
            {
                GameStart.Instance.ShowTip("当前用户没有人员管理权限");
            }

        });

        AuthManagerBtn.onClick.RemoveAllListeners();
         Log.Debug("人员类型:" + TypeMenuController.Ins.ChooseType);




        AuthManagerBtn.onClick.AddListener(() =>
        {
            if (LoginManager.Ins.IsPeopleManager)//权限限定
            {
                if (IsServerQuestData)
                {
                    //先请求所有的区域设备对应关系再根据当前选中的人员类型对应哪些区域设备做筛选
                    AreaScroll.SetActive(true);
                    if (!IsInitAuthManager)
                    {
                        InitDoorGroup();
                    }
                    IsInitAuthManager = true;
                }




            }
            else
            {
                GameStart.Instance.ShowTip("当前用户没有人员管理权限");
            }







        });
        saveAuthBtn.onClick.AddListener(() =>
        {
       
   
            Log.Debug(allDoorTreeViewItemList.Count);
            List<GetAreaGroupDevs> areaList = ProductCameraTreeGroup(allDoorTreeViewItemList);
            if (TypeMenuController.Ins.ShowtypesAreadevs.typeList.Exists(t => t.typeId == TypeMenuController.Ins.ChooseType))
            {
                userTypeAreaDev userTypeAreaDev = TypeMenuController.Ins.ShowtypesAreadevs.typeList.Find(t => t.typeId == TypeMenuController.Ins.ChooseType);
                userTypeAreaDev.areaList = areaList;
                ResponseGetuserTypesAreasDev response = new ResponseGetuserTypesAreasDev();
                response.typeList.Add(userTypeAreaDev);
                string str = JsonUtility.ToJson(response, true);
                Log.Debug("上传给服务器的人员类型对应区域权限信息:" + str);
                HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/tg/type/area/dev/perm/tree", PeopletypebandAreasCallback, true, true, true, str);
            }


        });


        backBtn.onClick.AddListener(() =>
        {
            if (treemanager.transform.childCount != 0)
            {
                for (int i = treemanager.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (treemanager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && treemanager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManagerUserManager.GetInstance().OnDeleteBtnClicked(treemanager.transform.GetChild(temp).GetComponent<TreeViewItem>());

                }

            }
            AreaScroll.SetActive(false);
            IsInitAuthManager = false;
            //for (int i = 0; i < DoorEquipManager.childCount; i++)
            //{
            //    GameObject.Destroy(DoorEquipManager.GetChild(i).gameObject);
            //}


        });
        SendPanel.Find("BG/closebtn").GetComponent<Button>().onClick.AddListener(() =>
        {

            SendPanel.gameObject.SetActive(false);
        });

        nameInput.onEndEdit.AddListener((txt)=> 
        {
            if (!string.IsNullOrEmpty(txt))
                userName = txt;
            else
                userName = "";
        
        });

        numInput.onEndEdit.AddListener((txt) =>
        {
            //Regex reg = new Regex("(\\d{15})|(^\\d{18})|(\\d{17}(\\d|X|x)$)");
            Regex reg = new Regex("(^\\d{15}$)|(^\\d{18}$)|(^\\d{17}(\\d|X|x)$)");
            //Regex reg = new Regex("/^[1-9]\\d{5}(18|19|20|(3\\d))\\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\\d{3}[0-9Xx]$/;\r\n ");
            if (reg.IsMatch(txt))
            {
                numInput.text = txt;
                cardNum = txt;
            }
            else
            {
                if (numInput.text == "")
                {
                    numInput.text = "";

                }
                else
                {
                    numInput.text = txt.Substring(0, txt.Length - 1);
                }
                cardNum = "";
                numInput.text = "";
                GameStart.Instance.ShowTip("身份证号不合法！");


            }


        });

        ShaiXuanTog.onValueChanged.AddListener((ison)=> {
            ShaiXuanPanel.gameObject.SetActive(true);
        
        });
        backbtn.onClick.AddListener(()=> {

            ShaiXuanPanel.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// 原分组下发按钮在预制体中默认隐藏且只有图标。运行时将其显示为明确的文字入口。
    /// </summary>
    private void PrepareCustomIssueEntry()
    {
        if (SendUserToDoorBtn == null) return;

        GameObject entry = SendUserToDoorBtn.gameObject;
        entry.name = "CustomPersonIssueButton";
        entry.SetActive(true);

        // 原按钮图片本身包含旧文字和图标，清空 Sprite，避免与新标签重叠。
        Image background = entry.GetComponent<Image>();
        if (background != null)
        {
            background.sprite = null;
            background.type = Image.Type.Simple;
            background.color = new Color(0.08f, 0.34f, 0.52f, 1f);
        }
        SendUserToDoorBtn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = SendUserToDoorBtn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.82f, 0.94f, 1f, 1f);
        colors.pressedColor = new Color(0.68f, 0.84f, 0.92f, 1f);
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);
        SendUserToDoorBtn.colors = colors;

        RectTransform rect = entry.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(650f, -30f);
            rect.sizeDelta = new Vector2(150f, 36f);
        }

        Text label = entry.GetComponentInChildren<Text>(true);
        if (label == null)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(entry.transform, false);
            label = labelObject.GetComponent<Text>();
        }

        label.font = NumPage != null && NumPage.font != null
            ? NumPage.font
            : Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.fontSize = 15;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.text = "自定义下发";
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }
    //修改人员类型后返回的需要下发哪些设备，删除哪些设备数据结构
    [Serializable]
    public class responseModifyUsertype
    {
        public string msg = string.Empty;
        public int code;
        public List<data> data;

    }
    [Serializable]
    public class data
    {
        public int typeId;//人员类型
        public List<int> userIdNumList = new List<int>();//要操作的人员id
        public List<int> delDevIdList = new List<int>();//要在哪些设备上删除
        public List<int> addDevIdList = new List<int>();//要在哪些设备上添加


    }

    public List<int> OperatePeopleIdList = new List<int>();//要操作的人员ID容器


    //修改人员类型对应的区域设备信息上传服务器回调
    private void PeopletypebandAreasCallback(HttpCallBackArgs args)
    {

        OperatePeopleIdList.Clear();
        if (!string.IsNullOrEmpty(args.Value))
        {
            Log.Debug("收到修改人员类型对应的区域设备信息上传服务器回调:" + args.Value);
            try
            {
                responseModifyUsertype response = JsonUtility.FromJson<responseModifyUsertype>(args.Value);

                if (response.code == 200)
                {
                   
                    GameStart.Instance.ShowTip("修改人员类型权限成功!");


                    if (response.data[0].delDevIdList.Count == 0 && response.data[0].addDevIdList.Count == 0)
                    {
                        //既没有要删除的，也没有要添加的

                    }
                    else
                    {
                        //下面这个地方不太合理，一个一个的再去请求设备信息太耗时，最好时带着所有的设备id一块去请求返回整个设备信息列表

               
                    }
                }
                else 
                {
                    GameStart.Instance.ShowTip(response.msg);
                }


            }
            catch (Exception e)
            {
                //GameStart.Instance.ShowTip(e.ToString());
                Log.Error(e.ToString());
            }


        }
        //获取人员类型对应的区域权限信息
        //HttpNetManager.GetInstance().SendDataObj("http://" + "192.168.110.2:7711" + "/api/perm/type/area/dev/info?typeId=" + 1, GetPeopletypesAreadevsCallback, false, false);
    }
    [Serializable]
    public class Nvrs
    {
        public List<NVRInformation> records = new List<NVRInformation>();
    }

  

 




    public Dictionary<int, string> AreaNameBandIdDic = new Dictionary<int, string>(); //区域名绑定区域ID
    public Dictionary<int, string> DeviceNameBandIdDic = new Dictionary<int, string>();//设备名绑定设备ID

 

   

  


   


    public AreaGroupDevs DoorAreaGroupDevs;
    [Serializable]
    public class AreaGroupDevs
    {
        public List<GetAreaGroupDevs> records = new List<GetAreaGroupDevs>();

    }

    /// <summary>
    /// 打开单个人的信息界面时展示其区域权限信息
    /// </summary>
    public void InitPersonDoorGroup()
    {
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=1", GetPersonDoorGroupDevs, false, false, false);

    }
    public void GetPersonDoorGroupDevs(HttpCallBackArgs args)
    {

        if (!string.IsNullOrEmpty(args.Value))
        {
            DoorAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevs>(args.Value);
            for (int i = 0; i < QuYu.transform.Find("peopleAreaScroll/Viewport/Content/DoorEquipManager").childCount; i++)
            {
                GameObject.Destroy(QuYu.transform.Find("peopleAreaScroll/Viewport/Content/DoorEquipManager").GetChild(i).gameObject);
            }
            for (int i = 0; i < DoorAreaGroupDevs.records.Count; i++)
            {
                int index = i;
                ObjectManager.Instance.InstantiateObjectAsync(ConStr.DOORAREAFILETOG, PersonDoorgroupVcallback, LoadResPriority.RES_MIDDLE, false, DoorAreaGroupDevs.records[index].areaName, DoorAreaGroupDevs.records[index].devList, DoorAreaGroupDevs.records[index].id);
            }

        }
    }
    private void PersonDoorgroupVcallback(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
        GameObject groupvsion = obj as GameObject;
        groupvsion.transform.SetParent(QuYu.transform.Find("peopleAreaScroll/Viewport/Content/DoorEquipManager"));
        List<NVRInformation> grouplist = param2 as List<NVRInformation>;
        resetPrefab(groupvsion);
        groupvsion.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        groupvsion.GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                groupvsion.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/分组/文件夹-2");
                groupvsion.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().SetNativeSize();

            }
            else
            {
                groupvsion.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/设备管理/监控门禁/分组/文件夹-1");
                groupvsion.transform.Find("Background").GetComponent<UnityEngine.UI.Image>().SetNativeSize();

            }
        });
        Transform group = groupvsion.transform.GetChild(0);
        group.Find("DoornameTxt").GetComponent<TextMeshProUGUI>().text = param1 as string;
        group.Find("contog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        group.Find("contog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                group.Find("contog/Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放");
                group.Find("contog/Background").GetComponent<UnityEngine.UI.Image>().SetNativeSize();

            }
            else
            {
                group.Find("contog/Background").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收");
                group.Find("contog/Background").GetComponent<UnityEngine.UI.Image>().SetNativeSize();

            }
            for (int i = 1; i < groupvsion.transform.childCount; i++)
            {
                groupvsion.transform.GetChild(i).gameObject.SetActive(ison);
            }
            SetContentSizeActive();
        });


        group.Find("choiceTog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        group.Find("choiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {


            for (int i = 1; i < groupvsion.transform.childCount; i++)
            {
                int index = i;
                groupvsion.transform.GetChild(index).Find("Background/CchoiceTog").GetComponent<Toggle>().isOn = ison;
            }
            //添加广播监听


        });


        List<Areas> areadataList = new List<Areas>();
        Areas doorAreasdata = new Areas(); //初始化区域存储模块，区域要跟人员类型挂钩

        Log.Debug("人员类型:" + TypeMenuController.Ins.ChooseType);

        //判断人员类型区域列表中是否存在该区域存在取出不存在添加
        if (TypeMenuController.Ins.ShowtypesAreadevs.typeList.Exists(t => t.typeId == TypeMenuController.Ins.ChooseType))
        {
            //areadataList = TypeMenuController.Ins.typesAreadevs.typeList.Find(t => t.typeId == TypeMenuController.Ins.ChooseType).areaList;
            if (areadataList.Exists(t => t.id == (int)param3))
            {
                doorAreasdata = areadataList.Find(t => t.id == (int)param3);
            }
            else
            {
                doorAreasdata.areaName = param1 as string;
                doorAreasdata.id = (int)param3;
                areadataList.Add(doorAreasdata);
            }
        }




        foreach (var item in grouplist)
        {

            GameObject equipnameObj = ObjectManager.Instance.InstantiateObject(ConStr.DOOREQUIPNAME);
            equipnameObj.transform.SetParent(groupvsion.transform);
            resetPrefab(equipnameObj);
            equipnameObj.transform.Find("equipnameTxt").GetComponent<TextMeshProUGUI>().text = item.cameraname;

            //判断该区域列表中是否已经存在该设备，存在自动设为true
            if (doorAreasdata.devList.Exists(t => t.id == item.id))
            {
                equipnameObj.transform.Find("Background/CchoiceTog").GetComponent<Toggle>().isOn = true;
            }

            equipnameObj.transform.Find("Background/CchoiceTog").GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            equipnameObj.transform.Find("Background/CchoiceTog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
            {
                if (ison)
                {

                    Log.Debug("勾选了:" + item.cameraname + "分组:" + param1 as string);
                    //判断区域列表中是否已经存在该设备，不存在添加
                    if (!doorAreasdata.devList.Exists(t => t.id == item.id))
                    {
                        Devs devnvr = new Devs();
                        devnvr.devName = item.cameraname;
                        devnvr.id = item.id;
                        doorAreasdata.devList.Add(devnvr);
                    }


                }
                else
                {
                    Log.Debug("取消勾选了:" + item.cameraname);

                    //判断区域列表中是否存在该设备,存在删除
                    if (doorAreasdata.devList.Exists(t => t.id == item.id))
                    {
                        Devs devnvr = doorAreasdata.devList.Find(t => t.id == item.id);
                        doorAreasdata.devList.Remove(devnvr);
                    }
                }

            });
        }
    }

    /// <summary>
    /// 请求服务器实例化门禁设备分组信息
    /// </summary>

    //private userTypeAreaDev areaDev;
    public void InitDoorGroup()
    {

        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=1", GetDoorGroupDevs, false, false, false);
        //if (TypeMenuController.Ins.ShowtypesAreadevs.typeList.Exists(t => t.typeId == TypeMenuController.Ins.ChooseType))
        //{
        //    areaDev = TypeMenuController.Ins.ShowtypesAreadevs.typeList.Find(t => t.typeId == TypeMenuController.Ins.ChooseType);

        //}

    }
    /// <summary>
    /// 获取门禁区域分组设备信息回调
    /// </summary>
    /// <param name="args"></param>
    AreaDevsData areaDevsData;
    List<OutlineInfo> outlineInfoList = new List<OutlineInfo>();
    public void GetDoorGroupDevs(HttpCallBackArgs args)
    {
        outlineInfoList.Clear();
        Log.Debug("从服务器接收到的区域分组设备信息：" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {

            areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
            Log.Debug("门禁分组个数:" + areaDevsData.records.Count);
            //清除
            if (treemanager.transform.childCount != 0)
            {
                for (int i = treemanager.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (treemanager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && treemanager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManager.GetInstance().OnDeleteBtnClicked(treemanager.transform.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }


            for (int i = 0; i < areaDevsData.records.Count; i++)
            {
                int index = i;
                //ObjectManager.Instance.InstantiateObjectAsync(ConStr.CAMERAGROUP, cameraVcallback, LoadResPriority.RES_MIDDLE, false, CameraAreaGroupDevs.records[index].areaName, CameraAreaGroupDevs.records[index].devList, CameraAreaGroupDevs.records[index].id);
                OutlineInfo outline = new OutlineInfo();
                outline.OutlineId = areaDevsData.records[index].id.ToString();
                outline.ParentId = areaDevsData.records[index].parentId.ToString();
                outline.OutlineName = areaDevsData.records[index].areaName;
                outline.Children = areaDevsData.records[index].children;
                outline.DevList = areaDevsData.records[index].devList;
                outlineInfoList.Add(outline);//收集区域分组信息
                for (int j = 0; j < outline.DevList.Count; j++)
                {
                    OutlineInfo outlinedev = new OutlineInfo();
                    outlinedev.OutlineId = outline.DevList[j].id.ToString();
                    outlinedev.ParentId = areaDevsData.records[index].id.ToString();
                    outlinedev.OutlineName = outline.DevList[j].cameraname;
                    outlinedev.Children = null;
                    outlinedev.DevList = null;
                    outlinedev.Level = "equip";
                    outlinedev.NVr = outline.DevList[j];
                    outlineInfoList.Add(outlinedev);
                }
            }

            InitDoorGroup(outlineInfoList);


        }
    }
    List<TreeViewItem> allDoorTreeViewItemList = new List<TreeViewItem>();



    //把TreeViewItem集合拼成树级结构  List<GetAreaGroupDevs>
    private List<GetAreaGroupDevs> ProductCameraTreeGroup(List<TreeViewItem> allDoorTreeViewItem, int parentId = 0)
    {
        var ItemList = allDoorTreeViewItem.Where(x =>
        {
            return parentId.Equals(int.Parse(x.GetComponent<ItemScript>().parentId));
        });

        List<GetAreaGroupDevs> Newgroups = new List<GetAreaGroupDevs>();
        foreach (var item in ItemList)
        {
            var view = new GetAreaGroupDevs();
            if (!string.IsNullOrEmpty(item.GetComponent<ItemScript>().id))
                view.id = int.Parse(item.GetComponent<ItemScript>().id);
            view.level = item.GetComponent<ItemScript>().level;
            view.parentId = int.Parse(item.GetComponent<ItemScript>().parentId);

            view.devList = item.GetComponent<ItemScript>().devList;
            if (int.Parse(item.GetComponent<ItemScript>().id) != 0 && item.GetComponent<ItemScript>().children != null)
              view.children = ProductCameraTreeGroup(allDoorTreeViewItem, int.Parse(item.GetComponent<ItemScript>().id));
            view.areaType = item.GetComponent<ItemScript>().type;
            view.areaName = item.GetComponent<ItemScript>().labelText.text;
            Newgroups.Add(view);


        }
        return Newgroups;


    }

    public void OpenChangePersonPage(string username, string salaryNum
   , string phoneNum, string unitName, string idNum, string address, string remarks, int type, string authDate, string authTime,string base64)
    {
        ChangePersonPage.SetActive(true);
        XingMing.text = username;
        BianHao.text = salaryNum;
        ShouJiHao.text = phoneNum;
        DanWei.text = unitName;
        ShenFenZheng.text = idNum;
        ZhuZhi.text = address;
        BeiZhu.text = remarks;
        Log.Error("时效性:" + authTime + "人员类型:" + type);
        string typename = string.Empty;
        for (int i = 0; i < TypeMenuController.typeArr.Count; i++)
        {
            if (TypeMenuController.typeArr[i].typeId == type) 
            {
                LeiXing.text = TypeMenuController.typeArr[i].typeName;
                typename= TypeMenuController.typeArr[i].typeName;
            }
        }
      

        if (authDate.Contains(","))
        {
            StartDate.text = authDate.Split(',')[0].ToString();
            EndDate.text = authDate.Split(',')[1].ToString();
            StartTime.text = authTime.Split(',')[0].ToString();
            if (authTime.Contains(","))
                EndTime.text = authTime.Split(',')[1].ToString();
        }
        string path = @"C:\facepicture";
        //判断人脸照片路径是否为null
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string filepath = Path.Combine(path, idNum) + ".jpg";
        Log.Debug("人脸路径：" + filepath);

        StartCoroutine(DownSprite(filepath));



        //清除
        if (QuyuTreeManger.transform.childCount != 0)
        {
            for (int i = QuyuTreeManger.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (QuyuTreeManger.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && QuyuTreeManger.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManager.GetInstance().OnDeleteBtnClicked(QuyuTreeManger.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }



        //InitPersonDoorGroup();
        //获取个区域权限信息
        HttpNetManager.GetInstance().SendDataStr("http://"+GameStart.IP+ "/api/door/tg/door/user/dev/issued?idNum="+idNum, GetOnePersonHasAreaAuth, false,false,false,"");



        //Timer.Register(0.2f, () =>
        //{
        //    InitSinglePersonAreaGroup(TypeMenuController.Ins.markoutlineInfoList);


        //}, isLooped: false);
      
        //给服务器发送重新下发单个人的权限
        XiuGaiBtn.onClick.RemoveAllListeners();
        XiuGaiBtn.onClick.AddListener(() =>
        {
            UserAdd user = new UserAdd();
            user.username = username;
            user.faceBase64 = base64;
            user.idNum = idNum;
            user.is2issued = "";
            user.salaryNum = salaryNum;
            user.authDate = authDate;
            user.authTime = authTime;
            user.type = type;
            user.typeName = typename;
            for (int i = 0; i < TypeMenuController.Ins.markoutlineInfoList.Count; i++)
            {
                if (TypeMenuController.Ins.markoutlineInfoList[i].DevList!=null&&TypeMenuController.Ins.markoutlineInfoList[i].DevList.Count > 0) 
                {
                    for (int j = 0; j < TypeMenuController.Ins.markoutlineInfoList[i].DevList.Count; j++)
                    {
                        user.toAddList.Add(TypeMenuController.Ins.markoutlineInfoList[i].DevList[j]);
                    }
                }
            }
            string str = JsonConvert.SerializeObject(user);
          
            //user.faceBase64 = "abc";
            Log.Error("重新下发人员信息Json:" + JsonUtility.ToJson(user,true));
            HttpNetManager.GetInstance().SendDataStr("http://"+GameStart.IP+ "/api/personnel/tg/mqtt/push/info", ModifyOnePersonAuth, true,true,false, str);


        });
    }
    private void GetOnePersonHasAreaAuth(HttpCallBackArgs args) 
    {
        if (!string.IsNullOrEmpty( args.Value))
        {
            Log.Error("个人区域权限信息："+args.Value);
            try
            {
                PersonAuth personAuth = JsonConvert.DeserializeObject<PersonAuth>(args.Value);
                if (personAuth.data != null) 
                {
                    Dictionary<string, int> dic = DeserializeStringToDictionary<string, int>(personAuth.data.ToString());
                    Log.Error("SSSSSSSS:" + dic.Count);
                    InitSinglePersonAreaGroup(TypeMenuController.Ins.markoutlineInfoList, dic);
                }
              
            }
            catch (Exception e)
            {
                Log.Error("个人区域权限信息反序列化报错：" + e.ToString());
               
            }
          
        }
    
    }
    //获取单个人的区域权限数据结构
    [Serializable]
    public class PersonAuth 
    {
        public string msg = string.Empty;
        public int code;
        public object data ;
    
    
    }


    //修改单个人的信息
    [Serializable]
    public class UserAdd
    {
        public string username = string.Empty;
        public string faceBase64 = string.Empty;
        public string idNum = string.Empty;
        public string is2issued = string.Empty;
        public string salaryNum = string.Empty;
        public string authDate = string.Empty;
        public string authTime = string.Empty;
        public int type=-1;
        public string typeName = string.Empty;
        public List<NVRInformation> toAddList = new List<NVRInformation>();
        public List<NVRInformation> toDelList = new List<NVRInformation>();



    }
    public static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
    {
        if (string.IsNullOrEmpty(jsonStr))
            return new Dictionary<TKey, TValue>();
        Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);
        return jsonDict;
    }

    private void ModifyOnePersonAuth(HttpCallBackArgs args) 
    {
        if (!string.IsNullOrEmpty( args.Value)&&args.Value.Contains("200"))
        {
            GameStart.Instance.ShowTip("重新下发该人员权限信息成功!");
        
        }
    }

    /// <summary>
    /// 初始化单个人员界面对应的区域权限信息
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitSinglePersonAreaGroup(List<OutlineInfo> outlineInfoList)
    {

        Log.Error("个人区域权限信息:"+outlineInfoList.Count);

        allDoorTreeViewItemList = new List<TreeViewItem>();
     
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {
                item1 = QuyuTreeManger.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                item1.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == item1.GetComponent<ItemScript>().id) != null)//该类型已经存在的区域设备做标记
                {
                    item1.GetComponent<ItemScript>().choiceTog.isOn = true;
                }
                allDoorTreeViewItemList.Add(item1);
              
            }
            else
            {
                for (int j = 0; j < allDoorTreeViewItemList.Count; j++)
                {
                    if (allDoorTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {
                        if (string.Equals(outlineInfoList[temp].Level, "equip"))//设备
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");

                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            //Log.Error("设备："+outlineInfoList[temp].OutlineName + "的ID是:" + outlineInfoList[temp].OutlineId);
                            //Log.Error("设备"+outlineInfoList[temp].OutlineName+"的父级ID是:"+ outlineInfoList[temp].ParentId);
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }


                        }
                        else //分组
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                            //Log.Error("分组：" + outlineInfoList[temp].OutlineName + "的ID是:" + outlineInfoList[temp].OutlineId);
                            //Log.Error("分组：" + outlineInfoList[temp].OutlineName + "的父级ID是:" + outlineInfoList[temp].ParentId);
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
               
                            allDoorTreeViewItemList.Add(childItem);
                            if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                        }

                    }
                }
            }
        }


    }

    /// <summary>
    /// 初始化单个人员界面对应的区域权限信息
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitSinglePersonAreaGroup(List<OutlineInfo> outlineInfoList, Dictionary<string, int> PerauthDic)
    {

        //Log.Error("个人区域权限信息:" + outlineInfoList.Count);
        //Log.Error("个人权限列表容器大小:" + personAuth.data.Count);
        allDoorTreeViewItemList = new List<TreeViewItem>();

        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {
                item1 = QuyuTreeManger.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                item1.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == item1.GetComponent<ItemScript>().id) != null)//该类型已经存在的区域设备做标记
                {
                    item1.GetComponent<ItemScript>().choiceTog.isOn = true;
                }
                allDoorTreeViewItemList.Add(item1);

            }
            else
            {
                for (int j = 0; j < allDoorTreeViewItemList.Count; j++)
                {
                    if (allDoorTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {
                        if (string.Equals(outlineInfoList[temp].Level, "equip"))//设备
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");

                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            //Log.Error("设备："+outlineInfoList[temp].OutlineName + "的ID是:" + outlineInfoList[temp].OutlineId);
                            //Log.Error("设备"+outlineInfoList[temp].OutlineName+"的父级ID是:"+ outlineInfoList[temp].ParentId);
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            int state;
                            if (PerauthDic.TryGetValue(childItem.GetComponent<ItemScript>().id,out state)&&state==1)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                            //foreach (var item in PerauthDic)
                            //{

                            //    Log.Error("设备id:" + childItem.GetComponent<ItemScript>().nvr.id);
                            //    Log.Error("设备信息:" + item);
                            //    if (string.Equals(item.Key, childItem.GetComponent<ItemScript>().nvr.id))
                            //    {
                            //        childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            //    }
                            //}


                        }
                        else //分组
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                            //Log.Error("分组：" + outlineInfoList[temp].OutlineName + "的ID是:" + outlineInfoList[temp].OutlineId);
                            //Log.Error("分组：" + outlineInfoList[temp].OutlineName + "的父级ID是:" + outlineInfoList[temp].ParentId);
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;

                            allDoorTreeViewItemList.Add(childItem);
                            if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                        }

                    }
                }
            }
        }


    }


    /// <summary>
    ///  生成门禁树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    /// 
    List<TreeViewItem> EquipDoorTreeViewItemList = new List<TreeViewItem>();
    public void InitDoorGroup(List<OutlineInfo> outlineInfoList)
    {

        allDoorTreeViewItemList = new List<TreeViewItem>();
        EquipDoorTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {
                Log.Debug("ssss:" + treemanager);
                item1 = treemanager.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                item1.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");

                Log.Debug("allDoorTreeViewItemList添加:" + item1.GetComponent<ItemScript>().labelText.text);
                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();
                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                {
                    //全选该层级
                    if (ison)
                    {
                        Log.Debug("全选门禁层级");
                        Log.Debug("该层级id:" + outlineInfoList[temp].OutlineId);
                        for (int a = 0; a < allDoorTreeViewItemList.Count; a++)
                        {
                            if (allDoorTreeViewItemList[a].GetComponent<ItemScript>().parentId == outlineInfoList[temp].OutlineId)
                            {
                                allDoorTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = true;


                            }
                        }
                    }
                    else
                    {
                        Log.Debug("取消全选门禁层级");
                        for (int a = 0; a < allDoorTreeViewItemList.Count; a++)
                        {
                            if (allDoorTreeViewItemList[a].GetComponent<ItemScript>().parentId == outlineInfoList[temp].OutlineId)
                            {
                                allDoorTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = false;

                            }
                        }
                    }

                });
                if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == item1.GetComponent<ItemScript>().id) != null)//该类型已经存在的区域设备做标记
                {
                    item1.GetComponent<ItemScript>().choiceTog.isOn = true;
                }
                allDoorTreeViewItemList.Add(item1);
            }
            else
            {
                for (int j = 0; j < allDoorTreeViewItemList.Count; j++)
                {
                    if (allDoorTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {




                        if (string.Equals(outlineInfoList[temp].Level, "equip"))//设备
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");

                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            //childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();
                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {



                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allDoorTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && !viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                        Log.Debug("添加：" + nvr.cameraname);
                                    }
                                    else
                                    {
                                        viewItem.GetComponent<ItemScript>().devList = new List<NVRInformation>();
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                        Log.Debug("添加：" + nvr.cameraname);
                                    }



                                }
                                else
                                {


                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allDoorTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Remove(nvr);
                                    }

                                }

                            });

                            //if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            //{
                            //    Log.Error("设备打勾:" + childItem.GetComponent<ItemScript>().labelText.text);
                            //    childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            //}

                            for (int m = 0; m < TypeMenuController.Ins.markoutlineInfoList.Count; m++)
                            {
                                if (TypeMenuController.Ins.markoutlineInfoList[m].OutlineId==childItem.GetComponent<ItemScript>().id&& TypeMenuController.Ins.markoutlineInfoList[m].OutlineName==childItem.GetComponent<ItemScript>().labelText.text)
                                {
                                    Log.Error("设备打勾:" + childItem.GetComponent<ItemScript>().labelText.text);
                                    childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                                }
                            }

                            EquipDoorTreeViewItemList.Add(childItem);
                        }
                        else //分组
                        {
                            TreeViewItem childItem = allDoorTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            //childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/设备管理/添加/取消");
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            //childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();
                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {
                                    Log.Debug("选择");
                                    for (int a = 0; a < allDoorTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allDoorTreeViewItemList[a].GetComponent<ItemScript>();

                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {
                                            
                                            item.choiceTog.isOn = true;
                                      


                                        }
                                        else
                                        {
                                            for (int b = 0; b < EquipDoorTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == EquipDoorTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    EquipDoorTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = true;
                                                }
                                            }
                                        }



                                    }




                                }
                                else
                                {
                                    for (int a = 0; a < allDoorTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allDoorTreeViewItemList[a].GetComponent<ItemScript>();

                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {

                                            item.choiceTog.isOn = false;



                                        }
                                        else
                                        {
                                            for (int b = 0; b < EquipDoorTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == EquipDoorTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    EquipDoorTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = false;
                                                }
                                            }
                                        }



                                    }

                                }

                            });
                            allDoorTreeViewItemList.Add(childItem);
                            Log.Debug("allDoorTreeViewItemList添加:" + childItem.GetComponent<ItemScript>().labelText.text);
                            if (TypeMenuController.Ins.markoutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                        }

                    }
                }
            }
        }


    }

    //根据选择的监控设备的父类id查找到对应的层及目录并把该设备加到父级目录的devList中
    private void FindAreaGroupAddnvr(List<GetAreaGroupDevs> AreaGroupList, int parentId, NVRInformation nVR)
    {
        if (AreaGroupList != null)
        {
            for (int i = 0; i < AreaGroupList.Count; i++)
            {
                if (AreaGroupList[i].id == parentId)
                {
                    if (AreaGroupList[i].devList != null && !AreaGroupList[i].devList.Exists(t => t.id == nVR.id))
                        AreaGroupList[i].devList.Add(nVR);
                    else if (AreaGroupList[i].devList == null)
                    {
                        AreaGroupList[i].devList = new List<NVRInformation>();
                        AreaGroupList[i].devList.Add(nVR);
                    }
                    return;
                }
                else if (AreaGroupList[i].children.Count != 0)
                {
                    FindAreaGroupAddnvr(AreaGroupList[i].children, parentId, nVR);
                }

            }


        }





    }

    //根据选择的监控设备的父类id查找到对应的层及目录并把父级目录devList中存在的nvr移除
    private void FindAreaGroupDelnvr(List<GetAreaGroupDevs> AreaGroupList, int parentId, NVRInformation nVR)
    {
        if (AreaGroupList != null)
        {
            for (int i = 0; i < AreaGroupList.Count; i++)
            {
                if (AreaGroupList[i].id == parentId)
                {
                    Log.Debug("找到了：" + AreaGroupList[i].areaName);
                    if (AreaGroupList[i].devList != null && AreaGroupList[i].devList.Exists(t => t.id == nVR.id))
                        AreaGroupList[i].devList.Remove(AreaGroupList[i].devList.Find(t => t.id == nVR.id));
                    return;
                }
                else if (AreaGroupList[i].children.Count != 0)
                {
                    FindAreaGroupDelnvr(AreaGroupList[i].children, parentId, nVR);
                }

            }

        }




    }





    public void SetContentSizeActive()
    {
        StartCoroutine(HelpSet());//及时触发
    }

    IEnumerator HelpSet()
    {
        AreaScroll.transform.Find("Viewport/Content/DoorEquipManager").GetComponent<ContentSizeFitter>().enabled = false;
        yield return null;
        AreaScroll.transform.Find("Viewport/Content/DoorEquipManager").GetComponent<ContentSizeFitter>().enabled = true;
    }

    public void ShowDateZCalendar()
    {
        _zcalendar.GetComponent<ZCalendar>().Show();
    }


   
    
    
    
    
    IEnumerator DownSprite(string url)
    {
        var uri = new System.Uri(Path.Combine(url));
        UnityWebRequest www = UnityWebRequest.Get(uri);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        www.downloadHandler = texDl;

        yield return www.SendWebRequest();

        if (www.isHttpError || www.isNetworkError)
        {
            Log.Debug(www.error);
        }
        else
        {
            Texture2D tex = new Texture2D(1, 1);
            tex = texDl.texture;
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            //_faceSearchImage.sprite = sprite;
            Face.texture = tex;
        }
    }
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
    }
}
