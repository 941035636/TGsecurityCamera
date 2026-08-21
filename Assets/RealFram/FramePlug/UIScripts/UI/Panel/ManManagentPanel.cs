using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using zFramework.Media;
using SuperTreeView;

public class ManManagentPanel : MonoBehaviour
{
    #region 左侧设备管理
    public Toggle EquipMantog;
    public Toggle camera_Tog;
    public Toggle cameragroup_Tog;
    public Toggle DoorMantog;
    public Toggle door_Tog;
    public Toggle doorgroup_Tog;
    public Transform RightCameraEquipPage;
    public Transform RightCameraGroupPage;
    public Transform RightDoorEquipPage;
    public Transform RightDoorGroupPage;
    public Transform DoorEquipContent;
    #endregion


    #region
    public Toggle AddCamera_Toggle;
    public Toggle DeleteCamera_Toggle;
    public Toggle DaleteCameraGroup_Toggle;
    public Toggle CameraSearchOnline_Toggle;
    public Toggle CameraRefrush_Toggle;
    public TextMeshProUGUI CameraStateTxt;


    public Toggle AddDoor_Toggle;
    public Toggle DeleteDoor_Toggle;
    public Toggle DoorSearchOnline_Toggle;
    public Toggle DoorRefrush_Toggle;
    public TextMeshProUGUI DoorStateTxt;
    #endregion


    #region  添加设备面板
    public Button AddCamera_Btn;
    public Button CancleCameraadd_Button;
    public TMP_InputField Name_input;
    public TMP_InputField Ip_input;
    public TMP_InputField Point_input;
    public TMP_InputField User_input;
    public TMP_InputField Pwd_input;
    public TMP_InputField Channel_input;
    public TMP_Dropdown DeviceBrand_drop;
    #endregion

    #region 监控item存放
    public Transform EquipContains;
    public Transform DoorEquipContains;
    #endregion

    public Transform MenuContainer;

    public TreeView CameramTreeView;
    public TreeView DoorTreeView;


    private static ManManagentPanel Instance;
    public static ManManagentPanel GetInstance() 
    {
        if (Instance==null)
        {
            Instance = new ManManagentPanel();
        }
        return Instance;
    
    }

    public void OnEnable()
    {

        EventCenter.addlistener<string>(Eventdefine.DeviceNetSate, EquipNetStateListener);
        EventCenter.addlistener<string>(Eventdefine.DoorNetState, DoorNetStateListener);
        cameraOnline = 0;
        cameraOutline = 0;
        doorOnline = 0;
        doorOutline = 0;
    }

    public void SetContentSizeActive()
    {
        StartCoroutine(HelpSet());//及时触发
    }

    IEnumerator HelpSet()
    {
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = false;
        yield return null;
        MenuContainer.GetComponent<ContentSizeFitter>().enabled = true;
    }

    /// <summary>
    /// 监控设备在线状态监听
    /// </summary>
    /// 
   public static int cameraOnline = 0;
   public static int cameraOutline = 0;
    private void EquipNetStateListener(string ip)
    {
        //cameraOnline = 0;
        //cameraOutline = 0;

        int loginhandle = 0;
        if (HikvisonNVR.LoginhandleDic.TryGetValue(ip, out loginhandle))
        {
            for (int i = 0; i < EquipContains.childCount; i++)
            {
              
                if (string.Equals(EquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text.Split(':')[0].Trim(), ip))
                {
  
                    if (HikvisonNVR.LoginhandleDic[ip] > -1)
                    {
                        EquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text ="<color=green> 在线状态</color>";
                        cameraOnline++;
                    }
                    else
                    {
                        EquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text = "<color=red> 离线状态</color>";
                        cameraOutline++;

                    }
                    //Log.Error("移除：" + ip);
                    //HikvisonNVR.LoginhandleDic.Remove(ip);
                }
            }
        }
        else
        {
            // Log.Debug(ip+"不存在于登陆句柄字典中");
            //DoorEquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text = "登录失败";
        }
        string serinum = string.Empty;
        if (HikvisonNVR.SerinumDic.TryGetValue(ip,out serinum))
        {
            for (int i = 0; i < EquipContains.childCount; i++)
            {

                if (string.Equals(EquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text.Split(':')[0].Trim(), ip))
                {

                    if (!string.IsNullOrEmpty( HikvisonNVR.SerinumDic[ip]))
                    {
                
                        EquipContains.GetChild(i).Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = HikvisonNVR.SerinumDic[ip];
                        EquipContains.GetChild(i).Find("SeafTxt").GetComponent<TextMeshProUGUI>().text = "高";
                    }
 
                }
            }

        }

        //在线离线数量展示
        CameraStateTxt.text = "<color=green>" + cameraOnline.ToString() + "</color>" + "/" + "<color=white>" + (cameraOnline+cameraOutline).ToString() + "</color>";
    }




    /// <summary>
    /// 门禁设备在线状态监听
    /// </summary>
    /// 
   public  int doorOnline = 0;
   public  int doorOutline = 0;
    public int AllDoorEquip = 0;
    private void DoorNetStateListener(string ip)
    {
         //doorOnline = 0;
         //doorOutline = 0;
        int loginhandle = 0;
        if (HikvisonNVR.LoginhandleDic.TryGetValue(ip,out loginhandle))
        {
            for (int i = 0; i < DoorEquipContains.childCount; i++)
            {
               
                if (string.Equals(DoorEquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text.Split(':')[0].Trim(),ip))
                {

                    if (HikvisonNVR.LoginhandleDic[ip] > -1)
                    {
                        DoorEquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text = "<color=green> 在线状态</color>";
                        doorOnline++;
                        NVRController.Instance().Logout(ip);//查询完在线状态后立马登出，不要占用通道影响门禁权限下发
                    }
                    else
                    {
                        DoorEquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text = "<color=red> 离线状态</color>";
                        doorOutline++;
                    }
                    //Log.Error("移除：" +ip);
                    //HikvisonNVR.LoginhandleDic.Remove(ip);
                } 
            }
        }
        else
        {
            // Log.Debug(ip+"不存在于登陆句柄字典中");
            //DoorEquipContains.GetChild(i).Find("NetTxt").GetComponent<TextMeshProUGUI>().text = "登录失败";
        }
        string serinum = string.Empty;
        if (HikvisonNVR.SerinumDic.TryGetValue(ip, out serinum))
        {
            for (int i = 0; i < DoorEquipContains.childCount; i++)
            {

                if (string.Equals(DoorEquipContains.GetChild(i).Find("IPTxt").GetComponent<TextMeshProUGUI>().text.Split(':')[0].Trim(), ip))
                {

                    if (!string.IsNullOrEmpty(HikvisonNVR.SerinumDic[ip]))
                    {
                        DoorEquipContains.GetChild(i).Find("SerialnumTxt").GetComponent<TextMeshProUGUI>().text = HikvisonNVR.SerinumDic[ip];
                        DoorEquipContains.GetChild(i).Find("SeafTxt").GetComponent<TextMeshProUGUI>().text = "高";
                    }

                }
            }

        }
        AllDoorEquip = doorOnline + doorOutline;
        Debug.LogError("离线门禁:"+doorOutline);
        //在线离线数量展示
        DoorStateTxt.text = "<color=green>" +doorOnline + "</color>" + "/" + "<color=white>" + AllDoorEquip + "</color>";

    }



    private void OnDestroy()
    {
      
        EventCenter.RemoveListener<string>(Eventdefine.DeviceNetSate,EquipNetStateListener);
        EventCenter.RemoveListener<string>(Eventdefine.DoorNetState, DoorNetStateListener);
    }
}
