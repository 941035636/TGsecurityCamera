using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using zFramework.Media;
public class DoorJson : MonoBehaviour
{

    private static DoorJson instance;
    public static DoorJson GetInstance()
    {

        if (instance == null)
        {
            instance = new DoorJson();
        }
        return instance;
    }
    public static Dictionary<Area, CameraGroupInformationD> AreadeviceDic;

    [SerializeField]

    void Awake()
    {
        UsertyeArea();
    }
    void Start()
    {
        AreadeviceDic = new Dictionary<Area, CameraGroupInformationD>();
        //从本地取
        string str = File.ReadAllText(Application.streamingAssetsPath + "/Configurations/DoorGroupConfiguration.json");
         Log.Debug(str);
        DKGroupWrapper GW = JsonUtility.FromJson<DKGroupWrapper>(str);
         Log.Debug(GW.CameraGroupList.Count);
        foreach (var item in GW.CameraGroupList)
        {



            if (!AreadeviceDic.ContainsKey(item.GroupName))
            {
                AreadeviceDic.Add(item.GroupName, item);
            }


        }

        //用
        //东一门
        //CameraGroupInformationD CameraGroupInformationList;
        //if (AreadeviceDic.TryGetValue(Area.Earth1, out CameraGroupInformationList))
        //{
        //    for (int i = 0; i < CameraGroupInformationList.CameraInfos.Count; i++)
        //    {
        //         Log.Debug("设备IP:" + CameraGroupInformationList.CameraInfos[i].Ip);
        //         Log.Debug("设备端口" + CameraGroupInformationList.CameraInfos[i].Port);
        //        //HKPerson.instance.Login(CameraGroupInformationList.CameraInfos[i].Ip,CameraGroupInformationList.CameraInfos[i].Port,CameraGroupInformationList.CameraInfos[i].userName,CameraGroupInformationList.CameraInfos[i].password);
        //    }
        //}




    }


    ///初始化人员类型和区域绑定
    void UsertyeArea()
    {
        UserTypeArea.GetInstance().init();
        for (int i = 0; i < Enum.GetValues(typeof(Area)).Length; i++)
        {

            //UserTypeArea.GetInstance().employeelist.Add((Area)(Enum.GetValues(typeof(Area)).GetValue(i)));
        }
        UserTypeArea.GetInstance().employeelist.Add(Area.West4);

        UserTypeArea.GetInstance().retireeslist.Add(Area.LifeArea1);
        UserTypeArea.GetInstance().retireeslist.Add(Area.LifeArea2);
        UserTypeArea.GetInstance().family_memlist.Add(Area.LifeArea1);
        UserTypeArea.GetInstance().family_memlist.Add(Area.LifeArea2);

        if (!UserTypeArea.GetInstance().usertypeareaDic.ContainsKey(Usertype.employee))
            UserTypeArea.GetInstance().usertypeareaDic.Add(Usertype.employee, UserTypeArea.GetInstance().employeelist);
        if (!UserTypeArea.GetInstance().usertypeareaDic.ContainsKey(Usertype.retirees))
            UserTypeArea.GetInstance().usertypeareaDic.Add(Usertype.retirees, UserTypeArea.GetInstance().retireeslist);
        if (!UserTypeArea.GetInstance().usertypeareaDic.ContainsKey(Usertype.family_mem))
            UserTypeArea.GetInstance().usertypeareaDic.Add(Usertype.family_mem, UserTypeArea.GetInstance().family_memlist);


    }





}



[Serializable]
public class DKGroupWrapper
{
    public List<CameraGroupInformationD> CameraGroupList;
    public DKGroupWrapper(List<CameraGroupInformationD> arr)
    {
        this.CameraGroupList = arr;
    }

    //public Dictionary<string, List<NVRInformation>> cameragroupDic;
    //public GroupWrapper(Dictionary<string, List<NVRInformation>> cameragroupDic)
    //{
    //    this.cameragroupDic = cameragroupDic;
    //}

}


[Serializable]
public class CameraGroupInformationD //区域
{
    ////分组名
    public Area GroupName;
    public int id;//id
    ////组下的监控
    public List<NVRInformation> CameraInfos;


}

