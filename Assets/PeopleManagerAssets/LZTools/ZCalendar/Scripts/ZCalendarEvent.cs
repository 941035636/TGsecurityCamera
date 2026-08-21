/*
 * JacobKay --20220903
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
using TMPro;
using System.Globalization;
/// <summary>
/// 使用示例
/// </summary>
public class ZCalendarEvent : SingletonManager<ZCalendarEvent>
{
    public ZCalendar zCalendar;
    private Button DateSaveBtn;
    private DateTime day1;
    private DateTime day2;
    public static string EventSearchStartTime=string.Empty;
    public static string EventSearchEndTime = string.Empty;
    // Start is called before the first frame update
    void Start()
    {
        day1 = DateTime.Today;
        day2 = DateTime.Today;
        DateSaveBtn = transform.Find("bak/DateSave").GetComponent<Button>();
        DateSaveBtn.onClick.RemoveAllListeners();
        DateSaveBtn.onClick.AddListener(SaveDate);
        zCalendar.UpdateDateEvent += ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent += ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent += ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent += ZCalendar_CompleteEvent;
   

    }
    /// <summary>
    /// 加载结束
    /// </summary>
    private void ZCalendar_CompleteEvent()
    {
         Log.Debug("ZCalendar加载结束");
        if (null != zCalendar.CrtTime)
        {
             Log.Debug($"当前时间{zCalendar.CrtTime.Day}");
        }
    }
    public void SaveDate()
    {
        EventSearchStartTime = day1.ToString("yyyy-MM-dd") +" "+ ChoiceTime.SHour+":" + ChoiceTime.SMinute+":"+ ChoiceTime.SSecond;
        EventSearchEndTime = day2.ToString("yyyy-MM-dd") + " " + ChoiceTimeEnd.EHour+":" + ChoiceTimeEnd.EMinute+":" + ChoiceTimeEnd.ESecond;

        if (day1.ToString("yyyy-MM-dd") == day2.ToString("yyyy-MM-dd")) 
        {
            if (int.Parse(ChoiceTime.SHour) > int.Parse(ChoiceTimeEnd.EHour))
            {
                GameStart.Instance.ShowTip("起始时间不能早于截至时间");

                return;

            }
            if (int.Parse(ChoiceTime.SHour) == int.Parse(ChoiceTimeEnd.EHour) &&int.Parse( ChoiceTime.SMinute)> int.Parse( ChoiceTimeEnd.EMinute))
            {
                GameStart.Instance.ShowTip("起始时间不能早于截至时间");

                return;

            }
            if (int.Parse(ChoiceTime.SHour) == int.Parse(ChoiceTimeEnd.EHour) && int.Parse(ChoiceTime.SMinute) == int.Parse(ChoiceTimeEnd.EMinute)&& int.Parse(ChoiceTime.SSecond)>int.Parse(ChoiceTimeEnd.ESecond))
            {
                GameStart.Instance.ShowTip("起始时间不能早于截至时间");

                return;

            }
            ZCalendarController.isInRange = !ZCalendarController.isInRange;
        }


        Log.Debug("保存日期day1:" + EventSearchStartTime + " day2:" + EventSearchEndTime);
        GetComponent<ZCalendar>().Hide();
        EventCenter.BroadCast<string, string>(Eventdefine.ChoiceTimeShow,day1.ToString("yyyy-MM-dd"),day2.ToString("yyyy-MM-dd"));

    }
    /// <summary>
    /// 区间时间
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void ZCalendar_RangeTimeEvent(ZCalendarDayItem arg1, ZCalendarDayItem arg2)
    {
        TimeSpan difference = arg2.dateTime - arg1.dateTime;
       
        int days = difference.Days;
        if (days <= 30)
        {
            Log.Debug($"选择的时间区间：{arg1.Year + "-" + arg1.Month + "-" + arg1.Day}到{arg2.Year + "-" + arg2.Month + "-" + arg2.Day}");
            day1 = arg1.dateTime;
            day2 = arg2.dateTime;
            Date = day2.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
            Log.Debug("DAte:"+Date);
       
            IsStart = false;
        }
        else
        {
             Log.Debug("不能超过三十天");
        }
    }

    /// <summary>n
    /// 获取选择的日期
    /// </summary>
    /// <param name="obj"></param>
    public static string Date = string.Empty;
    public static bool IsStart = false;
    private void ZCalendar_ChoiceDayEvent(ZCalendarDayItem arg1)
    {
        //EventCenter.BroadCast<DateTime>(Eventdefine.ChoiceDatetime, obj.dateTime);
         Log.Debug($"选择的日期：{arg1.Year+"-"+arg1.Month+"-"+arg1.Day}");
        Date = arg1.dateTime.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) ;
        day1 = arg1.dateTime;
        day2 = arg1.dateTime;
        Log.Debug("Date:" + Date);
        IsStart = true;
    }

    /// <summary>
    /// 切换月份时，可拿到每一天的item对象
    /// </summary>
    /// <param name="obj"></param>
    private void ZCalendar_UpdateDateEvent(ZCalendarDayItem obj)
    {
        //Log.Debug($"加载日期：{obj.Day}");

    }
    private void OnDestroy()
    {
        zCalendar.UpdateDateEvent -= ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent -= ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent -= ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent -= ZCalendar_CompleteEvent;
    }
}
