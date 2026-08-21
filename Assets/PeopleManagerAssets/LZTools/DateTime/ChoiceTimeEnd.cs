using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class ChoiceTimeEnd : MonoBehaviour
{
    [Tooltip("箭头图片")]
    public Sprite[] Arrows;
    //显示时间的文字
    //public TMP_Text ShowText;
    public static string EndTime;
    public InputField EHourInput;
    public InputField EMintueInput;
    public InputField ESecondInput;
    //显示箭头的图片
    public Image ArrowsImage;
    //选择时间界面
    public GameObject ChoiceTimeObj;
    //按钮
    public Button ChoiceBtn;
    //是否选择时间
    private bool isShowChoiceTime;

    private const string Hregex = "(0[0-9]|1[0-9]|2[0-3])";
    private const string Mregex = "[0-5][0-9]";
    private const string Sregex = "[0-5][0-9]";
    private static ChoiceTimeEnd Instance;
    public static ChoiceTimeEnd GetInstance()
    {
        if (Instance == null)
            Instance = new ChoiceTimeEnd();
        return Instance;
    }
    public static string EHour = "00";
    public static string EMinute = "00";
    public static string ESecond = "00";
    // Use this for initialization
    void Start()
    {
        ChoiceBtn.onClick.AddListener(StartChoiceTime);
        //开始默认选择系统时间
        // ShowText.text = DateTime.Now.ToString("HH : mm : ss");
        ChoiceTimeObj.SetActive(false);


        EHourInput.text = "00";
        EMintueInput.text = "00";
        ESecondInput.text = "00";
        EHourInput.onEndEdit.AddListener((htext) =>
        {


            Regex hreg = new Regex(Hregex);
            if (hreg.IsMatch(htext))
            {
                EHourInput.text = htext;
                EHour = htext;
            }
            else if (htext.Length == 2 && !hreg.IsMatch(htext))
            {
                GameStart.Instance.ShowTip("输入格式不正确!");
                EHourInput.text = "00";
            }
            else 
            {
                EHourInput.text = "0"+htext;
                EHour = "0"+htext;
            }

        });

        EMintueInput.onEndEdit.AddListener((mtext) =>
        {


            Regex hreg = new Regex(Mregex);
            if (hreg.IsMatch(mtext))
            {
                EMintueInput.text = mtext;
                EMinute = mtext;
            }
            else if (mtext.Length == 2 && !hreg.IsMatch(mtext))
            {
                GameStart.Instance.ShowTip("输入格式不正确!");
                EMintueInput.text = "00";
            }
            else 
            {
                EMintueInput.text = "0"+mtext;
                EMinute = "0"+mtext;
            }

        });

        ESecondInput.onEndEdit.AddListener((stext) =>
        {


            Regex hreg = new Regex(Sregex);
            if (hreg.IsMatch(stext))
            {
                ESecondInput.text = stext;
                ESecond = stext;
            }
            else if (stext.Length == 2 && !hreg.IsMatch(stext))
            {
                GameStart.Instance.ShowTip("输入格式不正确!");
                ESecondInput.text = "00";
            }
            else 
            {
                ESecondInput.text = "0"+stext;
                ESecond = "0"+stext;
            }

        });

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
        //作战时间的number为1,开始时间的number为2   
        if (!isShowChoiceTime)
        {
            //显示选择时间界面
            ChoiceTimeObj.SetActive(true);
            //箭头向下
            ArrowsImage.sprite = Arrows[1];


        }
        else
        {
            //关闭选择时间界面
            ChoiceTimeObj.SetActive(false);
            //是否显示时间选择界面为false
            isShowChoiceTime = false;
            //箭头向上
            ArrowsImage.sprite = Arrows[0];
            //判断选没选择日期，当只点开选择框没有选择时，默认的日期会变为001年。所以要判断下
            if (DatePickerGroup._selectTime.ToString("HH : mm : ss").Substring(0, 3) == "000")
            {
                //ShowText.text = DateTime.Now.ToString("HH : mm : ss");
            }
            else
            {
                EndTime = DatePickerGroup._selectTime.ToString("HH:mm:ss");
                Log.Error("StartTime:" + EndTime);
                EHourInput.text = EndTime.Split(':')[0].Trim();
                EHour = EndTime.Split(':')[0].Trim();
                EMintueInput.text = EndTime.Split(':')[1].Trim();
                EMinute = EndTime.Split(':')[1].Trim();
                ESecondInput.text = EndTime.Split(':')[2].Trim();
                ESecond = EndTime.Split(':')[2].Trim();
            }
        }
    }
}
