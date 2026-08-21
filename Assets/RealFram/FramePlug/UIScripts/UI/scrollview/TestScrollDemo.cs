using System.Collections;
using System.Collections.Generic;
using CircularScrollView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class TestScrollDemo : MonoBehaviour {

    private Button ReturnBtn;
    private static TestScrollDemo Instance;
    public static TestScrollDemo GetInstance() {

        if (Instance == null) {
            Instance = new TestScrollDemo();
        }
        return Instance;
    }

    public UICircularScrollView VerticalScroll_1;
    //public UICircularScrollView VerticalScroll_2;
    //public UICircularScrollView HorizontalScroll_1;
    //public UICircularScrollView HorizontalScroll_2;

    //public Button btn;

    // Use this for initialization
    void Start()
    {
        StartScrollView(500);
    }
    private void OnDestroy()
    {

    }



    public void StartScrollView(int item_count)
    {
        VerticalScroll_1.Init(NormalCallBack);
        VerticalScroll_1.ShowList(item_count);

        //VerticalScroll_2.Init(NormalCallBack);
        //VerticalScroll_2.ShowList(50);

        //HorizontalScroll_1.Init(NormalCallBack);
        //HorizontalScroll_1.ShowList(50);

        //HorizontalScroll_2.Init(NormalCallBack);
        //HorizontalScroll_2.ShowList(50);
    }
    GameObject cellArb;
    private void NormalCallBack(GameObject cell, string index)
    {
        Log.Debug("callback："+ cell.name);
        //cell.transform.Find("IdNum").GetComponent<TextMeshProUGUI>().text = index.ToString();
        //cell.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = index.ToString();
        cellArb = cell;
    HttprequestAlthriomInfo(cell.name);


    }
    void HttprequestAlthriomInfo(string num)
    {

        string url = "http://192.168.110.2:7711/api/alarm/info?pageNum="+num+"&pageSize=1";
        HttpNetManager.GetInstance().SendDataStr(url, MqtthttpAlthomriCallback, false, false, true);
    }
    //算法回调
    public void MqtthttpAlthomriCallback(HttpCallBackArgs args)
    {
         Log.Debug("Http收到服务器信息:" + args.Value);
        if (!string.IsNullOrEmpty(args.Value))
        {
            Althorims althorims = JsonUtility.FromJson<Althorims>(args.Value);
            cellArb.transform.Find("IdNum").GetComponent<TextMeshProUGUI>().text = althorims.records[0].camId;
            for (int i = 0; i < althorims.records.Count; i++)
            {
                EventCenter.BroadCast(Eventdefine.FacemesgListener, althorims.records[i]);
            }
        }

    }
}
