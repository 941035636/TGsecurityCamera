using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine;
using SpringGUI;

public class FirstUi : Window
{
    private FirstPagePanel m_MainPanel;

    public override void Awake(params object[] paralist)
    {
        m_MainPanel = GameObject.GetComponent<FirstPagePanel>();
       


        AddButtonClickListener(m_MainPanel.EquipManagentBtn, OnClickEquipManagentPage);
        AddButtonClickListener(m_MainPanel.DoorEquipManagentBtn, OnClickDoorManagentPage);
        AddToggleClickListener(m_MainPanel.T_EquipManagentToggle, OnClickT_EquipManagentLable);
        AddButtonClickListener(m_MainPanel.T_CloseEquipManagentButton, OnClickT_CloseEquipManagent);
        AddButtonClickListener(m_MainPanel.UserManagerBtn, OnClickPeopleManagentPage);
        AddToggleClickListener(m_MainPanel.T_PeopleManagentToggle, OnClickT_PeopleManagentLable);
        AddButtonClickListener(m_MainPanel.T_ClosePeopleManagentButton, OnClick_ClosePeopleManagent);


        AddButtonClickListener(m_MainPanel.m_PreviewButton, OnClickMianPreview);
        AddToggleClickListener(m_MainPanel.T_MainPreviewToggle, OnClickT_MainPreviewLable);
        AddButtonClickListener(m_MainPanel.T_CloseMainPreviewButton, OnClickT_CloseMainPreview);
        AddToggleClickListener(m_MainPanel.T_FirstPageToggle, OnClickT_BackToFirstPage);


        AddButtonClickListener(m_MainPanel.VistorManagerBtn, OnClickVistorManagentPage);
        AddToggleClickListener(m_MainPanel.T_VistorManagerToggle, OnClickT_VistorManagentLable);
        AddButtonClickListener(m_MainPanel.T_CloseVistorManagerButton, OnClick_CloseVistorManagent);

        AddButtonClickListener(m_MainPanel.m_ReplayButton, OnClickVideoReplayPage);
        AddToggleClickListener(m_MainPanel.T_VideoReplayToggle, OnClickT_VideoReplayLable);
        AddButtonClickListener(m_MainPanel.T_CloseReplayButton, OnClick_CloseVideoReplay);

        AddButtonClickListener(m_MainPanel.m_EventCenterButton, OnClickEventCenterPage);
        AddToggleClickListener(m_MainPanel.T_EventCenterToggle, OnClickT_EventCenterLable);
        AddButtonClickListener(m_MainPanel.T_CloseEventCenterButton, OnClick_CloseEventCenter);


        AddButtonClickListener(m_MainPanel.SystemUserManagerBtn, OnClick_UserManager);
        AddToggleClickListener(m_MainPanel.T_UserManagentToggle, OnClickT_UserManagerLable);
        AddButtonClickListener(m_MainPanel.T_CloseUserManagentButton, OnClick_CloseUserManager);
        AddToggleClickListener(m_MainPanel.downTog, downtogforexit);
        AddButtonClickListener(m_MainPanel.exitbtn, exit);

        ;        //ObjectManager.Instance.InstantiateObject("Assets/GameData/Prefabs/Attack.prefab");
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/Test1.png", OnLoadSpriteTest1, LoadResPriority.RES_MIDDLE, true);

        //LoadMonsterData();
    }

    void downtogforexit(bool ison)
    {
        if (ison)
        {

            m_MainPanel.exitbtn.transform.DOScale(new Vector3(1, 1, 1), 0.02f);

            m_MainPanel.exitbtn.transform.DOLocalMoveY(-31, 0.02f);

        }
        else
        {
            m_MainPanel.exitbtn.transform.DOScale(new Vector3(0, 0, 0), 0.02f);

            m_MainPanel.exitbtn.transform.DOLocalMoveY(0, 0.02f);
        }

    }
    void exit()
    {
        //返回登录页

        UIManager.Instance.CloseWnd(ConStr.USERMANAGER, true);
        UIManager.Instance.CloseWnd(ConStr.PEOPLEMANPAGE, true);
        UIManager.Instance.CloseWnd(ConStr.VISTORMANPAGE, true);
        UIManager.Instance.CloseWnd(ConStr.VIDEOREPLAY, true);
        //DestoryVideoReplay();
        UIManager.Instance.CloseWnd(ConStr.EventCenter, true);
        UIManager.Instance.CloseWnd(ConStr.MAINPREVIEW, true);
        UIManager.Instance.CloseWnd(ConStr.FIRSTPAGE, true);
        UIManager.Instance.PopUpWnd(ConStr.LOGINPAGE, true);//登录页
        PlayerPrefs.SetInt("IsLogin", 0);
        HttpNetManager.GetInstance().CancelAllRequests();
        HttpNetManager.ClearAuthorizationToken();
        LoginManager.Ins.ResetPermissions();
    }

    void LoadMonsterData()
    {
        //MonsterData monsterData = ConfigerManager.Instance.FindData<MonsterData>(CFG.TABLE_MONSTER);
        //Log.Error(monsterData);
        //for (int i = 0; i < monsterData.AllMonster.Count; i++)
        //{
        //     Log.Debug(string.Format("ID:{0} 名字：{1}  外观：{2}  高度：{3}  稀有度：{4}", monsterData.AllMonster[i].Id, monsterData.AllMonster[i].Name, monsterData.AllMonster[i].OutLook, monsterData.AllMonster[i].Height, monsterData.AllMonster[i].Rare));
        //}
    }

    void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        //if (obj != null)
        //{
        //    Sprite sp = obj as Sprite;
        //    m_MainPanel.m_Test1.sprite = sp;
        //     Log.Debug("图片1加载出来了");

        //}
    }



    public override void OnUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    ResourceManager.Instance.ReleaseResouce(m_MainPanel.m_Test1.sprite, true);
        //    m_MainPanel.m_Test1.sprite = null;
        //}
    }

    void OnClick_UserManager()
    {
        if (LoginManager.Ins.IsUserManager)

        {
            UIManager.Instance.PopUpWnd(ConStr.USERMANAGER);
         
            m_MainPanel.T_UserManagentToggle.gameObject.SetActive(true);
            m_MainPanel.T_UserManagentToggle.isOn = true;
        }
        else
        {
            GameStart.Instance.ShowTip("当前用户没有用户管理权限!");
        }


    }

    void OnClickMianPreview()
    {
        if (!EnsurePermission(LoginManager.Ins.IsPreview, "视频预览")) return;

        UIManager.Instance.PopUpWnd(ConStr.MAINPREVIEW);

        m_MainPanel.T_MainPreviewToggle.gameObject.SetActive(true);
        m_MainPanel.T_MainPreviewToggle.isOn = true;
    }
    void OnClickPeopleManagentPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsPeopleManager, "人员管理")) return;
        UIManager.Instance.PopUpWnd(ConStr.PEOPLEMANPAGE);
     
        m_MainPanel.T_PeopleManagentToggle.gameObject.SetActive(true);
        m_MainPanel.T_PeopleManagentToggle.isOn = true;
    }

    void OnClickVistorManagentPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsPeopleManager, "人员管理")) return;
        UIManager.Instance.PopUpWnd(ConStr.VISTORMANPAGE);
     
        m_MainPanel.T_VistorManagerToggle.gameObject.SetActive(true);
        m_MainPanel.T_VistorManagerToggle.isOn = true;
    }

    void OnClickVideoReplayPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsReplay, "录像回放")) return;
        UIManager.Instance.PopUpWnd(ConStr.VIDEOREPLAY);

        m_MainPanel.T_VideoReplayToggle.gameObject.SetActive(true);
        m_MainPanel.T_VideoReplayToggle.isOn = true;
        //InstantVideoReplay();
    }

    void InstantVideoReplay()
    {
        Transform replayVideoObj = this. Transform.parent.Find("VideoReplayPageAVPro(Clone)");
        if (replayVideoObj == null)
        {
            GameObject replayobj = Resources.Load<GameObject>("VideoReplay/VideoReplayPageAVPro");
            GameObject videoreplay = GameObject.Instantiate(replayobj, this.Transform.parent);

        }
        else 
        {
            replayVideoObj.transform.SetAsLastSibling();
        }
       


    }
    void DestoryVideoReplay() 
    {
        Transform replayVideo = this.Transform.parent.Find("VideoReplayPageAVPro(Clone)").transform;
        if(replayVideo.transform!=null)
        GameObject.Destroy(replayVideo.gameObject);
    }

    void OnClickEventCenterPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsCameraEvent || LoginManager.Ins.IsDoorEvent, "事件中心")) return;
        UIManager.Instance.PopUpWnd(ConStr.EventCenter);
       
        m_MainPanel.T_EventCenterToggle.gameObject.SetActive(true);
        m_MainPanel.T_EventCenterToggle.isOn = true;
    }

    void OnClickT_PeopleManagentLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsPeopleManager, "人员管理")) return;

            UIManager.Instance.ShowWnd(ConStr.PEOPLEMANPAGE);

        }
    }


    void OnClickT_UserManagerLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsUserManager, "用户管理")) return;
            UIManager.Instance.ShowWnd(ConStr.USERMANAGER);
        }

    }

    void OnClickT_VistorManagentLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsPeopleManager, "人员管理")) return;
            UIManager.Instance.ShowWnd(ConStr.VISTORMANPAGE);
        }

    }

    void OnClickT_VideoReplayLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsReplay, "录像回放")) return;

            UIManager.Instance.ShowWnd(ConStr.VIDEOREPLAY);
            //InstantVideoReplay();

        }
        else
        {

        }
        //if (VideoPlayBack.Ins.loadingani != null)
        //{
        //    VideoPlayBack.Ins.loadingani.gameObject.SetActive(false);
        //}
    }

    void OnClickT_EventCenterLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsCameraEvent || LoginManager.Ins.IsDoorEvent, "事件中心")) return;

            UIManager.Instance.ShowWnd(ConStr.EventCenter);

        }
    }


    void OnClickT_MainPreviewLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsPreview, "视频预览")) return;

            UIManager.Instance.ShowWnd(ConStr.MAINPREVIEW);

        }
        else
        {
            if (HotspotManger.ISSHOW) 
            {
                EventCenter.BroadCast(Eventdefine.ClosePlan);
                
            }
              
        }

    }
    void OnClick_CloseUserManager()
    {
        UIManager.Instance.CloseWnd(ConStr.USERMANAGER, true);
        m_MainPanel.T_UserManagentToggle.gameObject.SetActive(false);
    }

    void OnClick_ClosePeopleManagent()
    {

        UIManager.Instance.CloseWnd(ConStr.PEOPLEMANPAGE, true);
        m_MainPanel.T_PeopleManagentToggle.gameObject.SetActive(false);
    }

    void OnClick_CloseVistorManagent()
    {

        UIManager.Instance.CloseWnd(ConStr.VISTORMANPAGE, true);
        m_MainPanel.T_VistorManagerToggle.gameObject.SetActive(false);
    }

    void OnClick_CloseVideoReplay()
    {

        UIManager.Instance.CloseWnd(ConStr.VIDEOREPLAY, true);
        //DestoryVideoReplay();
        m_MainPanel.T_VideoReplayToggle.gameObject.SetActive(false);
    }
    void OnClick_CloseEventCenter()
    {

        UIManager.Instance.CloseWnd(ConStr.EventCenter, true);
        m_MainPanel.T_EventCenterToggle.gameObject.SetActive(false);
    }

    void OnClickT_CloseMainPreview()
    {
        UIManager.Instance.CloseWnd(ConStr.MAINPREVIEW, true);
        m_MainPanel.T_MainPreviewToggle.isOn = false;
        m_MainPanel.T_MainPreviewToggle.gameObject.SetActive(false);
    }

    //void OnClick_ClosePeopleManagent()
    //{
    //    UIManager.Instance.CloseWnd(ConStr.PEOPLEMANPAGE, true);
    //    m_MainPanel.T_PeopleManagentToggle.gameObject.SetActive(false);
    //}

    void OnClickT_EquipManagentLable(bool ison)
    {
        if (ison)
        {
            if (!EnsurePermission(LoginManager.Ins.IsCameraSet || LoginManager.Ins.IsDoorSet, "设备配置")) return;

            UIManager.Instance.ShowWnd(ConStr.MANDMANAGENT);
            ManManagentPanel.GetInstance().doorOutline = 0;
            ManManagentPanel.GetInstance().doorOnline = 0;
            ManManagentPanel.cameraOutline = 0;
            ManManagentPanel.cameraOnline = 0;
        }

    }

    void OnClickT_CloseEquipManagent()
    {

        //UIManager.Instance.HideWnd(ConStr.MAINPREVIEW);
        UIManager.Instance.CloseWnd(ConStr.MANDMANAGENT, true);
        ManManagentUi.IsInitCamera = false;
        ManManagentUi.IsInitDoor = false;
        ManManagentUi.IsInitDoorGroup = false;
        m_MainPanel.T_EquipManagentToggle.gameObject.SetActive(false);
    }

    void OnClickT_BackToFirstPage(bool ison)
    {
        //if (ison)
        //{

            //UIManager.Instance.SwitchStateByName(ConStr.FIRSTPAGE);
            //UIManager.Instance.HideUI();
            UIManager.Instance.ShowWnd(ConStr.FIRSTPAGE);

        //}

    }
    //监控设备
    void OnClickEquipManagentPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsCameraSet, "监控配置")) return;

        UIManager.Instance.PopUpWnd(ConStr.MANDMANAGENT);
    
        UIManager.Instance.SendMessageToWnd(ConStr.MANDMANAGENT, UIMsgID.ManCamera, new string[] { "监控管理" });
        m_MainPanel.T_EquipManagentToggle.gameObject.SetActive(true);
        m_MainPanel.T_EquipManagentToggle.isOn = true;
    }
    //门禁设备
    void OnClickDoorManagentPage()
    {
        if (!EnsurePermission(LoginManager.Ins.IsDoorSet, "门禁配置")) return;

        UIManager.Instance.PopUpWnd(ConStr.MANDMANAGENT);
        UIManager.Instance.SendMessageToWnd(ConStr.MANDMANAGENT, UIMsgID.ManDoor, new string[] { "门禁管理" });
     
        m_MainPanel.T_EquipManagentToggle.gameObject.SetActive(true);
        m_MainPanel.T_EquipManagentToggle.isOn = true;
    }

    private bool EnsurePermission(bool allowed, string permissionName)
    {
        if (allowed) return true;
        GameStart.Instance.ShowTip("当前用户没有" + permissionName + "权限!");
        return false;
    }


}
