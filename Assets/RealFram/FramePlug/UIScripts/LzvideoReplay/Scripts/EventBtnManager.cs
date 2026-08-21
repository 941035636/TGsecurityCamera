using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventBtnManager : MonoBehaviour
{
    private Toggle[] toggles;
    // Start is called before the first frame update
    void Start()
    {
        //找到所有的toggles
        toggles = transform.GetComponentsInChildren<Toggle>();
        //给toggle添加事件
        for (int i = 0; i < toggles.Length; i++)
        {
            //这一步是必须记录的，用来区分那个toggle
            int K = i;
      
            //toggles[K].onValueChanged.AddListener((ison) => { ToggleDebug(K, ison); });
        }
    }

    public void ToggleDebug(int index, bool value)
    {
        if (value)
        {
            transform.GetChild(index).GetComponent<Toggle>().interactable = false;
             Log.Debug("开启" + index);
        }
        else
        {
            transform.GetChild(index).GetComponent<Toggle>().interactable = true;
             Log.Debug("关闭" + index);
        }
        if (index == 0 && value)
        {
            EventPageController.Ins.CloseRightAllPage();
            EventPageController.Ins.RightAllPage[0].SetActive(true);
        }
        if (index == 1 && value)
        {
            EventPageController.Ins.CloseRightAllPage();
            EventPageController.Ins.RightAllPage[1].SetActive(true);
        }
    }
}
