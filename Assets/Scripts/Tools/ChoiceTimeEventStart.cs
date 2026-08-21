using System;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceTimeEventStart : MonoBehaviour
{
    //显示时间的文字
    public Text ShowText;
    //选择时间界面
    public GameObject ChoiceTimeObj;
    //按钮
    public Button ChoiceBtn;
    //是否选择时间
    private bool isShowChoiceTime;
    public static bool IsChoiceTimeStart;
    // Use this for initialization
    void Start()
    {
        
        ChoiceBtn.onClick.AddListener(StartChoiceTime);
        //开始默认选择系统时间
        ShowText.text = DateTime.Now.ToString("yyyy年MM月dd日 HH : mm : ss");
        ChoiceTimeObj.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (ChoiceTimeObj.activeSelf)
        {
            isShowChoiceTime = true;
        }
        else
        {
            isShowChoiceTime = false;
        }
    }


    public void StartChoiceTime()
    {
        if (!isShowChoiceTime)
        {
            IsChoiceTimeStart = true;
            //显示选择时间界面
            ChoiceTimeObj.SetActive(true);
        }
        else
        {
            //关闭选择时间界面
            ChoiceTimeObj.SetActive(false);
            //是否显示时间选择界面为false
            isShowChoiceTime = false;
            IsChoiceTimeStart = false;
            //判断选没选择日期，当只点开选择框没有选择时，默认的日期会变为001年。所以要判断下
            if (DatePickerGroupUser._selectTime.ToString("yyyy年MM月dd日 HH : mm : ss").Substring(0, 3) == "000")
            {
                ShowText.text = DateTime.Now.ToString("yyyy年MM月dd日 HH : mm : ss");
            }
            else
            {
                ShowText.text = DatePickerGroupUser._selectTime.ToString("yyyy年MM月dd日 HH : mm : ss");
            }
        }
    }
}
