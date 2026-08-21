using System.Globalization;
using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeopleInfo : MonoBehaviour
{
    public string username;
    public string salaryNum;
    public string phoneNum;
    public string unitName;
    public string idNum;
    public string equipName;
    public string address;
    public string remarks;
    public string authDate;
    public string authTime;
    public int type;
    public string base64;
    public string typeName;

    public void ShowChange()
    {
        PeopleController.Ins.OpenChangePersonPage(username, salaryNum, phoneNum, unitName, idNum, address, remarks, type, authDate, authTime,base64);
    }
}
