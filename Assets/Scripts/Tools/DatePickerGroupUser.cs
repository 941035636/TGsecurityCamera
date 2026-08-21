using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 日期选择组
/// </summary>
public class DatePickerGroupUser : MonoBehaviour
{
    /// <summary>
    /// 最小日期和最大日期
    /// </summary>
    public DateTime _minDate, _maxDate;
    /// <summary>
    /// 选择的日期（年月日时分秒）
    /// </summary>
    public DateTime _selectDate;
    /// <summary>
    /// 时间选择器列表
    /// </summary>
    public List<DatePickerUser> _datePickerList;
    /// <summary>
    /// 当选择日期的委托事件
    /// </summary>
    public event OnDateUpdate _OnDateUpdate;

    public static DateTime _selectTime;
    void Awake()
    {
        //设置最大最小日期
        _minDate = new DateTime(1999, 1, 1, 0, 0, 0);
        _maxDate = new DateTime(2050, 1, 1, 0, 0, 0);
        Init();
    }

    private void Update()
    {

    }
    public void Init(DateTime dt)
    {
        _selectDate = dt;
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].myGroup = this;
            _datePickerList[i].Init();
            _datePickerList[i]._onDateUpdate += onDateUpdate;
        }
    }
    public void Init()
    {
        _selectDate = DateTime.Now;
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].myGroup = this;
            _datePickerList[i].Init();
            _datePickerList[i]._onDateUpdate += onDateUpdate;
        }
    }

    /// <summary>
    /// 当选择的日期更新
    /// </summary>
    /// 
    public static string DeadLineTime = string.Empty;
    public static string ReplayStartTime = string.Empty;
    public static string ReplayStartDate = string.Empty;
    public static string EventCenterStartDate = string.Empty;
    public static string EventCenterEndDate = string.Empty;
    public static string _Time = string.Empty;

    public static bool IsnewPlay = false;
    public void onDateUpdate()
    {
        // Log.Debug("当前选择日期：" + _selectDate.ToString("yyyy-MM-dd HH : mm : ss"));
        ReplayUIController.IsPause = false;
        IsnewPlay = true;
        ReplayStartTime = _selectDate.ToString("yyyyMMddHHmmss").Trim();
        ReplayStartDate = _selectDate.ToString("yyyyMMdd").Trim();
        if(ChoiceTimeEventStart.IsChoiceTimeStart)
        EventCenterStartDate= _selectDate.ToString("yyyy-MM-dd HH:mm:ss").Trim();
        if(ChoiceTimeEventEnd.IsChoiceTimeEnd)
        EventCenterEndDate = _selectDate.ToString("yyyy-MM-dd HH:mm:ss").Trim();
        _Time = _selectDate.ToString("HH:mm:ss").Trim();
         Log.Debug("当前选择播放录像回放时间:" + ReplayStartTime);
        //将选中的时间给_selectTime ，供其他界面调用
        _selectTime = _selectDate;
        DeadLineTime = _selectDate.ToString("yyyy-MM-dd HH : mm : ss");
        for (int i = 0; i < _datePickerList.Count; i++)
        {
            _datePickerList[i].RefreshDateList();
        }
    }
}

