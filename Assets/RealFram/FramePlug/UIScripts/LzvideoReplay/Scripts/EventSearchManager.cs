using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventSearchManager : SingletonManager<EventSearchManager>
{
    private GameObject UpArea;
    private GameObject MiddleArea;
    public GameObject DownArea;
    private GameObject RealEventPrefabList;
    private Button ZhanKaiBtn;
    private Button YinCangBtn;
    void Start()
    {
        UpArea = transform.Find("Scroll View/Viewport/Content/UP").gameObject;
        MiddleArea = transform.Find("Scroll View/Viewport/Content/Middle").gameObject;
        //DownArea = transform.Find("Scroll View/Viewport/Content/Down").gameObject;
        RealEventPrefabList = transform.Find("Scroll View/Viewport/Content/UP/RealEventPrefabList").gameObject;
        ZhanKaiBtn = transform.Find("Scroll View/Viewport/Content/Middle/open").GetComponent<Button>();
        YinCangBtn = transform.Find("Scroll View/Viewport/Content/Middle/close").GetComponent<Button>();
        ZhanKaiBtn.onClick.AddListener(ZhanKaiDownArea);
        YinCangBtn.onClick.AddListener(YinCangDownArea);

    }
    void ZhanKaiDownArea()
    {
        DownArea.SetActive(true);
        ZhanKaiBtn.gameObject.SetActive(false);
        YinCangBtn.gameObject.SetActive(true);
        UpArea.GetComponent<RectTransform>().sizeDelta = new Vector2(UpArea.GetComponent<RectTransform>().sizeDelta.x, 575);
        RealEventPrefabList.GetComponent<RectTransform>().sizeDelta = new Vector2(RealEventPrefabList.GetComponent<RectTransform>().sizeDelta.x, 413.2f);
    }
    void YinCangDownArea()
    {
        DownArea.SetActive(false);
        UpArea.GetComponent<RectTransform>().sizeDelta = new Vector2(UpArea.GetComponent<RectTransform>().sizeDelta.x, 1016);
        RealEventPrefabList.GetComponent<RectTransform>().sizeDelta = new Vector2(RealEventPrefabList.GetComponent<RectTransform>().sizeDelta.x, 854.2f);
        ZhanKaiBtn.gameObject.SetActive(true);
        YinCangBtn.gameObject.SetActive(false);
    }
}
