using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

public class ChoiceTime : MonoBehaviour
{
    [Tooltip("箭头图片")]
    public Sprite[] Arrows;
    //显示时间的文字
    //public TMP_Text ShowText;
    public static string  StartTime;
    public InputField SHourInput;
    public InputField SMintueInput;
    public InputField SSecondInput;
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

    public static string SHour = "00";
    public static string SMinute = "00";
    public static string SSecond = "00";

    private static ChoiceTime instance;
    public static ChoiceTime GetInstance() 
    {
        if (instance==null)
        {
            instance = new ChoiceTime();
        }
        return instance;
    }

    // Use this for initialization
    void Start()
    {
        ChoiceBtn.onClick.AddListener(StartChoiceTime);
        //开始默认选择系统时间
        // ShowText.text = DateTime.Now.ToString("HH : mm : ss");
        ChoiceTimeObj.SetActive(false);


        SHourInput.text = "00";
        SMintueInput.text = "00";
        SSecondInput.text = "00";
        SHourInput.onEndEdit.AddListener((htext) => 
        {
      
 
            Regex hreg = new Regex(Hregex);
            if (hreg.IsMatch(htext))
            {
                SHourInput.text = htext;
                SHour = htext;
            }
            else if (htext.Length == 2 && !hreg.IsMatch(htext))
            {



                GameStart.Instance.ShowTip("输入格式不正确!");
                SHourInput.text = "00";
            }
            else 
            {
                SHourInput.text = "0"+htext;
                SHour = "0"+htext;

            }
        
        });

        SMintueInput.onEndEdit.AddListener((mtext) =>
        {

      
            Regex hreg = new Regex(Mregex);
            if (hreg.IsMatch(mtext))
            {
                SMintueInput.text = mtext;
                SMinute = mtext;
            }
            else if ((mtext.Length == 2 && !hreg.IsMatch(mtext)))
            {
                GameStart.Instance.ShowTip("输入格式不正确!");
                SMintueInput.text = "00";
            }
            else 
            {
                SMintueInput.text = "0"+mtext;
                SMinute = "0"+mtext;
            }

        });

        SSecondInput.onEndEdit.AddListener((stext) =>
        {


            Regex hreg = new Regex(Sregex);
            if (hreg.IsMatch(stext))
            {
                SSecondInput.text = stext;
                SSecond = stext;
            }
            else if (((stext.Length == 2 && !hreg.IsMatch(stext))))
            {
                GameStart.Instance.ShowTip("输入格式不正确!");
                SSecondInput.text = "00";
            }
            else 
            {
                SSecondInput.text = "0"+stext;
                SSecond = "0"+stext;

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
               StartTime = DatePickerGroup._selectTime.ToString("HH:mm:ss");
                Log.Error("StartTime:"+StartTime);
                SHourInput.text = StartTime.Split(':')[0].Trim();
                SHour= StartTime.Split(':')[0].Trim();
                SMintueInput.text = StartTime.Split(':')[1].Trim();
                SMinute= StartTime.Split(':')[1].Trim();
                SSecondInput.text = StartTime.Split(':')[2].Trim();
                SSecond = StartTime.Split(':')[2].Trim();
                Log.Error("STARTTIME:"+StartTime);

            }
        }
    }
}
