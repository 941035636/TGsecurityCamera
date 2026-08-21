/*
 * JacobKay --20220903
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZTools;
/// <summary>
/// 使用示例
/// </summary>
public class ZCalendarPeople : SingletonManager<ZCalendarPeople>
{
    public ZCalendar zCalendar;
    private Button DateSaveBtn;
    private DateTime day1;
    private DateTime day2;
    // Start is called before the first frame update
    void Start()
    {
        zCalendar.UpdateDateEvent += ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent += ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent += ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent += ZCalendar_CompleteEvent;
        day1 = DateTime.Today;
        day2 = DateTime.Today;
        DateSaveBtn = transform.Find("bak/DateSave").GetComponent<Button>();
        DateSaveBtn.onClick.AddListener(SaveDate);
        //zCalendar.Init();
        //zCalendar.Init(System.DateTime.Now);
        //zCalendar.Init("2022-02-02");
        //zCalendar.Show();
        //zCalendar.Hide();
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
        PeopleController.Ins.StartDate.text = day1.ToString("yyyy-MM-dd");
        PeopleController.Ins.EndDate.text = day2.ToString("yyyy-MM-dd");
        PeopleController.Ins._zcalendar.GetComponent<ZCalendar>().Hide();

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
         Log.Debug($"选择的时间区间：{arg1.Year + "-" + arg1.Month + "-" + arg1.Day}到{arg2.Year + "-" + arg2.Month + "-" + arg2.Day}");
        day1 = arg1.dateTime;
        day2 = arg2.dateTime;
    }

    /// <summary>n
    /// 获取选择的日期
    /// </summary>
    /// <param name="obj"></param>
    private void ZCalendar_ChoiceDayEvent(ZCalendarDayItem obj)
    {
         Log.Debug($"选择的日期：{obj.Day}");
    }

    /// <summary>
    /// 切换月份时，可拿到每一天的item对象
    /// </summary>
    /// <param name="obj"></param>
    private void ZCalendar_UpdateDateEvent(ZCalendarDayItem obj)
    {
        //  Log.Debug($"加载日期：{obj.Day}");
    }
    private void OnDestroy()
    {
        zCalendar.UpdateDateEvent -= ZCalendar_UpdateDateEvent;
        zCalendar.ChoiceDayEvent -= ZCalendar_ChoiceDayEvent;
        zCalendar.RangeTimeEvent -= ZCalendar_RangeTimeEvent;
        zCalendar.CompleteEvent -= ZCalendar_CompleteEvent;
    }
}
