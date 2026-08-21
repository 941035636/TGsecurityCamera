using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventPageController : SingletonManager<EventPageController>
{
    public GameObject[] RightAllPage;
    public Transform RealEventPrefabListContent;
    public Transform RealeventContent;
    // Start is called before the first frame update


    private new void Awake()
    {
        //EventCenter.addlistener<DoorAlarmData>(Eventdefine.InistEventListener, InistEventMesg);
    }

    //初始化门禁消息监听
    void InistEventMesg(DoorAlarmData data)
    {
        //GameObject dooreventObj= ObjectManager.Instance.InstantiateObject(ConStr.REALEVENTPREFAB);
        // dooreventObj.transform.SetParent(RealEventPrefabListContent);
        // resetPrefab(dooreventObj);
        // dooreventObj.GetComponent<RealEventInfo>().InitInfo(data.time,data.eventType,data.equipName,null,null, RealeventContent);
        //Log.Error("异步加载:"+data.idNum);

      

    }



    public void CloseRightAllPage()
    {
        for (int i = 0; i < RightAllPage.Length; i++)
        {
            RightAllPage[i].gameObject.SetActive(false);
        }
    }
    private new void OnDestroy()
    {
        //EventCenter.RemoveListener<DoorAlarmData>(Eventdefine.InistEventListener, InistEventMesg);
    }

    /// <summary>
    /// 重置预制体
    /// </summary>
    /// <param name="obj"></param>
    void resetPrefab(GameObject obj)
    {
        obj.transform.localScale = Vector3.one;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localPosition = Vector3.zero;
    }
}
