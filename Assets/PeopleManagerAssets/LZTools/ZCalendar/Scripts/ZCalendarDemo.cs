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
public class ZCalendarDemo : SingletonManager<ZCalendarDemo>
{
    public ZCalendar zCalendar;
    private Button DateSaveBtn;
    private DateTime day1;
    private DateTime day2;
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
      
        Log.Debug("保存日期day1:" + day1.ToString("yyyy-MM-dd") + " day2:" + day2.ToString("yyyy-MM-dd"));
        ReplayUIController.Ins.CreatedData(day1, day2);

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
