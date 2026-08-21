using DG.Tweening;
using SuperTreeView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using zFramework.Media;
using static ManManagentUi;

public class UIManger : MonoBehaviour
{
    public static bool IsInitCameraGroup = false;
    public InputField IFSearcher;
    public BaseLevelPartObj PrefabsPartObj;
    public Transform ReadObj;
    public Color[] ColorBtnSelf;
    public RectTransform ScrollViewRect;
    public static int ChoiceCameraId = 0;
    public static UIManger M_Instance
    {
        get
        {
            if (null == instance)
            {
                instance = FindObjectOfType<UIManger>();
            }
            return instance;
        }
    }
    public ContentSizeFitter ParentPartFirstLevel;//一级菜单的父物体

    public TreeView TreeviewReplay;
    private List<BaseLevelPartObj> listAllLevelPartObj = new List<BaseLevelPartObj>();
    private static UIManger instance;
    private string ChoiceCameraGroup = string.Empty;//选中的监控分组
    public TMP_InputField inputSearch;
    public Button searchBtn;
    public Button clearBtn;
    private string InputStrcamename = string.Empty;
    // Start is called before the first frame update
    void Start()
    {
    
     
       StartCoroutine( GetCameraGroupDevs());

        inputSearch.onEndEdit.AddListener((str) =>
        {

            InputStrcamename = str;

        });
        searchBtn.onClick.AddListener(() => {

            SearchCamera();


        });

        clearBtn.onClick.AddListener(() =>
        {
            if (TreeviewReplay.transform.childCount != 0)
            {
                for (int i = TreeviewReplay.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && TreeviewReplay.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManagerPreview.GetInstance().OnDeleteBtnClicked(TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }
            inputSearch.text = "";
            InputStrcamename = "";
            InitCameraGroup(outlineInfoList);


            searchBtn.gameObject.SetActive(true);
            clearBtn.gameObject.SetActive(false);

        });


    }

    /// <summary>
    /// 模糊查询监控设备
    /// </summary>
    List<OutlineInfo> SearchoutlineInfoList = new List<OutlineInfo>();
    private void SearchCamera()
    {
        SearchoutlineInfoList.Clear();
        //先清除
        if (TreeviewReplay.transform.childCount != 0)
        {
            for (int i = TreeviewReplay.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && TreeviewReplay.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManagerPreview.GetInstance().OnDeleteBtnClicked(TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }
        if (!string.IsNullOrEmpty(InputStrcamename))
        {
            for (int i = 0; i < outlineInfoList.Count; i++)
            {
                if (outlineInfoList[i].OutlineName.Contains(InputStrcamename))
                {
                    Log.Debug("找到相关设备：" + outlineInfoList[i].OutlineName);
                    //if(outlineInfoList[i].)
                    if (outlineInfoList[i].OutlineId == "")
                        SearchoutlineInfoList.Add(outlineInfoList[i]);
                }
            }
            if (SearchoutlineInfoList.Count > 0)
            {
                //再显示
                for (int i = 0; i < SearchoutlineInfoList.Count; i++)
                {

                    if (TreeviewReplay != null)
                    {

                        TreeViewItem childItem = TreeviewReplay.AppendItem("ItemPrefab1");
                        childItem.GetComponent<ItemScript>().id = SearchoutlineInfoList[i].OutlineId;
                        childItem.GetComponent<ItemScript>().parentId = SearchoutlineInfoList[i].ParentId;
                        childItem.GetComponent<ItemScript>().labelText.text = SearchoutlineInfoList[i].OutlineName;
                        childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                        childItem.GetComponent<ItemScript>().type = SearchoutlineInfoList[i].Type;
                        childItem.GetComponent<ItemScript>().children = SearchoutlineInfoList[i].Children;
                        childItem.GetComponent<ItemScript>().devList = SearchoutlineInfoList[i].DevList;
                        if (SearchoutlineInfoList[i].OutlineId == "")
                        {
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().nvr = SearchoutlineInfoList[i].NVr;

                        }






                    }


                }

            }
            else
            {
                GameStart.Instance.ShowTip("没有查到相关设备!");
            }

        }

        searchBtn.gameObject.SetActive(false);
        clearBtn.gameObject.SetActive(true);



    }
    //请求服务器
    //void InitCamreaGroup()
    //{

    //    HttpNetManager.GetInstance().SendDataStr("http://" +GameStart.IP  + "/api/personnel/tg/area/dev/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraGroupDevs, false, false, false);

    //}
    [Serializable]
    public class AreaGroupDevList
    {
        public List<GetAreaGroupDevs> records = new List<GetAreaGroupDevs>();

    }
    //请求分组设备信息数据结构
    //[Serializable]
    //public class GetAreaGroupDevs
    //{
    //    public int id;
    //    public string areaName = string.Empty;
    //    public string remark = string.Empty;
    //    public List<NVRInformation> devList = new List<NVRInformation>();
    //    public int areaType;//1是门禁，2是监控
    //}
    /// <summary>
    /// 获取监控区域分组设备信息
    /// </summary>
    //public AreaGroupDevList CameraAreaGroupDevs;
    //public void GetCameraGroupDevs(HttpCallBackArgs args)
    //{
    //    Log.Debug("从服务器接收到的监控区域分组设备信息：" + args.Value);
    //    if (!string.IsNullOrEmpty(args.Value))
    //    {

    //        CameraAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevList>(args.Value);
    //        Log.Debug("请求的监控分组为:" + CameraAreaGroupDevs.records.Count);
    //        for (int i = 0; i < CameraAreaGroupDevs.records.Count; i++)
    //            ObjectManager.Instance.InstantiateObjectAsync(ConStr.CAMERAGROUP, cameraVcallback, LoadResPriority.RES_MIDDLE, false, CameraAreaGroupDevs.records[i].areaName, CameraAreaGroupDevs.records[i].devList, CameraAreaGroupDevs.records[i].id);
    //    }
    //}
    /// <summary>
    /// 获取监控区域分组设备信息
    /// </summary>
    //public AreaGroupDevList CameraAreaGroupDevs;
    //AreaDevsData areaDevsData;
    List<OutlineInfo> outlineInfoList = new List<OutlineInfo>();
    //public void GetCameraGroupDevs(HttpCallBackArgs args)
    //{
    //    outlineInfoList.Clear();
    //    Log.Debug("从服务器接收到的监控区域分组设备信息：" + args.Value);
    //    if (!string.IsNullOrEmpty(args.Value))
    //    {

    //        areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
    //        Log.Debug("监控分组个数:" + areaDevsData.records.Count);
    //        if (TreeviewReplay.transform.childCount != 0)
    //        {
    //            for (int i = TreeviewReplay.transform.childCount - 1; i >= 0; i--)
    //            {
    //                int temp = i;
    //                if (TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && TreeviewReplay.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
    //                    TreeManagerPreview.GetInstance().OnDeleteBtnClicked(TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>());
    //            }

    //        }

    //        //过滤权限信息或者从服务器回来的就是经过权限过滤的
    //        for (int i = 0; i < areaDevsData.records.Count; i++)
    //        {
    //            int index = i;
  
    //            ProductDoorTreeDic(areaDevsData.records[index]);
    //        }

    //        InitCameraGroup(outlineInfoList);
    //    }
    //}

    private IEnumerator GetCameraGroupDevs() 
    {
        yield return new WaitForEndOfFrame();
        if (TreeviewReplay.transform.childCount != 0)
        {
            for (int i = TreeviewReplay.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && TreeviewReplay.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManagerPreview.GetInstance().OnDeleteBtnClicked(TreeviewReplay.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }

        //过滤权限信息或者从服务器回来的就是经过权限过滤的
        if (LoginManager.Ins.ReplayAuthList != null) 
        {
            for (int i = 0; i < LoginManager.Ins.ReplayAuthList.Count; i++)
            {
                int index = i;

                ProductDoorTreeDic(LoginManager.Ins.ReplayAuthList[index]);
            }
        }
    

        InitCameraGroup(outlineInfoList);


    }



    /// <summary>
    /// 递归拆分层及目录架构
    /// </summary>
    /// <param name="devs"></param>
    private void ProductDoorTreeDic(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        //outline.Type = 1;
        outline.DevList = devs.devList;
        outlineInfoList.Add(outline);
        if (outline.DevList != null&& outline != null)
        {
            for (int j = 0; j < outline.DevList.Count; j++)
            {
                OutlineInfo outlinedev = new OutlineInfo();
                //outlinedev.OutlineId = outline.DevList[j].id.ToString();
                outlinedev.OutlineId = "";
                outlinedev.ParentId = outline.OutlineId;
                outlinedev.OutlineName = outline.DevList[j].cameraname;
                outlinedev.Children = null;
                outlinedev.DevList = null;
                outlinedev.NVr = outline.DevList[j];
                outlineInfoList.Add(outlinedev);
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



    List<TreeViewItem> allCameraTreeViewItemList = new List<TreeViewItem>();
    /// <summary>
    ///  生成监控树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    public void InitCameraGroup(List<OutlineInfo> outlineInfoList)
    {




        allCameraTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();
        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            if (string.IsNullOrEmpty(outlineInfoList[i].ParentId) || int.Parse(outlineInfoList[i].ParentId) == 0)
            {
                item1 = TreeviewReplay.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());

                allCameraTreeViewItemList.Add(item1);
            }
            else
            {
                for (int j = 0; j < allCameraTreeViewItemList.Count; j++)
                {
                    if (allCameraTreeViewItemList[j].GetComponent<ItemScript>().id.Equals(outlineInfoList[i].ParentId))
                    {
                        TreeViewItem childItem = allCameraTreeViewItemList[j].ChildTree.AppendItem("ItemPrefab1");
                        childItem.GetComponent<ItemScript>().id = outlineInfoList[i].OutlineId;
                        childItem.GetComponent<ItemScript>().parentId = outlineInfoList[i].ParentId;
                        childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[i].OutlineName;
                        childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                        childItem.GetComponent<ItemScript>().type = outlineInfoList[i].Type;
                        childItem.GetComponent<ItemScript>().children = outlineInfoList[i].Children;
                        childItem.GetComponent<ItemScript>().devList = outlineInfoList[i].DevList;
                        //if (childItem.GetComponent<ItemScript>().children == null && childItem.GetComponent<ItemScript>().devList == null)
                        //{
                        //    Log.Debug(childItem.GetComponent<ItemScript>().labelText.text + "是监控设备");
                        //    childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                        //    childItem.GetComponent<ItemScript>().labelText.interactable = false;
                        //    childItem.GetComponent<ItemScript>().nvr = outlineInfoList[i].NVr;
                        //}
                        if (outlineInfoList[i].OutlineId == "")
                        {
                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[i].NVr;
                            //NVRController.Instance().Login(outlineInfoList[temp].NVr.host);
                        }
                        allCameraTreeViewItemList.Add(childItem);
                    }
                }
            }
        }


    }





    /// <summary>
    /// 重置预制体
    /// </summary>
    /// <param name="obj"></param>
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
    }
    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    //Init();
        //}
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    if (ParentPartFirstLevel.gameObject.transform.childCount > 0)
        //    {
        //        for (int i = 0; i < ParentPartFirstLevel.gameObject.transform.childCount; i++)
        //        {
        //            Destroy(ParentPartFirstLevel.gameObject.transform.GetChild(i).gameObject);
        //        }
        //    }

        //}
    }


    public void Init()
    {
        //先缓存一级和二级的菜单项
        Dictionary<string, BaseLevelPartObj> tempDicFirstLevelObj = new Dictionary<string, BaseLevelPartObj>();
        Dictionary<string, BaseLevelPartObj> tempDicSecLevelObj = new Dictionary<string, BaseLevelPartObj>();
      /*  Transform[] tempTransObj = ReadObj.GetComponentsInChildren<Transform>(true);
        //采用一次遍历创建完所有的层级菜单项
        foreach (Transform item in tempTransObj)
        {
            BaseLevelPartObj tempBLPObj = Instantiate(PrefabsPartObj);
            PartsLevel tempLevel = (PartsLevel)(GetLevel(item) - 1);
            //   Log.Debug(item.name + ":" + GetLevel(item) + ":p:"+(item.parent==null?"null":item.parent.name));
            switch (tempLevel)
            {
                case PartsLevel.None:
                    Destroy(tempBLPObj.gameObject);
                    continue;
                case PartsLevel.First:
                    tempBLPObj.Init(item.name, null, tempLevel);
                    tempDicFirstLevelObj.Add(item.name, tempBLPObj);
                    break;
                case PartsLevel.Second:
                    //如果临时一级菜单中有包含当前物体的父物体
                    if (tempDicFirstLevelObj.ContainsKey(item.parent.name))
                    {
                        BaseLevelPartObj tempParentObj = tempDicFirstLevelObj[item.parent.name];
                        tempParentObj.AddSubObj(1);
                        tempBLPObj.Init(item.name, tempParentObj, tempLevel);
                        //tempDicSecLevelObj.Add(item.name, tempBLPObj);
                    }
                    break;
                    // case PartsLevel.Third:
                    //     //如果临时的二级菜单中有包含当前物体的父物体
                    //     if (tempDicSecLevelObj.ContainsKey(item.parent.name))
                    //     {
                    //         BaseLevelPartObj tempParentObj = tempDicSecLevelObj[item.parent.name];
                    //         tempParentObj.AddSubObj(1);
                    //         tempBLPObj.Init(item.name, tempParentObj, tempLevel);
                    //     }

            }
            listAllLevelPartObj.Add(tempBLPObj);
        }*/
    }
    //搜索框输入内容的事件
    public void IFSearchChangeValue()
    {
        if (IFSearcher.isFocused)
        {
            for (int i = 0; i < listAllLevelPartObj.Count; i++)
            {
                BaseLevelPartObj tempBLPO = listAllLevelPartObj[i];
                //    tempBLPO.SearcherUnActive();
                if (tempBLPO.TextTitle.text.Contains(IFSearcher.text))
                {
                    tempBLPO.Btn_PointUp();
                    //  tempBLPO.SearcherMatch();
                    tempBLPO.SearcherSelectedUnFlod();

                    RectTransform tempContent = ParentPartFirstLevel.GetComponent<RectTransform>();
                    Vector3 tempVecCotent = tempContent.localPosition;
                    float tempOffsetHeight = tempContent.sizeDelta.y - ScrollViewRect.sizeDelta.y;
                    float tempSelectY = tempContent.parent.InverseTransformPoint(tempBLPO.LeftToolBar.position).y;
                    // Log.Debug("选中的高度:" + tempSelectY + "滚轮的高度：" + tempVecCotent.y);
                    ParentPartFirstLevel.transform.DOLocalMoveY(-tempSelectY, 0.35f);
                    break;
                }
            }
        }
        else
        {

        }
        bool tempIsNull = IFSearcher.text == "";
        if (tempIsNull)
        {
            //for (int i = 0; i < listAllLevelPartObj.Count; i++)
            //{
            //    BaseLevelPartObj tempBLPO = listAllLevelPartObj[i];
            //    tempBLPO.RestoreFromSearcher();

            //}
        }
    }
    //清空搜索框
    public void Btn_ClearSearcher()
    {
        IFSearcher.text = "";
        //  SelectedBLPobj(null);

    }
    //获取当前的层级
    public int GetLevel(Transform transfor)
    {
        int tempLevel = 1;

        if (transfor.parent != null)
        {
            return tempLevel + GetLevel(transfor.parent);
        }
        return tempLevel;
    }
}
public enum PartsLevel
{
    None = 0,
    First = 1,
    Second,
    Third,
}
