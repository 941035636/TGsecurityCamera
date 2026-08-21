using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperTreeView;

public class UserManagerPanel : MonoBehaviour
{
    public Button addRolebtn;
    public Button deleteRolebtn;

    public TMP_Dropdown userTypeDrop;
    public TMP_InputField UsernameInput;
    public TMP_InputField PasswordInput;
    public TMP_InputField RePasswordInput;
    public Transform MenuContainer;


    //权限
    public Toggle call;
    public Toggle cmonitorSet;
    public Toggle cdoorSet;
    public Toggle cpreview;
    public Toggle creplay;
    public Toggle ccameraevent;
    public Toggle cdoorevent;
    public Toggle cusermanager;
    public Toggle cpeoplemanager;

    public Toggle palltog;
    public Toggle pmonitorSettog;
    public Toggle pdoorSettog;
    public Toggle ppreviewtog;
    public Toggle preplaytog;
    public Toggle pcameraeventtog;
    public Toggle pdooreventtog;
    public Toggle pusermanagertog;
    public Toggle ppeoplemanagertog;

    public Transform UsernameContent;


    public Button SaveRoleButton;
    public Button CancleRoleButton;

    public Toggle userManTog;//用户管理
    public Transform userContent;//用户存放节点
    public Toggle roleManTog;//角色管理
    public Transform roleContent;//角色存放节点


    //用户管理对应权限展示节点
    public Transform UserMenuContainer;
    public TMP_InputField U_UsernameInput;
    public TMP_InputField U_PasswordInput;
    public TMP_InputField U_RePasswordInput;
    public GameObject U_ChoiceTime;

    public Button SaveUserButton;
    public Button CancleUserButton;

    public Toggle alltog;
    public Toggle usercalltog;
    public Toggle monitorSettog;
    public Toggle usercmonitorSettog;
    public Toggle doorSettog;
    public Toggle usercdoorSettog;
    public Toggle previewtog;
    public Toggle usercpreviewtog;
    public Toggle replaytog;
    public Toggle usercreplaytog;
    public Toggle cameraeventtog;
    public Toggle userccameraeventtog;
    public Toggle dooreventtog;
    public Toggle usercdooreventtog;
    public Toggle usermanagertog;
    public Toggle usercusermanagertog;
    public Toggle peoplemanagertog;
    public Toggle usercpeoplemanagertog;


    public Button AddUserBtn;
    public Button DeleteUserBtn;


    public TreeView TreeviewUserManager;
    public TreeView TreeviewRoleManager;

    public void SetMonitorsetContentSizeActive()
    {
        StartCoroutine(HelpSet());//Monitorset及时触发
    }
    IEnumerator HelpSet()
    {
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = false;
        yield return null;
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = true;
    }

  
}
