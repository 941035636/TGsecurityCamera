using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FirstPagePanel : MonoBehaviour
{
    //***********************************************************************************//
    #region VideoApplications_bg
    public Button m_PreviewButton;
    public Button m_ReplayButton;
    public Button m_AlgorButton;
    public Button m_WarningButton;
    public Button m_EventCenterButton;
    #endregion

    #region TopLable
    public Toggle T_MainPreviewToggle;
    public Toggle T_PeopleManagentToggle;
    public Button T_ClosePeopleManagentButton;
    public Button T_CloseMainPreviewButton;
    public Toggle T_FirstPageToggle;
    public Toggle T_EquipManagentToggle;
    public Button T_CloseEquipManagentButton;
    public Toggle T_VideoReplayToggle;
    public Button T_CloseReplayButton;
    public Toggle T_EventCenterToggle;
    public Button T_CloseEventCenterButton;
    public Toggle T_VistorManagerToggle;
    public Button T_CloseVistorManagerButton;
    public Toggle T_UserManagentToggle;
    public Button T_CloseUserManagentButton;
    #endregion

    #region 访问控制
    public Button UserManagerBtn;
    public Button VistorManagerBtn;

    #endregion

    #region 维护与管理
    public Button EquipManagentBtn;
    public Button DoorEquipManagentBtn;
    #endregion

    public Button SystemUserManagerBtn;

    public TextMeshProUGUI usernameTxt;
    public Toggle downTog;
    public Button exitbtn;

    private void OnEnable()
    {
        usernameTxt.text = LoginManager.UserName;
    }

}
