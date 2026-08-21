
using UnityEngine;
using System;
using UnityEngine.UI;
public class Example : MonoBehaviour
{
    public Text errorTips;
    public  DateTime starTime;//起始时间
    public DateTime endTime;//结束时间
    public Button button;
    public string starTimeDate;
    public string endTimeDate;
    private void Start()
    {
        this.transform.GetChild(2).GetComponent<Text>().text = DateTime.Now.ToString();
    }
    /// <summary>
    /// 起始时间
    /// </summary>
    /// <param name="datetime"></param>
    public void OnSelectedTime()
    {
        starTime = this.transform.GetChild(0).GetComponent<Calendar>().CurrentSelectedTime;
        Debug.Log(string.Format(">>>>{0}", starTime));
        this.transform.GetChild(0).gameObject.SetActive(false);
        this.transform.GetChild(1).gameObject.SetActive(true);  
        
    }

    /// <summary>
    /// 结束时间
    /// </summary>
    /// <param name="dateTime"></param>
   public void EndSelectTime()
    {
        endTime = this.transform.GetChild(1).GetComponent<Calendar>().CurrentSelectedTime;
       
        int compareResult = DateTime.Compare(starTime, endTime);
        if (compareResult > 0)
        {
            button.interactable = false;
            this.transform.GetChild(2).GetComponent<Text>().text = starTime.ToString("yyyy-MM-dd HH:mm") + "至" + endTime.ToString("yyyy-MM-dd HH:mm");
            Debug.Log("起始日期晚于结束日期！请重新选择日期");
            errorTips.text = "起始日期晚于结束日期！请重新选择日期";
            errorTips.color = Color.red;
        }
        else if (compareResult == 0)
        {
            button.interactable = false;
            Debug.Log("起始日期等于结束日期！请重新选择日期");
            errorTips.text = "起始日期等于结束日期！请重新选择日期";
            errorTips.color = Color.red;
            this.transform.GetChild(2).GetComponent<Text>().text = starTime.ToString("yyyy-MM-dd HH:mm") + "至" + endTime.ToString("yyyy-MM-dd HH:mm");
        }
        else
        {
            Debug.Log("起始时间：" + starTime + "结束时间:" + endTime);
            Debug.Log("请求时间段" + starTime.ToString("yyyy-MM-dd HH:mm:ss") + endTime.ToString("yyyy-MM-dd HH:mm:ss"));
            this.transform.GetChild(2).GetComponent<Text>().text = starTime.ToString("yyyy-MM-dd HH:mm") +"至"+ endTime.ToString("yyyy-MM-dd HH:mm");
            starTimeDate = starTime.ToString("yyyy-MM-dd HH:mm");
            endTimeDate=endTime.ToString("yyyy-MM-dd HH:mm");
            button.interactable = true;
        }
        this.transform.GetChild(1).gameObject.SetActive(false);
    }
    
}
