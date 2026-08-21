
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using ZTools;
using Newtonsoft.Json;
using TMPro;
using zFramework.Media;
using System;
using static ManManagentUi;
using UMP.Services.Helpers;

public class VisitorTypeMenuController : SingletonManager<VisitorTypeMenuController>
{
    public int ChooseType=-1;
    private Toggle[] toggles;
    void Start()
    {
      

        //BindToggle();

        //StartCoroutine(Delay());

        HTTPGetUserType();

   
    }


    /// <summary>
    /// http请求人员类型
    /// </summary>
    public void HTTPGetUserType()
    {

        HttpNetManager.GetInstance().SendDataStr("http://"+GameStart.IP+"/api/personnel/tg/user/type", PeopletypeshttpCallback, false, false, true);
    }
    //请求下来所有的人员类型都与区域List进行绑定
   public  ResponseGetuserTypesAreasDev ShowtypesAreadevs = new ResponseGetuserTypesAreasDev(); //展示用
    List<GetAreaGroupDevs> DoorAreaModelList = new List<GetAreaGroupDevs>();//门禁区域层级架构模板
    public  List<OutlineInfo> markoutlineInfoList = new List<OutlineInfo>();  //存放已经选择的outlineinfo
    /// <summary>
    /// 请求人员类型回调
    /// </summary>
    /// <param name="args"></param>
   public  List<UserType> VistortypeArr = new List<UserType>();
    public void PeopletypeshttpCallback(HttpCallBackArgs args)
    {

        Log.Debug("请求人员类型回传信息:"+args.Value);
        if (!string.IsNullOrEmpty(args.Value)) 
        {
            List<UserType> typeArr = JsonConvert.DeserializeObject<List<UserType>>(args.Value);
            //PersontypeAuthConfigs.GetInstance().SaveUserTypeonfiguration_outside(typeArr);
            VistortypeArr.Clear();
            for (int i = 0; i < typeArr.Count; i++)
            {
                int index = i;
                if (typeArr[index].typeName.Contains("访客"))
                {
                    UserType type = typeArr[index];
                    ObjectManager.Instance.InstantiateObjectAsync(ConStr.USERTYPETOG, UserTypecallback, LoadResPriority.RES_MIDDLE, false, type.typeName, type.typeId);
                    VistortypeArr.Add(type);
                }

                //ObjectManager.Instance.InstantiateObjectAsync(ConStr.USERTYPETOG, UserTypecallback, LoadResPriority.RES_MIDDLE, false, typeArr[i].typeName, typeArr[i].typeId);
            }

            StartCoroutine(GetPeopletypesAreaDevs(VistortypeArr));
            Log.Debug("访客人员类型数组大小:" + VistortypeArr.Count);

            //StartCoroutine(GetPeopletypesAreaDevs(typeArr));
            //Log.Debug("访客人员类型数组大小:" + typeArr.Count);


            Timer.Register(0.3f, () => { transform.GetChild(0).GetComponent<Toggle>().isOn = true; });
        }
      

    }

     /// <summary>
     /// 拿到人员类型后带着人员类型去请求该人员类型对应的区域设备信息(请求人员类型权限)
     /// </summary>
     /// <returns></returns>
    IEnumerator GetPeopletypesAreaDevs(List<UserType> typeArr)
    {

        for (int i = 0; i < typeArr.Count; i++)
        {
            yield return new WaitForSeconds(0.2f);
            HttpNetManager.GetInstance().SendDataObj("http://" +   GameStart.IP + "/api/perm/tg/type/area/dev/perm/list?typeId=" + typeArr[i].typeId, GetPeopletypesAreadevsCallback, false, false);

        }


        //获取人员类型对应的区域权限信息

    }

    /// <summary>
    /// 获取人员类型对应的区域设备权限信息回调
    /// </summary>
    /// <param name="args"></param>
    /// 
    //请求服务器人员类型对应的区域设备信息时一并把人员类型和设备进行绑定，以供后续分组下发时使用
    public static Dictionary<int, List<NVRInformation>> UserTypebandsDevs = new Dictionary<int, List<NVRInformation>>();

    /// <summary>
    /// 请求区域信息模板
    /// </summary>
    private void RequestCameraPreviewAreaModel()
    {
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/all/area/info?areaType=2", GetCameraPreviewGroupCallback, false, false, false);

    }
    /// <summary>
    /// 请求监控区域模板回调,初始化监控分组模板
    /// </summary>
    /// <param name="args"></param>
    public void GetCameraPreviewGroupCallback(HttpCallBackArgs args)
    {
        DoorAreaModelList.Clear();
        if (!string.IsNullOrEmpty(args.Value))
        {
            AreaGroupsList areagrouplist = JsonUtility.FromJson<AreaGroupsList>(args.Value);
            Log.Debug("监控区域递归层级:" + JsonUtility.ToJson(areagrouplist, true));
            for (int i = 0; i < areagrouplist.data.Count; i++)
            {
                SplitCameraGroupList(areagrouplist.data[i]);
            }
            for (int i = 0; i < ShowtypesAreadevs.typeList.Count; i++)
            {
                if (ShowtypesAreadevs.typeList[0].areaList == null)//初始化修改模板
                {
                    ShowtypesAreadevs.typeList[0].areaList = DoorAreaModelList;
      
                }
                else 
                {
                 
                
                }
            } 


        }
    }
    //递归划分层级，生成List<GetAreaGroupDevs>  初始化监控区域模板
    private void SplitCameraGroupList(GetAreaGroupDevs groups)
    {

        DoorAreaModelList.Add(groups);
        if (groups.children.Count != 0)
        {
            for (int i = 0; i < groups.children.Count; i++)
            {
                SplitCameraGroupList(groups.children[i]);
            }
        }


    }
    private void GetPeopletypesAreadevsCallback(HttpCallBackArgs args)
    {
        List<NVRInformation> nvrs = new List<NVRInformation>();//存储某一人员类型下所有的设备信息
        Log.Debug("人员类型对应的区域设备关系:"+args.Value);
        //nvrs.Clear();
        if (!string.IsNullOrEmpty(args.Value))
        {
            try
            {
                ResponseGetuserTypesAreasDev response = JsonUtility.FromJson<ResponseGetuserTypesAreasDev>(args.Value);
                if (!ShowtypesAreadevs.typeList.Exists(t => t.typeId == response.typeList[0].typeId))
                {
                  
                    ShowtypesAreadevs.typeList.Add(response.typeList[0]);//把请求的人员类型对应区域设备关系添加进TypeAreadevs供后续展示该人员类型已经绑定了哪些区域设备和下载设备使用
                    if (response.typeList[0].areaList == null)
                    {
                        RequestCameraPreviewAreaModel();
                    }
                    Log.Debug(" typesAreadevs.typeList绑定人员类型对应的区域");

                }
                else 
                {
                    userTypeAreaDev typeareadev = ShowtypesAreadevs.typeList.Find(t => t.typeId == response.typeList[0].typeId);
                    typeareadev = response.typeList[0];
                       

                }


            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Json解析异常:"+e.ToString());
                
            }
            
         

          
        }
    }

    //递归拆分人员类型对应的区域设备树级分组
    private void ProductDoorTreeDic(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        //outline.Type = 1;
        outline.DevList = devs.devList;
        //if (outline.DevList.Count != 0)
            markoutlineInfoList.Add(outline);
        if (outline.DevList != null) 
        {
            for (int j = 0; j < outline.DevList.Count; j++)
            {
                OutlineInfo outlinedev = new OutlineInfo();
                outlinedev.OutlineId = outline.DevList[j].id.ToString();
                outlinedev.ParentId = outline.OutlineId;
                outlinedev.OutlineName = outline.DevList[j].cameraname;
                outlinedev.Children = null;
                outlinedev.DevList = null;
                outlinedev.NVr = outline.DevList[j];
                outlinedev.Level = "equip";
                markoutlineInfoList.Add(outlinedev);
            }
        }
     

        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                ProductDoorTreeDic(devs.children[i]);
            }
        }

    }



    /// <summary>
    /// 获取人员类型对应的区域设备信息数据结构
    /// </summary>
    [Serializable]
    public class ResponseGetuserTypesAreas
    {
        public string msg = string.Empty;
        public int code;
        public List<userTypeAreaDevs> data = new List<userTypeAreaDevs>();

    }
    /// <summary>
    /// 人员类型对应的区域信息
    /// </summary>
    [Serializable]
    public class userTypeAreaDevs
    {
        public int id;
        public string typeName = string.Empty;
        public string remark = string.Empty;
        public int typeId ;
        public List<Areadevs> areaList = new List<Areadevs>();
             
    }
    /// <summary>
    /// 区域信息对应的设备信息
    /// </summary>
    [Serializable]
    public class Areadevs
    {
        public int id; //区域id
        public string areaName = string.Empty;
        public string remark = string.Empty;
        public int areaType; //区域类型 
        public List<NVRInformation> devList = new List<NVRInformation>();

    }

    //获取新改版人员类型对应区域设备信息
    [Serializable]
    public class ResponseGetuserTypesAreasDev
    {
        public List<userTypeAreaDev> typeList = new List<userTypeAreaDev>();

    }

    [Serializable]
    public class userTypeAreaDev
    {
        public int id;//人员类型Id
        public string typeName = string.Empty; //类型名字
        public string remark = string.Empty;
        public int typeId;
        public List<GetAreaGroupDevs> areaList = new List<GetAreaGroupDevs>();

    }





    private void UserTypecallback(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
 

        GameObject usertypeObj = obj as GameObject;
        usertypeObj.transform.SetParent(this.transform);
        usertypeObj.transform.localScale = Vector3.one;
        usertypeObj.transform.Find("typename").GetComponent<TextMeshProUGUI>().text = param1 as string;
        usertypeObj.transform.GetComponent<Toggle>().group = this.transform.GetComponent<ToggleGroup>();
        usertypeObj.transform.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        usertypeObj.transform.GetComponent<Toggle>().onValueChanged.AddListener((ison)=> {
            ToggleDebug((int)param2, ison);
            if (ison)
            {
                markoutlineInfoList.Clear();
                if (VistorController.Ins.AreaScroll.activeInHierarchy)
                    VistorController.Ins.InitDoorGroup();//打开权限管理的时候，点击人员类型按钮，权限也跟着刷新
                                                         //点击人员类型时生成可供调用的OUlineTree

                if (ShowtypesAreadevs.typeList.Exists(t => t.typeId == (int)param2)) 
                {
                    userTypeAreaDev areadevs= ShowtypesAreadevs.typeList.Find(t => t.typeId == (int)param2);
                    for (int i = 0; i < areadevs.areaList.Count; i++)
                    {
                        int index = i;
                        ProductDoorTreeDic(areadevs.areaList[index]);
                    }
                  

                }
                //关闭查询状态
                PageLoadingController.IsSearch = false;
              

            }
        });
        usertypeObj.transform.GetComponent<Toggle>().group = this.transform.GetComponent<ToggleGroup>();
       
    }


    //这里得动态绑定Toggle
    public void BindToggle()
    {
        //找到所有的toggles
        toggles = transform.GetComponentsInChildren<Toggle>();
        //给toggle添加事件
        for (int i = 0; i < toggles.Length; i++)
        {
            //这一步是必须记录的，用来区分那个toggle
            int K = i;
            // toggles[K].onValueChanged.AddListener((bool value) => SetEveryToggle(value, K));
            toggles[K].onValueChanged.AddListener((ison) => { ToggleDebug(K, ison); });
        }
    }
    void ClearPerson()
    {

        for (int i = 0; i < LzVistorJsonParsing.Instance.UserContent.transform.childCount; i++)
        {
            Destroy(LzVistorJsonParsing.Instance.UserContent.transform.GetChild(i).gameObject);
        }
    }
    public void ToggleDebug(int index, bool value)
    {
        if (value)
        {
            Log.Debug("Index:" + index);
            ChooseType = index;
            ClearPerson();
            PageLoadingController.IsSearch = false;
            LzVistorJsonParsing.Instance.PagingLoad.GetComponent<PageLoadingVisitor>().Init();
             Log.Debug("当前类型：" + ChooseType);

        }
    
    }

}
