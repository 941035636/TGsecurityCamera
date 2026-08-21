using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class MyData
{
    public string id;
    public string camId;
    public string eventTime;
    public string idNum;
    public string personAction;
    public string isFire;
    public string carNum;
    public string type;
    public string camName;
    public string username;
}
[Serializable]
public class RecordsData
{
    public List<MyData> records = new List<MyData>();
}

   

