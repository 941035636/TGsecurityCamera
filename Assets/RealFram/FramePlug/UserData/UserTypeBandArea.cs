using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#region  人员与用户类型绑定/门禁设备与区域绑定



//人员类型与区域绑定
[SerializeField]
public class UserTypeBandArea
{

    public string userType; 
    public  List<string> usertypeareaDic = new List<string>();     //人员类型与区域绑定



}
#endregion