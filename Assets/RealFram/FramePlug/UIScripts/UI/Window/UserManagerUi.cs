using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using zFramework.Media;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ManManagentUi;
using SuperTreeView;
using System.Collections;

//两个问题   1、切换角色管理界面时，默认读取第一个角色信息的权限，新建只能走添加按钮
//2、切换角色时权限没有刷新还是一样的


#region 数据模型相关
//获取角色权限
[Serializable]
public class roleListData
{
    public List<Authuser> roleList;

    public roleListData()
    {
        roleList = new List<Authuser>();
    }
}
[Serializable]
public class RoleAuth
{
    public string roleId;
    public string roleName;
    public List<MenuInfo> menuList = new List<MenuInfo>();

}



[Serializable]
public class treedata
{

    public int areaId;
    public int devId;
    public int menuId;
    public string areaName = string.Empty;
    public string devName = string.Empty;
    public string menuName = string.Empty;
    public string roleName = string.Empty;

}
[Serializable]
public class Authuser
{
    public string roleId;
    public string roleName;
    public List<MenuInfo> menuList = new List<MenuInfo>();
    //public Dictionary<string, List<treedata>> cameraSetDic = new Dictionary<string, List<treedata>>();
    public Authuser(string name_, string roleid, List<MenuInfo> menulist)
    {
        roleName = name_;
        roleId = roleid;
        menuList = menulist;
    }
}


//角色对应权限信息
[Serializable]
public class Roles
{

    public List<Role> roleList = new List<Role>();

}
//新建角色时数据模型
[Serializable]
public class Role
{
    public long roleId;
    public string roleName;
    public List<Menu> menuList = new List<Menu>();

}

//模块对应的区域设备信息
[Serializable]
public class Menu
{


    public string menuName;
    public int menuId;
    public List<GetAreaGroupDevs> areaList = new List<GetAreaGroupDevs>();
}


[Serializable]
public class AreaDevInfo
{
    public int areaId;
    public string areaName;
    public int parentId;
    public List<NVRInformation> devList = new List<NVRInformation>();
    public List<AreaDevInfo> children = new List<AreaDevInfo>();

}


//
[Serializable]
public class MenuInfo
{

    public string menuName;
    public int menuId;
    //public List<MenuInfo> areaList = new List<MenuInfo>();
    //public List<AreaDevInfo> areaList = new List<AreaDevInfo>();
    public List<GetAreaGroupDevs> areaList = new List<GetAreaGroupDevs>();

}


//请求区域分组信息数据模型，只包含区域分组层级目录结构，不包含设备信息
public class AreaGroupsList
{
    public string msg = string.Empty;
    public int code;
    //public List<GetAreaGroups> data = new List<GetAreaGroups>();
    public List<GetAreaGroupDevs> data = new List<GetAreaGroupDevs>();

}

//用户列表
[Serializable]
public class userlist
{
    public string msg = string.Empty;
    public int code;

    public List<userinfo> data = new List<userinfo>();
}
[Serializable]
public class userinfo
{
    public long id;
    public string userName = string.Empty;
    public string nickName = string.Empty;
    public string password = string.Empty;
    public string status = string.Empty;
    public string email = string.Empty;
    public string phonenumber = string.Empty;
    public string sex = string.Empty;
    public string avatar = string.Empty;
    public string userType = string.Empty;
    public string createBy = string.Empty;
    public string createTime = string.Empty;
    public string updateBy = string.Empty;
    public string updateTime = string.Empty;
    public int delFlag;
    public string roleName = string.Empty;
    public int roleId;
}

//角色列表
[Serializable]
public class rolelist
{
    public string msg = string.Empty;
    public int code;
    public List<roleinfo> data = new List<roleinfo>();
}
[Serializable]
public class roleinfo
{
    public int id;
    public string name;
    public string roleKey;
    public string status;
    public int delFlag;
    public string createBy;
    public string createTime;
    public string updateBy;
    public string updateTime;
    public string remark;
}
#endregion








public class UserManagerUi : Window
{
    public UserManagerPanel MainUserManager;
    string usertype;
    string username;
    string password;
    string repassword;

    //public Dictionary<string, List<treedata>> CameraSetDic = new Dictionary<string, List<treedata>>();

    #region  监控配置模块
    //treedata CameraSetData;
    public static readonly string CameraSetPath = "monitor_config";
    #endregion

    #region 门禁配置模块
    //treedata DoorSetData;
    public static readonly string DoorSetPath = "door_config";
    #endregion
    #region 监控预览模块
    //treedata PreviewData;
    public static readonly string PreviewPath = "monitor_preview";
    #endregion
    #region 监控回放模块
    //treedata ReplayData;
    public static readonly string ReplayPath = "monitor_playback";
    #endregion
    #region 监控事件模块
    //treedata cameraEventData;
    public static readonly string CameraEventPath = "monitor_event";
    #endregion
    #region 门禁事件模块
    //treedata DoorEventData;
    public static readonly string DoorEventPath = "door_event";
    #endregion
    #region 用户管理模块
    public static readonly string UserManagerPath = "user_management";
    #endregion
    #region 人员管理模块
    public static readonly string PeopleManagerPath = "personnel_management";

    #endregion


    //角色列表
    Roles roles = new Roles();
    //当前角色
    Role role = new Role();
    //新规则初始化
    Menu doorsetMdata = new Menu();//门禁点配置模块
    Menu camerasetMdata = new Menu();//监控点配置模块
    Menu previewMdata = new Menu();//视频预览模块
    Menu replayMdata = new Menu();//视频回放模块
    Menu cameraeventMdata = new Menu();//监控事件
    Menu dooreventMdata = new Menu();//门禁事件
    Menu peopleManagerMdata = new Menu();//人员管理
    Menu UserManagerMdata = new Menu();//用户管理
    private rolelist rolesList;//存储请求所有角色信息模型
    private long pendingSavedRoleId = -1;
    private bool isApplyingRolePermissions;
    private bool suppressPermissionTreeEvents;
    private readonly HashSet<string> previewSelectedKeys = new HashSet<string>();
    private readonly HashSet<string> replaySelectedKeys = new HashSet<string>();
    private List<GetAreaGroupDevs> previewAreaRoots = new List<GetAreaGroupDevs>();
    private List<GetAreaGroupDevs> replayAreaRoots = new List<GetAreaGroupDevs>();
    private readonly Dictionary<string, TreeViewItem> permissionTreeItems = new Dictionary<string, TreeViewItem>();
    private readonly Dictionary<string, string> permissionTreeParents = new Dictionary<string, string>();
    private readonly Dictionary<string, List<string>> permissionTreeChildren = new Dictionary<string, List<string>>();
    private bool permissionTreeIsReplay;
    private int activePermissionTreeMenuId;
    private Sprite permissionCameraIcon;
    Transform user_roleContent;

    List<GetAreaGroupDevs> MonitorPreviewList = new List<GetAreaGroupDevs>();


    //回放
    List<GetAreaGroupDevs> MonitorReplayList = new List<GetAreaGroupDevs>();





    public override void Awake(params object[] paralist)
    {

        InitPreviewdata();//预览
        InitReplaydata();//回放
        InitCameraEventdata();//监控事件
        InitDoorEventdata();//门禁事件
        InitDoorSetdata();//门禁配置
        InitCameraSetdata();//监控配置
        InitUserManagerdata();//用户管理
        InitPeopleManagerdata();//人员管理


        #region 监听
        MainUserManager = GameObject.GetComponent<UserManagerPanel>();
        user_roleContent = MainUserManager.transform.Find("user/AuthSetPage/userbg/userscroll/Viewport/Content");
        AddButtonClickListener(MainUserManager.addRolebtn, addRolebtnClick);
        AddButtonClickListener(MainUserManager.deleteRolebtn, deleteRolebtnClick);
        AddToggleClickListener(MainUserManager.palltog, palltoglistener);
        AddToggleClickListener(MainUserManager.call, calllistener);
        AddToggleClickListener(MainUserManager.pmonitorSettog, pmonitortoglistener);

        AddToggleClickListener(MainUserManager.cmonitorSet, cmonitorSetlistener);

        AddToggleClickListener(MainUserManager.pdoorSettog, pdoortoglistener);
        //  修改新权限注释
        AddToggleClickListener(MainUserManager.cdoorSet, cdoorSetlistener);
        AddToggleClickListener(MainUserManager.ppreviewtog, ppreviewtoglistener);
        AddToggleClickListener(MainUserManager.cpreview, cpreviewlistener);
        AddToggleClickListener(MainUserManager.preplaytog, preplaytoglistener);
        AddToggleClickListener(MainUserManager.creplay, creplaylistener);
        AddToggleClickListener(MainUserManager.pcameraeventtog, pcameraeventtoglistener);
        AddToggleClickListener(MainUserManager.ccameraevent, cCameraEventlistener);
        AddToggleClickListener(MainUserManager.pdooreventtog, pdooreventtoglistener);
        AddToggleClickListener(MainUserManager.cdoorevent, cDoorEventlistener);
        AddToggleClickListener(MainUserManager.pusermanagertog, pusermanagertoglistener);
        AddToggleClickListener(MainUserManager.cusermanager, cUserManagerlistener);
        AddToggleClickListener(MainUserManager.ppeoplemanagertog, pPeopleManagertoglistener);
        AddToggleClickListener(MainUserManager.cpeoplemanager, cPeopleManagerlistener);


        AddButtonClickListener(MainUserManager.SaveRoleButton, SaveRoleData);
        AddButtonClickListener(MainUserManager.CancleRoleButton, CancleAuthData);

        MainUserManager.U_UsernameInput.onEndEdit.AddListener((str) =>
            {
                username = str;
            });
        MainUserManager.U_PasswordInput.onEndEdit.AddListener((str) =>
        {
            password = str;
        });
        MainUserManager.U_RePasswordInput.onEndEdit.AddListener((str) =>
        {
            repassword = str;
            if (!string.Equals(password, repassword))
            {
                GameStart.Instance.ShowTip("两次输入的密码不一致！");
                str = "";
            }

        });
        AddToggleClickListener(MainUserManager.userManTog, UserManPanelShow);//展示用户管理界面权限
        AddToggleClickListener(MainUserManager.roleManTog, RoleManPanelShow);//展示角色管理界面权限
        #endregion

        MainUserManager.userManTog.isOn = true;



        //用户管理界面权限展示
        AddToggleClickListener(MainUserManager.alltog, alltoglistener);
        AddToggleClickListener(MainUserManager.monitorSettog, monitortoglistener);
        AddToggleClickListener(MainUserManager.doorSettog, doortoglistener);
        AddToggleClickListener(MainUserManager.previewtog, previewtoglistener);
        AddToggleClickListener(MainUserManager.replaytog, replaytoglistener);
        AddToggleClickListener(MainUserManager.cameraeventtog, cameraeventtoglistener);
        AddToggleClickListener(MainUserManager.dooreventtog, dooreventtoglistener);
        AddToggleClickListener(MainUserManager.peoplemanagertog, PeopleManagertoglistener);
        AddToggleClickListener(MainUserManager.usermanagertog, usermanagertoglistener);

        //用户管理界面添加用户
        AddButtonClickListener(MainUserManager.AddUserBtn, AddUser);
        AddButtonClickListener(MainUserManager.DeleteUserBtn, DeleteUser);//删除
        AddButtonClickListener(MainUserManager.SaveUserButton, SaveUserData);
        AddButtonClickListener(MainUserManager.CancleUserButton, CancleUserData);
    }
    /// <summary>
    /// 角色管理界面
    /// </summary>
    void RoleManPanelShow(bool ison)
    {
        if (IsAddUser)
        {
            Log.Debug("正在添加用户，是否退出?");
            ChoiceRole = -1;
        }
        previewMdata.areaList.Clear();//模块容器先清空
        replayMdata.areaList.Clear();
        previewSelectedKeys.Clear();
        replaySelectedKeys.Clear();
        camerasetMdata.areaList.Clear();
        doorsetMdata.areaList.Clear();
        cameraeventMdata.areaList.Clear();
        dooreventMdata.areaList.Clear();
        UserManagerMdata.areaList.Clear();
        peopleManagerMdata.areaList.Clear();
        DeleteRoleTreeItem();
        //清空rolelist列表
        if (MainUserManager.roleContent.childCount > 0)
        {
            for (int i = 0; i < MainUserManager.roleContent.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.roleContent.GetChild(i).gameObject);
            }
        }
        if (ison)
        {

            //再请求
            HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/role/list", GetRolesCallback, false, false);
        }
    }




    /// <summary>
    /// 保存新建或修改角色信息
    /// </summary>
    void SaveRoleData()
    {

        //角色
        if (!string.IsNullOrEmpty(MainUserManager.UsernameInput.text))
        {
            //新规则
            role.roleName = MainUserManager.UsernameInput.text.Trim();
            role.roleId = RoleId > 0 ? RoleId : 0;

        
            //Debug.LogError("预览Item.Count:" + MarkCameraPreviewOutlineInfoList.Count);
            for (int i = 0; i < MarkCameraPreviewOutlineInfoList.Count; i++)
            {
                int m = i;
                //Debug.LogError("选中的预览Item:" + MarkCameraPreviewOutlineInfoList[m].OutlineName);
                if (MarkCameraPreviewOutlineInfoList[m].Children != null && MarkCameraPreviewOutlineInfoList[m].Children.Count > 0)
                {
                    Debug.LogError(MarkCameraPreviewOutlineInfoList[m].OutlineName);

                }
            }

            if (role.menuList.Exists(t => t.menuId == replayMdata.menuId))
            {
                if (replayAreaRoots.Count > 0)
                {
                    replayMdata.areaList = BuildSelectedAreaList(replayAreaRoots, replaySelectedKeys);
                }
            }
            if (role.menuList.Exists(t => t.menuId == previewMdata.menuId))
            {
                if (previewAreaRoots.Count > 0)
                {
                    previewMdata.areaList = BuildSelectedAreaList(previewAreaRoots, previewSelectedKeys);
                }
            }

            Role payloadRole = new Role();
            payloadRole.roleId = role.roleId;
            payloadRole.roleName = role.roleName;
            payloadRole.menuList = role.menuList
                .Where(menu => menu != null)
                .GroupBy(menu => menu.menuId)
                .Select(group => group.First())
                .ToList();
            Roles payload = new Roles();
            payload.roleList.Add(payloadRole);
            pendingSavedRoleId = payloadRole.roleId;
            string SetStr = JsonConvert.SerializeObject(payload);
            Log.Debug("保存角色信息权限：" + SetStr);
            //上传服务器
            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/tg/role/module", AddAuthCallback, true, true, false, SetStr);
        }
        else
        {
            GameStart.Instance.ShowTip("请输入角色信息!");

        }

    }

    /// <summary>
    /// 添加或者修改角色权限回调
    /// </summary>
    /// <param name="args"></param>
    void AddAuthCallback(HttpCallBackArgs args)
    {
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            GameStart.Instance.ShowTip("角色权限保存失败，请检查服务器连接");
            MainUserManager.addRolebtn.interactable = true;
            pendingSavedRoleId = -1;
            return;
        }

        try
        {
            HttpResponse response = JsonUtility.FromJson<HttpResponse>(args.Value);
            if (response != null && response.code == 200)
            {
                Log.Debug(args.Value);
                GameStart.Instance.ShowTip(response.msg);
                if (pendingSavedRoleId == LoginManager.RoleId && LoginManager.Ins != null)
                {
                    LoginManager.Ins.RefreshCurrentPermissions();
                }
                //添加角色信息成功，重新刷一下
                for (int i = 0; i < MainUserManager.roleContent.childCount; i++)
                {
                    GameObject.Destroy(MainUserManager.roleContent.GetChild(i).gameObject);
                }//先清空

                HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/role/list", GetRolesCallback, false, false);
            }
            else
            {
                GameStart.Instance.ShowTip(response == null ? "服务器返回了无效的保存结果" : response.msg);
            }
        }
        catch (Exception exception)
        {
            Log.Error("角色权限保存结果解析失败: " + exception.Message);
            GameStart.Instance.ShowTip("角色权限保存结果格式错误");
        }
        pendingSavedRoleId = -1;
        MainUserManager.addRolebtn.interactable = true;
    }


    /// <summary>
    /// 新建角色按钮回调
    /// </summary>
    void addRolebtnClick()
    {





        MainUserManager.addRolebtn.interactable = false;
        RoleId = -1;
        RoleName = string.Empty;
        role = new Role();
        roles = new Roles();
        previewSelectedKeys.Clear();
        replaySelectedKeys.Clear();
        MainUserManager.UsernameInput.text = "新建角色";
        MainUserManager.UsernameInput.interactable = true;

        //点击新建角色的时候，界面展示的层级勾选全部清除

        MainUserManager.cmonitorSet.isOn = false;
        MainUserManager.cdoorSet.isOn = false;
        MainUserManager.cpreview.isOn = false;
        MainUserManager.creplay.isOn = false;
        MainUserManager.ccameraevent.isOn = false;
        MainUserManager.cdoorevent.isOn = false;
        MainUserManager.cusermanager.isOn = false;
        MainUserManager.cpeoplemanager.isOn = false;


        //MonitorPreviewList = CameraAreaModel.data;
        previewMdata.areaList.Clear();//模块容器需要清空 
        MarkCameraPreviewOutlineInfoList.Clear();//判断已经存在的区域设备打勾的outline容器




        MonitorPreviewList.Clear();//该角色下监控选中区域设备模块置空

        //清除层级目录架构
        DeleteRoleTreeItem();

    }
    //取消
    void CancleAuthData()
    {
        MainUserManager.addRolebtn.interactable = true;

    }
    /// <summary>
    /// 删除角色监听
    /// </summary>
    void deleteRolebtnClick()
    {
        if (RoleId != -1)
        {

            HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/tg/del/role/name?roleId=" + RoleId, DeleteRoleCallback, true, false);
        }
        else
        {
            GameStart.Instance.ShowTip("请选中角色后删除！");
        }
    }
    /// <summary>
    /// 删除角色回调
    /// </summary>
    /// <param name="args"></param>
    void DeleteRoleCallback(HttpCallBackArgs args)
    {
        Log.Debug("删除角色信息回调:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            HttpResponse response = JsonUtility.FromJson<HttpResponse>(args.Value);

            GameStart.Instance.ShowTip(response.msg);

            for (int i = 0; i < MainUserManager.roleContent.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.roleContent.GetChild(i).gameObject);
            }
            MainUserManager.UsernameInput.text = "";
            //for (int i = 0; i < user_roleContent.childCount; i++)
            //{
            //    GameObject.Destroy(user_roleContent.GetChild(i).gameObject);
            //}
            refrushtog(false);//刷一下都置为false
            previewMdata.areaList.Clear();//模块容器需要清空 
            replayMdata.areaList.Clear();
            camerasetMdata.areaList.Clear();
            doorsetMdata.areaList.Clear();
            cameraeventMdata.areaList.Clear();
            dooreventMdata.areaList.Clear();
            UserManagerMdata.areaList.Clear();
            peopleManagerMdata.areaList.Clear();
        }
        HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/role/list", GetRolesCallback, false, false);

    }



    /// <summary>
    ///1、 在角色界面请求获取所有角色名字的列表
    /// </summary>
    /// <param name="args"></param>
    void GetRolesCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            rolesList = JsonUtility.FromJson<rolelist>(args.Value);
            if (rolesList.code == 200)
            {
                for (int i = 0; i < rolesList.data.Count; i++)
                {
                    int index = i;
                    GameObject role_ = ObjectManager.Instance.InstantiateObject(ConStr.USERTOG);
                    role_.transform.SetParent(MainUserManager.roleContent);
                    resetPrefab(role_);
                    role_.transform.Find("nameTxt").GetComponent<TextMeshProUGUI>().text = rolesList.data[index].name;
                    role_.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    role_.GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
                    {
                        //获取该角色的权限
                        if (ison)
                        {



                            MainUserManager.UsernameInput.interactable = true;
                            MainUserManager.UsernameInput.text = rolesList.data[index].name;
                            RoleName = rolesList.data[index].name;
                            RoleId = rolesList.data[index].id;
                            role = new Role();
                            role.roleName = RoleName;
                            role.roleId = RoleId;
                            refrushtog(false);//刷一下都置为false
                            //监控预览清空
                            previewMdata.areaList.Clear();//模块容器需要清空 
                            MarkCameraPreviewOutlineInfoList.Clear();//判断已经存在的区域设备打勾的outline容器
                            previewSelectedKeys.Clear();


                            if (MonitorPreviewList != null)
                                MonitorPreviewList.Clear();//该角色下监控选中区域设备模块置空

                            //监控回放清空
                            replayMdata.areaList.Clear();//模块容器需要清空 
                            MarkCameraReplayOutlineInfoList.Clear();//切换角色时要清楚当前角色下选择 了但是没有保存的节点
                            replaySelectedKeys.Clear();

                            if (MonitorReplayList != null)
                                MonitorReplayList.Clear();//该角色下监控选中区域设备模块置空


                            camerasetMdata.areaList.Clear();
                            doorsetMdata.areaList.Clear();
                            cameraeventMdata.areaList.Clear();
                            dooreventMdata.areaList.Clear();
                            UserManagerMdata.areaList.Clear();
                            peopleManagerMdata.areaList.Clear();

                            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/tg/role/info?roleId=" + rolesList.data[index].id, GetRoleAuthCallback, false, false, false);


                        }
                    });
                    role_.GetComponent<Toggle>().group = MainUserManager.roleContent.GetComponent<ToggleGroup>();


                }
            }
            else
            {
                GameStart.Instance.ShowTip(rolesList.msg);
            }
        }
        else
        {
            GameStart.Instance.ShowTip("服务器连接失败!");
        }
    }


    /// <summary>
    ///2、 单机单个角色名字获取该角色权限的回调
    /// </summary>
    /// <param name="args"></param>
    void GetRoleAuthCallback(HttpCallBackArgs args)
    {
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            GameStart.Instance.ShowTip("角色权限加载失败");
            return;
        }

        try
        {
            Roles authList = ExtractRoles(args.Value);
            Role selectedRole = FindRole(authList, RoleId);
            if (selectedRole == null)
            {
                GameStart.Instance.ShowTip("服务器未返回所选角色的权限数据");
                return;
            }

            ApplyRolePermissionsToEditor(selectedRole);
        }
        catch (Exception exception)
        {
            Log.Error("角色权限加载失败: " + exception.Message);
            GameStart.Instance.ShowTip("角色权限数据解析失败");
        }
    }

    void GetRoleAuthCallbackLegacy(HttpCallBackArgs args)
    {
        MonitorPreviewList = new List<GetAreaGroupDevs>();
        MonitorReplayList = new List<GetAreaGroupDevs>();


        Log.Debug("获取权限；" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            Roles authList = ExtractRoles(args.Value);
            Role selectedRole = FindRole(authList, RoleId);
            if (selectedRole != null && selectedRole.menuList != null && selectedRole.menuList.Count != 0)
            {
                foreach (var item in selectedRole.menuList)
                {

                    switch (GetMenuKey(item))
                    {
                        case "monitor_config":
                            MainUserManager.cmonitorSet.isOn = true;

                            break;
                        case "door_config":
                            MainUserManager.cdoorSet.isOn = true;
                            break;
                        case "monitor_preview":
                            MainUserManager.cpreview.isOn = true;
                            MonitorPreviewList = item.areaList;
                            MarkCameraPreviewOutlineInfoList.Clear();
                            if (MonitorPreviewList != null)
                            {
                                for (int i = 0; i < MonitorPreviewList.Count; i++)
                                {
                                    SplitModifyCameraPreviewGroupModel(MonitorPreviewList[i]);//表示该角色具有视频预览中区域设备权限，选中预览模块时，用于标识出该区域下哪些设备是存在的（打钩）


                                }
                            }
                            Log.Error("MarkCameraPreviewOutlineInfoList.count:"+MarkCameraPreviewOutlineInfoList.Count);
                            for (int i = 0; i < MarkCameraPreviewOutlineInfoList.Count; i++)
                            {
                                
                                int m = i;
                                Debug.LogError("选中的预览Item:" + MarkCameraPreviewOutlineInfoList[m].OutlineName);
                                //if (MarkCameraPreviewOutlineInfoList[m].Children != null && MarkCameraPreviewOutlineInfoList[m].Children.Count > 0)
                                //{
                                //    Debug.LogError(MarkCameraPreviewOutlineInfoList[m].OutlineName);

                                //}
                            }


                            break;
                        case "monitor_playback":
                            MainUserManager.creplay.isOn = true;
                            MonitorReplayList = item.areaList;
                            if (MonitorReplayList != null)
                            {
                                for (int i = 0; i < MonitorReplayList.Count; i++)
                                {
                                    SplitModifyCameraReplayGroupModel(MonitorReplayList[i]);//表示该角色具有录像回放中区域设备权限，选中录像回放模块时，用于标识出该区域下哪些设备是存在的（打钩）


                                }
                            }

                            break;
                        case "monitor_event":

                            MainUserManager.ccameraevent.isOn = true;
                            break;
                        case "door_event":

                            MainUserManager.cdoorevent.isOn = true;
                            break;
                        case "user_management":
                            MainUserManager.cusermanager.isOn = true;
                            break;
                        case "personnel_management":
                            MainUserManager.cpeoplemanager.isOn = true;
                            break;
                        default:
                            break;
                    }

                }
            }




        }


        //只要点击了角色列表的图标，就要把上一次遗留的监控树级目录删除。
        if (MainUserManager.TreeviewRoleManager.transform.childCount != 0)
        {
            for (int i = MainUserManager.TreeviewRoleManager.transform.childCount - 1; i >= 0; i--)
            {
                int temp = i;
                if (MainUserManager.TreeviewRoleManager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && MainUserManager.TreeviewRoleManager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                    TreeManagerUserManager.GetInstance().OnDeleteBtnClicked(MainUserManager.TreeviewRoleManager.transform.GetChild(temp).GetComponent<TreeViewItem>());
            }

        }

    }



    /// <summary>
    ///1、 初始化角色权限时如果预览勾选则会触发生成 监控区域分组结构
    /// </summary>
    /// <param name="ison"></param>
    void ppreviewtoglistener(bool ison)
    {
        if (isApplyingRolePermissions)
        {
            return;
        }
        if (ison)
        {
            activePermissionTreeMenuId = previewMdata.menuId;
            //请求预览模块区域分组关系
            requestCameraGroupDev();

        }

    }
    /// <summary>
    ///2、 先实例化监控预览区域设备层级架构
    /// </summary>
    private void requestCameraGroupDev()
    {

        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/dev/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraPreviewDevsCallback, false, false, false);

    }


    private void requestCameraGroupDevAllChoice()
    {

        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/dev/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraPreviewDevsCallbackAllChoice, false, false, false);

    }

    /// <summary>
    ///3、 获取监控预览区域分组设备信息http回调
    /// </summary>
    /// <param name="args"></param>
    private AreaDevsData areaDevsData;
    public void GetCameraPreviewDevsCallback(HttpCallBackArgs args)
    {
        PreviewoutlineInfoList.Clear();
        areaDevsData = null;
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            GameStart.Instance.ShowTip("监控区域树加载失败");
            return;
        }
        Log.Debug("角色管理界面服务器接收到的监控区域分组设备信息：" + args.Value);
        areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
        if (areaDevsData == null || areaDevsData.records == null)
        {
            GameStart.Instance.ShowTip("监控区域树数据格式错误");
            return;
        }
        previewAreaRoots = areaDevsData.records;
        if (activePermissionTreeMenuId == previewMdata.menuId)
        {
            InitRoleCameraPreviewGroup(PreviewoutlineInfoList, false);
        }

    }

    private AreaDevsData areaCameras;
    public void GetCameraPreviewDevsCallbackAllChoice(HttpCallBackArgs args)
    {
        PreviewoutlineInfoList.Clear();
        areaCameras = null;
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            GameStart.Instance.ShowTip("监控区域树加载失败");
            return;
        }
        Log.Debug("角色管理界面服务器接收到的监控区域分组设备信息：" + args.Value);
        areaCameras = JsonUtility.FromJson<AreaDevsData>(args.Value);
        if (areaCameras == null || areaCameras.records == null)
        {
            GameStart.Instance.ShowTip("监控区域树数据格式错误");
            return;
        }
        previewAreaRoots = areaCameras.records;
        if (activePermissionTreeMenuId == previewMdata.menuId)
        {
            InitRoleCameraPreviewGroup(PreviewoutlineInfoList, true, true);
        }

    }




    /// <summary>
    /// 4、角色界面  生成视频预览监控树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    static List<TreeViewItem> allCameraPreviewTreeViewItemList = new List<TreeViewItem>();//很重要，存储着对该层级元素所有的操作记录
    List<TreeViewItem> EquipCameraPreviewTreeViewItemList = new List<TreeViewItem>();
    static TreeViewItem itemPreviewPar = new TreeViewItem();
    static TreeViewItem itemPreviewGr = new TreeViewItem();
    public void InitRoleCameraPreviewGroup(List<OutlineInfo> outlineInfoList, bool IsallChoice = false, bool isonvalue = false)
    {
        if (IsallChoice)
        {
            previewSelectedKeys.Clear();
            if (isonvalue)
            {
                AddAllPermissionKeys(previewAreaRoots, previewSelectedKeys);
            }
        }
        RenderPermissionTree(previewAreaRoots, previewSelectedKeys, false);
    }

    public void InitRoleCameraPreviewGroupLegacy(List<OutlineInfo> outlineInfoList, bool IsallChoice=false,bool isonvalue=false)
    {
        if (previewMdata.areaList != null)
            previewMdata.areaList.Clear();
        allCameraPreviewTreeViewItemList.Clear();
        MarkCameraPreviewOutlineInfoList.Clear();
        EquipCameraPreviewTreeViewItemList = new List<TreeViewItem>();


        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            //Log.Error("元素:"+outlineInfoList[i].OutlineName);
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {
                TreeViewItem item1 = new TreeViewItem();
                item1 = MainUserManager.TreeviewRoleManager.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                Debug.LogError("预览分组添加：" + item1.GetComponent<ItemScript>().labelText.text);
                itemPreviewPar = item1;
                allCameraPreviewTreeViewItemList.Add(itemPreviewPar);
                //父级目录，即使没有选择设备信息，父级目录还是要打勾的。
                if (MarkCameraPreviewOutlineInfoList.Find(t => t.OutlineId == item1.GetComponent<ItemScript>().id) != null)
                {
                    item1.GetComponent<ItemScript>().choiceTog.isOn = true;
                }
                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();
                if (IsallChoice)
                {
                    Timer.Register(0.1f, () =>
                    {
                        item1.GetComponent<ItemScript>().choiceTog.isOn = isonvalue;
                    });
                }




                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                {
                    //ProductCameraTreeGroup(allCameraTreeViewItemList);
                    //全选该层级
                    if (ison)
                    {
                        for (int a = 0; a < allCameraPreviewTreeViewItemList.Count; a++)
                        {
                            if (allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>().parentId == item1.GetComponent<ItemScript>().id)
                            {
                                allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = true;

                            }
                        }
                        //来回标记
                        if (!MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId&&t.Level==""))
                        {

                            MarkCameraPreviewOutlineInfoList.Add(outlineInfoList[temp]);
                            Log.Error("添加父级:" + outlineInfoList[temp].OutlineName);
                        }
                      
                    }
                    else
                    {
                        Log.Debug("取消全选监控预览层级");

                        for (int a = 0; a < allCameraPreviewTreeViewItemList.Count; a++)
                        {
                            if (allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>().parentId == item1.GetComponent<ItemScript>().id)
                            {

                                allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = false;

                            }
                        }

                        //来回取消标记
                        if (MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId && t.Level == ""))
                        {
                            OutlineInfo outline = MarkCameraPreviewOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                            MarkCameraPreviewOutlineInfoList.Remove(outline);
                        }
                    }
                });
            }
            else
            {
                for (int j = 0; j < allCameraPreviewTreeViewItemList.Count; j++)
                {
                    int temp2 = j;
                    if (allCameraPreviewTreeViewItemList[temp2].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {
                        if (outlineInfoList[temp].Level == "equip")//设备
                        {
                            TreeViewItem childItem = new TreeViewItem();
                            childItem = allCameraPreviewTreeViewItemList[temp2].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;

                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();

                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {
                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allCameraPreviewTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && !viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                    }
                                    else if (viewItem.GetComponent<ItemScript>().devList == null)
                                    {
                                        viewItem.GetComponent<ItemScript>().devList = new List<NVRInformation>();
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                    }

                                    //来回标记
                                    if (!MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId&&t.Level== "equip"))
                                    {

                                        MarkCameraPreviewOutlineInfoList.Add(outlineInfoList[temp]);
                                        Log.Error("添加设备:" + outlineInfoList[temp].OutlineName);
                                    }
                                }
                                else
                                {
                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allCameraPreviewTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Remove(nvr);
                                    }

                                    //来回取消标记
                                    if (MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId && t.Level == "equip"))
                                    {
                                        OutlineInfo outline = MarkCameraPreviewOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                                        MarkCameraPreviewOutlineInfoList.Remove(outline);
                                    }

                                }

                            });

                            for (int n = 0; n < MarkCameraPreviewOutlineInfoList.Count; n++)
                            {
                                if (MarkCameraPreviewOutlineInfoList[n].OutlineId == childItem.GetComponent<ItemScript>().nvr.id.ToString() && MarkCameraPreviewOutlineInfoList[n].ParentId == childItem.GetComponent<ItemScript>().parentId)
                                {
                                    childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                                }
                            }

                            EquipCameraPreviewTreeViewItemList.Add(childItem);

                        }
                        else //分组 
                        {
                            TreeViewItem childItem = new TreeViewItem();
                            childItem = allCameraPreviewTreeViewItemList[temp2].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;

                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {
                                    for (int a = 0; a < allCameraPreviewTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>();
                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {
                                            item.choiceTog.isOn = true;

                                        }
                                        else
                                        {
                                            for (int b = 0; b < EquipCameraPreviewTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == EquipCameraPreviewTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    EquipCameraPreviewTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = true;
                                                }
                                            }

                                        }
                                    }


                                    //来回标记
                                    if (!MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId&&t.Level==""))
                                    {

                                        MarkCameraPreviewOutlineInfoList.Add(outlineInfoList[temp]);
                                        Log.Error("添加分组:" + outlineInfoList[temp].OutlineName);
                                    }

                                }
                                else
                                {
                                    for (int a = 0; a < allCameraPreviewTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allCameraPreviewTreeViewItemList[a].GetComponent<ItemScript>();
                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {
                                            item.choiceTog.isOn = false;

                                        }
                                        else
                                        {
                                            for (int b = 0; b < EquipCameraPreviewTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == EquipCameraPreviewTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    EquipCameraPreviewTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = false;
                                                }
                                            }

                                        }
                                    }

                                    //来回取消标记
                                    if (MarkCameraPreviewOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId&&t.Level==""))
                                    {
                                        OutlineInfo outline = MarkCameraPreviewOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                                        MarkCameraPreviewOutlineInfoList.Remove(outline);
                                    }

                                }


                            });
                            if (MarkCameraPreviewOutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                            Debug.LogError("预览分组添加：" + childItem.GetComponent<ItemScript>().labelText.text);
                            itemPreviewGr = childItem;
                            allCameraPreviewTreeViewItemList.Add(itemPreviewGr);
                        }


                    }
                }
            }
        }

    }


    void cpreviewlistener(bool ison)
    {
        if (isApplyingRolePermissions)
        {
            return;
        }
        if (ison)
        {
            activePermissionTreeMenuId = previewMdata.menuId;
            requestCameraGroupDev();

            if (!role.menuList.Contains(previewMdata))
                role.menuList.Add(previewMdata);//添加视频预览模块


        }
        else
        {

            DeleteRoleTreeItem();

            if (role.menuList.Contains(previewMdata))
                role.menuList.Remove(previewMdata);//删除视频预览模块

        }

    }

    //其他模块分组结构监听
    //----------------------------------------------------------------回放模块-----------------------------------------------------------------------------
    /// <summary>
    /// 1、
    /// </summary>
    /// <param name="ison"></param>
    void preplaytoglistener(bool ison)
    {
        if (isApplyingRolePermissions)
        {
            return;
        }
        if (ison)
        {
            activePermissionTreeMenuId = replayMdata.menuId;
            requestCameraReplayGroupDev();

        }

    }
    /// <summary>
    ///2、 实例化监控预览区域设备层级架构
    /// </summary>
    private void requestCameraReplayGroupDev()
    {

        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/dev/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", GetCameraReplayDevsCallback, false, false, false);

    }
    /// <summary>
    ///3、 获取监控回放区域分组设备信息http回调
    /// </summary>
    /// <param name="args"></param>
    public void GetCameraReplayDevsCallback(HttpCallBackArgs args)
    {
        ReplayoutlineInfoList.Clear();
        areaDevsData = null;
        if (args == null || args.HasError || string.IsNullOrEmpty(args.Value))
        {
            GameStart.Instance.ShowTip("录像回放区域树加载失败");
            return;
        }
        Log.Debug("角色管理界面服务器接收到的监控区域分组设备信息：" + args.Value);
        areaDevsData = JsonUtility.FromJson<AreaDevsData>(args.Value);
        if (areaDevsData == null || areaDevsData.records == null)
        {
            GameStart.Instance.ShowTip("录像回放区域树数据格式错误");
            return;
        }
        replayAreaRoots = areaDevsData.records;

        if (activePermissionTreeMenuId == replayMdata.menuId)
        {
            InitRoleCameraReplayGroup(ReplayoutlineInfoList);
        }

    }

    /// <summary>
    /// 4、角色界面  生成视频回放监控树级目录
    /// </summary>
    /// <param name="outlineInfoList"></param>
    static List<TreeViewItem> allCameraReplayTreeViewItemList = new List<TreeViewItem>();
    List<TreeViewItem> equipCameraReplayTreeViewItemList = new List<TreeViewItem>();
    TreeViewItem itemReplayPar = new TreeViewItem();
    TreeViewItem itemReplayGr = new TreeViewItem();
    public void InitRoleCameraReplayGroup(List<OutlineInfo> outlineInfoList)
    {
        RenderPermissionTree(replayAreaRoots, replaySelectedKeys, true);
    }

    public void InitRoleCameraReplayGroupLegacy(List<OutlineInfo> outlineInfoList)
    {
        if (replayMdata.areaList != null)
            replayMdata.areaList.Clear();
        allCameraReplayTreeViewItemList.Clear();
        equipCameraReplayTreeViewItemList = new List<TreeViewItem>();
        TreeViewItem item1 = new TreeViewItem();

        for (int i = 0; i < outlineInfoList.Count; i++)
        {
            int temp = i;
            if (string.IsNullOrEmpty(outlineInfoList[temp].ParentId) || int.Parse(outlineInfoList[temp].ParentId) == 0)
            {
                item1 = MainUserManager.TreeviewRoleManager.AppendItem("ItemPrefab1");
                item1.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                item1.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                item1.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                item1.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                item1.GetComponent<ItemScript>().SetItem(item1.GetComponent<ItemScript>());
                Debug.LogError("回放分组添加：" + item1.GetComponent<ItemScript>().id);

                itemReplayPar = item1;
                allCameraReplayTreeViewItemList.Add(itemReplayPar);
                //父级目录，即使没有选择设备信息，父级目录还是要打勾的。
                if (MarkCameraReplayOutlineInfoList.Find(t => t.OutlineId == item1.GetComponent<ItemScript>().id) != null)
                {

                    item1.GetComponent<ItemScript>().choiceTog.isOn = true;
                }
                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();
                item1.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                {
                    //ProductCameraTreeGroup(allCameraTreeViewItemList);
                    //全选该层级
                    if (ison)
                    {
                        for (int a = 0; a < allCameraReplayTreeViewItemList.Count; a++)
                        {
                            if (allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>().parentId == item1.GetComponent<ItemScript>().id)
                            {
                                allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = true;

                            }
                        }

                        //来回标记
                        if (!MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                        {

                            MarkCameraReplayOutlineInfoList.Add(outlineInfoList[temp]);
                        }
                    }
                    else
                    {
                        Log.Debug("取消全选监控预览层级");
                        for (int a = 0; a < allCameraReplayTreeViewItemList.Count; a++)
                        {
                            if (allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>().parentId == item1.GetComponent<ItemScript>().id)
                            {

                                allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>().choiceTog.isOn = false;

                            }
                        }
                        //来回取消标记
                        if (MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                        {
                            OutlineInfo outline = MarkCameraReplayOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                            MarkCameraReplayOutlineInfoList.Remove(outline);
                        }
                    }
                });
            }
            else
            {
                for (int j = 0; j < allCameraReplayTreeViewItemList.Count; j++)
                {
                    int temp2 = j;
                    if (allCameraReplayTreeViewItemList[temp2].GetComponent<ItemScript>().id.Equals(outlineInfoList[temp].ParentId))
                    {
                        if (outlineInfoList[temp].Level == "equip")//设备
                        {
                            TreeViewItem childItem = allCameraReplayTreeViewItemList[temp2].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;

                            childItem.GetComponent<ItemScript>().icon.sprite = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
                            childItem.GetComponent<ItemScript>().labelText.interactable = false;
                            childItem.GetComponent<ItemScript>().nvr = outlineInfoList[temp].NVr;
                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.RemoveAllListeners();

                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {

                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allCameraReplayTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && !viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                    }
                                    else if (viewItem.GetComponent<ItemScript>().devList == null)
                                    {
                                        viewItem.GetComponent<ItemScript>().devList = new List<NVRInformation>();
                                        viewItem.GetComponent<ItemScript>().devList.Add(nvr);
                                    }

                                    //来回标记
                                    if (!MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                                    {

                                        MarkCameraReplayOutlineInfoList.Add(outlineInfoList[temp]);
                                    }
                                }
                                else
                                {
                                    NVRInformation nvr = new NVRInformation();
                                    nvr.cameraname = outlineInfoList[temp].OutlineName;
                                    nvr.id = int.Parse(outlineInfoList[temp].OutlineId);
                                    TreeViewItem viewItem = allCameraReplayTreeViewItemList.Find(t => t.GetComponent<ItemScript>().id == outlineInfoList[temp].ParentId);
                                    if (viewItem.GetComponent<ItemScript>().devList != null && viewItem.GetComponent<ItemScript>().devList.Exists(t => t.id == nvr.id))
                                    {
                                        viewItem.GetComponent<ItemScript>().devList.Remove(nvr);
                                    }


                                    //来回取消标记
                                    if (MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                                    {
                                        OutlineInfo outline = MarkCameraReplayOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                                        MarkCameraReplayOutlineInfoList.Remove(outline);
                                    }

                                }

                            });

                            for (int n = 0; n < MarkCameraReplayOutlineInfoList.Count; n++)
                            {
                                if (MarkCameraReplayOutlineInfoList[n].OutlineId == childItem.GetComponent<ItemScript>().nvr.id.ToString() && MarkCameraReplayOutlineInfoList[n].ParentId == childItem.GetComponent<ItemScript>().parentId)
                                {
                                    childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                                }
                            }


                            equipCameraReplayTreeViewItemList.Add(childItem);

                        }
                        else //分组 
                        {
                            TreeViewItem childItem = allCameraReplayTreeViewItemList[temp2].ChildTree.AppendItem("ItemPrefab1");
                            childItem.GetComponent<ItemScript>().id = outlineInfoList[temp].OutlineId;
                            childItem.GetComponent<ItemScript>().parentId = outlineInfoList[temp].ParentId;
                            childItem.GetComponent<ItemScript>().labelText.text = outlineInfoList[temp].OutlineName;
                            childItem.GetComponent<ItemScript>().SetItem(childItem.GetComponent<ItemScript>());
                            childItem.GetComponent<ItemScript>().type = outlineInfoList[temp].Type;
                            childItem.GetComponent<ItemScript>().children = outlineInfoList[temp].Children;
                            childItem.GetComponent<ItemScript>().devList = outlineInfoList[temp].DevList;

                            childItem.GetComponent<ItemScript>().choiceTog.onValueChanged.AddListener((ison) =>
                            {
                                if (ison)
                                {
                                    for (int a = 0; a < allCameraReplayTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>();
                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {
                                            item.choiceTog.isOn = true;

                                        }
                                        else
                                        {
                                            for (int b = 0; b < equipCameraReplayTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == equipCameraReplayTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    equipCameraReplayTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = true;
                                                }
                                            }

                                        }
                                    }


                                    //来回标记
                                    if (!MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                                    {

                                        MarkCameraReplayOutlineInfoList.Add(outlineInfoList[temp]);
                                    }

                                }
                                else
                                {
                                    for (int a = 0; a < allCameraReplayTreeViewItemList.Count; a++)
                                    {
                                        ItemScript item = allCameraReplayTreeViewItemList[a].GetComponent<ItemScript>();
                                        if (item.parentId == childItem.GetComponent<ItemScript>().id)
                                        {
                                            item.choiceTog.isOn = false;

                                        }
                                        else
                                        {
                                            for (int b = 0; b < equipCameraReplayTreeViewItemList.Count; b++)
                                            {
                                                if (childItem.GetComponent<ItemScript>().id == equipCameraReplayTreeViewItemList[b].GetComponent<ItemScript>().parentId)
                                                {
                                                    equipCameraReplayTreeViewItemList[b].GetComponent<ItemScript>().choiceTog.isOn = false;
                                                }
                                            }

                                        }
                                    }


                                    //来回取消标记
                                    if (MarkCameraReplayOutlineInfoList.Exists(t => t.OutlineId == outlineInfoList[temp].OutlineId))
                                    {
                                        OutlineInfo outline = MarkCameraReplayOutlineInfoList.Find(t => t.OutlineId == outlineInfoList[temp].OutlineId);
                                        MarkCameraReplayOutlineInfoList.Remove(outline);
                                    }

                                }


                            });
                            if (MarkCameraReplayOutlineInfoList.Find(t => t.OutlineId == childItem.GetComponent<ItemScript>().id) != null)
                            {
                                childItem.GetComponent<ItemScript>().choiceTog.isOn = true;
                            }
                            Debug.LogError("回放分组添加：" + childItem.GetComponent<ItemScript>().id);

                            itemReplayGr = childItem;
                            allCameraReplayTreeViewItemList.Add(itemReplayGr);
                        }


                    }
                }
            }
        }





    }



    void creplaylistener(bool ison)
    {
        if (isApplyingRolePermissions)
        {
            return;
        }
        if (ison)
        {
            activePermissionTreeMenuId = replayMdata.menuId;
            requestCameraReplayGroupDev();

            if (!role.menuList.Contains(replayMdata))
                role.menuList.Add(replayMdata);//添加回放模块
        }
        else
        {
            DeleteRoleTreeItem();
            if (role.menuList.Contains(replayMdata))
                role.menuList.Remove(replayMdata);//删除回放模块



        }
    }

    void palltoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();
        }

    }
    void calllistener(bool ison)
    {
        if (ison)
        {

            MainUserManager.cmonitorSet.isOn = true;
            MainUserManager.cdoorSet.isOn = true;
            MainUserManager.cpreview.isOn = true;
            MainUserManager.creplay.isOn = true;
            MainUserManager.ccameraevent.isOn = true;
            MainUserManager.cdoorevent.isOn = true;
            MainUserManager.cusermanager.isOn = true;
            MainUserManager.cpeoplemanager.isOn = true;

        }
        else
        {
            MainUserManager.cmonitorSet.isOn = false;
            MainUserManager.cdoorSet.isOn = false;
            MainUserManager.cpreview.isOn = false;
            MainUserManager.creplay.isOn = false;
            MainUserManager.ccameraevent.isOn = false;
            MainUserManager.cdoorevent.isOn = false;
            MainUserManager.cusermanager.isOn = false;
            MainUserManager.cpeoplemanager.isOn = false;

        }
        DeleteRoleTreeItem();
    }

    void pmonitortoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();
        }


    }
    void cmonitorSetlistener(bool ison)
    {
        if (ison)
        {
            Log.Debug("开启监控点配置");
            if (!role.menuList.Exists(t => t.menuName == CameraSetPath))
                role.menuList.Add(camerasetMdata);//添加监控配置模块
        }
        else
        {
            if (role.menuList.Exists(t => t.menuName == CameraSetPath))
                role.menuList.Remove(camerasetMdata);//删除监控配置模块

        }

    }

    void pdoortoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();
        }

    }
    void cdoorSetlistener(bool ison)
    {
        if (ison)
        {
            if (!role.menuList.Contains(doorsetMdata))
                role.menuList.Add(doorsetMdata);//添加门禁配置模块

        }
        else
        {
            if (role.menuList.Contains(doorsetMdata))
                role.menuList.Remove(doorsetMdata);//删除门禁配置模块



        }

    }

    void pcameraeventtoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();


        }

    }
    void cCameraEventlistener(bool ison)
    {
        if (ison)
        {
            MainUserManager.pcameraeventtog.onValueChanged.Invoke(true);

            if (!role.menuList.Contains(cameraeventMdata))
                role.menuList.Add(cameraeventMdata);//添加监控事件模块
        }
        else
        {
            if (role.menuList.Contains(cameraeventMdata))
                role.menuList.Remove(cameraeventMdata);//删除监控事件模块

        }

    }

    void pdooreventtoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();

        }

    }
    void cDoorEventlistener(bool ison)
    {
        if (ison)
        {
            MainUserManager.pdooreventtog.onValueChanged.Invoke(true);

            if (!role.menuList.Contains(dooreventMdata))
                role.menuList.Add(dooreventMdata);//添加门禁事件模块
        }
        else
        {
            if (role.menuList.Contains(dooreventMdata))
                role.menuList.Remove(dooreventMdata);//删除门禁事件模块


        }
    }


    void pusermanagertoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();
        }


    }
    void cUserManagerlistener(bool ison)
    {
        if (ison)
        {
            if (!role.menuList.Contains(UserManagerMdata))
                role.menuList.Add(UserManagerMdata);//添加用户管理模块

        }
        else
        {
            if (role.menuList.Contains(UserManagerMdata))
                role.menuList.Remove(UserManagerMdata);//删除用户管理模块



        }
    }


    void pPeopleManagertoglistener(bool ison)
    {
        if (ison)
        {
            DeleteRoleTreeItem();
        }

    }
    void cPeopleManagerlistener(bool ison)
    {
        if (ison)
        {
            if (!role.menuList.Contains(peopleManagerMdata))
                role.menuList.Add(peopleManagerMdata);//添加人员管理模块

        }
        else
        {
            if (role.menuList.Contains(peopleManagerMdata))
                role.menuList.Remove(peopleManagerMdata);//删除人员管理模块



        }
    }



    #region 模块初始化
    //新规则  模块 1
    void InitPreviewdata()//预览初始化
    {

        //previewMdata.id = 1;
        previewMdata.menuName = PreviewPath;
        previewMdata.menuId = 14;
        //previewMdata.parentId = 0;

    }
    void InitReplaydata()//回放初始化
    {
        replayMdata.menuName = ReplayPath;
        replayMdata.menuId = 15;

    }
    void InitCameraEventdata()//监控事件初始化
    {
        cameraeventMdata.menuName = CameraEventPath;
        cameraeventMdata.menuId = 16;

    }
    void InitDoorEventdata()//门禁事件初始化
    {
        dooreventMdata.menuName = DoorEventPath;
        dooreventMdata.menuId = 17;

    }
    void InitDoorSetdata()//门禁配置初始化
    {

        doorsetMdata.menuName = DoorSetPath;
        doorsetMdata.menuId = 13;

    }
    void InitCameraSetdata()//监控配置初始化
    {

        camerasetMdata.menuName = CameraSetPath;
        camerasetMdata.menuId = 12;

    }
    void InitPeopleManagerdata()//人员管理初始化
    {

        peopleManagerMdata.menuName = PeopleManagerPath;
        peopleManagerMdata.menuId = 18;

    }
    void InitUserManagerdata()//用户管理初始化
    {
        UserManagerMdata.menuName = UserManagerPath;
        UserManagerMdata.menuId = 19;

    }
    #endregion

    #region 功能性脚本

    /// <summary>
    /// 删除角色展示模块层级目录
    /// </summary>
    private void DeleteRoleTreeItem()
    {
        activePermissionTreeMenuId = 0;
        for (int i = MainUserManager.TreeviewRoleManager.transform.childCount - 1; i >= 0; i--)
        {
            int temp = i;
            if (MainUserManager.TreeviewRoleManager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && MainUserManager.TreeviewRoleManager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                TreeManagerUserManager.GetInstance().OnDeleteBtnClicked(MainUserManager.TreeviewRoleManager.transform.GetChild(temp).GetComponent<TreeViewItem>());
            //GameObject.Destroy(MainUserManager.TreeviewRoleManager.transform.GetChild(temp).gameObject);

        }

    }
    /// <summary>
    /// 删除用户展示模块层级目录
    /// </summary>
    private void DeleteUserTreeItem()
    {
        for (int i = MainUserManager.TreeviewUserManager.transform.childCount - 1; i >= 0; i--)
        {
            int temp = i;
            if (MainUserManager.TreeviewUserManager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && MainUserManager.TreeviewUserManager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                TreeManagerUserManager.GetInstance().OnDeleteBtnClicked(MainUserManager.TreeviewUserManager.transform.GetChild(temp).GetComponent<TreeViewItem>());
        }

    }


    /// <summary>
    /// 递归拆分监控预览层及目录架构    *重要
    /// </summary>
    /// <param name="devs"></param>
    List<OutlineInfo> PreviewoutlineInfoList = new List<OutlineInfo>();
    private void SplitCameraPreviewTreeView(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        //outline.Type = 1;
        outline.DevList = devs.devList;
        PreviewoutlineInfoList.Add(outline);
        for (int j = 0; j < outline.DevList.Count; j++)
        {
            OutlineInfo outlinedev = new OutlineInfo();
            outlinedev.OutlineId = outline.DevList[j].id.ToString();
            //outlinedev.OutlineId = "";
            outlinedev.Level = "equip";
            outlinedev.ParentId = outline.OutlineId;
            outlinedev.OutlineName = outline.DevList[j].cameraname;
            outlinedev.Children = null;
            outlinedev.DevList = null;
            outlinedev.NVr = outline.DevList[j];
            PreviewoutlineInfoList.Add(outlinedev);
        }

        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                SplitCameraPreviewTreeView(devs.children[i]);
            }
        }

    }
    /// <summary>
    /// 递归拆分监控预览已经勾选的层级目录架构，用于判断该区域模块是否已经是选择状态    *重要
    /// </summary>
    /// <param name="devs">传入树级层及目录</param>
    public List<OutlineInfo> MarkCameraPreviewOutlineInfoList = new List<OutlineInfo>(); //OutlineInfoList标记出预览哪些是已经勾选的
    private void SplitModifyCameraPreviewGroupModel(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        outline.Type = 2;
        outline.DevList = devs.devList;
        MarkCameraPreviewOutlineInfoList.Add(outline);
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
                MarkCameraPreviewOutlineInfoList.Add(outlinedev);
            }
        }

        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                SplitModifyCameraPreviewGroupModel(devs.children[i]);
            }
        }

    }



    /// <summary>
    ///  递归拆分录像回放层及目录架构    *重要
    /// </summary>
    List<OutlineInfo> ReplayoutlineInfoList = new List<OutlineInfo>();
    private void SplitCameraReplayTreeView(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        //outline.Type = 1;
        outline.DevList = devs.devList;
        ReplayoutlineInfoList.Add(outline);
        for (int j = 0; j < outline.DevList.Count; j++)
        {
            OutlineInfo outlinedev = new OutlineInfo();
            outlinedev.OutlineId = outline.DevList[j].id.ToString();
            //outlinedev.OutlineId = "";
            outlinedev.Level = "equip";
            outlinedev.ParentId = outline.OutlineId;
            outlinedev.OutlineName = outline.DevList[j].cameraname;
            outlinedev.Children = null;
            outlinedev.DevList = null;
            outlinedev.NVr = outline.DevList[j];
            ReplayoutlineInfoList.Add(outlinedev);
        }

        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                SplitCameraReplayTreeView(devs.children[i]);
            }
        }

    }
    /// <summary>
    /// 递归拆分监控回放已经勾选的层级目录架构，用于判断该区域模块是否已经是选择状态    *重要
    /// </summary>
    /// <param name="devs">传入树级层及目录</param>
    public List<OutlineInfo> MarkCameraReplayOutlineInfoList = new List<OutlineInfo>(); //OutlineInfoList标记出回放哪些是已经勾选的
    private void SplitModifyCameraReplayGroupModel(GetAreaGroupDevs devs)
    {

        OutlineInfo outline = new OutlineInfo();
        outline.OutlineId = devs.id.ToString();
        outline.ParentId = devs.parentId.ToString();
        outline.OutlineName = devs.areaName;
        outline.Children = devs.children;
        outline.Type = 2;
        outline.DevList = devs.devList;
        MarkCameraReplayOutlineInfoList.Add(outline);
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
                MarkCameraReplayOutlineInfoList.Add(outlinedev);
            }
        }

        if (devs.children.Count != 0)
        {
            for (int i = 0; i < devs.children.Count; i++)
            {
                SplitModifyCameraReplayGroupModel(devs.children[i]);
            }
        }

    }

    private static string AreaPermissionKey(int areaId)
    {
        return "area:" + areaId;
    }

    private static string DevicePermissionKey(int areaId, int deviceId)
    {
        return "device:" + areaId + ":" + deviceId;
    }

    private void RenderPermissionTree(List<GetAreaGroupDevs> roots, HashSet<string> selectedKeys, bool isReplay)
    {
        DeleteRoleTreeItem();
        permissionTreeItems.Clear();
        permissionTreeParents.Clear();
        permissionTreeChildren.Clear();
        permissionTreeIsReplay = isReplay;
        allCameraPreviewTreeViewItemList.Clear();
        EquipCameraPreviewTreeViewItemList.Clear();
        allCameraReplayTreeViewItemList.Clear();
        equipCameraReplayTreeViewItemList.Clear();

        if (roots == null)
        {
            return;
        }

        suppressPermissionTreeEvents = true;
        try
        {
            for (int i = 0; i < roots.Count; i++)
            {
                AppendPermissionArea(roots[i], MainUserManager.TreeviewRoleManager, string.Empty, selectedKeys, isReplay);
            }
        }
        finally
        {
            suppressPermissionTreeEvents = false;
        }
    }

    private void AppendPermissionArea(GetAreaGroupDevs area, TreeList targetTree, string parentKey,
        HashSet<string> selectedKeys, bool isReplay)
    {
        if (area == null)
        {
            return;
        }

        string areaKey = AreaPermissionKey(area.id);
        if (permissionTreeItems.ContainsKey(areaKey))
        {
            Log.Error("权限树存在重复区域ID，已跳过: " + area.id);
            return;
        }
        TreeViewItem areaItem = targetTree.AppendItem("ItemPrefab1");
        ItemScript areaScript = areaItem.GetComponent<ItemScript>();
        areaScript.id = area.id.ToString();
        areaScript.parentId = area.parentId.ToString();
        areaScript.level = area.level;
        areaScript.type = area.areaType;
        areaScript.labelText.text = area.areaName;
        areaScript.children = area.children;
        areaScript.devList = area.devList;
        areaScript.SetItem(areaScript);
        RegisterPermissionTreeItem(areaKey, parentKey, areaItem, selectedKeys, isReplay, false);

        if (isReplay)
        {
            allCameraReplayTreeViewItemList.Add(areaItem);
        }
        else
        {
            allCameraPreviewTreeViewItemList.Add(areaItem);
        }

        if (area.children != null)
        {
            for (int i = 0; i < area.children.Count; i++)
            {
                AppendPermissionArea(area.children[i], areaItem.ChildTree, areaKey, selectedKeys, isReplay);
            }
        }

        if (area.devList == null)
        {
            return;
        }

        for (int i = 0; i < area.devList.Count; i++)
        {
            NVRInformation device = area.devList[i];

            string deviceKey = DevicePermissionKey(area.id, device.id);
            if (permissionTreeItems.ContainsKey(deviceKey))
            {
                continue;
            }
            TreeViewItem deviceItem = areaItem.ChildTree.AppendItem("ItemPrefab1");
            ItemScript deviceScript = deviceItem.GetComponent<ItemScript>();
            deviceScript.id = device.id.ToString();
            deviceScript.parentId = area.id.ToString();
            deviceScript.level = "equip";
            deviceScript.labelText.text = device.cameraname;
            deviceScript.nvr = device;
            deviceScript.SetItem(deviceScript);
            if (permissionCameraIcon == null)
            {
                permissionCameraIcon = Resources.Load<Sprite>("UI/录像回放/摄像头-0");
            }
            deviceScript.icon.sprite = permissionCameraIcon;
            deviceScript.labelText.interactable = false;
            RegisterPermissionTreeItem(deviceKey, areaKey, deviceItem, selectedKeys, isReplay, true);

            if (isReplay)
            {
                equipCameraReplayTreeViewItemList.Add(deviceItem);
            }
            else
            {
                EquipCameraPreviewTreeViewItemList.Add(deviceItem);
            }
        }
    }

    private void RegisterPermissionTreeItem(string key, string parentKey, TreeViewItem treeItem,
        HashSet<string> selectedKeys, bool isReplay, bool isDevice)
    {
        permissionTreeItems[key] = treeItem;
        if (!string.IsNullOrEmpty(parentKey))
        {
            permissionTreeParents[key] = parentKey;
            List<string> children;
            if (!permissionTreeChildren.TryGetValue(parentKey, out children))
            {
                children = new List<string>();
                permissionTreeChildren[parentKey] = children;
            }
            children.Add(key);
        }

        ItemScript itemScript = treeItem.GetComponent<ItemScript>();
        itemScript.choiceTog.onValueChanged.RemoveAllListeners();
        itemScript.choiceTog.isOn = selectedKeys.Contains(key);
        itemScript.choiceTog.onValueChanged.AddListener(isOn =>
        {
            HandlePermissionTreeToggle(key, isOn, isReplay);
        });
    }

    private void HandlePermissionTreeToggle(string key, bool isOn, bool isReplay)
    {
        if (suppressPermissionTreeEvents || permissionTreeIsReplay != isReplay)
        {
            return;
        }

        HashSet<string> selectedKeys = isReplay ? replaySelectedKeys : previewSelectedKeys;
        SetPermissionSelection(key, isOn, selectedKeys);

        List<string> descendants;
        if (!permissionTreeChildren.TryGetValue(key, out descendants))
        {
            descendants = new List<string>();
        }
        for (int i = 0; i < descendants.Count; i++)
        {
            SetPermissionSubtree(descendants[i], isOn, selectedKeys);
        }

        UpdatePermissionAncestors(key, selectedKeys);
    }

    private void SetPermissionSubtree(string key, bool isOn, HashSet<string> selectedKeys)
    {
        SetPermissionSelection(key, isOn, selectedKeys);
        List<string> children;
        if (!permissionTreeChildren.TryGetValue(key, out children))
        {
            return;
        }
        for (int i = 0; i < children.Count; i++)
        {
            SetPermissionSubtree(children[i], isOn, selectedKeys);
        }
    }

    private void SetPermissionSelection(string key, bool isOn, HashSet<string> selectedKeys)
    {
        if (isOn)
        {
            selectedKeys.Add(key);
        }
        else
        {
            selectedKeys.Remove(key);
        }

        TreeViewItem treeItem;
        if (permissionTreeItems.TryGetValue(key, out treeItem) && treeItem != null)
        {
            suppressPermissionTreeEvents = true;
            try
            {
                treeItem.GetComponent<ItemScript>().choiceTog.isOn = isOn;
            }
            finally
            {
                suppressPermissionTreeEvents = false;
            }
        }
    }

    private void UpdatePermissionAncestors(string key, HashSet<string> selectedKeys)
    {
        string parentKey;
        while (permissionTreeParents.TryGetValue(key, out parentKey))
        {
            List<string> children;
            bool hasSelectedChild = permissionTreeChildren.TryGetValue(parentKey, out children) &&
                children.Any(selectedKeys.Contains);
            SetPermissionSelection(parentKey, hasSelectedChild, selectedKeys);
            key = parentKey;
        }
    }

    private static void AddAllPermissionKeys(List<GetAreaGroupDevs> areas, HashSet<string> selectedKeys)
    {
        if (areas == null)
        {
            return;
        }
        for (int i = 0; i < areas.Count; i++)
        {
            GetAreaGroupDevs area = areas[i];
            if (area == null)
            {
                continue;
            }
            selectedKeys.Add(AreaPermissionKey(area.id));
            if (area.devList != null)
            {
                for (int j = 0; j < area.devList.Count; j++)
                {
                    selectedKeys.Add(DevicePermissionKey(area.id, area.devList[j].id));
                }
            }
            AddAllPermissionKeys(area.children, selectedKeys);
        }
    }

    private static void LoadPermissionKeys(List<GetAreaGroupDevs> areas, HashSet<string> selectedKeys)
    {
        selectedKeys.Clear();
        AddAllPermissionKeys(areas, selectedKeys);
    }

    private static List<GetAreaGroupDevs> BuildSelectedAreaList(List<GetAreaGroupDevs> sourceAreas,
        HashSet<string> selectedKeys)
    {
        List<GetAreaGroupDevs> result = new List<GetAreaGroupDevs>();
        if (sourceAreas == null)
        {
            return result;
        }

        for (int i = 0; i < sourceAreas.Count; i++)
        {
            GetAreaGroupDevs selectedArea = BuildSelectedArea(sourceAreas[i], selectedKeys);
            if (selectedArea != null)
            {
                result.Add(selectedArea);
            }
        }
        return result;
    }

    private static GetAreaGroupDevs BuildSelectedArea(GetAreaGroupDevs source, HashSet<string> selectedKeys)
    {
        if (source == null)
        {
            return null;
        }

        List<GetAreaGroupDevs> selectedChildren = BuildSelectedAreaList(source.children, selectedKeys);
        List<NVRInformation> selectedDevices = new List<NVRInformation>();
        if (source.devList != null)
        {
            for (int i = 0; i < source.devList.Count; i++)
            {
                NVRInformation device = source.devList[i];
                if (selectedKeys.Contains(DevicePermissionKey(source.id, device.id)))
                {
                    selectedDevices.Add(device);
                }
            }
        }

        if (!selectedKeys.Contains(AreaPermissionKey(source.id)) && selectedChildren.Count == 0 && selectedDevices.Count == 0)
        {
            return null;
        }

        GetAreaGroupDevs result = new GetAreaGroupDevs();
        result.id = source.id;
        result.areaName = source.areaName;
        result.remark = source.remark;
        result.areaType = source.areaType;
        result.parentId = source.parentId;
        result.level = source.level;
        result.devList = selectedDevices;
        result.children = selectedChildren;
        return result;
    }

    //把TreeViewItem集合拼成树级结构  List<GetAreaGroupDevs>
    private List<GetAreaGroupDevs> ProductCameraTreeGroup(List<TreeViewItem> allDoorTreeViewItem, int parentId = 0)
    {
        if (allDoorTreeViewItem != null && allDoorTreeViewItem.Count > 0)
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
        else
        {
            return new List<GetAreaGroupDevs>();

        }



    }

    //把OutlineInfo集合拼成树级结构 
    private List<GetAreaGroupDevs> ProductCameraTreeByOutline(List<OutlineInfo> outlineInfoList,int parentId=0)
    {

        if (outlineInfoList!=null&&outlineInfoList.Count>0)
        {
            var ItemList = outlineInfoList.Where(x=> 
            {
                return parentId.Equals(int.Parse(x.ParentId));
            
            });

            List<GetAreaGroupDevs> Newgroups = new List<GetAreaGroupDevs>();
            foreach (var item in ItemList)
            {
                var view = new GetAreaGroupDevs();
                if (!string.IsNullOrEmpty(item.OutlineId))
                    view.id = int.Parse(item.OutlineId);
                view.level = item.Level;
                view.parentId = int.Parse(item.ParentId);

                view.devList = item.DevList;
                if (int.Parse(item.OutlineId) != 0 && item.Children != null&&item.Children.Count>0)
                  view.children = ProductCameraTreeByOutline(outlineInfoList, int.Parse(item.OutlineId));
                view.areaType = item.Type;
                view.areaName = item.OutlineName;
                Newgroups.Add(view);

            }
            return Newgroups;
        }
        else 
        {
            return new List<GetAreaGroupDevs>();

        }


    
    
    }





    #endregion



    #region        角色管理逻辑结束











    //--------------------------------------------------------以前的逻辑----------------------------------------------------------------








    public AreaGroupDevList DoorAreaGroupDevs;


    #endregion





    /// <summary>
    /// 用户管理界面
    /// </summary>
    /// <param name="ison"></param>
    void UserManPanelShow(bool ison)
    {
        if (ison)
        {
            previewMdata.areaList.Clear();//模块容器先清空
            replayMdata.areaList.Clear();
            camerasetMdata.areaList.Clear();
            doorsetMdata.areaList.Clear();
            cameraeventMdata.areaList.Clear();
            dooreventMdata.areaList.Clear();
            UserManagerMdata.areaList.Clear();
            peopleManagerMdata.areaList.Clear();
            if (MainUserManager.userContent.childCount != 0)
            {
                for (int i = 0; i < MainUserManager.userContent.childCount; i++)
                {
                    GameObject.Destroy(MainUserManager.userContent.GetChild(i).gameObject);
                }//先清空
            }
            MainUserManager.transform.Find("user/AuthSetPage/PasswordInput").gameObject.SetActive(false);//隐藏密码
            MainUserManager.transform.Find("user/AuthSetPage/RePasswordInput").gameObject.SetActive(false);
            //再请求
            HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/all/sys/user", GetUsersCallback, false, false);

            //正处于新建用户时切换界面需要重置信息
            MainUserManager.U_PasswordInput.gameObject.SetActive(false);
            MainUserManager.U_RePasswordInput.gameObject.SetActive(false);
            MainUserManager.U_ChoiceTime.gameObject.SetActive(false);
            MainUserManager.U_UsernameInput.text = "";
            for (int i = 0; i < user_roleContent.childCount; i++)
            {
                GameObject.Destroy(user_roleContent.GetChild(i).gameObject);
            }

        }
    }
    //获取所有的用户回调
    private long Userid = -1;

    void GetUsersCallback(HttpCallBackArgs args)
    {
        Log.Debug("获取用户:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            userlist users = JsonUtility.FromJson<userlist>(args.Value);
            if (users.code == 200)
            {
                Log.Debug(args.Value);
                Log.Debug(users.data.Count);
                for (int i = 0; i < users.data.Count; i++)
                {
                    int index = i;
                    GameObject user = ObjectManager.Instance.InstantiateObject(ConStr.USERTOG);
                    user.transform.SetParent(MainUserManager.userContent);
                    resetPrefab(user);
                    user.transform.Find("nameTxt").GetComponent<TextMeshProUGUI>().text = users.data[index].userName;
                    user.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    user.GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
                    {
                        //获取该用户的信息
                        if (ison)
                        {
                            MainUserManager.transform.Find("user/AuthSetPage/UserNameInput").GetComponent<TMP_InputField>().text = users.data[index].userName;
                            Userid = users.data[index].id;
                            MainUserManager.U_PasswordInput.gameObject.SetActive(false);
                            MainUserManager.U_RePasswordInput.gameObject.SetActive(false);
                            MainUserManager.U_ChoiceTime.gameObject.SetActive(false);
                            string rolename = users.data[index].roleName;
                            Log.Debug(users.data[index].userName + "的权限是:" + users.data[index].id + "用户id是:" + Userid);
                            for (int j = 0; j < user_roleContent.childCount; j++)
                            {
                                GameObject.Destroy(user_roleContent.GetChild(j).gameObject);
                            }
                            for (int j = 0; j < MainUserManager.UserMenuContainer.childCount; j++)
                            {
                                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(j).gameObject);
                            }
                            Userrefrushtog(false);
                            previewMdata.areaList.Clear();//模块容器先清空
                            replayMdata.areaList.Clear();
                            camerasetMdata.areaList.Clear();
                            doorsetMdata.areaList.Clear();
                            cameraeventMdata.areaList.Clear();
                            dooreventMdata.areaList.Clear();
                            UserManagerMdata.areaList.Clear();
                            peopleManagerMdata.areaList.Clear();
                            if (!string.IsNullOrEmpty(rolename))
                            {

                                GameObject roleob = ObjectManager.Instance.InstantiateObject(ConStr.USERTOG);
                                roleob.transform.SetParent(user_roleContent);
                                resetPrefab(roleob);
                                roleob.transform.Find("nameTxt").GetComponent<TextMeshProUGUI>().text = rolename;
                                //请求角色权限
                                roleob.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                                roleob.GetComponent<Toggle>().onValueChanged.AddListener((value) =>
                                {
                                    if (value)
                                    {
                                        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/tg/role/info?roleId=" + users.data[index].roleId, UserGetRoleAuthCallback, false, false, false);
                                    }
                                });

                            }
                            else
                            {
                                GameStart.Instance.ShowTip("该人员创建时没有绑定角色信息！");
                            }


                        }
                    });
                    user.GetComponent<Toggle>().group = MainUserManager.userContent.GetComponent<ToggleGroup>();
                }
            }
            else
            {
                GameStart.Instance.ShowTip(users.msg);
            }
        }
    }
    public bool IsAddUser = false;
    void AddUser()
    {
        IsAddUser = true;
        Userid = -1;
        ChoiceRole = -1;
        //GameObject userObj = ObjectManager.Instance.InstantiateObject(ConStr.USERTOG);
        //userObj.transform.SetParent(MainUserManager.userContent);
        //resetPrefab(userObj);
        MainUserManager.U_UsernameInput.text = "新建用户";
        MainUserManager.U_PasswordInput.gameObject.SetActive(true);
        MainUserManager.U_RePasswordInput.gameObject.SetActive(true);
        MainUserManager.U_ChoiceTime.gameObject.SetActive(true);
        //清空角色名字
        for (int i = 0; i < user_roleContent.childCount; i++)
        {
            GameObject.Destroy(user_roleContent.GetChild(i).gameObject);
        }
        //获取所有的权限
        HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/tg/all/role/name", UserGetAllRolesNameCallback, false, false);

    }

    void DeleteUser()
    {
        if (Userid <= 0)
        {
            GameStart.Instance.ShowTip("请先选择要删除的用户");
            return;
        }
        HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/del/sys/user?id=" + Userid, DeleteUserCallback, true
            , false);

    }
    void DeleteUserCallback(HttpCallBackArgs args)
    {
        if (!string.IsNullOrEmpty(args.Value))
        {
            HttpResponse response = JsonUtility.FromJson<HttpResponse>(args.Value);

            GameStart.Instance.ShowTip(response.msg);

            for (int i = 0; i < MainUserManager.userContent.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.userContent.GetChild(i).gameObject);
            }
            MainUserManager.U_UsernameInput.text = "";
            Userid = -1;
            for (int i = 0; i < user_roleContent.childCount; i++)
            {
                GameObject.Destroy(user_roleContent.GetChild(i).gameObject);
            }
        }
        HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/all/sys/user", GetUsersCallback, false, false);
    }

    //保存新建用户信息
    void SaveUserData()
    {

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repassword))
        {
            GameStart.Instance.ShowTip("用户名或密码不能为空");
            return;
        }
        if (!string.Equals(password, repassword))
        {
            GameStart.Instance.ShowTip("两次输入密码不一致");
            return;
        }
        if (ChoiceRole <= 0)
        {
            GameStart.Instance.ShowTip("请选择用户角色");
            return;
        }
        user us = new user(username, password, ChoiceRole, DatePickerGroupUser.DeadLineTime);

        string userstr = JsonUtility.ToJson(us);
        Log.Debug("发送保存用户信息请求");
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/sys/user/info", AddUserCallback, true, true, false, userstr);
    }
    [Serializable]
    class user
    {
        public string userName = string.Empty;
        public string password = string.Empty;
        public int roleId;
        public string roleExpireTime = string.Empty;
        public user(string name, string pwd, int roleid, string time)
        {
            userName = name;
            password = pwd;
            roleId = roleid;
            roleExpireTime = time;
        }

    }
    //创建用户信息回调
    void AddUserCallback(HttpCallBackArgs args)
    {
        Log.Debug("添加用户回传信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            HttpResponse response = JsonUtility.FromJson<HttpResponse>(args.Value);
            if (response.code == 200)
            {
                GameStart.Instance.ShowTip(response.msg);
                MainUserManager.U_UsernameInput.text = "";
                MainUserManager.U_PasswordInput.text = "";
                MainUserManager.U_RePasswordInput.text = "";
                //再次请求获取所有的用户信息，刷新一下界面
                for (int i = 0; i < MainUserManager.userContent.childCount; i++)
                {
                    GameObject.Destroy(MainUserManager.userContent.GetChild(i).gameObject);
                }
                //previewMdata.children.Clear();//模块容器先清空
                //replayMdata.children.Clear();
                //camerasetMdata.children.Clear();
                //doorsetMdata.children.Clear();
                //cameraeventMdata.children.Clear();
                //dooreventMdata.children.Clear();
                //UserManagerMdata.children.Clear();
                //peopleManagerMdata.children.Clear();
                //for (int i = 0; i < MainUserManager.roleContent.childCount; i++)
                //{
                //    GameObject.Destroy(MainUserManager.roleContent.GetChild(i).gameObject);
                //}
                HttpNetManager.GetInstance().SendDataObj("http://" + GameStart.IP + "/api/perm/all/sys/user", GetUsersCallback, false, false);
            }
            else
            {
                GameStart.Instance.ShowTip(response.msg);
            }
        }
        //ChoiceRole = string.Empty;

    }


    //取消新建用户信息
    void CancleUserData()
    {
        IsAddUser = false;
        ChoiceRole = -1;
        MainUserManager.U_UsernameInput.text = string.Empty;
        MainUserManager.U_PasswordInput.text = string.Empty;
        MainUserManager.U_RePasswordInput.text = string.Empty;
        MainUserManager.U_PasswordInput.gameObject.SetActive(false);
        MainUserManager.U_RePasswordInput.gameObject.SetActive(false);
        MainUserManager.U_ChoiceTime.gameObject.SetActive(false);
        for (int i = user_roleContent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(user_roleContent.GetChild(i).gameObject);
        }
    }
    private int ChoiceRole;




    //用户管理界面获取所有的角色名称        标记
    void UserGetAllRolesNameCallback(HttpCallBackArgs args)
    {
        Log.Debug("获取所有的角色名称:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            rolelist rolelist = JsonUtility.FromJson<rolelist>(args.Value);
            if (rolelist.code == 200)
            {

                for (int i = 0; i < rolelist.data.Count; i++)
                {
                    int index = i;
                    GameObject user = ObjectManager.Instance.InstantiateObject(ConStr.USERTOG);
                    user.transform.SetParent(user_roleContent);
                    resetPrefab(user);
                    user.transform.Find("nameTxt").GetComponent<TextMeshProUGUI>().text = rolelist.data[index].name;
                    user.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                    user.GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
                    {
                        //获取该角色的权限
                        if (ison)
                        {

                            Userrefrushtog(false);//刷一下都置为false
                            previewMdata.areaList.Clear();//模块容器需要清空 
                            replayMdata.areaList.Clear();
                            camerasetMdata.areaList.Clear();
                            doorsetMdata.areaList.Clear();
                            cameraeventMdata.areaList.Clear();
                            dooreventMdata.areaList.Clear();
                            UserManagerMdata.areaList.Clear();
                            peopleManagerMdata.areaList.Clear();
                            Log.Debug("请求：" + rolelist.data[index].name + "的权限");
                            ChoiceRole = rolelist.data[index].id;
                            HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/perm/tg/role/info?roleId=" + rolelist.data[index].id, UserGetRoleAuthCallback, false, false, false);
                        }
                    });
                    user.GetComponent<Toggle>().group = user_roleContent.GetComponent<ToggleGroup>();
                }
            }
            else
            {
                GameStart.Instance.ShowTip(rolelist.msg);
            }
        }
        else
        {
            GameStart.Instance.ShowTip("服务器连接失败!");
        }

    }


    //获取角色信息回调
    private string RoleName = string.Empty;
    private long RoleId = -1;


    //用户管理界面获取单个角色模块权限的展示
    void UserGetRoleAuthCallback(HttpCallBackArgs args)
    {
        Log.Debug("获取权限；" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            //roleListData authList = JsonConvert.DeserializeObject<roleListData>(args.Value);
            Roles authList = ExtractRoles(args.Value);
            Role selectedRole = FindRole(authList, ChoiceRole);
            if (selectedRole == null || selectedRole.menuList == null)
            {
                GameStart.Instance.ShowTip("服务器未返回所选角色的权限数据");
                return;
            }
            foreach (var item in selectedRole.menuList)
            {
                Log.Debug("模块名称:" + item.menuName);
                switch (GetMenuKey(item))
                {
                    case "monitor_config":
                        MainUserManager.usercmonitorSettog.isOn = true;

                        break;
                    case "door_config":
                        MainUserManager.usercdoorSettog.isOn = true;

                        break;
                    case "monitor_preview":

                        //for (int i = 0; i < item.Value.Count; i++)
                        //{

                        //    Log.Debug("区域id:" + item.Value[i].areaId + " 设备id:" + item.Value[i].devId);
                        //    if (!previewMdata.areaList.Exists(t => t.menuId == item.Value[i].areaId))//是否存在该id的区域,不存在加入
                        //    {
                        //        MenuInfo previewAreadata = new MenuInfo();
                        //        previewAreadata.menuName = item.Value[i].areaName;
                        //        previewAreadata.menuId = item.Value[i].areaId;

                        //        MenuInfo previewdevdata = new MenuInfo();
                        //        previewdevdata.menuId = item.Value[i].devId;
                        //        previewdevdata.menuName = item.Value[i].devName;

                        //        previewAreadata.areaList.Add(previewdevdata);
                        //        previewMdata.areaList.Add(previewAreadata);

                        //    }
                        //    else
                        //    {
                        //        MenuInfo previewAreadata = previewMdata.areaList.Find(t => t.menuId == item.Value[i].areaId);//存在该区域就取出
                        //        if (!previewAreadata.areaList.Exists(t => t.menuId == item.Value[i].devId))
                        //        {
                        //            MenuInfo previewdevdata = new MenuInfo();
                        //            previewdevdata.menuId = item.Value[i].devId;
                        //            previewdevdata.menuName = item.Value[i].devName;
                        //            previewAreadata.areaList.Add(previewdevdata);
                        //        }

                        //    }

                        //}
                        MainUserManager.usercpreviewtog.isOn = true;

                        break;
                    case "monitor_playback":
                        //for (int i = 0; i < item.Value.Count; i++)
                        //{

                        //    Log.Debug("区域id:" + item.Value[i].areaId + " 设备id:" + item.Value[i].devId);
                        //    if (!replayMdata.areaList.Exists(t => t.menuId == item.Value[i].areaId))//是否存在该id的区域,不存在加入
                        //    {
                        //        MenuInfo replayAreadata = new MenuInfo();
                        //        replayAreadata.menuName = item.Value[i].areaName;
                        //        replayAreadata.menuId = item.Value[i].areaId;

                        //        MenuInfo replaydevdata = new MenuInfo();
                        //        replaydevdata.menuId = item.Value[i].devId;
                        //        replaydevdata.menuName = item.Value[i].devName;

                        //        replayAreadata.areaList.Add(replaydevdata);
                        //        replayMdata.areaList.Add(replayAreadata);

                        //    }
                        //    else
                        //    {
                        //        MenuInfo replayAreadata = replayMdata.areaList.Find(t => t.menuId == item.Value[i].areaId);//存在该区域就取出
                        //        if (!replayAreadata.areaList.Exists(t => t.menuId == item.Value[i].devId))
                        //        {
                        //            MenuInfo replaydevdata = new MenuInfo();
                        //            replaydevdata.menuId = item.Value[i].devId;
                        //            replaydevdata.menuName = item.Value[i].devName;
                        //            replayAreadata.areaList.Add(replaydevdata);
                        //        }

                        //    }

                        //}
                        MainUserManager.usercreplaytog.isOn = true;

                        break;
                    case "monitor_event":
                        //for (int i = 0; i < item.Value.Count; i++)
                        //{

                        //    Log.Debug("区域id:" + item.Value[i].areaId + " 设备id:" + item.Value[i].devId);
                        //    if (!cameraeventMdata.areaList.Exists(t => t.menuId == item.Value[i].areaId))//是否存在该id的区域,不存在加入
                        //    {
                        //        MenuInfo cameraeventAreadata = new MenuInfo();
                        //        cameraeventAreadata.menuName = item.Value[i].areaName;
                        //        cameraeventAreadata.menuId = item.Value[i].areaId;

                        //        MenuInfo cameraeventdevdata = new MenuInfo();
                        //        cameraeventdevdata.menuId = item.Value[i].devId;
                        //        cameraeventdevdata.menuName = item.Value[i].devName;

                        //        cameraeventAreadata.areaList.Add(cameraeventdevdata);
                        //        cameraeventMdata.areaList.Add(cameraeventAreadata);

                        //    }
                        //    else
                        //    {
                        //        MenuInfo cameraeventAreadata = cameraeventMdata.areaList.Find(t => t.menuId == item.Value[i].areaId);//存在该区域就取出
                        //        if (!cameraeventAreadata.areaList.Exists(t => t.menuId == item.Value[i].devId))
                        //        {
                        //            MenuInfo cameraeventdevdata = new MenuInfo();
                        //            cameraeventdevdata.menuId = item.Value[i].devId;
                        //            cameraeventdevdata.menuName = item.Value[i].devName;
                        //            cameraeventAreadata.areaList.Add(cameraeventdevdata);
                        //        }

                        //    }

                        //}
                        MainUserManager.userccameraeventtog.isOn = true;

                        break;
                    case "door_event":
                        //for (int i = 0; i < item.Value.Count; i++)
                        //{

                        //    Log.Debug("区域id:" + item.Value[i].areaId + " 设备id:" + item.Value[i].devId);
                        //    if (!dooreventMdata.areaList.Exists(t => t.menuId == item.Value[i].areaId))//是否存在该id的区域,不存在加入
                        //    {
                        //        MenuInfo dooreventAreadata = new MenuInfo();
                        //        dooreventAreadata.menuName = item.Value[i].areaName;
                        //        dooreventAreadata.menuId = item.Value[i].areaId;

                        //        MenuInfo dooreventdevdata = new MenuInfo();
                        //        dooreventdevdata.menuId = item.Value[i].devId;
                        //        dooreventdevdata.menuName = item.Value[i].devName;

                        //        dooreventAreadata.areaList.Add(dooreventdevdata);
                        //        dooreventMdata.areaList.Add(dooreventAreadata);

                        //    }
                        //    else
                        //    {
                        //        MenuInfo dooreventAreadata = dooreventMdata.areaList.Find(t => t.menuId == item.Value[i].areaId);//存在该区域就取出
                        //        if (!dooreventAreadata.areaList.Exists(t => t.menuId == item.Value[i].devId))
                        //        {
                        //            MenuInfo dooreventdevdata = new MenuInfo();
                        //            dooreventdevdata.menuId = item.Value[i].devId;
                        //            dooreventdevdata.menuName = item.Value[i].devName;
                        //            dooreventAreadata.areaList.Add(dooreventdevdata);
                        //        }

                        //    }

                        //}
                        MainUserManager.usercdooreventtog.isOn = true;

                        break;
                    case "user_management":
                        MainUserManager.usercusermanagertog.isOn = true;

                        break;
                    case "personnel_management":
                        MainUserManager.usercpeoplemanagertog.isOn = true;

                        break;
                    default:
                        break;
                }

            }
        }

    }

    private void ApplyRolePermissionsToEditor(Role selectedRole)
    {
        List<Menu> sourceMenus = selectedRole.menuList ?? new List<Menu>();
        Menu previewSource = sourceMenus.Find(menu => GetMenuKey(menu) == "monitor_preview");
        Menu replaySource = sourceMenus.Find(menu => GetMenuKey(menu) == "monitor_playback");

        previewMdata.areaList = previewSource != null && previewSource.areaList != null
            ? previewSource.areaList
            : new List<GetAreaGroupDevs>();
        replayMdata.areaList = replaySource != null && replaySource.areaList != null
            ? replaySource.areaList
            : new List<GetAreaGroupDevs>();
        LoadPermissionKeys(previewMdata.areaList, previewSelectedKeys);
        LoadPermissionKeys(replayMdata.areaList, replaySelectedKeys);

        isApplyingRolePermissions = true;
        try
        {
            refrushtog(false);
            for (int i = 0; i < sourceMenus.Count; i++)
            {
                switch (GetMenuKey(sourceMenus[i]))
                {
                    case "monitor_config": MainUserManager.cmonitorSet.isOn = true; break;
                    case "door_config": MainUserManager.cdoorSet.isOn = true; break;
                    case "monitor_preview": MainUserManager.cpreview.isOn = true; break;
                    case "monitor_playback": MainUserManager.creplay.isOn = true; break;
                    case "monitor_event": MainUserManager.ccameraevent.isOn = true; break;
                    case "door_event": MainUserManager.cdoorevent.isOn = true; break;
                    case "user_management": MainUserManager.cusermanager.isOn = true; break;
                    case "personnel_management": MainUserManager.cpeoplemanager.isOn = true; break;
                }
            }
        }
        finally
        {
            isApplyingRolePermissions = false;
        }

        role.menuList.Clear();
        AddCanonicalMenuIfPresent(sourceMenus, camerasetMdata);
        AddCanonicalMenuIfPresent(sourceMenus, doorsetMdata);
        AddCanonicalMenuIfPresent(sourceMenus, previewMdata);
        AddCanonicalMenuIfPresent(sourceMenus, replayMdata);
        AddCanonicalMenuIfPresent(sourceMenus, cameraeventMdata);
        AddCanonicalMenuIfPresent(sourceMenus, dooreventMdata);
        AddCanonicalMenuIfPresent(sourceMenus, peopleManagerMdata);
        AddCanonicalMenuIfPresent(sourceMenus, UserManagerMdata);
        DeleteRoleTreeItem();
    }

    private void AddCanonicalMenuIfPresent(List<Menu> sourceMenus, Menu canonicalMenu)
    {
        Menu source = sourceMenus.Find(menu => menu != null && menu.menuId == canonicalMenu.menuId);
        if (source == null)
        {
            source = sourceMenus.Find(menu => GetMenuKey(menu) == canonicalMenu.menuName);
        }
        if (source == null)
        {
            return;
        }
        if (source.areaList != null)
        {
            canonicalMenu.areaList = source.areaList;
        }
        role.menuList.Add(canonicalMenu);
    }

    private static Role FindRole(Roles authList, long roleId)
    {
        if (authList == null || authList.roleList == null || authList.roleList.Count == 0)
        {
            return null;
        }

        Role selected = authList.roleList.Find(item => item != null && item.roleId == roleId);
        return selected ?? authList.roleList[0];
    }

    private static Roles ExtractRoles(string json)
    {
        JObject root = JToken.Parse(json) as JObject;
        if (root == null)
        {
            return null;
        }

        JToken roleListToken = root["roleList"] ?? root.SelectToken("data.roleList");
        if (roleListToken != null && roleListToken.Type == JTokenType.Array)
        {
            Roles result = new Roles();
            result.roleList = roleListToken.ToObject<List<Role>>() ?? new List<Role>();
            return result;
        }

        JToken roleToken = null;
        JObject dataObject = root["data"] as JObject;
        if (dataObject != null && (dataObject["menuList"] != null || dataObject["menus"] != null))
        {
            roleToken = dataObject;
        }
        else if (root["menuList"] != null || root["menus"] != null)
        {
            roleToken = root;
        }

        if (roleToken == null)
        {
            return null;
        }

        Role roleItem = roleToken.ToObject<Role>();
        if (roleItem != null && roleItem.menuList == null && roleToken["menus"] != null)
        {
            roleItem.menuList = roleToken["menus"].ToObject<List<Menu>>() ?? new List<Menu>();
        }
        Roles singleRoleResult = new Roles();
        if (roleItem != null)
        {
            singleRoleResult.roleList.Add(roleItem);
        }
        return singleRoleResult;
    }

    private static string GetMenuKey(Menu item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        switch (item.menuId)
        {
            case 12: return "monitor_config";
            case 13: return "door_config";
            case 14: return "monitor_preview";
            case 15: return "monitor_playback";
            case 16: return "monitor_event";
            case 17: return "door_event";
            case 18: return "personnel_management";
            case 19: return "user_management";
        }

        string normalized = (item.menuName ?? string.Empty).Trim().ToLowerInvariant()
            .Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
        switch (normalized)
        {
            case "monitorconfig": return "monitor_config";
            case "doorconfig": return "door_config";
            case "monitorpreview": return "monitor_preview";
            case "monitorplayback": return "monitor_playback";
            case "monitorevent": return "monitor_event";
            case "doorevent": return "door_event";
            case "personnelmanagement": return "personnel_management";
            case "usermanagement": return "user_management";
            default: return item.menuName ?? string.Empty;
        }
    }

    /// <summary>
    /// 把模块子tog置为false
    /// </summary>
    void refrushtog(bool ison)
    {
        MainUserManager.cmonitorSet.isOn = ison;
        MainUserManager.cdoorSet.isOn = ison;
        MainUserManager.cpreview.isOn = ison;
        MainUserManager.creplay.isOn = ison;
        MainUserManager.ccameraevent.isOn = ison;
        MainUserManager.cdoorevent.isOn = ison;
        MainUserManager.cusermanager.isOn = ison;
        MainUserManager.cpeoplemanager.isOn = ison;
    }

    /// <summary>
    /// 用户管理把模块子tog置为false
    /// </summary>
    void Userrefrushtog(bool ison)
    {
        MainUserManager.usercmonitorSettog.isOn = ison;
        MainUserManager.usercdoorSettog.isOn = ison;
        MainUserManager.usercpreviewtog.isOn = ison;
        MainUserManager.usercreplaytog.isOn = ison;
        MainUserManager.userccameraeventtog.isOn = ison;
        MainUserManager.usercdooreventtog.isOn = ison;
        MainUserManager.usercusermanagertog.isOn = ison;
        MainUserManager.usercpeoplemanagertog.isOn = ison;

        MainUserManager.usercalltog.interactable = false;
        MainUserManager.usercpeoplemanagertog.interactable = false;
        MainUserManager.usercusermanagertog.interactable = false;
        MainUserManager.usercdooreventtog.interactable = false;
        MainUserManager.userccameraeventtog.interactable = false;
        MainUserManager.usercreplaytog.interactable = false;
        MainUserManager.usercpreviewtog.interactable = false;
        MainUserManager.usercmonitorSettog.interactable = false;
        MainUserManager.usercdoorSettog.interactable = false;
    }



    [Serializable]
    public class AreaGroupDevList
    {
        public List<GetAreaGroupDevs> records = new List<GetAreaGroupDevs>();

    }





    /// <summary>
    /// 获取监控区域分组设备信息
    /// </summary>
    public AreaGroupDevList CameraAreaGroupDevs;





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





    //用户信息页面角色权限展示功能----------------------------------------------------------------------------------------------------------------------//
    void alltoglistener(bool ison)
    {
        if (ison)
        {

        }

    }
    void monitortoglistener(bool ison)
    {
        if (ison)
        {

        }


    }
    void doortoglistener(bool ison)
    {
        if (ison)
        {

        }

    }
    void previewtoglistener(bool ison)
    {
        if (ison)
        {


        }
        else
        {
            if (MainUserManager.TreeviewUserManager.transform.childCount != 0)
            {
                for (int i = MainUserManager.TreeviewUserManager.transform.childCount - 1; i >= 0; i--)
                {
                    int temp = i;
                    if (MainUserManager.TreeviewUserManager.transform.GetChild(temp).GetComponent<TreeViewItem>() != null && MainUserManager.TreeviewUserManager.transform.GetChild(temp).name == "ItemPrefab1(Clone)")
                        TreeManagerUserManager.GetInstance().OnDeleteBtnClicked(MainUserManager.TreeviewUserManager.transform.GetChild(temp).GetComponent<TreeViewItem>());
                }

            }
        }

    }
    void replaytoglistener(bool ison)
    {
        if (ison)
        {
            for (int i = 0; i < MainUserManager.UserMenuContainer.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(i).gameObject);
            }
            UserInitCamreaReplayGroup();
            MainUserManager.UserMenuContainer.gameObject.SetActive(true);


        }

    }
    void cameraeventtoglistener(bool ison)
    {
        if (ison)
        {
            for (int i = 0; i < MainUserManager.UserMenuContainer.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(i).gameObject);
            }

            MainUserManager.UserMenuContainer.gameObject.SetActive(true);

        }

    }
    void dooreventtoglistener(bool ison)
    {
        if (ison)
        {
            for (int i = 0; i < MainUserManager.UserMenuContainer.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(i).gameObject);
            }


            MainUserManager.UserMenuContainer.gameObject.SetActive(true);
        }

    }
    void usermanagertoglistener(bool ison)
    {
        if (ison)
        {
            for (int i = 0; i < MainUserManager.UserMenuContainer.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(i).gameObject);
            }
        }


    }
    void PeopleManagertoglistener(bool ison)
    {
        if (ison)
        {
            for (int i = 0; i < MainUserManager.UserMenuContainer.childCount; i++)
            {
                GameObject.Destroy(MainUserManager.UserMenuContainer.GetChild(i).gameObject);
            }
        }

    }








    /// <summary>
    /// 实例化回放分组
    /// </summary>
    void UserInitCamreaReplayGroup()
    {
        HttpNetManager.GetInstance().SendDataStr("http://" + GameStart.IP + "/api/personnel/tg/area/info?pageNum=1&pageSize=10000&areaId=&areaName=&areaType=2", UserGetCameraReplayDevs, false, false, false);
    }
    public void UserGetCameraReplayDevs(HttpCallBackArgs args)
    {
        Log.Debug("从服务器接收到的监控区域分组设备信息：" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {

            CameraAreaGroupDevs = JsonUtility.FromJson<AreaGroupDevList>(args.Value);
            Log.Debug("请求的监控分组为:" + CameraAreaGroupDevs.records.Count);
            for (int i = 0; i < CameraAreaGroupDevs.records.Count; i++)
                ObjectManager.Instance.InstantiateObjectAsync(ConStr.CAMERAAUTHGROUP, UsercameraReplaycallback, LoadResPriority.RES_MIDDLE, false, CameraAreaGroupDevs.records[i].areaName, CameraAreaGroupDevs.records[i].devList, CameraAreaGroupDevs.records[i].id);
        }
    }
    private void UsercameraReplaycallback(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {

        GameObject groupvsion = obj as GameObject;
        groupvsion.transform.SetParent(MainUserManager.UserMenuContainer);
        List<NVRInformation> grouplist = param2 as List<NVRInformation>;
        resetPrefab(groupvsion);

        Transform group = groupvsion.transform.GetChild(0);
        group.Find("Text").GetComponent<TextMeshProUGUI>().text = param1 as string;
        group.Find("show_tog").GetComponent<Toggle>().onValueChanged.AddListener((ison) =>
        {
            if (ison)
            {
                group.Find("show_tog").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/放");
                group.Find("show_tog").GetComponent<Image>().SetNativeSize();

            }
            else
            {
                group.Find("show_tog").GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/视频预览/收");
                group.Find("show_tog").GetComponent<Image>().SetNativeSize();

            }
            for (int i = 1; i < groupvsion.transform.childCount; i++)
            {
                groupvsion.transform.GetChild(i).gameObject.SetActive(ison);
            }
            MainUserManager.SetMonitorsetContentSizeActive();
        });
        group.GetComponent<Button>().onClick.RemoveAllListeners();

        //新规则 区域
        MenuInfo replayAreadata = new MenuInfo(); ;
        //if (replayMdata.areaList.Exists(t => t.menuId == (int)param3))//存在取出
        //{
        //    replayAreadata = replayMdata.areaList.Find(t => t.menuId == ((int)param3));
        //}
        //else
        //{
        //    replayAreadata = new MenuInfo();//区域
        //    replayAreadata.menuName = param1 as string;
        //    replayAreadata.menuId = (int)param3;
        //}


        //初始化监控回放下监控实例
        for (int i = 0; i < grouplist.Count; i++)
        {
            int index = i;
            GameObject cameobj = ObjectManager.Instance.InstantiateObject(ConStr.CAMERAuth);
            cameobj.transform.SetParent(groupvsion.transform);
            cameobj.transform.GetComponent<Toggle>().group = groupvsion.GetComponent<ToggleGroup>();
            resetPrefab(cameobj);
            cameobj.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = grouplist[index].cameraname;
            //新规则 设备
            //for (int m = 0; m < replayAreadata.areaList.Count; m++)
            //{
            //    if (replayAreadata.areaList[m].menuId == grouplist[index].id)
            //    {

            //        cameobj.transform.Find("choicetog").GetComponent<Toggle>().isOn = true;
            //    }
            //}
            MenuInfo devdata = new MenuInfo();
            //devdata.id = 20;
            devdata.menuName = grouplist[index].cameraname;
            devdata.menuId = grouplist[index].id;
            //devdata.path = "";
            //devdata.status = "";
            //devdata.parentId = 10;


            cameobj.transform.Find("choicetog").GetComponent<Toggle>().interactable = false;



        }
        groupvsion.transform.GetChild(0).Find("choicetog").GetComponent<Toggle>().interactable = false;


    }






}
