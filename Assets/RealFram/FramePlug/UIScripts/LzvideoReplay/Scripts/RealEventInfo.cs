using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RealEventInfo : MonoBehaviour
{
    private static RealEventInfo Instance;
    public static RealEventInfo GetInstance()
    {
        if (Instance==null)
        {
            Instance = new RealEventInfo();
        }
        return Instance;
    }

    public Button ChaKan;
    public TMP_Text Time;
    public TMP_Text EventType;
    public TMP_Text CameraName;
    public string EventVideoURL;
    public string EventDetail;

    private void Start()
    {
        //ChaKan.onClick.RemoveAllListeners();
        //ChaKan.onClick.AddListener(SendInfo);
 
    }
    public void SendInfo()
    {
        //RealTimeManager.Ins.DownArea.GetComponent<EventDownManager>().PlayEventVideo(EventVideoURL, EventDetail);
  
    }



    public void InitInfo(string time, string eventType, string cameraName, string eventVideoURL, string eventDetail,Transform Content)
    {
        Time.text = time;
        EventType.text = eventType;
        CameraName.text = cameraName;
        EventVideoURL = eventVideoURL;
        EventDetail = eventDetail;
        Log.Debug(ChaKan.name);
        ChaKan.onClick.RemoveAllListeners();
        ChaKan.onClick.AddListener(() =>
        {
            Content.Find("Middle").gameObject.SetActive(true);
            Content.Find("Down").gameObject.SetActive(true);
            Content.Find("Middle/close").GetComponent<Button>().onClick.RemoveAllListeners();
            Content.Find("Middle/close").GetComponent<Button>().onClick.AddListener(() =>
            {
                Content.Find("Middle").gameObject.SetActive(false);
                Content.Find("Down").gameObject.SetActive(false);
            });
        });
    }
}
