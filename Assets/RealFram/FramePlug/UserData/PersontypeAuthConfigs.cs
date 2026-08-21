using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PersontypeAuthConfigs : ScriptableObject
{
    private static PersontypeAuthConfigs instance;
    public static PersontypeAuthConfigs GetInstance()
    {
        if (instance==null)
        {
            instance = new PersontypeAuthConfigs();

        }
        return instance;
    }

    public string PersontypeAuth = "PersontypeAuthConfiguration.json";

    public string jsonPath = Path.Combine(Application.streamingAssetsPath, "PersontypeAuth");

    public void SaveDoorGroupConfiguration_outside(AllPersontypeAuth allPersontypeAuth)
    {

        if (!Directory.Exists(jsonPath))
        {
            Directory.CreateDirectory(jsonPath);
        }
 
        var info = JsonUtility.ToJson(allPersontypeAuth, true);
        var file = Path.Combine(jsonPath, PersontypeAuth);
        File.WriteAllText(file, info, Encoding.UTF8);
    }
    public string UsertypeAuth = "Usertype.json";

    public void SaveUserTypeonfiguration_outside(List<UserType> usertypelist)
    {

        if (!Directory.Exists(jsonPath))
        {
            Directory.CreateDirectory(jsonPath);
        }
 
        var info = JsonUtility.ToJson(usertypelist, true);
        var file = Path.Combine(jsonPath, UsertypeAuth);
        if (File.Exists(file))
        {
            File.Delete(file);
        }
        File.WriteAllText(file, info, Encoding.UTF8);
    }


}
