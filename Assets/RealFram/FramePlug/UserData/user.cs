using UnityEngine;


[SerializeField]
public class user
{
    public string username = string.Empty;
    public string password = string.Empty;

    public user(string user,string pwd) 
    {
        username = user;
        password = pwd;
    }
   
}
